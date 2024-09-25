namespace Devv.Gateway.Data.Entities;

public class ActiveHealthCheckConfig : EntityBase
{
    public bool Enabled { get; set; }
    public TimeSpan? Interval { get; set; }
    public TimeSpan? Timeout { get; set; }
    public string? Policy { get; set; }
    public string? Path { get; set; }
    public string? Query { get; set; }

    // Foreign key to HealthCheckConfig
    public Guid HealthCheckConfigId { get; set; }
    public HealthCheckConfig? HealthCheckConfig { get; set; }
}