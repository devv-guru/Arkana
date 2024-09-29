namespace Devv.Gateway.Data.Entities;

public class HttpClientSettings : EntityBase
{
    public string? SslProtocols { get; set; }
    public bool DangerousAcceptAnyServerCertificate { get; set; }
    public int MaxConnectionsPerServer { get; set; }
    public bool EnableMultipleHttp2Connections { get; set; }
    public string? RequestHeaderEncoding { get; set; }
    public string? ResponseHeaderEncoding { get; set; }

    // Foreign key to ClusterConfig
    public Guid ClusterConfigId { get; set; }
    public Cluster? ClusterConfig { get; set; }
}