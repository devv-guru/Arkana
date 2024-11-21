namespace Proxy.Telemetry;

public struct HttpTelemetryMetric
{
    public string CorrelationId { get; set; }
    public DateTime RequestStartTime { get; set; }
    public DateTime? RequestStopTime { get; set; }
    public TimeSpan? RequestDuration => RequestStopTime.HasValue ? RequestStopTime - RequestStartTime : null;
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string Scheme { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string PathAndQuery { get; set; }
    public bool IsComplete => RequestStopTime.HasValue || !string.IsNullOrEmpty(ErrorMessage);
}
