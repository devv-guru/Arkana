using LiteDB;

namespace Data.Entities.Metric;

public sealed class ErrorMetric
{
    public ObjectId Id { get; set; }
    public Guid HostId { get; set; }
    public string Path { get; set; }
    public string Method { get; set; }
    public string ExceptionType { get; set; }
    public string ErrorMessage { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}