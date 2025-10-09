namespace Graph.User.Mcp.Server.Configuration;

public class McpConfiguration
{
    public const string SectionName = "McpServer";

    public SecurityOptions Security { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
    public PerformanceOptions Performance { get; set; } = new();
    public FeaturesOptions Features { get; set; } = new();
}

public class SecurityOptions
{
    public bool ValidateProxyHeaders { get; set; } = true;
    public bool ValidateArkToken { get; set; } = true;
    public List<string> AllowedProxyHeaders { get; set; } = new() { "X-MS-Graph-Proxy" };
    public string RequiredProxyHeaderValue { get; set; } = "true";
}

public class LoggingOptions
{
    public bool EnableStructuredLogging { get; set; } = true;
    public bool LogGraphApiCalls { get; set; } = true;
    public bool LogRequestHeaders { get; set; } = false; // Security: avoid logging sensitive headers
    public string LogLevel { get; set; } = "Information";
}

public class PerformanceOptions
{
    public int GraphApiTimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
    public bool EnableResponseCaching { get; set; } = false;
    public int CacheDurationSeconds { get; set; } = 300; // 5 minutes
}

public class FeaturesOptions
{
    public bool EnableDiagnostics { get; set; } = false;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableDistributedTracing { get; set; } = true;
    public List<string> EnabledTools { get; set; } = new()
    {
        "get_my_profile",
        "list_users", 
        "list_groups",
        "get_group_members",
        "search_users",
        "get_my_messages",
        "get_my_events"
    };
}