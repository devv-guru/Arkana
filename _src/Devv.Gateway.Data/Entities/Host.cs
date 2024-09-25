namespace Devv.Gateway.Data.Entities;

public class Host : EntityBase
{
    public string? HostName { get; set; }
    public string? Url { get; set; }

    public Guid CertificateId { get; set; }
    public CertificateConfig Certificate { get; set; }

    public Guid ClusterId { get; set; }
    public ClusterConfig Cluster { get; set; }

    public List<RouteConfig> Routes { get; set; }
}