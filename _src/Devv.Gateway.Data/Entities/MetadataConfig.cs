namespace Devv.Gateway.Data.Entities;

public class MetadataConfig : EntityBase
{
    public Dictionary<string, string> Data { get; set; }

    // Foreign key to either RouteConfig or ClusterConfig
    public Guid? RouteConfigId { get; set; }
    public RouteConfig RouteConfig { get; set; }

    public Guid? ClusterConfigId { get; set; }
    public ClusterConfig ClusterConfig { get; set; }
}