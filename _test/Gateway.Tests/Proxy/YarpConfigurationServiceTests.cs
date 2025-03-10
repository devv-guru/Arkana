using System.Collections.Generic;
using Gateway.Configuration;
using Gateway.Proxy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Yarp.ReverseProxy.Configuration;

namespace Gateway.Tests.Proxy;

public class YarpConfigurationServiceTests
{
    // Test implementation of GatewayConfigurationService for testing
    private class TestGatewayConfigurationService : GatewayConfigurationService
    {
        private readonly GatewayConfigurationOptions? _config;

        public TestGatewayConfigurationService(GatewayConfigurationOptions? config)
            : base(Mock.Of<ILogger<GatewayConfigurationService>>(), Mock.Of<IHostEnvironment>())
        {
            _config = config;
        }

        public override GatewayConfigurationOptions? GetConfiguration() => _config;
    }

    [Fact]
    public void GetConfig_NoConfiguration_ReturnsEmptyConfig()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<YarpConfigurationService>>();
        var gatewayConfigService = new TestGatewayConfigurationService(null);
        
        var service = new YarpConfigurationService(
            mockLogger.Object,
            gatewayConfigService);
        
        // Act
        var config = service.GetConfig();
        
        // Assert
        Assert.NotNull(config);
        Assert.Empty(config.Routes);
        Assert.Empty(config.Clusters);
        Assert.NotNull(config.ChangeToken);
    }
    
    [Fact]
    public void UpdateConfig_ValidConfiguration_BuildsRoutesAndClusters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<YarpConfigurationService>>();
        var gatewayConfig = new GatewayConfigurationOptions
        {
            Hosts = new List<HostConfig>
            {
                new HostConfig
                {
                    Name = "Test Host",
                    HostNames = new List<string> { "localhost" },
                    Certificate = new CertificateConfig
                    {
                        Name = "Test Certificate",
                        Source = "InMemory"
                    }
                }
            },
            ProxyRules = new List<ProxyRuleConfig>
            {
                new ProxyRuleConfig
                {
                    Name = "Test Rule",
                    Hosts = new List<string> { "localhost" },
                    PathPrefix = "/api",
                    StripPrefix = true,
                    Methods = new List<string> { "GET" },
                    Cluster = new Gateway.Configuration.ClusterConfig
                    {
                        Name = "Test Cluster",
                        LoadBalancingPolicy = "RoundRobin",
                        Destinations = new List<Gateway.Configuration.DestinationConfig>
                        {
                            new Gateway.Configuration.DestinationConfig
                            {
                                Name = "test",
                                Address = "http://localhost:5000"
                            }
                        }
                    }
                }
            }
        };
        
        var gatewayConfigService = new TestGatewayConfigurationService(gatewayConfig);
        
        var service = new YarpConfigurationService(
            mockLogger.Object,
            gatewayConfigService);
        
        // Act
        service.UpdateConfig();
        var config = service.GetConfig();
        
        // Assert
        Assert.NotNull(config);
        Assert.Single(config.Routes);
        Assert.Single(config.Clusters);
        
        // Verify route
        var route = config.Routes[0];
        Assert.Equal("Test Rule", route.RouteId);
        Assert.Equal("Test Cluster", route.ClusterId);
        Assert.Contains("localhost", route.Match.Hosts);
        Assert.Equal("/api/{**catchAll}", route.Match.Path);
        Assert.Contains("GET", route.Match.Methods);
        
        // Verify transforms
        Assert.NotNull(route.Transforms);
        Assert.Single(route.Transforms);
        Assert.Equal("PathRemovePrefix", route.Transforms[0].Keys.First());
        Assert.Equal("/api", route.Transforms[0]["PathRemovePrefix"]);
        
        // Verify cluster
        var cluster = config.Clusters[0];
        Assert.Equal("Test Cluster", cluster.ClusterId);
        Assert.Equal("RoundRobin", cluster.LoadBalancingPolicy);
        
        // Verify destinations
        Assert.Single(cluster.Destinations);
        Assert.Contains("test", cluster.Destinations.Keys);
        Assert.Equal("http://localhost:5000", cluster.Destinations["test"].Address);
    }
    
    [Fact]
    public void UpdateConfig_NoConfiguration_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<YarpConfigurationService>>();
        var gatewayConfigService = new TestGatewayConfigurationService(null);
        
        var service = new YarpConfigurationService(
            mockLogger.Object,
            gatewayConfigService);
        
        // Act
        service.UpdateConfig();
        
        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No proxy rules configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
