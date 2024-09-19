namespace Devv.Data.Entities;

public class MatchConfig
{
    public int Id { get; set; }
    public string Path { get; set; }
    public List<string> Hosts { get; set; }
    public List<string> Methods { get; set; }

    public List<HeaderMatchConfig> Headers { get; set; }
    public List<QueryParameterMatchConfig> QueryParameters { get; set; }

    // Foreign key to RouteConfig
    public int RouteConfigId { get; set; }
    public RouteConfig RouteConfig { get; set; }
}