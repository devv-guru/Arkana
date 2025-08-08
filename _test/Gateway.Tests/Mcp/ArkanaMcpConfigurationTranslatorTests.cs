using Data.Enums;
using Microsoft.Extensions.Logging;
using Shared.Models.Mcp;
using System.Text.Json;
using Xunit;

namespace Gateway.Tests.Mcp;

public class ArkanaMcpConfigurationTests
{
    [Fact]
    public void ArkanaMcpConfiguration_DefaultValues_AreSetCorrectly()
    {
        // Act
        var config = new ArkanaMcpConfiguration();

        // Assert
        Assert.Equal(string.Empty, config.Name);
        Assert.Equal(string.Empty, config.Description);
        Assert.Equal(string.Empty, config.Endpoint);
        Assert.Equal(McpProtocolType.WebSocket, config.Protocol);
        Assert.True(config.IsEnabled);
        Assert.NotNull(config.Security);
        Assert.NotNull(config.Access);
        Assert.NotNull(config.Performance);
    }

    [Fact]
    public void ArkanaMcpSecurityConfig_DefaultValues_AreSetCorrectly()
    {
        // Act
        var config = new ArkanaMcpSecurityConfig();

        // Assert
        Assert.Equal(McpAuthType.None, config.AuthenticationType);
        Assert.True(config.RequireHttps);
        Assert.False(config.AllowPerUserCredentials);
        Assert.Equal(60, config.TokenCacheMinutes);
        Assert.Null(config.OAuth2);
        Assert.Null(config.ApiKey);
    }

    [Fact]
    public void ArkanaMcpAccessConfig_DefaultValues_AreSetCorrectly()
    {
        // Act
        var config = new ArkanaMcpAccessConfig();

        // Assert
        Assert.Equal(ArkanaMcpAccessMode.RoleBased, config.Mode);
        Assert.Empty(config.AllowedUsers);
        Assert.Empty(config.AllowedRoles);
        Assert.Null(config.AccessExpiresAt);
        Assert.True(config.RequireActiveDirectory);
    }

    [Fact]
    public void ArkanaMcpPerformanceConfig_DefaultValues_AreSetCorrectly()
    {
        // Act
        var config = new ArkanaMcpPerformanceConfig();

        // Assert
        Assert.Equal(30, config.ConnectionTimeoutSeconds);
        Assert.Equal(100, config.MaxConcurrentConnections);
        Assert.True(config.EnableConnectionPooling);
        Assert.True(config.EnableHealthChecks);
        Assert.Equal(30, config.HealthCheckIntervalSeconds);
        Assert.Equal("/health", config.HealthCheckPath);
        Assert.True(config.EnableRateLimiting);
        Assert.Equal(1000, config.RequestsPerMinute);
        Assert.Equal(10000, config.RequestsPerHour);
    }

    [Fact]
    public void ArkanaMcpConfiguration_CanSerializeToJson()
    {
        // Arrange
        var config = new ArkanaMcpConfiguration
        {
            Name = "TestServer",
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
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"Name\": \"TestServer\"", json);
        Assert.Contains("\"Protocol\": 0", json); // WebSocket = 0
        // Note: JsonSerializer uses enum integer values by default
        Assert.Contains("\"RequireHttps\": true", json);
        Assert.Contains("\"ClientId\": \"test-client-id\"", json);
    }

    [Fact]
    public void ArkanaMcpConfiguration_CanDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""Name"": ""TestServer"",
            ""Description"": ""Test MCP Server"",
            ""Endpoint"": ""wss://example.com/mcp"",
            ""Protocol"": 0,
            ""IsEnabled"": true,
            ""Security"": {
                ""AuthenticationType"": 2,
                ""RequireHttps"": true,
                ""TokenCacheMinutes"": 60,
                ""ApiKey"": {
                    ""HeaderName"": ""X-API-Key"",
                    ""HeaderFormat"": ""Bearer {key}""
                }
            },
            ""Access"": {
                ""Mode"": 0,
                ""AllowedRoles"": [""admin"", ""user""]
            },
            ""Performance"": {
                ""ConnectionTimeoutSeconds"": 45,
                ""MaxConcurrentConnections"": 200
            }
        }";

        // Act
        var config = JsonSerializer.Deserialize<ArkanaMcpConfiguration>(json);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("TestServer", config.Name);
        Assert.Equal("Test MCP Server", config.Description);
        Assert.Equal("wss://example.com/mcp", config.Endpoint);
        Assert.Equal(McpProtocolType.WebSocket, config.Protocol);
        Assert.True(config.IsEnabled);
        
        Assert.Equal(McpAuthType.ApiKey, config.Security.AuthenticationType);
        Assert.True(config.Security.RequireHttps);
        Assert.Equal(60, config.Security.TokenCacheMinutes);
        Assert.NotNull(config.Security.ApiKey);
        Assert.Equal("X-API-Key", config.Security.ApiKey.HeaderName);
        
        Assert.Equal(ArkanaMcpAccessMode.RoleBased, config.Access.Mode);
        Assert.Equal(2, config.Access.AllowedRoles.Count);
        Assert.Contains("admin", config.Access.AllowedRoles);
        Assert.Contains("user", config.Access.AllowedRoles);
        
        Assert.Equal(45, config.Performance.ConnectionTimeoutSeconds);
        Assert.Equal(200, config.Performance.MaxConcurrentConnections);
    }

    [Fact]
    public void BackwardCompatibility_OldClassNames_StillWork()
    {
        // Act & Assert - using old class names should not throw exceptions
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
                Mode = (ArkanaMcpAccessMode)AccessMode.Open
            }
        };
        #pragma warning restore CS0612

        Assert.NotNull(oldConfig);
        Assert.Equal("BackwardCompatTest", oldConfig.Name);
        Assert.Equal("wss://test.com", oldConfig.Endpoint);
        Assert.Equal(McpAuthType.None, oldConfig.Security.AuthenticationType);
    }
}