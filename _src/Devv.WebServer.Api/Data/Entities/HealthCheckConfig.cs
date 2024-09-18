namespace Devv.WebServer.Api.Data.Entities;

public class HealthCheckConfig
{
    public int Id { get; set; }
    public ActiveHealthCheckConfig? Active { get; set; }
    public PassiveHealthCheckConfig? Passive { get; set; }

    // Foreign key to ClusterConfig
    public int ClusterConfigId { get; set; }
    public ClusterConfig? ClusterConfig { get; set; }
}