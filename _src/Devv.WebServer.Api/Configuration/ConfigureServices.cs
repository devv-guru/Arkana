using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Devv.WebServer.Api.Configuration.AwsSecretManager;
using Devv.WebServer.Api.Configuration.EnvironmentVariables;
using Devv.WebServer.Api.Configuration.StartParameters;

namespace Devv.WebServer.Api.Configuration;

public static class ConfigureServices
{
    public static void AddConfigurationSources(this IConfigurationBuilder configurationManager,
        IConfiguration configuration, string[] args)
    {
        var configurationStore =
            configuration.GetSection(ConfigurationStore.SectionName).Get<ConfigurationStore>();

        if (configurationStore is null)
            throw new Exception("Configuration store is not defined");

        switch (configurationStore.Type)
        {
            case ConfigurationStoreTypes.AppSettings:
                break;
            case ConfigurationStoreTypes.AzureKeyVault:
                configurationManager.ConfigureAzureKeyVault(configurationStore.AzureKeyVaultUri);
                break;
            case ConfigurationStoreTypes.AwsSecretsManager:
                configurationManager.AddAmazonSecretsManager(configurationStore.AwsSecretsManagerRegion,
                    configurationStore.AwsSecretsManagerSecretName);
                break;
            case ConfigurationStoreTypes.EnvironmentVariables:
                configurationManager.ConfigureEnvironmentVariables(configurationStore.EnvironmentVariables);
                break;
            case ConfigurationStoreTypes.StartupParameters:
                configurationManager.ConfigureStartupParameters(configurationStore.StartupParameters, args);
                break;
            default:
                throw new Exception($"Invalid configuration store type: {configurationStore.Type}");
        }
    }

    private static void ConfigureAzureKeyVault(this IConfigurationBuilder configurationManager,
        string? keyVaultUri)
    {
        if (string.IsNullOrWhiteSpace(keyVaultUri))
        {
            throw new Exception("Azure Key Vault URI is not defined");
        }

        configurationManager.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }

    private static void AddAmazonSecretsManager(this IConfigurationBuilder configurationManager,
        string? region, string? secretName)
    {
        if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(secretName))
        {
            throw new Exception("AWS Secrets Manager configuration is not defined");
        }

        var configurationSource = new AmazonSecretsManagerConfigurationSource(region, secretName);

        configurationManager.Add(configurationSource);
    }

    private static void ConfigureEnvironmentVariables(this IConfigurationBuilder configurationManager,
        Dictionary<string, string>? mappings)
    {
        if (mappings is null)
        {
            throw new Exception("Environment variables mappings are not defined");
        }

        var configurationSource =
            new EnvironmentVariableConfigurationSource(mappings);
        configurationManager.Add(configurationSource);
    }

    private static void ConfigureStartupParameters(this IConfigurationBuilder configurationManager,
        Dictionary<string, string>? mappings, string[] args)
    {
        if (mappings is null)
        {
            throw new Exception("Startup parameters mappings are not defined");
        }

        if (args.Length == 0)
        {
            throw new Exception("Startup parameters are not defined");
        }

        var configurationSource = new StartupParametersConfigurationSource(mappings, args);

        configurationManager.Add(configurationSource);
    }
}