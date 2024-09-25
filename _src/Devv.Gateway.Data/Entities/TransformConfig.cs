namespace Devv.Gateway.Data.Entities;

public class TransformConfig : EntityBase
{
    public string? RequestHeader { get; set; }
    public string? Set { get; set; }

    // Foreign key to RouteConfig
    public Guid RouteConfigId { get; set; }
    public RouteConfig? RouteConfig { get; set; }
}