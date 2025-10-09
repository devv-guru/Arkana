using Data.Entities;

namespace Data.Entities.Metrics;

public class RequestMetric : EntityBase
{
    public string? RouteId { get; set; }
    public string? ClusterId { get; set; }
    public string? DestinationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? Host { get; set; }
    public int StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public long RequestSize { get; set; }
    public long ResponseSize { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 400;
    public bool IsError => StatusCode >= 400;
    public bool IsServerError => StatusCode >= 500;
}
