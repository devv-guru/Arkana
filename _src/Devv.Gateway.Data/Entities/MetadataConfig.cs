namespace Devv.Gateway.Data.Entities;

public class MetadataConfig
{
    public int Id { get; set; }
    public Dictionary<string, string> Data { get; set; }

    // Foreign key to either RouteConfig or ClusterConfig
    public int? RouteConfigId { get; set; }
    public RouteConfig RouteConfig { get; set; }

    public int? ClusterConfigId { get; set; }
    public ClusterConfig ClusterConfig { get; set; }
}