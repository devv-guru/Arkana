namespace Devv.Gateway.Data.Entities;

public class Match : EntityBase
{
    public string Path { get; set; }
    public List<string> Hosts { get; set; }
    public List<string> Methods { get; set; }

    public List<HeaderMatch> Headers { get; set; }
    public List<QueryParameterMatch> QueryParameters { get; set; }

    // Foreign key to RouteConfig
    public Guid RouteConfigId { get; set; }
    public Route Route { get; set; }
}