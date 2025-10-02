using Gateway.Models;

namespace Gateway.Services;

public interface IConfigurationUpdateService
{
    /// <summary>
    /// Updates the gateway configuration with validation, translation, and transactional commit
    /// </summary>
    Task<ConfigurationUpdateResult> UpdateConfigurationAsync(
        GatewayConfigurationModel configuration, 
        ConfigurationUpdateOptions? options = null);

    /// <summary>
    /// Validates a configuration without applying changes
    /// </summary>
    Task<ValidationResult> ValidateConfigurationAsync(GatewayConfigurationModel configuration);

    /// <summary>
    /// Gets the current configuration status
    /// </summary>
    Task<ConfigurationStatus> GetConfigurationStatusAsync();

    /// <summary>
    /// Rollback to the previous configuration version
    /// </summary>
    Task<ConfigurationUpdateResult> RollbackConfigurationAsync();
}

public class ConfigurationUpdateOptions
{
    /// <summary>
    /// Perform health checks on destinations before committing
    /// </summary>
    public bool ValidateDestinationHealth { get; set; } = false;

    /// <summary>
    /// Skip validation and force update (use with caution)
    /// </summary>
    public bool ForceUpdate { get; set; } = false;

    /// <summary>
    /// Create backup before update for rollback capability
    /// </summary>
    public bool CreateBackup { get; set; } = true;

    /// <summary>
    /// Timeout for the entire update operation
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Dry run - validate and translate but don't commit changes
    /// </summary>
    public bool DryRun { get; set; } = false;
}

public class ConfigurationUpdateResult
{
    public bool IsSuccess { get; set; }
    public string? BackupId { get; set; }
    public ConfigurationStatus? Status { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public TimeSpan Duration { get; set; }
    
    public static ConfigurationUpdateResult Success(ConfigurationStatus status, string? backupId = null)
    {
        return new ConfigurationUpdateResult
        {
            IsSuccess = true,
            Status = status,
            BackupId = backupId
        };
    }

    public static ConfigurationUpdateResult Failure(params string[] errors)
    {
        return new ConfigurationUpdateResult
        {
            IsSuccess = false,
            Errors = errors.ToList()
        };
    }

    public void AddError(string error)
    {
        Errors.Add(error);
        IsSuccess = false;
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}