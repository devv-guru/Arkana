namespace Devv.WebServer.Api.Data.Entities;

public class HeaderMatchConfig
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<string> Values { get; set; }
    public string Mode { get; set; } // "ExactHeader", "HeaderPrefix", etc.
    public bool IsCaseSensitive { get; set; }

    // Foreign key to MatchConfig
    public int MatchConfigId { get; set; }
    public MatchConfig MatchConfig { get; set; }
}