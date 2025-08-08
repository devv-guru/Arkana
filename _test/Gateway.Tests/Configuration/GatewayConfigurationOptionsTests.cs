using System.Text.Json;
using Shared.Models;
using Xunit;

namespace Gateway.Tests.Configuration;

public class GatewayConfigurationOptionsTests
{
    [Fact]
    public void CanDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""ConfigurationStoreType"": ""File"",
            ""ConfigurationFilePath"": ""_config/test-config.json"",
            ""AwsRegion"": ""us-west-2"",
            ""AwsSecretName"": ""test-secret"",
            ""AzureKeyVaultUri"": ""https://test.vault.azure.net/"",
            ""AzureKeyVaultSecretName"": ""test-config"",
            ""EnvironmentVariableName"": ""TEST_CONFIG"",
            ""ReloadIntervalSeconds"": 120
        }";

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var config = JsonSerializer.Deserialize<GatewayConfigurationOptions>(json, options);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("File", config.ConfigurationStoreType);
        Assert.Equal("_config/test-config.json", config.ConfigurationFilePath);
        Assert.Equal("us-west-2", config.AwsRegion);
        Assert.Equal("test-secret", config.AwsSecretName);
        Assert.Equal("https://test.vault.azure.net/", config.AzureKeyVaultUri);
        Assert.Equal("test-config", config.AzureKeyVaultSecretName);
        Assert.Equal("TEST_CONFIG", config.EnvironmentVariableName);
        Assert.Equal(120, config.ReloadIntervalSeconds);
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Act
        var config = new GatewayConfigurationOptions();

        // Assert
        Assert.Equal("File", config.ConfigurationStoreType);
        Assert.Equal("_config/gateway-config.json", config.ConfigurationFilePath);
        Assert.Equal("us-east-1", config.AwsRegion);
        Assert.Equal("gateway-config", config.AwsSecretName);
        Assert.Equal("", config.AzureKeyVaultUri);
        Assert.Equal("gateway-config", config.AzureKeyVaultSecretName);
        Assert.Equal("GATEWAY_CONFIG", config.EnvironmentVariableName);
        Assert.Equal(60, config.ReloadIntervalSeconds);
    }

    [Fact]
    public void CanSerializeToJson()
    {
        // Arrange
        var config = new GatewayConfigurationOptions
        {
            ConfigurationStoreType = "AWS",
            AwsRegion = "eu-west-1",
            AwsSecretName = "my-gateway-config",
            ReloadIntervalSeconds = 300
        };

        // Act
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"ConfigurationStoreType\": \"AWS\"", json);
        Assert.Contains("\"AwsRegion\": \"eu-west-1\"", json);
        Assert.Contains("\"AwsSecretName\": \"my-gateway-config\"", json);
        Assert.Contains("\"ReloadIntervalSeconds\": 300", json);
    }
}
