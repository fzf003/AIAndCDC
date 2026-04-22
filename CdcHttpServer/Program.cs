using System.Text.Json;
using CdcHttpServer.Models;
using CdcHttpServer.Services;

var builder = WebApplication.CreateBuilder(args);

var serverUrls = builder.Configuration["Server:Urls"] ?? "http://*:8889";
builder.WebHost.UseUrls(serverUrls);

var storeCapacity = builder.Configuration.GetValue<int?>("EventStore:Capacity") ?? 1000;
var logRawPayload = builder.Configuration.GetValue<bool?>("Diagnostics:LogRawEventPayload") ?? false;

builder.Services.AddSingleton<IEventStore>(new InMemoryEventStore(storeCapacity));

var app = builder.Build();

app.Logger.LogInformation("CDC HTTP Server starting on {Urls}", serverUrls);
app.Logger.LogInformation("Event store capacity: {Capacity}", storeCapacity);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

app.MapPost("/cdc", async (HttpContext ctx, IEventStore eventStore, ILogger<Program> logger) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();

    if (string.IsNullOrWhiteSpace(json))
    {
        return CreateProblem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "请求体不能为空。",
            detail: "POST /cdc 需要包含合法的 CDC JSON 负载。");
    }

    try
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var schema = "?";
        var table = "?";
        var op = "?";
        long tsMs = 0;

        if (root.TryGetProperty("source", out var source))
        {
            schema = source.TryGetProperty("schema", out var schemaProp)
                ? schemaProp.GetString() ?? "?"
                : "?";
            table = source.TryGetProperty("table", out var tableProp)
                ? tableProp.GetString() ?? "?"
                : "?";
        }

        if (root.TryGetProperty("op", out var opProp))
        {
            op = opProp.GetString() ?? "?";
        }

        if (root.TryGetProperty("ts_ms", out var tsProp) && tsProp.TryGetInt64(out var parsedTsMs))
        {
            tsMs = parsedTsMs;
        }

        object? beforeData = null;
        object? afterData = null;

        if (root.TryGetProperty("before", out var beforeProp) && beforeProp.ValueKind != JsonValueKind.Null)
        {
            beforeData = JsonSerializer.Deserialize<object>(beforeProp.GetRawText(), jsonOptions);
        }

        if (root.TryGetProperty("after", out var afterProp) && afterProp.ValueKind != JsonValueKind.Null)
        {
            afterData = JsonSerializer.Deserialize<object>(afterProp.GetRawText(), jsonOptions);
        }

        var cdcEvent = new CdcEvent
        {
            SourceSchema = schema,
            SourceTable = table,
            Op = op,
            Before = beforeData,
            After = afterData,
            TsMs = tsMs,
            RawJson = json,
            ReceivedAt = DateTimeOffset.UtcNow
        };

        eventStore.Add(cdcEvent);

        logger.LogInformation(
            "CDC event stored. Schema={Schema}, Table={Table}, Op={Op}, Count={Count}",
            schema,
            table,
            op,
            eventStore.Count);

        if (logRawPayload)
        {
            logger.LogInformation("Raw CDC payload: {Payload}", json);
        }

        return Results.Ok(new
        {
            message = "CDC event received.",
            eventId = cdcEvent.Id
        });
    }
    catch (JsonException ex)
    {
        logger.LogWarning(ex, "Invalid CDC payload received.");
        return CreateProblem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "CDC 事件 JSON 无法解析。",
            detail: ex.Message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to process CDC payload.");
        return CreateProblem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "处理 CDC 事件失败。",
            detail: ex.Message);
    }
});

app.MapGet("/api/events", (IEventStore eventStore, int? page, int? size, string? op, ILogger<Program> logger) =>
{
    try
    {
        var pageNumber = page ?? 1;
        var pageSize = size ?? 100;

        if (pageNumber < 1)
        {
            return CreateProblem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "分页参数不合法。",
                detail: "page 必须大于或等于 1。");
        }

        if (pageSize < 1 || pageSize > 200)
        {
            return CreateProblem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "分页参数不合法。",
                detail: "size 必须在 1 到 200 之间。");
        }

        var events = eventStore.GetAll();

        if (!string.IsNullOrWhiteSpace(op))
        {
            events = events
                .Where(x => string.Equals(x.Op, op, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var total = events.Count;
        var items = events
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Results.Ok(new PagedResult<CdcEvent>
        {
            Items = items,
            Total = total,
            Page = pageNumber,
            Size = pageSize
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to query CDC events.");
        return CreateProblem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "查询 CDC 事件失败。",
            detail: ex.Message);
    }
});

app.MapGet("/api/events/{id}", (string id, IEventStore eventStore, ILogger<Program> logger) =>
{
    try
    {
        var evt = eventStore.GetById(id);

        return evt is null
            ? CreateProblem(
                statusCode: StatusCodes.Status404NotFound,
                title: "未找到对应事件。",
                detail: $"事件 ID '{id}' 不存在。")
            : Results.Ok(evt);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to query CDC event {EventId}.", id);
        return CreateProblem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "查询单条 CDC 事件失败。",
            detail: ex.Message);
    }
});

app.MapGet("/api/stats", (IEventStore eventStore, ILogger<Program> logger) =>
{
    try
    {
        return Results.Ok(eventStore.GetStats());
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to query CDC stats.");
        return CreateProblem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "查询 CDC 统计失败。",
            detail: ex.Message);
    }
});

app.MapGet("/", () => Results.Ok(new
{
    service = "CdcHttpServer",
    message = "CDC HTTP server is running.",
    recommendedClient = "CdcWebUIClient"
}));

app.MapGet("/health", (IConfiguration configuration) => Results.Ok(new
{
    status = "ok",
    service = "CdcHttpServer",
    checks = new
    {
        http = "ok",
        eventStore = "ok",
        redisConsumer = "externalized"
    },
    configuration = new
    {
        serverUrls = configuration["Server:Urls"] ?? "http://*:8889",
        eventStoreCapacity = configuration.GetValue<int?>("EventStore:Capacity") ?? 1000
    },
    timestamp = DateTimeOffset.UtcNow
}));

app.Run();

static IResult CreateProblem(int statusCode, string title, string detail) =>
    Results.Problem(
        statusCode: statusCode,
        title: title,
        detail: detail);

public partial class Program;
