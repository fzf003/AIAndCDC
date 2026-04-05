using System.Text.Json;
using System.Text.Json.Serialization;

namespace CdcRedisConsumer;

/// <summary>
/// Debezium CDC 消息结构
/// </summary>
public record CdcMessage
{
    /// <summary>
    /// 操作类型: c=CREATE, u=UPDATE, d=DELETE, r=READ(snapshot)
    /// </summary>
    [JsonPropertyName("op")]
    public string? Op { get; init; }

    /// <summary>
    /// 变更前数据（UPDATE/DELETE 时有值）
    /// </summary>
    [JsonPropertyName("before")]
    public JsonElement? Before { get; init; }

    /// <summary>
    /// 变更后数据（CREATE/UPDATE 时有值）
    /// </summary>
    [JsonPropertyName("after")]
    public JsonElement? After { get; init; }

    /// <summary>
    /// 源信息
    /// </summary>
    [JsonPropertyName("source")]
    public SourceInfo? Source { get; init; }

    /// <summary>
    /// 消息时间戳（毫秒）
    /// </summary>
    [JsonPropertyName("ts_ms")]
    public long? TimestampMs { get; init; }
}

public record SourceInfo
{
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("connector")]
    public string? Connector { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("db")]
    public string? Database { get; init; }

    [JsonPropertyName("schema")]
    public string? Schema { get; init; }

    [JsonPropertyName("table")]
    public string? Table { get; init; }

    [JsonPropertyName("change_lsn")]
    public string? ChangeLsn { get; init; }

    [JsonPropertyName("commit_lsn")]
    public string? CommitLsn { get; init; }
}
