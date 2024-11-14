namespace Data.Entities.Proxy;

public class QueryParameterMatch : EntityBase
{
    public string Name { get; set; }
    public List<string> Values { get; set; }
    public string Mode { get; set; } // "Exact", "Prefix", etc.
    public bool IsCaseSensitive { get; set; }

    // Foreign key to MatchConfig
    public Guid MatchConfigId { get; set; }
    public Match Match { get; set; }
}