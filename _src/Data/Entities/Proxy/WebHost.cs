namespace Data.Entities.Proxy;

public class WebHost : EntityBase
{
    public string? Name { get; set; }
    public string? HostName { get; set; }

    public bool IsDefault { get; set; }

    public Guid CertificateId { get; set; }
    public Certificate? Certificate { get; set; }

    public Guid? ClusterId { get; set; }
    public Cluster? Cluster { get; set; }

    public List<Route> Routes { get; set; }
}