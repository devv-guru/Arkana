namespace Devv.Gateway.Data.Entities;

public class HealthCheckConfig : EntityBase
{
    public ActiveHealthCheckConfig? Active { get; set; }
    public PassiveHealthCheckConfig? Passive { get; set; }

    // Foreign key to ClusterConfig
    public Guid ClusterConfigId { get; set; }
    public ClusterConfig? ClusterConfig { get; set; }
}