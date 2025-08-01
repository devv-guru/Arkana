using Data.Enums;
using Gateway.Mcp;
using Microsoft.Extensions.Logging;
using Shared.Models.Mcp;
using Xunit;

namespace Gateway.Tests.Mcp;

public class ArkanaMcpConfigurationTranslatorTests
{
    private readonly ArkanaMcpConfigurationTranslator _translator;

    public ArkanaMcpConfigurationTranslatorTests()
    {
        var logger = new LoggerFactory().CreateLogger<ArkanaMcpConfigurationTranslator>();
        _translator = new ArkanaMcpConfigurationTranslator(logger);
    }

    [Fact]
    public void TranslateToYarp_WithBasicConfiguration_CreatesValidYarpConfig()
    {
        // Arrange
        var arkanaConfig = new ArkanaMcpConfiguration
        {
            Name = "TestMcpServer",
            Description = "Test MCP Server",
            Endpoint = "wss://example.com/mcp",
            Protocol = McpProtocolType.WebSocket,
            IsEnabled = true,
            Security = new ArkanaMcpSecurityConfig
            {
                AuthenticationType = McpAuthType.OAuth2,
                RequireHttps = true,
                TokenCacheMinutes = 30,
                OAuth2 = new ArkanaMcpOAuth2Settings
                {
                    ClientId = "test-client-id",
                    TokenEndpoint = "https://auth.example.com/token",
                    Scopes = "read write"
                }
            },
            Access = new ArkanaMcpAccessConfig
            {
                Mode = ArkanaMcpAccessMode.RoleBased,
                AllowedRoles = new List<string> { "admin", "user" }
            },
            Performance = new ArkanaMcpPerformanceConfig
            {
                EnableHealthChecks = true,
                ConnectionTimeoutSeconds = 30,
                MaxConcurrentConnections = 100
            }
        };

        // Act
        var (route, cluster) = _translator.TranslateToYarp(arkanaConfig);

        // Assert
        Assert.NotNull(route);
        Assert.NotNull(cluster);
        
        // Verify route configuration
        Assert.Equal("mcp-testmcpserver", route.RouteId);
        Assert.Equal("mcp-cluster-testmcpserver", route.ClusterId);
        Assert.Equal("/mcp/testmcpserver/{**path}", route.Match.Path);
        Assert.Contains("GET", route.Match.Methods);
        
        // Verify cluster configuration
        Assert.Equal("mcp-cluster-testmcpserver", cluster.ClusterId);
        Assert.Single(cluster.Destinations);
        Assert.Equal("wss://example.com/mcp", cluster.Destinations["primary"].Address);
        
        // Verify security metadata
        Assert.True(route.Metadata.ContainsKey("mcp.security.requireHttps"));
        Assert.Equal("True", route.Metadata["mcp.security.requireHttps"]);
        Assert.Equal("OAuth2", route.Metadata["mcp.security.authType"]);
        
        // Verify performance metadata
        Assert.True(cluster.Metadata.ContainsKey("Yarp.MaxConcurrentRequests"));
        Assert.Equal("100", cluster.Metadata["Yarp.MaxConcurrentRequests"]);
    }

    [Fact]
    public void TranslateToYarp_WithApiKeyAuth_CreatesCorrectTransforms()
    {
        // Arrange
        var arkanaConfig = new ArkanaMcpConfiguration
        {
            Name = "ApiKeyMcpServer",
            Endpoint = "https://api.example.com/mcp",
            Protocol = McpProtocolType.Http,
            Security = new ArkanaMcpSecurityConfig
            {
                AuthenticationType = McpAuthType.ApiKey,
                ApiKey = new ArkanaMcpApiKeySettings
                {
                    HeaderName = "X-API-Key",
                    HeaderFormat = "Bearer {key}"
                }
            }
        };

        // Act
        var (route, cluster) = _translator.TranslateToYarp(arkanaConfig);

        // Assert
        Assert.NotNull(route);
        var authTransform = route.Transforms.FirstOrDefault(t => 
            t.ContainsKey("RequestHeader") && t["RequestHeader"] == "X-MCP-Auth-Type");
        Assert.NotNull(authTransform);
        Assert.Equal("ApiKey", authTransform["Set"]);
    }

    [Fact]
    public void BackwardCompatibility_OldClassNames_StillWork()
    {
        // Arrange - using old class names to verify backward compatibility
        #pragma warning disable CS0612 // Type or member is obsolete
        var oldConfig = new McpSimpleConfiguration
        {
            Name = "BackwardCompatTest",
            Endpoint = "wss://test.com",
            Security = new SecurityConfig
            {
                AuthenticationType = McpAuthType.None
            },
            Access = new AccessConfig
            {
                Mode = AccessMode.Open
            }
        };
        #pragma warning restore CS0612

        // Act & Assert - should not throw any exceptions
        var (route, cluster) = _translator.TranslateToYarp(oldConfig);
        Assert.NotNull(route);
        Assert.NotNull(cluster);
    }
}