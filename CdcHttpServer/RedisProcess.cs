using StackExchange.Redis;
using System.Text.Json;

public static class RedisProcess
{


    public static async Task Start()
    {
            var options = new JsonSerializerOptions
{
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
};

        var redis = await ConnectionMultiplexer.ConnectAsync(
            Environment.GetEnvironmentVariable("REDIS_URL") ?? "localhost:6379");
        var db = redis.GetDatabase();

        string stream = Environment.GetEnvironmentVariable("REDIS_STREAM_KEY")
            ?? throw new InvalidOperationException("Missing env: REDIS_STREAM_KEY");
        string group = "cdc-group";
        string consumer = "consumer-1";

        // 创建消费组（只需执行一次）
        try
        {
            await db.StreamCreateConsumerGroupAsync(stream, group, "$");
        }
        catch
        {
            // 已存在忽略
        }

        while (true)
        {
            var entries = await db.StreamReadGroupAsync(
                stream,
                group,
                consumer,
                ">", // 只读新消息
                count: 10
            );

            foreach (var entry in entries)
            {
                foreach (var field in entry.Values)
                {
                    var json = field.Value;

                    var evt = JsonSerializer.Deserialize<DebeziumEvent<Product>>(json,options);

                    if(!string.IsNullOrWhiteSpace(json))
                    {
                    Console.WriteLine($"数据: {JsonSerializer.Serialize(json,options)}");
                    }


                    Console.WriteLine($"操作: {evt.op}");
                    Console.WriteLine($"数据: {JsonSerializer.Serialize(evt.after,options)}");
                }

                // ACK（非常重要）
                await db.StreamAcknowledgeAsync(stream, group, entry.Id);
            }
        }
    }
}

// CDC 事件结构
public class DebeziumEvent<T>
{
    public T before { get; set; }
    public T after { get; set; }
    public string op { get; set; } // c/u/d/r
    public long ts_ms { get; set; }
}

public class Product
{
    public int CustomerId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }

    public string Sku { get; set; }

}
