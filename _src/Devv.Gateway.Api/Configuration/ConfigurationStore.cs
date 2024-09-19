namespace Devv.Gateway.Api.Configuration;

public class ConfigurationStore
{
    public const string SectionName = "ConfigurationStore";

    // Configuration store type
    public string? Type { get; init; }

    // Azure Key Vault configuration
    public string? AzureKeyVaultUri { get; init; }

    // AWS Secrets Manager configuration
    public string? AwsSecretsManagerRegion { get; init; }
    public string? AwsSecretsManagerSecretName { get; init; }

    // Environment variables configuration
    public Dictionary<string, string>? EnvironmentVariables { get; init; }

    // Startup parameters configuration
    public Dictionary<string, string>? StartupParameters { get; init; }
}