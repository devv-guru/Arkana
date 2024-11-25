namespace Data.Entities;

public class Metadata : EntityBase
{
    public Dictionary<string, string> Data { get; set; }

    // Foreign key to either RouteConfig or ClusterConfig
    public Guid? RouteConfigId { get; set; }
    public Route? Route { get; set; }

    public Guid? ClusterConfigId { get; set; }
    public Cluster? Cluster { get; set; }
}