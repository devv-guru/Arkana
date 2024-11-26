namespace Shared.Options;

public class DataContextOptions
{
    public const string SectionName = "DataContextOptions";
    public string? ConnectionString { get; init; }
    public string? Provider { get; init; }

    public string? MySqlVersion { get; init; } = "AutoDetect";
    public string? MariaDbVersion { get; init; } = "AutoDetect";
}