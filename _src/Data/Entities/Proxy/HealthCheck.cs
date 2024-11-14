namespace Data.Entities.Proxy;

public class HealthCheck : EntityBase
{
    public ActiveHealthCheck? Active { get; set; }
    public PassiveHealthCheck? Passive { get; set; }

    // Foreign key to ClusterConfig
    public Guid ClusterConfigId { get; set; }
    public Cluster? ClusterConfig { get; set; }
}