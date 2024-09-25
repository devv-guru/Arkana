namespace Devv.Gateway.Data.Entities;

public class MatchConfig : EntityBase
{
    public string Path { get; set; }
    public List<string> Hosts { get; set; }
    public List<string> Methods { get; set; }

    public List<HeaderMatchConfig> Headers { get; set; }
    public List<QueryParameterMatchConfig> QueryParameters { get; set; }

    // Foreign key to RouteConfig
    public Guid RouteConfigId { get; set; }
    public RouteConfig RouteConfig { get; set; }
}