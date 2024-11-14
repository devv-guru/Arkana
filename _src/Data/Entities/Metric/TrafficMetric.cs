using LiteDB;

namespace Data.Entities.Metric;

public sealed class TrafficMetric
{
    public ObjectId Id { get; set; }
    public Guid HostId { get; set; }
    public long RequestSizeBytes { get; set; }
    public long ResponseSizeBytes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
