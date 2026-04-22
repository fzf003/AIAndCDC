namespace CdcHttpServer.Models;

/// <summary>
/// CDC 事件统一数据模型。
/// </summary>
public class CdcEvent
{
    /// <summary>
    /// 事件唯一标识。
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 来源 Schema。
    /// </summary>
    public string SourceSchema { get; set; } = string.Empty;

    /// <summary>
    /// 来源表名。
    /// </summary>
    public string SourceTable { get; set; } = string.Empty;

    /// <summary>
    /// Debezium 原始操作类型：c/u/d/r。
    /// </summary>
    public string Op { get; set; } = string.Empty;

    /// <summary>
    /// 变更前数据。
    /// </summary>
    public object? Before { get; set; }

    /// <summary>
    /// 变更后数据。
    /// </summary>
    public object? After { get; set; }

    /// <summary>
    /// 上游事件时间戳（毫秒）。
    /// </summary>
    public long TsMs { get; set; }

    /// <summary>
    /// 服务端接收时间。
    /// </summary>
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 原始 JSON 内容。
    /// </summary>
    public string RawJson { get; set; } = string.Empty;
}
