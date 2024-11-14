using LiteDB;

namespace Data.Entities.Metric;

public sealed class RateLimitMetric
{
    public ObjectId Id { get; set; }
    public Guid HostId { get; set; }
    public string Path { get; set; }
    public string Method { get; set; }
    public bool IsRateLimited { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
