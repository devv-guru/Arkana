using Shared.Models;

namespace Gateway.Configuration;

public class GatewayConfigurationOptions
{
    public List<HostConfig> Hosts { get; set; } = new();
    public List<ProxyRuleConfig> ProxyRules { get; set; } = new();
    public UIOptions? UI { get; set; }
}

public class HostConfig
{
    public string Name { get; set; } = string.Empty;
    public List<string> HostNames { get; set; } = new();
    public CertificateConfig? Certificate { get; set; }
}

public class CertificateConfig
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FilePassword { get; set; }
    public string? KeyVaultUri { get; set; }
    public string? KeyVaultCertificateName { get; set; }
    public string? KeyVaultCertificatePasswordName { get; set; }
    public string? AwsRegion { get; set; }
    public string? AwsCertificateName { get; set; }
    public string? AwsCertificatePasswordName { get; set; }
    public List<string> SubjectAlternativeNames { get; set; } = new();
}

public class ProxyRuleConfig
{
    public string Name { get; set; } = string.Empty;
    public List<string> Hosts { get; set; } = new();
    public string? PathPrefix { get; set; }
    public bool StripPrefix { get; set; }
    public List<string> Methods { get; set; } = new();
    public ClusterConfig Cluster { get; set; } = new();
}

public class ClusterConfig
{
    public string Name { get; set; } = string.Empty;
    public string LoadBalancingPolicy { get; set; } = "RoundRobin";
    public HealthCheckConfig? HealthCheck { get; set; }
    public HttpRequestSettingsConfig? HttpRequest { get; set; }
    public List<Dictionary<string, string>> Transforms { get; set; } = new();
    public List<DestinationConfig> Destinations { get; set; } = new();
}

public class HealthCheckConfig
{
    public bool Enabled { get; set; }
    public string Interval { get; set; } = "00:00:10";
    public string Timeout { get; set; } = "00:00:10";
    public int Threshold { get; set; } = 3;
    public string? Path { get; set; }
}

public class HttpRequestSettingsConfig
{
    public string? Version { get; set; }
    public string? VersionPolicy { get; set; }
    public bool AllowResponseBuffering { get; set; }
    public string? ActivityTimeout { get; set; }
}

public class DestinationConfig
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int? Weight { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }

}
