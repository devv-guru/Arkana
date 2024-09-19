namespace Devv.Gateway.Data.Entities;

public class HttpRequestConfig
{
    public int Id { get; set; }
    public TimeSpan ActivityTimeout { get; set; }
    public string Version { get; set; }
    public string VersionPolicy { get; set; }
    public bool AllowResponseBuffering { get; set; }

    // Foreign key to ClusterConfig
    public int ClusterConfigId { get; set; }
    public ClusterConfig ClusterConfig { get; set; }
}