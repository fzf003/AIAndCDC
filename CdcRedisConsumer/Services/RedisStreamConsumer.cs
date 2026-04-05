using System.Text.Json;
using StackExchange.Redis;
using CdcRedisConsumer;
using CdcRedisConsumer.Handlers;

namespace CdcRedisConsumer.Services;

/// <summary>
/// Redis Stream 消费者服务
/// </summary>
public class RedisStreamConsumer : BackgroundService
{
    private readonly ILogger<RedisStreamConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly IEnumerable<ICdcHandler> _handlers;
    private readonly JsonSerializerOptions _jsonOptions;

    // Stream Key 格式: <topic.prefix>.<database>.<schema>.<table>
    // 示例: product.fzf003.dbo.Product
    private readonly string _streamKey;
    private readonly string _consumerGroup;
    private readonly string _consumerName;

    public RedisStreamConsumer(
        ILogger<RedisStreamConsumer> logger,
        IConfiguration config,
        IEnumerable<ICdcHandler> handlers)
    {
        _logger = logger;
        _config = config;
        _handlers = handlers;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // 配置（从 appsettings.json 读取，不要硬编码）
        _streamKey = config["Redis:StreamKey"]
            ?? throw new InvalidOperationException("Missing config: Redis:StreamKey");
        _consumerGroup = config["Redis:ConsumerGroup"] ?? "cdc-consumers";
        _consumerName = config["Redis:ConsumerName"] ?? $"consumer-{Environment.MachineName}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redisUrl = _config["Redis:Url"]
            ?? throw new InvalidOperationException("Missing config: Redis:Url");

        _logger.LogInformation("=== CDC Redis Stream Consumer ===");
        _logger.LogInformation(@"
            ____  _____ ____  ___ ____  
            |  _ \| ____|  _ \|_ _/ ___| 
            | |_) |  _| | | | || | |     
            |  _ <| |___| |_| || | |___  
            |_| \_\_____|____/|___\____| 

                    Redis Consumer
            ~~~~~~~~~~~~~~~~~~~~~~~~~~~

                ┌───────────────┐
                │   STREAM IN   │
                └──────┬────────┘
                        │
                >>>>>>> ▼ >>>>>>>
                    Processing...
                <<<<<< ▲ <<<<<<
                        │
                ┌──────┴────────┐
                │   ACK / DONE  │
                └───────────────┘

            [ Worker Running ✔ ]
            ");
        _logger.LogInformation("=== CDC Redis Stream Consumer ===");
        _logger.LogInformation("Redis: {Url}", redisUrl);
        _logger.LogInformation("Stream: {Stream}", _streamKey);
        _logger.LogInformation("Consumer Group: {Group}", _consumerGroup);
        _logger.LogInformation("Consumer Name: {Name}", _consumerName);
        _logger.LogInformation("Handlers: {Count}", _handlers.Count());

        try
        {
            var redis = await ConnectionMultiplexer.ConnectAsync(redisUrl);
            var db = redis.GetDatabase();

            // 确保消费者组存在
            await EnsureConsumerGroupAsync(db);

            // 开始消费
            await ConsumeAsync(db, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis connection failed");
            throw;
        }
    }

    /// <summary>
    /// 确保消费者组存在
    /// </summary>
    private async Task EnsureConsumerGroupAsync(IDatabase db)
    {
        try
        {
            // 尝试创建消费者组（从最新消息开始）
            await db.StreamCreateConsumerGroupAsync(
                _streamKey,
                _consumerGroup,
                StreamPosition.NewMessages);

            _logger.LogInformation("Created consumer group: {Group}", _consumerGroup);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // 消费者组已存在
            _logger.LogInformation("Consumer group already exists: {Group}", _consumerGroup);
        }
    }

    /// <summary>
    /// 消费 Stream 消息
    /// </summary>
    private async Task ConsumeAsync(IDatabase db, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting to consume messages...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 读取消息（阻塞 5 秒）
                var entries = await db.StreamReadGroupAsync(
                    _streamKey,
                    _consumerGroup,
                    _consumerName,
                    StreamPosition.NewMessages,
                    count: 10);

                if (entries.Length == 0)
                {
                    // 没有消息，继续等待
                    continue;
                }

                _logger.LogDebug("Received {Count} messages", entries.Length);

                foreach (var entry in entries)
                {
                    await ProcessEntryAsync(db, entry, stoppingToken);
                }
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error while consuming");
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer stopped");
                break;
            }
        }
    }

    /// <summary>
    /// 处理单条消息
    /// </summary>
    private async Task ProcessEntryAsync(IDatabase db, StreamEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            // Debezium Redis Stream 消息格式:
            // entry.Values 是 NameValueEntry[]，每个元素有 .Name 和 .Value
            // 格式: Name = "{\"Sku\":\"...\"}", Value = "{\"before\":...,\"after\":...,\"op\":\"u\",...}"
            //
            // 示例:
            // {
            //   "{\"Sku\":\"CDC Test-92163\"}": "{\"before\":{\"Sku\":\"CDC Test-92163\"...},\"op\":\"u\",...}"
            // }

            if (entry.Values.Length == 0)
            {
                _logger.LogWarning("Empty message: {Id}", entry.Id);
                return;
            }

            // CDC payload 在第一个元素的 Value 中
            var jsonPayload = entry.Values[0].Value.ToString();

            // 验证是有效的 JSON
            if (string.IsNullOrEmpty(jsonPayload) || !jsonPayload.TrimStart().StartsWith('{'))
            {
                _logger.LogWarning("Invalid JSON payload: {Id}", entry.Id);
                return;
            }

            // 解析 CDC 消息
            var message = JsonSerializer.Deserialize<CdcMessage>(jsonPayload, _jsonOptions);

            if (message == null)
            {
                _logger.LogWarning("Failed to deserialize message: {Id}", entry.Id);
                return;
            }

            // 输出原始消息（调试用）
            _logger.LogDebug("Raw message: Id={Id}, Op={Op}, Table={Table}",
                entry.Id, message.Op, message.Source?.Table);

            // 查找匹配的处理器
            var table = message.Source?.Table ?? "unknown";
            var handler = FindHandler(table);

            // 处理消息
            await handler.HandleAsync(message, cancellationToken);

            // 确认消息（ACK）
            await db.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);

            _logger.LogDebug("Processed and ACK: {Id}", entry.Id);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parse error for message: {Id}, skipping", entry.Id);
            // ACK 掉无法解析的消息，避免无限重试
            await db.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Id}", entry.Id);
            // 不 ACK，消息会保留在 Pending 列表中，稍后重试
        }
    }

    /// <summary>
    /// 查找匹配的处理器
    /// </summary>
    private ICdcHandler FindHandler(string table)
    {
        // 精确匹配
        var handler = _handlers.FirstOrDefault(h =>
            h.TablePattern.Equals(table, StringComparison.OrdinalIgnoreCase));

        if (handler != null)
            return handler;

        // 通配符匹配
        handler = _handlers.FirstOrDefault(h =>
            h.TablePattern == "*" ||
            h.TablePattern.EndsWith("*") &&
            table.StartsWith(h.TablePattern.TrimEnd('*'), StringComparison.OrdinalIgnoreCase));

        return handler ?? new DefaultHandler(_logger as ILogger<DefaultHandler> ?? throw new InvalidOperationException());
    }
}
