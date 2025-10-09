using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Gateway.HealthChecks;

/// <summary>
/// Health check that reports MCP server status by making HTTP calls to health endpoints
/// </summary>
public class McpServerHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<McpServerHealthCheck> _logger;
    private readonly IConfiguration _configuration;

    public McpServerHealthCheck(HttpClient httpClient, ILogger<McpServerHealthCheck> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthData = new Dictionary<string, object>();
        var allHealthy = true;
        var issues = new List<string>();

        try
        {
            // Check MCP servers from YARP configuration
            var proxyConfig = _configuration.GetSection("ReverseProxy");
            var clusters = proxyConfig.GetSection("Clusters").GetChildren();
            var totalServers = 0;
            var healthyServers = 0;

            foreach (var cluster in clusters)
            {
                var clusterName = cluster.Key;
                var destinations = cluster.GetSection("Destinations").GetChildren();

                foreach (var destination in destinations)
                {
                    var address = destination.GetValue<string>("Address");
                    if (!string.IsNullOrEmpty(address))
                    {
                        totalServers++;
                        var destinationId = destination.Key;
                        
                        try
                        {
                            var healthUrl = $"{address.TrimEnd('/')}/health";
                            _logger.LogDebug("Checking MCP server health: {Url}", healthUrl);

                            var response = await _httpClient.GetAsync(healthUrl, cancellationToken);
                            
                            if (response.IsSuccessStatusCode)
                            {
                                healthyServers++;
                                healthData[$"{clusterName}_{destinationId}_status"] = "Healthy";
                                healthData[$"{clusterName}_{destinationId}_address"] = address;
                                healthData[$"{clusterName}_{destinationId}_health_url"] = healthUrl;

                                _logger.LogDebug("MCP server {Cluster}/{Destination} is healthy", clusterName, destinationId);
                            }
                            else
                            {
                                allHealthy = false;
                                var issue = $"{clusterName}/{destinationId}: HTTP {response.StatusCode}";
                                issues.Add(issue);
                                healthData[$"{clusterName}_{destinationId}_status"] = $"Unhealthy - HTTP {response.StatusCode}";
                                healthData[$"{clusterName}_{destinationId}_address"] = address;
                                healthData[$"{clusterName}_{destinationId}_health_url"] = healthUrl;
                                
                                _logger.LogWarning("MCP server {Cluster}/{Destination} returned {StatusCode}", 
                                    clusterName, destinationId, response.StatusCode);
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            allHealthy = false;
                            var issue = $"{clusterName}/{destinationId}: Connection failed";
                            issues.Add(issue);
                            healthData[$"{clusterName}_{destinationId}_status"] = $"Unhealthy - Connection failed";
                            healthData[$"{clusterName}_{destinationId}_address"] = address;
                            healthData[$"{clusterName}_{destinationId}_error"] = ex.Message;
                            
                            _logger.LogWarning(ex, "Failed to connect to MCP server {Cluster}/{Destination} at {Address}", 
                                clusterName, destinationId, address);
                        }
                        catch (TaskCanceledException)
                        {
                            allHealthy = false;
                            var issue = $"{clusterName}/{destinationId}: Timeout";
                            issues.Add(issue);
                            healthData[$"{clusterName}_{destinationId}_status"] = "Unhealthy - Timeout";
                            healthData[$"{clusterName}_{destinationId}_address"] = address;
                            
                            _logger.LogWarning("MCP server {Cluster}/{Destination} health check timed out", clusterName, destinationId);
                        }
                    }
                }
            }

            healthData["total_mcp_servers"] = totalServers;
            healthData["healthy_servers"] = healthyServers;
            healthData["unhealthy_servers"] = totalServers - healthyServers;
            healthData["yarp_also_monitors"] = "YARP performs its own health checks every 30 seconds";

            if (allHealthy && totalServers > 0)
            {
                return HealthCheckResult.Healthy(
                    $"All {totalServers} MCP servers are healthy", healthData);
            }
            else if (totalServers == 0)
            {
                return HealthCheckResult.Healthy(
                    "No MCP servers configured", healthData);
            }
            else
            {
                var message = $"{healthyServers}/{totalServers} MCP servers healthy. Issues: {string.Join(", ", issues)}";
                return HealthCheckResult.Degraded(message, null, healthData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking MCP server health");
            healthData["error"] = ex.Message;
            return HealthCheckResult.Unhealthy(
                "Failed to check MCP server health", ex, healthData);
        }
    }
}