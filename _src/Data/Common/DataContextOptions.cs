namespace Data.Common;

public class DataContextOptions
{
    public const string SectionName = "DataContextOptions";
    public string? ConfigurationConnectionString { get; init; }
    public string? MetrixConnectionString { get; init; }
    public string? DatabasePassword { get; init; }
}