using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities;
using Gateway.Configuration;
using Gateway.Tests.Helpers;
using Gateway.WebServer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Gateway.Tests.Configuration;

public class GatewayConfigurationServiceTests
{
    [Fact]
    public async Task LoadConfigurationAsync_ValidConfig_LoadsConfiguration()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GatewayConfigurationService>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        var mockCertificateManager = new Mock<CertificateManager>(
            new Mock<HostCertificateCache>(
                new Mock<ILogger<HostCertificateCache>>().Object, 
                null).Object, 
            new Mock<ILogger<CertificateManager>>().Object);

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
                    StripPrefix = true,
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

        // Create a test service with the configuration
        var service = new TestGatewayConfigurationService(
            mockLogger.Object,
            mockEnvironment.Object,
            config);

        // Act
        await service.LoadConfigurationAsync(CancellationToken.None);
        var result = service.GetConfiguration();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Hosts.Count);
        Assert.Equal(1, result.ProxyRules.Count);
        Assert.Equal("Test Host", result.Hosts[0].Name);
        Assert.Equal("Test Rule", result.ProxyRules[0].Name);
    }

    [Fact]
    public async Task LoadConfigurationAsync_FileNotFound_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GatewayConfigurationService>>();
        var mockEnvironment = new Mock<IHostEnvironment>();

        // Create a test service with no configuration and file not found
        var service = new TestGatewayConfigurationService(
            mockLogger.Object,
            mockEnvironment.Object,
            null,
            false);

        // Act
        await service.LoadConfigurationAsync(CancellationToken.None);
        var config = service.GetConfiguration();

        // Assert
        Assert.Null(config);
    }
}
