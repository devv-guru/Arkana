using Data.Enums;
using Gateway.Mcp;
using Microsoft.Extensions.Logging;
using Shared.Models.Mcp;
using Xunit;

namespace Gateway.Tests.Mcp;

/// <summary>
/// Simple validation tests for the refactored ArkanaMcpConfigurationTranslator
/// </summary>
public class ArkanaMcpTranslatorValidationTests
{
    private readonly ArkanaMcpConfigurationTranslator _translator;

    public ArkanaMcpTranslatorValidationTests()
    {
        var logger = new LoggerFactory().CreateLogger<ArkanaMcpConfigurationTranslator>();
        _translator = new ArkanaMcpConfigurationTranslator(logger);
    }

    [Fact]
    public void TranslateToYarp_WithMinimalConfig_DoesNotThrow()
    {
        // Arrange
        var config = new ArkanaMcpConfiguration
        {
            Name = "TestServer",
            Endpoint = "wss://example.com/mcp",
            Protocol = McpProtocolType.WebSocket
        };

        // Act & Assert - should not throw
        var (route, cluster) = _translator.TranslateToYarp(config);
        
        Assert.NotNull(route);
        Assert.NotNull(cluster);
        Assert.Equal("mcp-testserver", route.RouteId);
        Assert.Equal("mcp-cluster-testserver", cluster.ClusterId);
    }

    [Fact] 
    public void TranslateToYarp_WithOAuth2Config_SetsCorrectTransforms()
    {
        // Arrange
        var config = new ArkanaMcpConfiguration
        {
            Name = "OAuth2Server",
            Endpoint = "https://api.example.com/mcp",
            Security = new ArkanaMcpSecurityConfig
            {
                AuthenticationType = McpAuthType.OAuth2,
                OAuth2 = new ArkanaMcpOAuth2Settings
                {
                    ClientId = "test-client",
                    TokenEndpoint = "https://auth.example.com/token"
                }
            }
        };

        // Act
        var (route, cluster) = _translator.TranslateToYarp(config);

        // Assert
        Assert.NotNull(route.Transforms);
        Assert.Contains(route.Transforms, t => 
            t.ContainsKey("RequestHeader") && 
            t["RequestHeader"] == "X-MCP-Auth-Type" && 
            t["Set"] == "OAuth2");
    }

    [Fact]
    public void TranslateToYarp_WithHttpProtocol_SetsCorrectMethods()
    {
        // Arrange
        var config = new ArkanaMcpConfiguration
        {
            Name = "HttpServer",
            Endpoint = "https://api.example.com/mcp",
            Protocol = McpProtocolType.Http
        };

        // Act
        var (route, cluster) = _translator.TranslateToYarp(config);

        // Assert
        Assert.Contains("GET", route.Match.Methods);
        Assert.Contains("POST", route.Match.Methods);
        Assert.Contains("PUT", route.Match.Methods);
        Assert.Contains("DELETE", route.Match.Methods);
    }

    [Fact]
    public void TranslateToYarp_WithWebSocketProtocol_SetsOnlyGetMethod()
    {
        // Arrange
        var config = new ArkanaMcpConfiguration
        {
            Name = "WebSocketServer",
            Endpoint = "wss://example.com/mcp",
            Protocol = McpProtocolType.WebSocket
        };

        // Act
        var (route, cluster) = _translator.TranslateToYarp(config);

        // Assert
        Assert.Single(route.Match.Methods);
        Assert.Contains("GET", route.Match.Methods);
    }
}