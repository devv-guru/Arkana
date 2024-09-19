namespace Devv.Data.Entities;

public class RouteConfig
{
    public int Id { get; set; }
    public string RouteId { get; set; }
    public string ClusterId { get; set; }
    public int? Order { get; set; }
    public long? MaxRequestBodySize { get; set; }
    public string AuthorizationPolicy { get; set; }
    public string CorsPolicy { get; set; }

    public MatchConfig Match { get; set; }
    public MetadataConfig Metadata { get; set; }
    public List<TransformConfig> Transforms { get; set; }

    // Certificate Configuration
    public CertificateConfig Certificate { get; set; }
}