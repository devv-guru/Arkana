namespace Data.Entities.Proxy;

public class Transform : EntityBase
{
    public string? RequestHeader { get; set; }
    public string? Set { get; set; }

    // Foreign key to RouteConfig
    public Guid RouteConfigId { get; set; }
    public Route? RouteConfig { get; set; }
}