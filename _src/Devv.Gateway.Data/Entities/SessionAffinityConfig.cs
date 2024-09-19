namespace Devv.Gateway.Data.Entities;

public class SessionAffinityConfig
{
    public int Id { get; set; }

    public bool Enabled { get; set; }
    public string Policy { get; set; }
    public string FailurePolicy { get; set; }
    public Dictionary<string, string> Settings { get; set; }

    // Foreign key to ClusterConfig
    public int ClusterConfigId { get; set; }
    public ClusterConfig ClusterConfig { get; set; }
}