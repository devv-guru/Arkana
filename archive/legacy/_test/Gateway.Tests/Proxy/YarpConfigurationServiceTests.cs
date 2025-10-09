using Shared.ViewModels;
using System.Text.Json;
using Xunit;

namespace Gateway.Tests.Proxy;

public class GatewayViewModelTests
{
    [Fact]
    public void GatewayConfigurationViewModel_DefaultValues_AreSetCorrectly()
    {
        // Act
        var viewModel = new GatewayConfigurationViewModel();

        // Assert
        Assert.Equal("File", viewModel.ConfigurationStoreType);
        Assert.Equal("_config/gateway-config.json", viewModel.ConfigurationFilePath);
        Assert.Equal("us-east-1", viewModel.AwsRegion);
        Assert.Equal("gateway-config", viewModel.AwsSecretName);
        Assert.Equal("", viewModel.AzureKeyVaultUri);
        Assert.Equal("gateway-config", viewModel.AzureKeyVaultSecretName);
        Assert.Equal("GATEWAY_CONFIG", viewModel.EnvironmentVariableName);
        Assert.Equal(60, viewModel.ReloadIntervalSeconds);
        Assert.NotNull(viewModel.Hosts);
        Assert.Empty(viewModel.Hosts);
        Assert.NotNull(viewModel.ProxyRules);
        Assert.Empty(viewModel.ProxyRules);
        Assert.NotNull(viewModel.UI);
    }

    [Fact]
    public void HostConfig_CanBeCreatedWithValidData()
    {
        // Arrange & Act
        var hostConfig = new HostConfig
        {
            Name = "Test Host",
            HostNames = new List<string> { "localhost", "example.com" },
            Certificate = new CertificateConfig
            {
                Name = "Test Certificate"
            }
        };

        // Assert
        Assert.Equal("Test Host", hostConfig.Name);
        Assert.Equal(2, hostConfig.HostNames.Count);
        Assert.Contains("localhost", hostConfig.HostNames);
        Assert.Contains("example.com", hostConfig.HostNames);
        Assert.NotNull(hostConfig.Certificate);
        Assert.Equal("Test Certificate", hostConfig.Certificate.Name);
    }

    [Fact]
    public void ProxyRuleConfig_CanBeCreatedWithValidData()
    {
        // Arrange & Act
        var proxyRule = new ProxyRuleConfig
        {
            Name = "Test Rule",
            Hosts = new List<string> { "localhost" },
            PathPrefix = "/api",
            StripPrefix = true,
            Cluster = new ClusterConfig
            {
                Name = "Test Cluster"
            }
        };

        // Assert
        Assert.Equal("Test Rule", proxyRule.Name);
        Assert.Single(proxyRule.Hosts);
        Assert.Equal("localhost", proxyRule.Hosts[0]);
        Assert.Equal("/api", proxyRule.PathPrefix);
        Assert.True(proxyRule.StripPrefix);
        Assert.NotNull(proxyRule.Cluster);
        Assert.Equal("Test Cluster", proxyRule.Cluster.Name);
    }

    [Fact]
    public void GatewayConfigurationViewModel_CanSerializeToJson()
    {
        // Arrange
        var viewModel = new GatewayConfigurationViewModel
        {
            ConfigurationStoreType = "AWS",
            AwsRegion = "eu-west-1",
            AwsSecretName = "my-gateway-config",
            ReloadIntervalSeconds = 120,
            Hosts = new List<HostConfig>
            {
                new HostConfig
                {
                    Name = "Production Host",
                    HostNames = new List<string> { "api.prod.com" },
                    Certificate = new CertificateConfig { Name = "Prod Cert" }
                }
            },
            ProxyRules = new List<ProxyRuleConfig>
            {
                new ProxyRuleConfig
                {
                    Name = "API Rule",
                    Hosts = new List<string> { "api.prod.com" },
                    PathPrefix = "/api/v1",
                    StripPrefix = false,
                    Cluster = new ClusterConfig { Name = "API Cluster" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(viewModel, new JsonSerializerOptions { WriteIndented = true });

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"ConfigurationStoreType\": \"AWS\"", json);
        Assert.Contains("\"AwsRegion\": \"eu-west-1\"", json);
        Assert.Contains("\"ReloadIntervalSeconds\": 120", json);
        Assert.Contains("\"Name\": \"Production Host\"", json);
        Assert.Contains("\"PathPrefix\": \"/api/v1\"", json);
        Assert.Contains("\"StripPrefix\": false", json);
    }

    [Fact]
    public void GatewayConfigurationViewModel_CanDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""ConfigurationStoreType"": ""Azure"",
            ""AzureKeyVaultUri"": ""https://test.vault.azure.net/"",
            ""ReloadIntervalSeconds"": 300,
            ""Hosts"": [
                {
                    ""Name"": ""Test Host"",
                    ""HostNames"": [""test.com"", ""www.test.com""],
                    ""Certificate"": {
                        ""Name"": ""Test SSL Cert""
                    }
                }
            ],
            ""ProxyRules"": [
                {
                    ""Name"": ""Main Rule"",
                    ""Hosts"": [""test.com""],
                    ""PathPrefix"": ""/app"",
                    ""StripPrefix"": true,
                    ""Cluster"": {
                        ""Name"": ""App Cluster""
                    }
                }
            ]
        }";

        // Act
        var viewModel = JsonSerializer.Deserialize<GatewayConfigurationViewModel>(json);

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal("Azure", viewModel.ConfigurationStoreType);
        Assert.Equal("https://test.vault.azure.net/", viewModel.AzureKeyVaultUri);
        Assert.Equal(300, viewModel.ReloadIntervalSeconds);
        
        Assert.Single(viewModel.Hosts);
        var host = viewModel.Hosts[0];
        Assert.Equal("Test Host", host.Name);
        Assert.Equal(2, host.HostNames.Count);
        Assert.Equal("Test SSL Cert", host.Certificate.Name);
        
        Assert.Single(viewModel.ProxyRules);
        var rule = viewModel.ProxyRules[0];
        Assert.Equal("Main Rule", rule.Name);
        Assert.Equal("/app", rule.PathPrefix);
        Assert.True(rule.StripPrefix);
        Assert.Equal("App Cluster", rule.Cluster.Name);
    }

    [Fact]
    public void UIViewModel_DefaultValues_AreSetCorrectly()
    {
        // Act
        var uiViewModel = new UIViewModel();

        // Assert
        Assert.True(uiViewModel.Enabled);
        Assert.Equal("/ui", uiViewModel.Path);
        Assert.Equal("UI/BlazorWasm/wwwroot", uiViewModel.PhysicalPath);
        Assert.False(uiViewModel.RequireAuthentication);
    }
}
