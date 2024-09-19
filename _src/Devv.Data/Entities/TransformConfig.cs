namespace Devv.Data.Entities;

public class TransformConfig
{
    public int Id { get; set; }
    public string RequestHeader { get; set; }
    public string Set { get; set; }

    // Foreign key to RouteConfig
    public int RouteConfigId { get; set; }
    public RouteConfig RouteConfig { get; set; }
}