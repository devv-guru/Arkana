namespace Devv.WebServer.Api.Data.Entities;

public class PassiveHealthCheckConfig
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public string Policy { get; set; }
    public TimeSpan ReactivationPeriod { get; set; }

    // Foreign key to HealthCheckConfig
    public int HealthCheckConfigId { get; set; }
    public HealthCheckConfig HealthCheckConfig { get; set; }
}