namespace Data.Entities;

public class ActiveHealthCheck : EntityBase
{
    public bool Enabled { get; set; }
    public TimeSpan? Interval { get; set; }
    public TimeSpan? Timeout { get; set; }
    public string? Policy { get; set; }
    public string? Path { get; set; }
    public string? Query { get; set; }

    // Foreign key to HealthCheckConfig
    public Guid HealthCheckConfigId { get; set; }
    public HealthCheck? HealthCheckConfig { get; set; }
}