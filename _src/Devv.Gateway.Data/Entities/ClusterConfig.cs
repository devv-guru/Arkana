namespace Devv.Gateway.Data.Entities;

public class ClusterConfig : EntityBase
{
    public string? LoadBalancingPolicy { get; set; }
    public Guid HostId { get; set; }
    public Host? Host { get; set; }
    public SessionAffinityConfig? SessionAffinity { get; set; }
    public HealthCheckConfig? HealthCheck { get; set; }
    public HttpClientConfig? HttpClient { get; set; }
    public HttpRequestConfig? HttpRequest { get; set; }
    public MetadataConfig? Metadata { get; set; }
    public ICollection<DestinationConfig>? Destinations { get; set; }
}