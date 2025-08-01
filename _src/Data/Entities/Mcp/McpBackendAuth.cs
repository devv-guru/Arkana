using Data.Enums;

namespace Data.Entities.Mcp;

public class McpBackendAuth : EntityBase
{
    public Guid McpServerId { get; set; }
    public McpAuthType AuthType { get; set; } = McpAuthType.None;
    
    // OAuth2 Configuration
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; } // Encrypted
    public string? TokenEndpoint { get; set; }
    public string? AuthorizationEndpoint { get; set; }
    public string? Scope { get; set; }
    public string? RedirectUri { get; set; }
    
    // API Key Configuration
    public string? ApiKey { get; set; } // Encrypted - global API key
    public string? ApiKeyHeader { get; set; } = "Authorization";
    public string? ApiKeyPrefix { get; set; } = "Bearer";
    
    // Per-user API key support
    public bool AllowPerUserApiKeys { get; set; } = false;
    
    // Additional headers to inject
    public string? CustomHeaders { get; set; } // JSON serialized dictionary
    
    // Token caching settings
    public int TokenCacheTtlSeconds { get; set; } = 3600;
    public bool EnableTokenRefresh { get; set; } = true;

    // Navigation properties
    public virtual McpServer McpServer { get; set; } = null!;
    public virtual ICollection<McpUserApiKey> UserApiKeys { get; set; } = new List<McpUserApiKey>();
}