namespace Data.Entities;

public class HeaderMatch : EntityBase
{
    public string? Name { get; set; }
    public List<string> Values { get; set; }
    public string? Mode { get; set; } // "ExactHeader", "HeaderPrefix", etc.
    public bool IsCaseSensitive { get; set; }

    // Foreign key to MatchConfig
    public Guid MatchConfigId { get; set; }
    public Match? MatchConfig { get; set; }
}