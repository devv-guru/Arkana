using Data.Entities;

namespace Gateway.Services;

/// <summary>
/// Provides abstraction for configuration backup and restore operations
/// </summary>
public interface IConfigurationBackupService
{
    /// <summary>
    /// Creates a backup of the current configuration
    /// </summary>
    /// <returns>Backup identifier for later restoration</returns>
    Task<string> CreateBackupAsync();
    
    /// <summary>
    /// Restores configuration from a backup
    /// </summary>
    /// <param name="backupId">The backup identifier</param>
    Task RestoreFromBackupAsync(string backupId);
    
    /// <summary>
    /// Checks if a backup exists
    /// </summary>
    /// <param name="backupId">The backup identifier</param>
    Task<bool> BackupExistsAsync(string backupId);
    
    /// <summary>
    /// Gets information about available backups
    /// </summary>
    Task<IEnumerable<BackupInfo>> GetBackupsAsync();
    
    /// <summary>
    /// Deletes old backups beyond the retention policy
    /// </summary>
    Task CleanupOldBackupsAsync();
}

/// <summary>
/// Information about a configuration backup
/// </summary>
public record BackupInfo(
    string Id,
    DateTime CreatedAt,
    int RouteCount,
    int ClusterCount,
    int DestinationCount,
    int WebHostCount,
    long SizeBytes);

/// <summary>
/// Configuration backup options
/// </summary>
public class BackupOptions
{
    /// <summary>
    /// Maximum number of backups to retain
    /// </summary>
    public int MaxBackups { get; set; } = 10;
    
    /// <summary>
    /// Maximum age of backups to retain
    /// </summary>
    public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(30);
    
    /// <summary>
    /// Backup storage location
    /// </summary>
    public string BackupPath { get; set; } = "backups";
}