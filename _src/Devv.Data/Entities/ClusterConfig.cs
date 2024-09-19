namespace Devv.Data.Entities;

public class ClusterConfig
{
    public int Id { get; set; }
    public string ClusterId { get; set; }

    public string LoadBalancingPolicy { get; set; } // "PowerOfTwoChoices", etc.
    public SessionAffinityConfig SessionAffinity { get; set; }
    public HealthCheckConfig HealthCheck { get; set; }
    public HttpClientConfig HttpClient { get; set; }
    public HttpRequestConfig HttpRequest { get; set; }
    public MetadataConfig Metadata { get; set; }

    public List<DestinationConfig> Destinations { get; set; }
}