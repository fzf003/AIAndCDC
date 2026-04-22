using System.Text.Json;
using CdcRedisConsumer.Handlers;
using StackExchange.Redis;

namespace CdcRedisConsumer.Services;

/// <summary>
/// Redis Stream 消费后台服务。
/// </summary>
public class RedisStreamConsumer : BackgroundService
{
    private readonly ILogger<RedisStreamConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly IEnumerable<ICdcHandler> _handlers;
    private readonly JsonSerializerOptions _jsonOptions;
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

        _streamKey = config["Redis:StreamKey"]
            ?? throw new InvalidOperationException("Missing config: Redis:StreamKey");
        _consumerGroup = config["Redis:ConsumerGroup"] ?? "cdc-consumers";
        _consumerName = config["Redis:ConsumerName"] ?? $"consumer-{Environment.MachineName}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redisUrl = _config["Redis:Url"]
            ?? throw new InvalidOperationException("Missing config: Redis:Url");

        _logger.LogInformation("CDC Redis consumer starting.");
        _logger.LogInformation("Redis: {Url}", redisUrl);
        _logger.LogInformation("Stream: {Stream}", _streamKey);
        _logger.LogInformation("Consumer Group: {Group}", _consumerGroup);
        _logger.LogInformation("Consumer Name: {Name}", _consumerName);
        _logger.LogInformation("Registered handlers: {Count}", _handlers.Count());

        try
        {
            var redis = await ConnectionMultiplexer.ConnectAsync(redisUrl);
            var db = redis.GetDatabase();

            await EnsureConsumerGroupAsync(db);
            await ConsumeAsync(db, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis consumer failed to start.");
            throw;
        }
    }

    private async Task EnsureConsumerGroupAsync(IDatabase db)
    {
        try
        {
            await db.StreamCreateConsumerGroupAsync(
                _streamKey,
                _consumerGroup,
                StreamPosition.NewMessages);

            _logger.LogInformation("Created consumer group: {Group}", _consumerGroup);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            _logger.LogInformation("Consumer group already exists: {Group}", _consumerGroup);
        }
    }

    private async Task ConsumeAsync(IDatabase db, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Waiting for Redis stream messages.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(
                    _streamKey,
                    _consumerGroup,
                    _consumerName,
                    StreamPosition.NewMessages,
                    count: 10);

                if (entries.Length == 0)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                _logger.LogDebug("Received {Count} Redis stream messages.", entries.Length);

                foreach (var entry in entries)
                {
                    await ProcessEntryAsync(db, entry, stoppingToken);
                }
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error while consuming messages.");
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Redis consumer stopped.");
                break;
            }
        }
    }

    private async Task ProcessEntryAsync(IDatabase db, StreamEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            if (entry.Values.Length == 0)
            {
                _logger.LogWarning("Received empty stream entry: {Id}", entry.Id);
                return;
            }

            var jsonPayload = entry.Values[0].Value.ToString();

            if (string.IsNullOrWhiteSpace(jsonPayload) || !jsonPayload.TrimStart().StartsWith('{'))
            {
                _logger.LogWarning("Invalid JSON payload in stream entry: {Id}", entry.Id);
                return;
            }

            var message = JsonSerializer.Deserialize<CdcMessage>(jsonPayload, _jsonOptions);

            if (message == null)
            {
                _logger.LogWarning("Failed to deserialize stream entry: {Id}", entry.Id);
                return;
            }

            var table = message.Source?.Table ?? "unknown";
            var handler = FindHandler(table);

            _logger.LogInformation(
                "Processing Redis CDC message. EntryId={EntryId}, Table={Table}, Op={Op}, Handler={Handler}",
                entry.Id,
                table,
                message.Op,
                handler.GetType().Name);

            await handler.HandleAsync(message, cancellationToken);
            await db.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);

            _logger.LogDebug("Acknowledged stream entry: {Id}", entry.Id);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parse error for Redis stream entry: {Id}", entry.Id);
            await db.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Redis stream entry: {Id}", entry.Id);
        }
    }

    private ICdcHandler FindHandler(string table)
    {
        var handler = _handlers.FirstOrDefault(h =>
            h.TablePattern.Equals(table, StringComparison.OrdinalIgnoreCase));

        if (handler != null)
        {
            return handler;
        }

        handler = _handlers.FirstOrDefault(h =>
            h.TablePattern == "*" ||
            (h.TablePattern.EndsWith("*") &&
             table.StartsWith(h.TablePattern.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)));

        return handler ?? throw new InvalidOperationException("No CDC handler is registered.");
    }
}
