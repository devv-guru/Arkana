namespace Devv.WebServer.Api.Data.Entities;

public class DestinationConfig
{
    public int Id { get; set; }
    public string DestinationId { get; set; }
    public string Address { get; set; }
    public string Health { get; set; }

    // Foreign key to ClusterConfig
    public int ClusterConfigId { get; set; }
    public ClusterConfig ClusterConfig { get; set; }
}