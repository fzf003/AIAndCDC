using System.Collections.Concurrent;
using System.Diagnostics;
using CdcRedisConsumer.Handlers;
using CdcRedisConsumer;
using CdcRedisConsumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Xunit;

namespace AIAndCDC.Tests;

public class RedisConsumerIntegrationTests
{
    [Fact]
    public async Task RedisConsumer_ShouldMatchProductHandler_AndAcknowledgeMessage()
    {
        var stream = $"p2.test.product.{Guid.NewGuid():N}";
        var group = $"p2-group-{Guid.NewGuid():N}";
        var consumer = $"p2-consumer-{Guid.NewGuid():N}";

        using var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = redis.GetDatabase();

        await db.KeyDeleteAsync(stream);

        var logs = new ConcurrentQueue<string>();
        using var loggerProvider = new InMemoryLoggerProvider(logs);
        var probeHandler = new ProbeProductHandler();

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Redis:Url"] = "localhost:6379",
                    ["Redis:StreamKey"] = stream,
                    ["Redis:ConsumerGroup"] = group,
                    ["Redis:ConsumerName"] = consumer
                });
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(loggerProvider);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton(probeHandler);
                services.AddSingleton<ICdcHandler>(probeHandler);
                services.AddSingleton<ICdcHandler, DefaultHandler>();
                services.AddHostedService<RedisStreamConsumer>();
            })
            .Build();

        await host.StartAsync();

        try
        {
            var ready = await WaitUntilAsync(
                () => logs.Any(x => x.Contains("Waiting for Redis stream messages.", StringComparison.OrdinalIgnoreCase)),
                TimeSpan.FromSeconds(10));

            Assert.True(ready);

            var payload = """
            {
              "op": "c",
              "before": null,
              "after": {
                "Sku": "TEST-REDIS-001",
                "Price": 12.34,
                "ProductName": "Redis 测试商品",
                "CustomerId": 7
              },
              "source": {
                "db": "testdb",
                "schema": "dbo",
                "table": "Product"
              },
              "ts_ms": 1713777777000
            }
            """;

            await db.StreamAddAsync(stream, "payload", payload);

            var matched = await WaitUntilAsync(
                () => probeHandler.HandledCount > 0,
                TimeSpan.FromSeconds(10));

            Assert.True(matched);

            var pending = await db.ExecuteAsync("XPENDING", stream, group);
            var pendingArray = (RedisResult[])pending!;
            var pendingCount = (int)pendingArray[0];

            Assert.Equal(0, pendingCount);
        }
        finally
        {
            await host.StopAsync();
            await db.KeyDeleteAsync(stream);
        }
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(200);
        }

        return false;
    }
}

internal sealed class ProbeProductHandler : ICdcHandler
{
    private int _handledCount;

    public string TablePattern => "Product";

    public int HandledCount => _handledCount;

    public Task HandleAsync(CdcMessage message, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _handledCount);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<string> _logs;

    public InMemoryLoggerProvider(ConcurrentQueue<string> logs)
    {
        _logs = logs;
    }

    public ILogger CreateLogger(string categoryName) => new InMemoryLogger(_logs, categoryName);

    public void Dispose()
    {
    }

    private sealed class InMemoryLogger : ILogger
    {
        private readonly ConcurrentQueue<string> _logs;
        private readonly string _categoryName;

        public InMemoryLogger(ConcurrentQueue<string> logs, string categoryName)
        {
            _logs = logs;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _logs.Enqueue($"{_categoryName}|{logLevel}|{formatter(state, exception)}");
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
