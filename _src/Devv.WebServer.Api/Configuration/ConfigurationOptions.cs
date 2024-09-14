namespace Devv.WebServer.Api.Configuration;

public class ConfigurationOptions
{
    public const string SectionName = "ConfigurationOptions";
    public string? ManagementDomain { get; init; }
}