using LiteDB;

namespace Data.Entities.Metric;

public sealed class HealthCheckMetric
{
    public ObjectId Id { get; set; }
    public Guid HostId { get; set; }
    public bool IsHealthy { get; set; }
    public string Message { get; set; } // Additional info, e.g., response or error details
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
