using Gateway.Models;

namespace Gateway.Services;

public interface IConfigurationValidationService
{
    /// <summary>
    /// Validates a gateway configuration before applying changes
    /// </summary>
    Task<ValidationResult> ValidateConfigurationAsync(GatewayConfigurationModel configuration);
    
    /// <summary>
    /// Validates individual proxy rule
    /// </summary>
    ValidationResult ValidateProxyRule(ProxyRuleConfigurationModel proxyRule);
    
    /// <summary>
    /// Validates cluster configuration
    /// </summary>
    ValidationResult ValidateCluster(ClusterConfigurationModel cluster);
    
    /// <summary>
    /// Validates that destinations are reachable (optional check)
    /// </summary>
    Task<ValidationResult> ValidateDestinationHealthAsync(List<DestinationConfigurationModel> destinations);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(params string[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
    
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }
    
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
    
    public void Merge(ValidationResult other)
    {
        Errors.AddRange(other.Errors);
        Warnings.AddRange(other.Warnings);
        if (!other.IsValid) IsValid = false;
    }
}