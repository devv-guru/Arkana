using Gateway.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Gateway.Services;

public class ConfigurationValidationService : IConfigurationValidationService
{
    private readonly ILogger<ConfigurationValidationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly HealthCheckValidationOptions _healthCheckOptions;

    public ConfigurationValidationService(
        ILogger<ConfigurationValidationService> logger,
        HttpClient httpClient,
        IOptions<HealthCheckValidationOptions> healthCheckOptions)
    {
        _logger = logger;
        _httpClient = httpClient;
        _healthCheckOptions = healthCheckOptions.Value;
    }

    public async Task<ValidationResult> ValidateConfigurationAsync(GatewayConfigurationModel configuration)
    {
        var result = new ValidationResult();
        
        _logger.LogInformation("Validating gateway configuration with {HostCount} hosts and {ProxyRuleCount} proxy rules", 
            configuration.Hosts.Count, configuration.ProxyRules.Count);

        // Basic validation
        if (!configuration.Hosts.Any() && configuration.ProxyRules.Any())
        {
            result.AddWarning("No hosts defined but proxy rules exist. Rules may not be reachable.");
        }

        // Validate each host
        foreach (var host in configuration.Hosts)
        {
            var hostResult = ValidateHost(host);
            result.Merge(hostResult);
        }

        // Validate each proxy rule
        foreach (var proxyRule in configuration.ProxyRules)
        {
            var ruleResult = ValidateProxyRule(proxyRule);
            result.Merge(ruleResult);
        }

        // Check for duplicate host names
        var duplicateHosts = configuration.Hosts
            .SelectMany(h => h.HostNames)
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicate in duplicateHosts)
        {
            result.AddError($"Duplicate hostname found: {duplicate}");
        }

        // Check for overlapping proxy rules
        var overlappingRules = FindOverlappingRules(configuration.ProxyRules);
        foreach (var overlap in overlappingRules)
        {
            result.AddWarning($"Potential rule overlap detected: {overlap}");
        }

        return result;
    }

    public ValidationResult ValidateProxyRule(ProxyRuleConfigurationModel proxyRule)
    {
        var result = new ValidationResult();

        // Required fields
        if (string.IsNullOrWhiteSpace(proxyRule.Name))
            result.AddError("Proxy rule name is required");

        if (string.IsNullOrWhiteSpace(proxyRule.PathPrefix))
            result.AddError("Path prefix is required for proxy rule");
        else if (!proxyRule.PathPrefix.StartsWith("/"))
            result.AddError("Path prefix must start with '/'");

        if (!proxyRule.Hosts.Any())
            result.AddWarning($"No hosts specified for proxy rule '{proxyRule.Name}'. Rule may not be accessible.");

        // Validate cluster
        var clusterResult = ValidateCluster(proxyRule.Cluster);
        result.Merge(clusterResult);

        // Validate HTTP methods
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
        foreach (var method in proxyRule.Methods)
        {
            if (!validMethods.Contains(method.ToUpperInvariant()))
            {
                result.AddWarning($"Unusual HTTP method specified: {method}");
            }
        }

        return result;
    }

    public ValidationResult ValidateCluster(ClusterConfigurationModel cluster)
    {
        var result = new ValidationResult();

        // Required fields
        if (string.IsNullOrWhiteSpace(cluster.Name))
            result.AddError("Cluster name is required");

        if (!cluster.Destinations.Any())
            result.AddError($"Cluster '{cluster.Name}' must have at least one destination");

        // Validate load balancing policy
        var validPolicies = new[] { "RoundRobin", "LeastRequests", "Random", "PowerOfTwo" };
        if (!validPolicies.Contains(cluster.LoadBalancingPolicy, StringComparer.OrdinalIgnoreCase))
        {
            result.AddWarning($"Unknown load balancing policy: {cluster.LoadBalancingPolicy}. Using RoundRobin as fallback.");
        }

        // Validate destinations
        foreach (var destination in cluster.Destinations)
        {
            var destResult = ValidateDestination(destination);
            result.Merge(destResult);
        }

        // Validate health check configuration
        if (cluster.HealthCheck != null)
        {
            var healthResult = ValidateHealthCheck(cluster.HealthCheck);
            result.Merge(healthResult);
        }

        // Validate HTTP request configuration
        if (cluster.HttpRequest != null)
        {
            var httpResult = ValidateHttpRequest(cluster.HttpRequest);
            result.Merge(httpResult);
        }

        return result;
    }

    public async Task<ValidationResult> ValidateDestinationHealthAsync(List<DestinationConfigurationModel> destinations)
    {
        var result = new ValidationResult();
        
        _logger.LogInformation("Performing health validation for {DestinationCount} destinations with timeout {Timeout}ms", 
            destinations.Count, _healthCheckOptions.Timeout.TotalMilliseconds);

        using var cts = new CancellationTokenSource(_healthCheckOptions.Timeout);
        var semaphore = new SemaphoreSlim(_healthCheckOptions.MaxConcurrency);
        
        var healthCheckTasks = destinations.Select(async destination =>
        {
            await semaphore.WaitAsync(cts.Token);
            try
            {
                return await ValidateDestinationWithRetriesAsync(destination, cts.Token);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var healthResults = await Task.WhenAll(healthCheckTasks);
        
        foreach (var error in healthResults.Where(r => r != null))
        {
            if (_healthCheckOptions.TreatFailuresAsWarnings)
            {
                result.AddWarning(error!);
            }
            else
            {
                result.AddError(error!);
            }
        }

        return result;
    }
    
    private async Task<string?> ValidateDestinationWithRetriesAsync(DestinationConfigurationModel destination, CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        
        for (int attempt = 0; attempt <= _healthCheckOptions.RetryCount; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    await Task.Delay(_healthCheckOptions.RetryDelay, cancellationToken);
                }
                
                var healthEndpoint = $"{destination.Address.TrimEnd('/')}/health";
                
                using var request = new HttpRequestMessage(HttpMethod.Get, healthEndpoint);
                request.Headers.Add("User-Agent", _healthCheckOptions.UserAgent);
                
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                
                if (_healthCheckOptions.HealthyStatusCodes.Contains((int)response.StatusCode))
                {
                    return null; // Success
                }
                
                lastException = new HttpRequestException($"Health check returned status code: {response.StatusCode}");
            }
            catch (Exception ex) when (attempt < _healthCheckOptions.RetryCount)
            {
                lastException = ex;
                _logger.LogDebug("Health check attempt {Attempt} failed for destination '{Destination}': {Error}", 
                    attempt + 1, destination.Name, ex.Message);
            }
        }
        
        return $"Destination '{destination.Name}' health check failed after {_healthCheckOptions.RetryCount + 1} attempts: {lastException?.Message}";
    }

    private ValidationResult ValidateHost(HostConfigurationModel host)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(host.Name))
            result.AddError("Host name is required");

        if (!host.HostNames.Any())
            result.AddError($"Host '{host.Name}' must have at least one hostname");

        foreach (var hostname in host.HostNames)
        {
            if (!IsValidHostname(hostname))
            {
                result.AddError($"Invalid hostname format: {hostname}");
            }
        }

        if (host.Certificate != null)
        {
            var certResult = ValidateCertificate(host.Certificate);
            result.Merge(certResult);
        }

        return result;
    }

    private ValidationResult ValidateDestination(DestinationConfigurationModel destination)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(destination.Name))
            result.AddError("Destination name is required");

        if (string.IsNullOrWhiteSpace(destination.Address))
            result.AddError($"Destination '{destination.Name}' address is required");
        else if (!Uri.TryCreate(destination.Address, UriKind.Absolute, out var uri) || 
                 (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            result.AddError($"Destination '{destination.Name}' has invalid address format: {destination.Address}");
        }

        return result;
    }

    private ValidationResult ValidateHealthCheck(HealthCheckConfigurationModel healthCheck)
    {
        var result = new ValidationResult();

        if (healthCheck.Interval <= TimeSpan.Zero)
            result.AddError("Health check interval must be positive");

        if (healthCheck.Timeout <= TimeSpan.Zero)
            result.AddError("Health check timeout must be positive");

        if (healthCheck.Timeout >= healthCheck.Interval)
            result.AddWarning("Health check timeout should be less than interval");

        if (healthCheck.Threshold <= 0)
            result.AddError("Health check threshold must be positive");

        if (!string.IsNullOrEmpty(healthCheck.Path) && !healthCheck.Path.StartsWith("/"))
            result.AddError("Health check path must start with '/'");

        return result;
    }

    private ValidationResult ValidateHttpRequest(HttpRequestConfigurationModel httpRequest)
    {
        var result = new ValidationResult();

        var validVersions = new[] { "1.0", "1.1", "2.0", "3.0" };
        if (!validVersions.Contains(httpRequest.Version))
        {
            result.AddWarning($"Unusual HTTP version specified: {httpRequest.Version}");
        }

        var validVersionPolicies = new[] { "RequestVersionOrLower", "RequestVersionOrHigher", "RequestVersionExact" };
        if (!validVersionPolicies.Contains(httpRequest.VersionPolicy))
        {
            result.AddError($"Invalid HTTP version policy: {httpRequest.VersionPolicy}");
        }

        if (httpRequest.ActivityTimeout <= TimeSpan.Zero)
            result.AddError("Activity timeout must be positive");

        return result;
    }

    private ValidationResult ValidateCertificate(CertificateConfigurationModel certificate)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(certificate.Name))
            result.AddError("Certificate name is required");

        if (string.IsNullOrWhiteSpace(certificate.Source))
            result.AddError("Certificate source is required");

        var validSources = new[] { "File", "KeyVault", "Store" };
        if (!validSources.Contains(certificate.Source, StringComparer.OrdinalIgnoreCase))
        {
            result.AddError($"Invalid certificate source: {certificate.Source}");
        }

        if (certificate.Source.Equals("File", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(certificate.Path))
                result.AddError("Certificate path is required for file source");
        }
        else if (certificate.Source.Equals("KeyVault", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(certificate.KeyVaultUri))
                result.AddError("Key Vault URI is required for KeyVault source");
            if (string.IsNullOrWhiteSpace(certificate.SecretName))
                result.AddError("Secret name is required for KeyVault source");
        }

        return result;
    }

    private static bool IsValidHostname(string hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname) || hostname.Length > 253)
            return false;

        // Allow wildcards for subdomains
        if (hostname.StartsWith("*."))
            hostname = hostname.Substring(2);

        // Basic hostname validation
        var hostnameRegex = new Regex(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$");
        return hostnameRegex.IsMatch(hostname);
    }

    private static List<string> FindOverlappingRules(List<ProxyRuleConfigurationModel> proxyRules)
    {
        var overlaps = new List<string>();

        for (int i = 0; i < proxyRules.Count; i++)
        {
            for (int j = i + 1; j < proxyRules.Count; j++)
            {
                var rule1 = proxyRules[i];
                var rule2 = proxyRules[j];

                // Check if rules have overlapping hosts and paths
                var sharedHosts = rule1.Hosts.Intersect(rule2.Hosts, StringComparer.OrdinalIgnoreCase);
                if (sharedHosts.Any())
                {
                    // Check for path conflicts
                    if (DoPathsOverlap(rule1.PathPrefix, rule2.PathPrefix))
                    {
                        overlaps.Add($"Rules '{rule1.Name}' and '{rule2.Name}' may conflict on hosts [{string.Join(", ", sharedHosts)}] with paths '{rule1.PathPrefix}' and '{rule2.PathPrefix}'");
                    }
                }
            }
        }

        return overlaps;
    }

    private static bool DoPathsOverlap(string path1, string path2)
    {
        // Simple overlap check - more sophisticated logic could be added
        var p1 = path1.TrimEnd('/');
        var p2 = path2.TrimEnd('/');

        return p1.StartsWith(p2, StringComparison.OrdinalIgnoreCase) ||
               p2.StartsWith(p1, StringComparison.OrdinalIgnoreCase);
    }
}