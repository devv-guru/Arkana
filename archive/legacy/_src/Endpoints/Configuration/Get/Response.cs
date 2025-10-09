namespace Endpoints.Configuration.Get;

public class Response
{
    public ConfigurationModel? Configuration { get; set; }
}

public class ConfigurationModel
{
    public List<HostConfigModel> Hosts { get; set; } = new();
    public List<ProxyRuleConfigModel> ProxyRules { get; set; } = new();
}

public class HostConfigModel
{
    public string Name { get; set; } = string.Empty;
    public List<string> HostNames { get; set; } = new();
    public CertificateConfigModel Certificate { get; set; } = new();
}

public class CertificateConfigModel
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = "InMemory";
    
    // For InMemory certificates
    public List<string>? SubjectAlternativeNames { get; set; }
    
    // For Azure Key Vault
    public string? KeyVaultUri { get; set; }
    public string? KeyVaultCertificateName { get; set; }
    public string? KeyVaultCertificatePasswordName { get; set; }
    
    // For AWS Secret Manager
    public string? AwsRegion { get; set; }
    public string? AwsCertificateName { get; set; }
    public string? AwsCertificatePasswordName { get; set; }
    
    // For File certificates
    public string? FilePath { get; set; }
    public string? FilePassword { get; set; }
}

public class ProxyRuleConfigModel
{
    public string Name { get; set; } = string.Empty;
    public List<string> Hosts { get; set; } = new();
    public string? PathPrefix { get; set; }
    public bool StripPrefix { get; set; }
    public List<string> Methods { get; set; } = new() { "GET", "POST", "PUT", "DELETE" };
    public ClusterConfigModel Cluster { get; set; } = new();
}

public class ClusterConfigModel
{
    public string Name { get; set; } = string.Empty;
    public string LoadBalancingPolicy { get; set; } = "RoundRobin";
    public HealthCheckConfigModel? HealthCheck { get; set; }
    public HttpRequestConfigModel? HttpRequest { get; set; }
    public List<Dictionary<string, string>> Transforms { get; set; } = new();
    public List<DestinationConfigModel> Destinations { get; set; } = new();
}

public class HealthCheckConfigModel
{
    public bool Enabled { get; set; } = true;
    public string Interval { get; set; } = "00:00:10";
    public string Timeout { get; set; } = "00:00:10";
    public int Threshold { get; set; } = 5;
    public string? Path { get; set; }
    public string? Query { get; set; }
}

public class HttpRequestConfigModel
{
    public string Version { get; set; } = "2";
    public string VersionPolicy { get; set; } = "RequestVersionOrLower";
    public bool AllowResponseBuffering { get; set; }
    public string ActivityTimeout { get; set; } = "00:02:00";
}

public class DestinationConfigModel
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
