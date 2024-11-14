using LiteDB;

namespace Data.Entities.Metric;

public sealed class AuthMetric
{
    public ObjectId Id { get; set; }
    public Guid HostId { get; set; }
    public bool IsAuthenticated { get; set; }
    public string AuthMethod { get; set; } // Type of auth used, e.g., JWT, OAuth
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}