using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace CdcWebUI;

public record CdcEvent(DateTime Timestamp, string database, string Table, string Op, string Data);

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://*:8889");
        var app = builder.Build();
        app.UseStaticFiles();

        Console.WriteLine("=== CDC Web UI ===");
        Console.WriteLine("Listening on http://*:8889");
        Console.WriteLine("Open: http://localhost:8889");

        var events = new ConcurrentQueue<CdcEvent>();
        const int MaxEvents = 100;

        app.MapPost(
            "/cdc",
            async (HttpContext ctx) =>
            {
                var json = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    var database =
                        root.GetProperty("source").GetProperty("db").GetString() ?? "Unknown";
                    var table =
                        root.GetProperty("source").GetProperty("table").GetString() ?? "Unknown";
                    var op = root.GetProperty("op").GetString() ?? "unknown";

                    op = op switch
                    {
                        "c" => "CREATE",
                        "u" => "UPDATE",
                        "d" => "DELETE",
                        "r" => "READ",
                        _ => ""
                    };

                    var after =
                        root.TryGetProperty("after", out var a) && a.ValueKind != JsonValueKind.Null
                            ? a
                            : root.GetProperty("before");
                    events.Enqueue(
                        new CdcEvent(DateTime.Now, database, table, op, after.ToString())
                    );
                    while (events.Count > MaxEvents)
                        events.TryDequeue(out _);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {table}.{op}");
                }
                catch { }
                return Results.Ok();
            }
        );

        app.MapGet(
            "/api/events",
            () => Results.Json(events.OrderByDescending(e => e.Timestamp).Take(50))
        );
        app.MapGet(
            "/api/stats",
            () =>
            {
                var now = DateTime.Now;
                return Results.Json(
                    new
                    {
                        Total = events.Count,
                        LastMinute = events.Count(e => (now - e.Timestamp).TotalMinutes <= 1),
                        LastHour = events.Count(e => (now - e.Timestamp).TotalHours <= 1)
                    }
                );
            }
        );

        app.Run();
    }
}
