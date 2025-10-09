namespace Data.Entities;

public class PassiveHealthCheck : EntityBase
{
    public bool Enabled { get; set; }
    public string? Policy { get; set; }
    public TimeSpan? ReactivationPeriod { get; set; }

    // Foreign key to HealthCheckConfig
    public Guid HealthCheckConfigId { get; set; }
    public HealthCheck? HealthCheckConfig { get; set; }
}