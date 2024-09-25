namespace Devv.Gateway.Data.Entities;

public class SessionAffinityConfig : EntityBase
{
    public bool Enabled { get; set; }
    public string? Policy { get; set; }
    public string? FailurePolicy { get; set; }
    public Dictionary<string, string>? Settings { get; set; }

    // Foreign key to ClusterConfig
    public Guid ClusterConfigId { get; set; }
    public ClusterConfig? ClusterConfig { get; set; }
}