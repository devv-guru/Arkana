namespace Devv.Gateway.Api.Configuration;

public class EnvironmentOptions
{
    public const string StaticRequestPath = "2ffc6c1d-6784-4ddf-ae9e-42898518e6b6";
    public string CertificatePath { get; set; } = string.Empty;
    public string LogsPath { get; set; } = string.Empty;
    public string ConfigPath { get; set; } = string.Empty;
    public string StaticFilesPath { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
}