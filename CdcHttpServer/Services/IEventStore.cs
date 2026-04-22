using CdcHttpServer.Models;

namespace CdcHttpServer.Services;

/// <summary>
/// CDC 事件存储接口，预留未来扩展持久化实现的空间。
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// 添加事件。
    /// </summary>
    void Add(CdcEvent evt);

    /// <summary>
    /// 获取全部事件，按接收时间倒序返回。
    /// </summary>
    IReadOnlyList<CdcEvent> GetAll();

    /// <summary>
    /// 按事件 ID 获取单条记录。
    /// </summary>
    CdcEvent? GetById(string id);

    /// <summary>
    /// 获取统计信息。
    /// </summary>
    EventStats GetStats();

    /// <summary>
    /// 当前存储中的事件数量。
    /// </summary>
    int Count { get; }
}

/// <summary>
/// 事件统计信息。
/// </summary>
public class EventStats
{
    /// <summary>
    /// 事件总数。
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// 按操作类型统计。
    /// </summary>
    public Dictionary<string, int> ByOp { get; set; } = new();

    /// <summary>
    /// 为前端兼容保留的总数别名。
    /// </summary>
    public int Total => TotalEvents;

    /// <summary>
    /// 最近一分钟事件数。
    /// </summary>
    public int LastMinute { get; set; }

    /// <summary>
    /// 最近一小时事件数。
    /// </summary>
    public int LastHour { get; set; }
}

/// <summary>
/// 分页响应模型。
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// 当前页数据。
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// 总记录数。
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码。
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页条数。
    /// </summary>
    public int Size { get; set; }
}
