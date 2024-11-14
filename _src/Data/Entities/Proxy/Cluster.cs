namespace Data.Entities.Proxy;

public class Cluster : EntityBase
{
    public string? LoadBalancingPolicy { get; set; }
    public Guid HostId { get; set; }
    public WebHost? Host { get; set; }
    public SessionAffinity? SessionAffinity { get; set; }
    public HealthCheck? HealthCheck { get; set; }
    public HttpClientSettings? HttpClient { get; set; }
    public HttpRequestSettings? HttpRequest { get; set; }
    public Metadata? Metadata { get; set; }
    public ICollection<Destination>? Destinations { get; set; }
}