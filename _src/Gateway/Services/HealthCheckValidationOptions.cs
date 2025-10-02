namespace Gateway.Services;

/// <summary>
/// Configuration options for health check validation
/// </summary>
public class HealthCheckValidationOptions
{
    /// <summary>
    /// Timeout for individual destination health checks
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Maximum number of concurrent health checks
    /// </summary>
    public int MaxConcurrency { get; set; } = 10;
    
    /// <summary>
    /// Number of retry attempts for failed health checks
    /// </summary>
    public int RetryCount { get; set; } = 2;
    
    /// <summary>
    /// Delay between retry attempts
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Whether to treat health check failures as warnings instead of errors
    /// </summary>
    public bool TreatFailuresAsWarnings { get; set; } = true;
    
    /// <summary>
    /// Custom user agent for health check requests
    /// </summary>
    public string UserAgent { get; set; } = "Arkana-Gateway-HealthCheck/1.0";
    
    /// <summary>
    /// Expected HTTP status codes that indicate healthy destinations
    /// </summary>
    public int[] HealthyStatusCodes { get; set; } = { 200, 204 };
}