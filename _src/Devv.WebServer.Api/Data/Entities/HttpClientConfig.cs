namespace Devv.WebServer.Api.Data.Entities;

public class HttpClientConfig
{
    public int Id { get; set; }
    public string SslProtocols { get; set; }
    public bool DangerousAcceptAnyServerCertificate { get; set; }
    public int MaxConnectionsPerServer { get; set; }
    public bool EnableMultipleHttp2Connections { get; set; }
    public string RequestHeaderEncoding { get; set; }
    public string ResponseHeaderEncoding { get; set; }

    // Foreign key to ClusterConfig
    public int ClusterConfigId { get; set; }
    public ClusterConfig ClusterConfig { get; set; }
}