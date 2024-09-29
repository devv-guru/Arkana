namespace Devv.Gateway.Data.Entities;

public class Route : EntityBase
{
    public Guid ClusterId { get; set; }

    public int? Order { get; set; }
    public long? MaxRequestBodySize { get; set; }
    public string? AuthorizationPolicy { get; set; }
    public string? CorsPolicy { get; set; }

    public Guid HostId { get; set; }
    public WebHost? Host { get; set; }
    public Match? Match { get; set; }
    public Metadata? Metadata { get; set; }
    public ICollection<Transform>? Transforms { get; set; }
}