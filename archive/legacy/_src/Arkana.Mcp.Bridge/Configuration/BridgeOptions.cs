using System.ComponentModel.DataAnnotations;

namespace Arkana.Mcp.Bridge.Configuration;

public class BridgeOptions
{
    public const string SectionName = "Bridge";
    
    [Required]
    public string GatewayUrl { get; set; } = string.Empty;
    
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    
    public bool EnableDetailedLogging { get; set; } = false;
}

public class AuthenticationOptions
{
    public const string SectionName = "Authentication";
    
    [Required]
    public string TenantId { get; set; } = string.Empty;
    
    [Required]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    public string[] Scopes { get; set; } = ["api://arkana-gateway/.default"];
}

public class McpOptions
{
    public const string SectionName = "MCP";
    
    public string Name { get; set; } = "arkana-bridge";
    
    public string Version { get; set; } = "1.0.0";
    
    public string Description { get; set; } = "Arkana MCP Bridge - Connect MCP clients to enterprise tools via Arkana Gateway";
}