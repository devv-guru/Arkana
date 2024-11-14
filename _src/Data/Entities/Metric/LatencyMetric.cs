using LiteDB;

namespace Data.Entities.Metric;

public sealed class LatencyMetric
{
    public ObjectId Id { get; set; }
    public Guid HostId { get; set; }
    public double DnsLookupMs { get; set; }
    public double TcpConnectionMs { get; set; }
    public double TlsHandshakeMs { get; set; }
    public double ResponseTimeMs { get; set; } // Total time taken for response
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}