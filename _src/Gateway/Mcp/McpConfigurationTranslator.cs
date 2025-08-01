using Data.Enums;
using Shared.Models.Mcp;
using Yarp.ReverseProxy.Forwarder;
using YarpRouteConfig = Yarp.ReverseProxy.Configuration.RouteConfig;
using YarpClusterConfig = Yarp.ReverseProxy.Configuration.ClusterConfig;
using YarpDestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;
using YarpRouteMatch = Yarp.ReverseProxy.Configuration.RouteMatch;
using YarpHealthCheckConfig = Yarp.ReverseProxy.Configuration.HealthCheckConfig;
using YarpActiveHealthCheckConfig = Yarp.ReverseProxy.Configuration.ActiveHealthCheckConfig;

namespace Gateway.Mcp;

/// <summary>
/// Translates simplified Arkana MCP configuration to secure, optimized YARP configuration
/// Focuses on security-first design with performance optimization
/// </summary>
public class ArkanaMcpConfigurationTranslator
{
    private readonly ILogger<ArkanaMcpConfigurationTranslator> _logger;

    public ArkanaMcpConfigurationTranslator(ILogger<ArkanaMcpConfigurationTranslator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts simplified Arkana MCP configuration to YARP route and cluster configurations
    /// with security and performance optimizations
    /// </summary>
    public (YarpRouteConfig Route, YarpClusterConfig Cluster) TranslateToYarp(ArkanaMcpConfiguration mcpConfig)
    {
        var routeId = $"mcp-{mcpConfig.Name.ToLowerInvariant()}";
        var clusterId = $"mcp-cluster-{mcpConfig.Name.ToLowerInvariant()}";

        var route = CreateSecureRoute(routeId, clusterId, mcpConfig);
        var cluster = CreateOptimizedCluster(clusterId, mcpConfig);

        _logger.LogDebug("Translated MCP config '{Name}' to YARP route '{RouteId}' and cluster '{ClusterId}'", 
            mcpConfig.Name, routeId, clusterId);

        return (route, cluster);
    }

    private YarpRouteConfig CreateSecureRoute(string routeId, string clusterId, ArkanaMcpConfiguration config)
    {
        var transforms = new List<Dictionary<string, string>>();

        // Security-first transforms
        AddSecurityTransforms(transforms, config);
        
        // Authentication transforms
        AddAuthenticationTransforms(transforms, config);

        // Performance transforms
        AddPerformanceTransforms(transforms, config);

        return new YarpRouteConfig
        {
            RouteId = routeId,
            ClusterId = clusterId,
            Match = new YarpRouteMatch
            {
                // MCP-specific routing pattern for clear separation
                Path = $"/mcp/{config.Name.ToLowerInvariant()}/{{**path}}",
                
                // Only allow WebSocket upgrades and standard HTTP methods
                Methods = GetAllowedMethods(config.Protocol),
                
                // Note: Authorization will be validated by middleware, not routing
                Headers = null
            },
            Transforms = transforms,
            
            // Security metadata
            Metadata = new Dictionary<string, string>
            {
                ["mcp.security.requireHttps"] = config.Security.RequireHttps.ToString(),
                ["mcp.security.authType"] = config.Security.AuthenticationType.ToString(),
                ["mcp.performance.rateLimit"] = config.Performance.EnableRateLimiting.ToString(),
                ["mcp.access.mode"] = config.Access.Mode.ToString()
            }
        };
    }

    private YarpClusterConfig CreateOptimizedCluster(string clusterId, ArkanaMcpConfiguration config)
    {
        var destinations = new Dictionary<string, YarpDestinationConfig>
        {
            ["primary"] = new YarpDestinationConfig
            {
                Address = config.Endpoint,
                
                // Security metadata
                Metadata = new Dictionary<string, string>
                {
                    ["mcp.protocol"] = config.Protocol.ToString(),
                    ["mcp.requiresAuth"] = (config.Security.AuthenticationType != McpAuthType.None).ToString()
                }
            }
        };

        return new YarpClusterConfig
        {
            ClusterId = clusterId,
            
            // Performance optimization - single destination for MCP servers
            LoadBalancingPolicy = "FirstAlphabetical",
            
            Destinations = destinations,
            
            // Health checks for reliability
            HealthCheck = config.Performance.EnableHealthChecks ? new YarpHealthCheckConfig
            {
                Active = new YarpActiveHealthCheckConfig
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(config.Performance.HealthCheckIntervalSeconds),
                    Timeout = TimeSpan.FromSeconds(config.Performance.ConnectionTimeoutSeconds),
                    Path = config.Performance.HealthCheckPath,
                    Policy = "ConsecutiveFailures"
                }
            } : null,

            // Performance configuration
            HttpRequest = new ForwarderRequestConfig
            {
                // WebSocket/SSE optimizations
                Version = config.Protocol == McpProtocolType.Http ? new Version(2, 0) : new Version(1, 1),
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
                
                // Streaming for real-time protocols
                AllowResponseBuffering = config.Protocol == McpProtocolType.Http,
                
                // Timeout configuration
                ActivityTimeout = TimeSpan.FromSeconds(config.Performance.ConnectionTimeoutSeconds),
            },

            // Security and performance metadata
            Metadata = new Dictionary<string, string>
            {
                // Rate limiting configuration
                ["Microsoft.AspNetCore.RateLimiting.EnableRateLimiting"] = config.Performance.EnableRateLimiting.ToString(),
                ["Microsoft.AspNetCore.RateLimiting.RequestsPerMinute"] = config.Performance.RequestsPerMinute.ToString(),
                
                // Connection limits
                ["Yarp.MaxConcurrentRequests"] = config.Performance.MaxConcurrentConnections.ToString(),
                
                // Security settings
                ["mcp.security.requireHttps"] = config.Security.RequireHttps.ToString(),
                ["mcp.security.tokenCacheMinutes"] = config.Security.TokenCacheMinutes.ToString()
            }
        };
    }

    private void AddSecurityTransforms(List<Dictionary<string, string>> transforms, ArkanaMcpConfiguration config)
    {
        // Always add security headers
        transforms.Add(new Dictionary<string, string>
        {
            ["RequestHeader"] = "X-MCP-Gateway",
            ["Set"] = "Arkana-MCP-Gateway/1.0"
        });

        // Remove potentially dangerous headers
        transforms.Add(new Dictionary<string, string>
        {
            ["RequestHeader"] = "X-Forwarded-Host",
            ["Set"] = "{Host}"
        });

        // HTTPS enforcement
        if (config.Security.RequireHttps)
        {
            transforms.Add(new Dictionary<string, string>
            {
                ["RequestHeader"] = "X-Forwarded-Proto",
                ["Set"] = "https"
            });
        }

        // Path sanitization for MCP routing
        transforms.Add(new Dictionary<string, string>
        {
            ["PathRemovePrefix"] = $"/mcp/{config.Name.ToLowerInvariant()}"
        });
    }

    private void AddAuthenticationTransforms(List<Dictionary<string, string>> transforms, ArkanaMcpConfiguration config)
    {
        switch (config.Security.AuthenticationType)
        {
            case McpAuthType.OAuth2:
                // OAuth2 token will be injected by middleware
                transforms.Add(new Dictionary<string, string>
                {
                    ["RequestHeader"] = "X-MCP-Auth-Type",
                    ["Set"] = "OAuth2"
                });
                break;

            case McpAuthType.ApiKey:
                // API key will be injected by middleware
                transforms.Add(new Dictionary<string, string>
                {
                    ["RequestHeader"] = "X-MCP-Auth-Type",
                    ["Set"] = "ApiKey"
                });
                break;

            case McpAuthType.Bearer:
                // Bearer token passthrough
                transforms.Add(new Dictionary<string, string>
                {
                    ["RequestHeader"] = "X-MCP-Auth-Type",
                    ["Set"] = "Bearer"
                });
                break;
        }
    }

    private void AddPerformanceTransforms(List<Dictionary<string, string>> transforms, ArkanaMcpConfiguration config)
    {
        // Add performance tracking headers
        transforms.Add(new Dictionary<string, string>
        {
            ["RequestHeader"] = "X-MCP-Server",
            ["Set"] = config.Name
        });

        // Connection optimization for WebSocket/SSE
        if (config.Protocol != McpProtocolType.Http)
        {
            transforms.Add(new Dictionary<string, string>
            {
                ["RequestHeader"] = "Connection",
                ["Set"] = "Upgrade"
            });
        }
    }

    private string[] GetAllowedMethods(McpProtocolType protocol)
    {
        return protocol switch
        {
            McpProtocolType.WebSocket => new[] { "GET" }, // WebSocket upgrade
            McpProtocolType.ServerSentEvents => new[] { "GET" }, // SSE connection
            McpProtocolType.Http => new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" }, // Full HTTP
            _ => new[] { "GET" }
        };
    }
}