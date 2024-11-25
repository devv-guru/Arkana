namespace Gateway.Configuration;

public class EnvironmentOptions
{
    public const string StaticRequestPath = "2ffc6c1d67844ddfae9e42898518e6b6";
    public string CertificatePath { get; set; } = string.Empty;
    public string LogsPath { get; set; } = string.Empty;
    public string ConfigPath { get; set; } = string.Empty;
    public string StaticFilesPath { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
}