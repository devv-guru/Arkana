using LiteDB;

namespace Data.Entities.Metric;

public sealed class RequestMetric
{
    public ObjectId Id { get; set; }
    public Guid HostId { get; set; }
    public string Path { get; set; }
    public string Method { get; set; }
    public int StatusCode { get; set; }
    public double DurationMs { get; set; }
    public long ResponseSizeBytes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}