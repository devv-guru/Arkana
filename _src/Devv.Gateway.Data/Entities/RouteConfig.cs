namespace Devv.Gateway.Data.Entities;

public class RouteConfig : EntityBase
{
    public Guid ClusterId { get; set; }

    public int? Order { get; set; }
    public long? MaxRequestBodySize { get; set; }
    public string? AuthorizationPolicy { get; set; }
    public string? CorsPolicy { get; set; }

    public Guid HostId { get; set; }
    public Host? Host { get; set; }
    public MatchConfig? Match { get; set; }
    public MetadataConfig? Metadata { get; set; }
    public ICollection<TransformConfig>? Transforms { get; set; }
}