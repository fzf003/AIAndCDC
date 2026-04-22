using System.Collections.Concurrent;
using CdcHttpServer.Models;

namespace CdcHttpServer.Services;

/// <summary>
/// 内存事件存储实现，支持容量限制和 FIFO 淘汰策略
/// </summary>
public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentQueue<CdcEvent> _events = new();
    private readonly int _capacity;

    public InMemoryEventStore(int capacity = 1000)
    {
        _capacity = capacity > 0 ? capacity : 1000;
    }

    public int Count => _events.Count;

    public void Add(CdcEvent evt)
    {
        _events.Enqueue(evt);

        // FIFO 淘汰：超出容量时移除最旧的事件
        while (_events.Count > _capacity)
        {
            _events.TryDequeue(out _);
        }
    }

    public IReadOnlyList<CdcEvent> GetAll()
    {
        return _events.ToArray().Reverse().ToList();
    }

    public CdcEvent? GetById(string id)
    {
        return _events.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public EventStats GetStats()
    {
        var all = _events.ToArray();
        var now = DateTimeOffset.UtcNow;

        return new EventStats
        {
            TotalEvents = all.Length,
            ByOp = all.GroupBy(e => e.Op?.ToLower() ?? "unknown")
                      .ToDictionary(g => g.Key, g => g.Count()),
            LastMinute = all.Count(e => e.ReceivedAt >= now.AddMinutes(-1)),
            LastHour = all.Count(e => e.ReceivedAt >= now.AddHours(-1))
        };
    }
}
