namespace Data.Entities.Proxy;

public class Destination : EntityBase
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Health { get; set; }

    // Foreign key to ClusterConfig
    public Guid ClusterConfigId { get; set; }
    public Cluster? ClusterConfig { get; set; }
}