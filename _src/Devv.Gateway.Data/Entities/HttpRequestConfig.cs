namespace Devv.Gateway.Data.Entities;

public class HttpRequestConfig : EntityBase
{
    public TimeSpan ActivityTimeout { get; set; }
    public string? Version { get; set; }
    public string? VersionPolicy { get; set; }
    public bool AllowResponseBuffering { get; set; }

    // Foreign key to ClusterConfig
    public Guid ClusterConfigId { get; set; }
    public ClusterConfig? ClusterConfig { get; set; }
}