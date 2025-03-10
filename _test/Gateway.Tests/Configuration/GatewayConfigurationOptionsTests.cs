using System.Text.Json;
using Gateway.Configuration;
using Xunit;

namespace Gateway.Tests.Configuration;

public class GatewayConfigurationOptionsTests
{
    [Fact]
    public void CanDeserializeFromJson()
    {
        // Arrange
        var json = @"{
            ""Hosts"": [
                {
                    ""Name"": ""Test Host"",
                    ""HostNames"": [""localhost"", ""example.com""],
                    ""Certificate"": {
                        ""Name"": ""Test Certificate"",
                        ""Source"": ""InMemory"",
                        ""SubjectAlternativeNames"": [""localhost"", ""example.com""]
                    }
                }
            ],
            ""UI"": {
                ""Enabled"": true,
                ""Path"": ""ui"",
                ""RequireAuthentication"": false
            },
            ""ProxyRules"": [
                {
                    ""Name"": ""Test Rule"",
                    ""Hosts"": [""localhost""],
                    ""PathPrefix"": ""/api"",
                    ""StripPrefix"": true,
                    ""Methods"": [""GET"", ""POST""],
                    ""Cluster"": {
                        ""Name"": ""Test Cluster"",
                        ""LoadBalancingPolicy"": ""RoundRobin"",
                        ""HealthCheck"": {
                            ""Enabled"": true,
                            ""Interval"": ""00:00:10"",
                            ""Timeout"": ""00:00:10"",
                            ""Threshold"": 5,
                            ""Path"": ""/health""
                        },
                        ""HttpRequest"": {
                            ""Version"": ""2"",
                            ""VersionPolicy"": ""RequestVersionOrLower"",
                            ""AllowResponseBuffering"": false,
                            ""ActivityTimeout"": ""00:02:00""
                        },
                        ""Transforms"": [
                            {
                                ""RequestHeader"": ""X-Forwarded-Host"",
                                ""Set"": ""{Host}""
                            }
                        ],
                        ""Destinations"": [
                            {
                                ""Name"": ""test"",
                                ""Address"": ""http://localhost:5000""
                            }
                        ]
                    }
                }
            ]
        }";

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var config = JsonSerializer.Deserialize<GatewayConfigurationOptions>(json, options);

        // Assert
        Assert.NotNull(config);
        Assert.Single(config.Hosts);
        Assert.Single(config.ProxyRules);
        
        // Verify host
        var host = config.Hosts[0];
        Assert.Equal("Test Host", host.Name);
        Assert.Equal(2, host.HostNames.Count);
        Assert.Contains("localhost", host.HostNames);
        Assert.Contains("example.com", host.HostNames);
        
        // Verify certificate
        var certificate = host.Certificate;
        Assert.Equal("Test Certificate", certificate.Name);
        Assert.Equal("InMemory", certificate.Source);
        Assert.Equal(2, certificate.SubjectAlternativeNames.Count);
        
        // Verify UI options
        var ui = config.UI;
        Assert.True(ui.Enabled);
        Assert.Equal("ui", ui.Path);
        Assert.False(ui.RequireAuthentication);
        
        // Verify proxy rule
        var rule = config.ProxyRules[0];
        Assert.Equal("Test Rule", rule.Name);
        Assert.Single(rule.Hosts);
        Assert.Equal("/api", rule.PathPrefix);
        Assert.True(rule.StripPrefix);
        Assert.Equal(2, rule.Methods.Count);
        
        // Verify cluster
        var cluster = rule.Cluster;
        Assert.Equal("Test Cluster", cluster.Name);
        Assert.Equal("RoundRobin", cluster.LoadBalancingPolicy);
        
        // Verify health check
        var healthCheck = cluster.HealthCheck;
        Assert.NotNull(healthCheck);
        Assert.True(healthCheck.Enabled);
        Assert.Equal("00:00:10", healthCheck.Interval);
        Assert.Equal("/health", healthCheck.Path);
        
        // Verify HTTP request
        var httpRequest = cluster.HttpRequest;
        Assert.NotNull(httpRequest);
        Assert.Equal("2", httpRequest.Version);
        Assert.Equal("RequestVersionOrLower", httpRequest.VersionPolicy);
        
        // Verify transforms
        Assert.Single(cluster.Transforms);
        var transform = cluster.Transforms[0];
        Assert.Equal("X-Forwarded-Host", transform["RequestHeader"]);
        Assert.Equal("{Host}", transform["Set"]);
        
        // Verify destinations
        Assert.Single(cluster.Destinations);
        var destination = cluster.Destinations[0];
        Assert.Equal("test", destination.Name);
        Assert.Equal("http://localhost:5000", destination.Address);
    }
}
