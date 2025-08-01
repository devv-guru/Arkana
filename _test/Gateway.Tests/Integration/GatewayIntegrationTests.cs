using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Gateway.Configuration;
using Gateway.Tests.Helpers;
using Gateway.WebServer;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Gateway.Tests.Integration;

// Custom implementation of IHostEnvironment for testing
public class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = default!;
    public string ApplicationName { get; set; } = default!;
    public string ContentRootPath { get; set; } = default!;
    public IFileProvider ContentRootFileProvider { get; set; } = default!;
}

public class GatewayIntegrationTests
{
    [Fact]
    public void YarpConfigurationService_BuildsCorrectConfig()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Gateway.Proxy.YarpConfigurationService>>();
        
        // Create a test configuration
        var config = new GatewayConfigurationOptions
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
            UI = new Shared.Models.UIOptions
            {
                Enabled = true,
                Path = "ui",
                RequireAuthentication = false
            },
            ProxyRules = new List<ProxyRuleConfig>
            {
                new ProxyRuleConfig
                {
                    Name = "Test Rule",
                    Hosts = new List<string> { "localhost" },
                    PathPrefix = "/api",
                    StripPrefix = false,
                    Methods = new List<string> { "GET" },
                    Cluster = new ClusterConfig
                    {
                        Name = "Test Cluster",
                        LoadBalancingPolicy = "RoundRobin",
                        Destinations = new List<DestinationConfig>
                        {
                            new DestinationConfig
                            {
                                Name = "test",
                                Address = "http://localhost:5000"
                            }
                        }
                    }
                }
            }
        };
        
        // Create a test gateway configuration service
        var gatewayConfigService = new TestGatewayConfigurationService(
            Mock.Of<ILogger<GatewayConfigurationService>>(),
            Mock.Of<IHostEnvironment>(),
            config);
        
        // Create the YARP configuration service
        var yarpConfigService = new Gateway.Proxy.YarpConfigurationService(
            mockLogger.Object,
            gatewayConfigService);
        
        // Act
        yarpConfigService.UpdateConfig();
        var yarpConfig = yarpConfigService.GetConfig();
        
        // Assert
        Assert.NotNull(yarpConfig);
        Assert.Single(yarpConfig.Routes);
        Assert.Single(yarpConfig.Clusters);
        
        // Verify route
        var route = yarpConfig.Routes[0];
        Assert.Equal("Test Rule", route.RouteId);
        Assert.Equal("Test Cluster", route.ClusterId);
        Assert.Contains("localhost", route.Match.Hosts);
        Assert.Equal("/api/{**catchAll}", route.Match.Path);
        Assert.Contains("GET", route.Match.Methods);
        
        // Verify cluster
        var cluster = yarpConfig.Clusters[0];
        Assert.Equal("Test Cluster", cluster.ClusterId);
        Assert.Equal("RoundRobin", cluster.LoadBalancingPolicy);
        
        // Verify destinations
        Assert.Single(cluster.Destinations);
        Assert.Contains("test", cluster.Destinations.Keys);
        Assert.Equal("http://localhost:5000", cluster.Destinations["test"].Address);
    }
}
