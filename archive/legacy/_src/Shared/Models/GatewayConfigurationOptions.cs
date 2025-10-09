namespace Shared.Models;

/// <summary>
/// Options for gateway configuration
/// </summary>
public class GatewayConfigurationOptions
{
    /// <summary>
    /// Gets or sets the configuration store type
    /// </summary>
    public string ConfigurationStoreType { get; set; } = "File";
    
    /// <summary>
    /// Gets or sets the configuration file path
    /// </summary>
    public string ConfigurationFilePath { get; set; } = "_config/gateway-config.json";
    
    /// <summary>
    /// Gets or sets the AWS region
    /// </summary>
    public string AwsRegion { get; set; } = "us-east-1";
    
    /// <summary>
    /// Gets or sets the AWS secret name
    /// </summary>
    public string AwsSecretName { get; set; } = "gateway-config";
    
    /// <summary>
    /// Gets or sets the Azure Key Vault URI
    /// </summary>
    public string AzureKeyVaultUri { get; set; } = "";
    
    /// <summary>
    /// Gets or sets the Azure Key Vault secret name
    /// </summary>
    public string AzureKeyVaultSecretName { get; set; } = "gateway-config";
    
    /// <summary>
    /// Gets or sets the environment variable name
    /// </summary>
    public string EnvironmentVariableName { get; set; } = "GATEWAY_CONFIG";
    
    /// <summary>
    /// Gets or sets the reload interval in seconds
    /// </summary>
    public int ReloadIntervalSeconds { get; set; } = 60;
}
