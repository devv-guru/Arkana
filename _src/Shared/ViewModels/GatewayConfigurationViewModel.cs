using System;
using System.Collections.Generic;

namespace Shared.ViewModels;

/// <summary>
/// View model for gateway configuration
/// </summary>
public class GatewayConfigurationViewModel
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

    /// <summary>
    /// Gets or sets the hosts configuration
    /// </summary>
    public List<HostConfig> Hosts { get; set; } = new List<HostConfig>();

    /// <summary>
    /// Gets or sets the proxy rules configuration
    /// </summary>
    public List<ProxyRuleConfig> ProxyRules { get; set; } = new List<ProxyRuleConfig>();

    /// <summary>
    /// Gets or sets the UI configuration
    /// </summary>
    public UIViewModel UI { get; set; } = new UIViewModel();
}

/// <summary>
/// Host configuration
/// </summary>
public class HostConfig
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the host names
    /// </summary>
    public List<string> HostNames { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the certificate
    /// </summary>
    public CertificateConfig Certificate { get; set; }
}

/// <summary>
/// Certificate configuration
/// </summary>
public class CertificateConfig
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Proxy rule configuration
/// </summary>
public class ProxyRuleConfig
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the hosts
    /// </summary>
    public List<string> Hosts { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the path prefix
    /// </summary>
    public string PathPrefix { get; set; }

    /// <summary>
    /// Gets or sets whether to strip the prefix
    /// </summary>
    public bool StripPrefix { get; set; }

    /// <summary>
    /// Gets or sets the cluster
    /// </summary>
    public ClusterConfig Cluster { get; set; }
}

/// <summary>
/// Cluster configuration
/// </summary>
public class ClusterConfig
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }
}
