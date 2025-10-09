namespace Gateway.Models;

public class GatewayConfigurationModel
{
    public List<HostConfigurationModel> Hosts { get; set; } = new();
    public List<ProxyRuleConfigurationModel> ProxyRules { get; set; } = new();
}

public class HostConfigurationModel
{
    public string Name { get; set; } = string.Empty;
    public List<string> HostNames { get; set; } = new();
    public CertificateConfigurationModel? Certificate { get; set; }
}

public class CertificateConfigurationModel
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public List<string> SubjectAlternativeNames { get; set; } = new();
    public string? Path { get; set; }
    public string? Password { get; set; }
    public string? KeyVaultUri { get; set; }
    public string? SecretName { get; set; }
}

public class ProxyRuleConfigurationModel
{
    public string Name { get; set; } = string.Empty;
    public List<string> Hosts { get; set; } = new();
    public string PathPrefix { get; set; } = string.Empty;
    public bool StripPrefix { get; set; }
    public List<string> Methods { get; set; } = new();
    public ClusterConfigurationModel Cluster { get; set; } = new();
}

public class ClusterConfigurationModel
{
    public string Name { get; set; } = string.Empty;
    public string LoadBalancingPolicy { get; set; } = "RoundRobin";
    public HealthCheckConfigurationModel? HealthCheck { get; set; }
    public HttpRequestConfigurationModel? HttpRequest { get; set; }
    public List<TransformConfigurationModel> Transforms { get; set; } = new();
    public List<DestinationConfigurationModel> Destinations { get; set; } = new();
}

public class HealthCheckConfigurationModel
{
    public bool Enabled { get; set; } = true;
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    public int Threshold { get; set; } = 3;
    public string Path { get; set; } = "/health";
    public string? Query { get; set; }
}

public class HttpRequestConfigurationModel
{
    public string Version { get; set; } = "2";
    public string VersionPolicy { get; set; } = "RequestVersionOrLower";
    public bool AllowResponseBuffering { get; set; } = false;
    public TimeSpan ActivityTimeout { get; set; } = TimeSpan.FromMinutes(2);
}

public class TransformConfigurationModel
{
    public string? RequestHeader { get; set; }
    public string? Set { get; set; }
    public string? PathSet { get; set; }
    public string? PathPattern { get; set; }
    public string? ResponseHeader { get; set; }
    public string? Append { get; set; }
}

public class DestinationConfigurationModel
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Health { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class ConfigurationStatus
{
    public bool IsValid { get; set; }
    public int RouteCount { get; set; }
    public int ClusterCount { get; set; }
    public int DestinationCount { get; set; }
    public int WebHostCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}