using Data.Enums;

namespace Shared.Models.Mcp;

/// <summary>
/// Simplified MCP configuration model for easy UI management
/// Abstracts complex YARP configuration into user-friendly settings
/// </summary>
public class ArkanaMcpConfiguration
{
    /// <summary>
    /// Basic server information
    /// </summary>
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public McpProtocolType Protocol { get; set; } = McpProtocolType.WebSocket;
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Security configuration - simplified for UI
    /// </summary>
    public ArkanaMcpSecurityConfig Security { get; set; } = new();

    /// <summary>
    /// Access control - who can use this MCP server
    /// </summary>
    public ArkanaMcpAccessConfig Access { get; set; } = new();

    /// <summary>
    /// Performance and reliability settings
    /// </summary>
    public ArkanaMcpPerformanceConfig Performance { get; set; } = new();
}

public class ArkanaMcpSecurityConfig
{
    /// <summary>
    /// How to authenticate with the backend system
    /// </summary>
    public McpAuthType AuthenticationType { get; set; } = McpAuthType.None;

    /// <summary>
    /// OAuth2 Settings (only shown when AuthenticationType = OAuth2)
    /// </summary>
    public ArkanaMcpOAuth2Settings? OAuth2 { get; set; }

    /// <summary>
    /// API Key Settings (only shown when AuthenticationType = ApiKey)
    /// </summary>
    public ArkanaMcpApiKeySettings? ApiKey { get; set; }

    /// <summary>
    /// Security policies
    /// </summary>
    public bool RequireHttps { get; set; } = true;
    public bool AllowPerUserCredentials { get; set; } = false;
    public int TokenCacheMinutes { get; set; } = 60;
}

public class ArkanaMcpOAuth2Settings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty; // Will be encrypted or retrieved from Azure Key Vault
    public string TokenEndpoint { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public bool EnableAutomaticRefresh { get; set; } = true;
    
    /// <summary>
    /// Azure-specific settings
    /// </summary>
    public string? TenantId { get; set; }
    public string? AuthorityUrl { get; set; } // e.g., https://login.microsoftonline.com/{tenant}
    
    /// <summary>
    /// Whether to use Azure Key Vault for client secret storage
    /// </summary>
    public bool UseAzureKeyVault { get; set; } = false;
    public string? KeyVaultSecretName { get; set; }
}

public class ArkanaMcpApiKeySettings
{
    public string GlobalApiKey { get; set; } = string.Empty; // Will be encrypted or retrieved from Azure Key Vault
    public string HeaderName { get; set; } = "Authorization";
    public string HeaderFormat { get; set; } = "Bearer {key}"; // {key} will be replaced
    public Dictionary<string, string> AdditionalHeaders { get; set; } = new();
    
    /// <summary>
    /// Whether to use Azure Key Vault for API key storage
    /// </summary>
    public bool UseAzureKeyVault { get; set; } = false;
    public string? KeyVaultSecretName { get; set; }
}

public class ArkanaMcpAccessConfig
{
    /// <summary>
    /// Access control mode
    /// </summary>
    public ArkanaMcpAccessMode Mode { get; set; } = ArkanaMcpAccessMode.RoleBased;

    /// <summary>
    /// Specific users who can access (when Mode = UserBased or Mixed)
    /// </summary>
    public List<string> AllowedUsers { get; set; } = new();

    /// <summary>
    /// Roles/groups that can access (when Mode = RoleBased or Mixed)
    /// </summary>
    public List<string> AllowedRoles { get; set; } = new();

    /// <summary>
    /// Access restrictions
    /// </summary>
    public DateTime? AccessExpiresAt { get; set; }
    public bool RequireActiveDirectory { get; set; } = true;
}

public class ArkanaMcpPerformanceConfig
{
    /// <summary>
    /// Connection settings
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public int MaxConcurrentConnections { get; set; } = 100;
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Health monitoring
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;
    public int HealthCheckIntervalSeconds { get; set; } = 30;
    public string HealthCheckPath { get; set; } = "/health";

    /// <summary>
    /// Rate limiting
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;
    public int RequestsPerMinute { get; set; } = 1000;
    public int RequestsPerHour { get; set; } = 10000;
}

public enum ArkanaMcpAccessMode
{
    /// <summary>
    /// Access granted based on OIDC roles/groups
    /// </summary>
    RoleBased = 0,
    
    /// <summary>
    /// Access granted to specific users
    /// </summary>
    UserBased = 1,
    
    /// <summary>
    /// Mixed mode - both users and roles
    /// </summary>
    Mixed = 2,
    
    /// <summary>
    /// Open access (not recommended for production)
    /// </summary>
    Open = 3
}

// Backward compatibility aliases (will be removed in future versions)
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
[Obsolete("Use ArkanaMcpConfiguration instead to avoid naming conflicts", false)]
public class McpSimpleConfiguration : ArkanaMcpConfiguration { }

[Obsolete("Use ArkanaMcpSecurityConfig instead to avoid naming conflicts", false)]
public class SecurityConfig : ArkanaMcpSecurityConfig { }

[Obsolete("Use ArkanaMcpAccessConfig instead to avoid naming conflicts", false)]
public class AccessConfig : ArkanaMcpAccessConfig { }

[Obsolete("Use ArkanaMcpPerformanceConfig instead to avoid naming conflicts", false)]
public class PerformanceConfig : ArkanaMcpPerformanceConfig { }

[Obsolete("Use ArkanaMcpOAuth2Settings instead to avoid naming conflicts", false)]
public class OAuth2Settings : ArkanaMcpOAuth2Settings { }

[Obsolete("Use ArkanaMcpApiKeySettings instead to avoid naming conflicts", false)]
public class ApiKeySettings : ArkanaMcpApiKeySettings { }

[Obsolete("Use ArkanaMcpAccessMode instead to avoid naming conflicts", false)]
public enum AccessMode
{
    RoleBased = 0,
    UserBased = 1,
    Mixed = 2,
    Open = 3
}
#pragma warning restore CS0108