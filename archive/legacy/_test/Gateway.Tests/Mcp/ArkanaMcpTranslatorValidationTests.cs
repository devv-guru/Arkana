using Data.Enums;
using Shared.Models.Mcp;
using Xunit;

namespace Gateway.Tests.Mcp;

/// <summary>
/// Validation tests for MCP configuration models
/// </summary>
public class ArkanaMcpValidationTests
{
    [Fact]
    public void ArkanaMcpOAuth2Settings_DefaultValues_AreSetCorrectly()
    {
        // Act
        var settings = new ArkanaMcpOAuth2Settings();

        // Assert
        Assert.Equal(string.Empty, settings.ClientId);
        Assert.Equal(string.Empty, settings.ClientSecret);
        Assert.Equal(string.Empty, settings.TokenEndpoint);
        Assert.Equal(string.Empty, settings.Scopes);
        Assert.True(settings.EnableAutomaticRefresh);
        Assert.Null(settings.TenantId);
        Assert.Null(settings.AuthorityUrl);
        Assert.False(settings.UseAzureKeyVault);
        Assert.Null(settings.KeyVaultSecretName);
    }

    [Fact]
    public void ArkanaMcpApiKeySettings_DefaultValues_AreSetCorrectly()
    {
        // Act
        var settings = new ArkanaMcpApiKeySettings();

        // Assert
        Assert.Equal(string.Empty, settings.GlobalApiKey);
        Assert.Equal("Authorization", settings.HeaderName);
        Assert.Equal("Bearer {key}", settings.HeaderFormat);
        Assert.NotNull(settings.AdditionalHeaders);
        Assert.Empty(settings.AdditionalHeaders);
        Assert.False(settings.UseAzureKeyVault);
        Assert.Null(settings.KeyVaultSecretName);
    }

    [Fact]
    public void ArkanaMcpConfiguration_WithCompleteOAuth2Config_IsValid()
    {
        // Arrange & Act
        var config = new ArkanaMcpConfiguration
        {
            Name = "OAuth2Server",
            Description = "OAuth2 MCP Server",
            Endpoint = "https://api.example.com/mcp",
            Protocol = McpProtocolType.Http,
            IsEnabled = true,
            Security = new ArkanaMcpSecurityConfig
            {
                AuthenticationType = McpAuthType.OAuth2,
                RequireHttps = true,
                OAuth2 = new ArkanaMcpOAuth2Settings
                {
                    ClientId = "test-client-id",
                    ClientSecret = "test-secret",
                    TokenEndpoint = "https://auth.example.com/token",
                    Scopes = "read write",
                    EnableAutomaticRefresh = true,
                    TenantId = "test-tenant",
                    AuthorityUrl = "https://login.microsoftonline.com/test-tenant",
                    UseAzureKeyVault = true,
                    KeyVaultSecretName = "oauth-secret"
                }
            }
        };

        // Assert
        Assert.Equal("OAuth2Server", config.Name);
        Assert.Equal(McpProtocolType.Http, config.Protocol);
        Assert.Equal(McpAuthType.OAuth2, config.Security.AuthenticationType);
        Assert.NotNull(config.Security.OAuth2);
        Assert.Equal("test-client-id", config.Security.OAuth2.ClientId);
        Assert.Equal("https://auth.example.com/token", config.Security.OAuth2.TokenEndpoint);
        Assert.True(config.Security.OAuth2.UseAzureKeyVault);
    }

    [Fact]
    public void ArkanaMcpConfiguration_WithApiKeyAuth_IsValid()
    {
        // Arrange & Act
        var config = new ArkanaMcpConfiguration
        {
            Name = "ApiKeyServer",
            Endpoint = "https://api.example.com/mcp",
            Protocol = McpProtocolType.Http,
            Security = new ArkanaMcpSecurityConfig
            {
                AuthenticationType = McpAuthType.ApiKey,
                ApiKey = new ArkanaMcpApiKeySettings
                {
                    GlobalApiKey = "test-api-key",
                    HeaderName = "X-API-Key",
                    HeaderFormat = "Bearer {key}",
                    AdditionalHeaders = new Dictionary<string, string>
                    {
                        { "X-Client-Version", "1.0" },
                        { "X-Source", "Arkana" }
                    },
                    UseAzureKeyVault = true,
                    KeyVaultSecretName = "api-key-secret"
                }
            }
        };

        // Assert
        Assert.Equal(McpAuthType.ApiKey, config.Security.AuthenticationType);
        Assert.NotNull(config.Security.ApiKey);
        Assert.Equal("X-API-Key", config.Security.ApiKey.HeaderName);
        Assert.Equal("Bearer {key}", config.Security.ApiKey.HeaderFormat);
        Assert.Equal(2, config.Security.ApiKey.AdditionalHeaders.Count);
        Assert.True(config.Security.ApiKey.UseAzureKeyVault);
        Assert.Equal("api-key-secret", config.Security.ApiKey.KeyVaultSecretName);
    }

    [Fact]
    public void ArkanaMcpAccessMode_EnumValues_AreCorrect()
    {
        // Assert
        Assert.Equal(0, (int)ArkanaMcpAccessMode.RoleBased);
        Assert.Equal(1, (int)ArkanaMcpAccessMode.UserBased);
        Assert.Equal(2, (int)ArkanaMcpAccessMode.Mixed);
        Assert.Equal(3, (int)ArkanaMcpAccessMode.Open);
    }

    [Fact]
    public void ArkanaMcpConfiguration_AccessControlConfiguration_IsValid()
    {
        // Arrange & Act
        var config = new ArkanaMcpConfiguration
        {
            Name = "AccessControlledServer",
            Endpoint = "wss://secure.example.com/mcp",
            Access = new ArkanaMcpAccessConfig
            {
                Mode = ArkanaMcpAccessMode.Mixed,
                AllowedUsers = new List<string> { "user1@example.com", "user2@example.com" },
                AllowedRoles = new List<string> { "Administrator", "PowerUser" },
                AccessExpiresAt = DateTime.UtcNow.AddMonths(6),
                RequireActiveDirectory = true
            }
        };

        // Assert
        Assert.Equal(ArkanaMcpAccessMode.Mixed, config.Access.Mode);
        Assert.Equal(2, config.Access.AllowedUsers.Count);
        Assert.Contains("user1@example.com", config.Access.AllowedUsers);
        Assert.Equal(2, config.Access.AllowedRoles.Count);
        Assert.Contains("Administrator", config.Access.AllowedRoles);
        Assert.NotNull(config.Access.AccessExpiresAt);
        Assert.True(config.Access.RequireActiveDirectory);
    }
}