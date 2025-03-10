using Data.Entities.Metrics;

namespace Endpoints.Metrics.GetRequest;

public class Response
{
    public List<RequestMetricDto> Metrics { get; set; } = new();
}

public class RequestMetricDto
{
    public string Id { get; set; } = string.Empty;
    public string? RouteId { get; set; }
    public string? ClusterId { get; set; }
    public string? DestinationId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? Host { get; set; }
    public int StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public long RequestSize { get; set; }
    public long ResponseSize { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsError { get; set; }
    public bool IsServerError { get; set; }
}
