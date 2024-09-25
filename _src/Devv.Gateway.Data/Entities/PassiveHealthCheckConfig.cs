namespace Devv.Gateway.Data.Entities;

public class PassiveHealthCheckConfig : EntityBase
{
    public bool Enabled { get; set; }
    public string? Policy { get; set; }
    public TimeSpan? ReactivationPeriod { get; set; }

    // Foreign key to HealthCheckConfig
    public Guid HealthCheckConfigId { get; set; }
    public HealthCheckConfig? HealthCheckConfig { get; set; }
}