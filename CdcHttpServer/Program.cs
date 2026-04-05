using System.Text;
using System.Text.Json;



await RedisProcess.Start();

Console.ReadKey();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:8889");
var app = builder.Build();

Console.WriteLine("=== CDC HTTP Server ===");
Console.WriteLine($"Listening on http://*:8889/cdc");
Console.WriteLine($"Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine();

// 日志文件
var logFile = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".qclaw", "workspace", "projects", "CdcService", "CdcHttpServer", "cdc_events.log");
File.AppendAllText(logFile, $"=== CDC Server Started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");

  var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

       

app.MapPost("/cdc", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var json = await reader.ReadToEndAsync();
    
    // 记录完整日志
    var logEntry = $"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] 📨 CDC Event\n{json}\n";
    File.AppendAllText(logFile, logEntry);
    
    Console.WriteLine();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📨 CDC Event Received");
    Console.WriteLine($"  Full JSON saved to: {logFile}");
    
    try
    {
        Console.WriteLine("  Parsing JSON...");


        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

       // 3. 序列化 RootElement 并输出
        string formattedJson = JsonSerializer.Serialize(doc.RootElement, options);
        Console.WriteLine(formattedJson);


        Console.WriteLine("  Parsed JSON:");
        
        if (root.TryGetProperty("source", out var source))
        {
            var schema = source.TryGetProperty("schema", out var s) ? s.GetString() : "?";
            var table = source.TryGetProperty("table", out var t) ? t.GetString() : "?";
            Console.WriteLine($"  Table: {schema}.{table}");
        }
        
        if (root.TryGetProperty("op", out var op))
        {
            Console.WriteLine($"  Op: {GetOpIcon(op.GetString())} {op.GetString()?.ToUpper()}");
        }
        
        // 显示所有字段
        var target = root.TryGetProperty("after", out var after) && after.ValueKind != JsonValueKind.Null 
            ? after 
            : (root.TryGetProperty("before", out var before) ? before : default);
        
        if (target.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine($"  Data:");
            foreach (var prop in target.EnumerateObject())
            {
                Console.WriteLine($"    {prop.Name}: {prop.Value}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ⚠️ Parse Error: {ex.Message}");
    }
    
    Console.WriteLine($"  Raw JSON ({json.Length} bytes):");
    Console.WriteLine($"  {json[..Math.Min(300, json.Length)]}{(json.Length > 300 ? "..." : "")}");
    
    return Results.Ok();
});

app.MapGet("/", () => "CDC HTTP Server is running");
app.MapGet("/health", () => "OK");

app.Run();

static string GetOpIcon(string? op) => op?.ToLower() switch
{
    "c" => "🟢 CREATE",
    "u" => "🟡 UPDATE", 
    "d" => "🔴 DELETE",
    "r" => "🔵 READ",
    _ => "⚪"
};
