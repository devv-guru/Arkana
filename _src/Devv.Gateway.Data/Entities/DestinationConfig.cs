namespace Devv.Gateway.Data.Entities;

public class DestinationConfig : EntityBase
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Health { get; set; }

    // Foreign key to ClusterConfig
    public Guid ClusterConfigId { get; set; }
    public ClusterConfig? ClusterConfig { get; set; }
}