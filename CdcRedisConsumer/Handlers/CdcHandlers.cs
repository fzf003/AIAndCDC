using System.Text.Json;
using CdcRedisConsumer;

namespace CdcRedisConsumer.Handlers;

/// <summary>
/// CDC 消息处理器接口
/// </summary>
public interface ICdcHandler
{
    /// <summary>
    /// 表名（支持通配符，如 "dbo.*" 或 "*"）
    /// </summary>
    string TablePattern { get; }

    /// <summary>
    /// 处理 CDC 消息
    /// </summary>
    Task HandleAsync(CdcMessage message, CancellationToken cancellationToken);
}

/// <summary>
/// Product 表处理器示例
/// </summary>
public class ProductHandler : ICdcHandler
{
    private readonly ILogger<ProductHandler> _logger;

    public string TablePattern => "Product";

    public ProductHandler(ILogger<ProductHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(CdcMessage message, CancellationToken cancellationToken)
    {
        var op = message.Op switch
        {
            "c" => "CREATE",
            "u" => "UPDATE",
            "d" => "DELETE",
            "r" => "READ",
            _ => message.Op
        };

        _logger.LogInformation("[Product] {Op}", op);

        // 输出 Before
        if (message.Before.HasValue)
        {
            var before = message.Before.Value;
            _logger.LogInformation("  Before: Sku={Sku}, Price={Price}, Name={Name}, CustomerId={CustomerId}",
                before.GetProperty("Sku").GetString(),
                before.GetProperty("Price").GetString(),
                before.TryGetProperty("ProductName", out var bn) ? bn.GetString() : null,
                before.GetProperty("CustomerId").GetInt32());
        }

        // 输出 After
        if (message.After.HasValue)
        {
            var after = message.After.Value;
            _logger.LogInformation("  After: Sku={Sku}, Price={Price}, Name={Name}, CustomerId={CustomerId}",
                after.GetProperty("Sku").GetString(),
                after.GetProperty("Price").GetString(),
                after.TryGetProperty("ProductName", out var an) ? an.GetString() : null,
                after.GetProperty("CustomerId").GetInt32());
        }

        // TODO: 实现业务逻辑
        // - 写入数据库
        // - 调用 API
        // - 发送通知
        // - 更新缓存

        return Task.CompletedTask;
    }
}

/// <summary>
/// 默认处理器（处理所有未匹配的表）
/// </summary>
public class DefaultHandler : ICdcHandler
{
    private readonly ILogger<DefaultHandler> _logger;

    public string TablePattern => "*";

    public DefaultHandler(ILogger<DefaultHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(CdcMessage message, CancellationToken cancellationToken)
    {
        var table = message.Source?.Table ?? "unknown";
        _logger.LogInformation(
            "[{Table}] {Op} - No specific handler, using default",
            table, message.Op);

        return Task.CompletedTask;
    }
}
