using Endpoints.Configuration.Services;
using Gateway.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Shared.Models;
using Shared.ViewModels;
using System.Text.Json;
using Xunit;

namespace Gateway.Tests.Integration;

// Custom implementation of IHostEnvironment for testing
public class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Test";
    public string ApplicationName { get; set; } = "Gateway.Tests";
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

public class GatewayIntegrationTests
{
    [Fact]
    public void ConfigurationService_CanLoadAndSaveConfiguration()
    {
        // Arrange
        var initialConfig = new GatewayConfigurationOptions
        {
            ConfigurationStoreType = "File",
            ConfigurationFilePath = "test-config.json",
            AwsRegion = "us-west-2",
            ReloadIntervalSeconds = 120
        };
        
        var configService = new TestConfigurationService(initialConfig);
        
        // Act
        var loadedConfig = configService.GetConfiguration() as GatewayConfigurationOptions;
        
        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Equal("File", loadedConfig.ConfigurationStoreType);
        Assert.Equal("test-config.json", loadedConfig.ConfigurationFilePath);
        Assert.Equal("us-west-2", loadedConfig.AwsRegion);
        Assert.Equal(120, loadedConfig.ReloadIntervalSeconds);
    }

    [Fact]
    public async Task ConfigurationService_CanUpdateConfiguration()
    {
        // Arrange
        var initialConfig = new GatewayConfigurationOptions
        {
            ConfigurationStoreType = "File",
            ReloadIntervalSeconds = 60
        };
        
        var configService = new TestConfigurationService(initialConfig);
        
        // Act
        var updateAction = new Action<object>(config => 
        {
            if (config is GatewayConfigurationOptions options)
            {
                options.ReloadIntervalSeconds = 300;
                options.ConfigurationStoreType = "AWS";
            }
        });
        
        var success = await configService.UpdateConfigurationAsync(updateAction);
        var updatedConfig = configService.GetConfiguration() as GatewayConfigurationOptions;
        
        // Assert
        Assert.True(success);
        Assert.NotNull(updatedConfig);
        Assert.Equal("AWS", updatedConfig.ConfigurationStoreType);
        Assert.Equal(300, updatedConfig.ReloadIntervalSeconds);
    }

    [Fact]
    public void GatewayConfigurationViewModel_IntegrationWithJsonSerialization()
    {
        // Arrange
        var viewModel = new GatewayConfigurationViewModel
        {
            ConfigurationStoreType = "Azure",
            AzureKeyVaultUri = "https://test.vault.azure.net/",
            ReloadIntervalSeconds = 180,
            Hosts = new List<HostConfig>
            {
                new HostConfig
                {
                    Name = "Production Host",
                    HostNames = new List<string> { "api.prod.com", "www.prod.com" },
                    Certificate = new CertificateConfig { Name = "Prod SSL" }
                }
            },
            ProxyRules = new List<ProxyRuleConfig>
            {
                new ProxyRuleConfig
                {
                    Name = "API Proxy",
                    Hosts = new List<string> { "api.prod.com" },
                    PathPrefix = "/api/v2",
                    StripPrefix = true,
                    Cluster = new ClusterConfig { Name = "Backend Cluster" }
                }
            }
        };
        
        // Act - Serialize and then deserialize
        var json = JsonSerializer.Serialize(viewModel, new JsonSerializerOptions { WriteIndented = true });
        var deserializedViewModel = JsonSerializer.Deserialize<GatewayConfigurationViewModel>(json);
        
        // Assert
        Assert.NotNull(deserializedViewModel);
        Assert.Equal("Azure", deserializedViewModel.ConfigurationStoreType);
        Assert.Equal("https://test.vault.azure.net/", deserializedViewModel.AzureKeyVaultUri);
        Assert.Equal(180, deserializedViewModel.ReloadIntervalSeconds);
        
        Assert.Single(deserializedViewModel.Hosts);
        var host = deserializedViewModel.Hosts[0];
        Assert.Equal("Production Host", host.Name);
        Assert.Equal(2, host.HostNames.Count);
        
        Assert.Single(deserializedViewModel.ProxyRules);
        var rule = deserializedViewModel.ProxyRules[0];
        Assert.Equal("API Proxy", rule.Name);
        Assert.Equal("/api/v2", rule.PathPrefix);
        Assert.True(rule.StripPrefix);
        Assert.Equal("Backend Cluster", rule.Cluster.Name);
    }
    
    [Fact]
    public void ConfigurationService_FileNotFoundScenario_ReturnsNull()
    {
        // Arrange
        var configService = new TestConfigurationService();
        configService.SimulateFileNotFound();
        
        // Act
        var config = configService.GetConfiguration();
        
        // Assert
        Assert.Null(config);
        Assert.False(configService.HasConfiguration);
    }
}
