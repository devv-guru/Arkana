using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Data.Entities;
using Shared.Services;

namespace Gateway.Services;

/// <summary>
/// File system implementation of configuration backup service
/// </summary>
public class FileSystemConfigurationBackupService : IConfigurationBackupService
{
    private readonly IWriteOnlyContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<FileSystemConfigurationBackupService> _logger;
    private readonly BackupOptions _options;

    public FileSystemConfigurationBackupService(
        IWriteOnlyContext context,
        IDateTimeProvider dateTimeProvider,
        IOptions<BackupOptions> options,
        ILogger<FileSystemConfigurationBackupService> logger)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> CreateBackupAsync()
    {
        var backupId = $"backup-{_dateTimeProvider.UtcNow:yyyyMMdd-HHmmss}";
        var backupPath = Path.Combine(_options.BackupPath, $"{backupId}.json");
        
        _logger.LogInformation("Creating configuration backup: {BackupId}", backupId);

        try
        {
            // Ensure backup directory exists
            Directory.CreateDirectory(_options.BackupPath);

            // Get current configuration
            var backup = await CreateBackupDataAsync();
            backup.Id = backupId;
            backup.CreatedAt = _dateTimeProvider.UtcNow;

            // Serialize and save
            var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(backupPath, json);

            _logger.LogInformation("Configuration backup created successfully: {BackupId} ({Size} bytes)", 
                backupId, json.Length);

            return backupId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create configuration backup: {BackupId}", backupId);
            throw;
        }
    }

    public async Task RestoreFromBackupAsync(string backupId)
    {
        var backupPath = Path.Combine(_options.BackupPath, $"{backupId}.json");
        
        _logger.LogInformation("Restoring configuration from backup: {BackupId}", backupId);

        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException($"Backup not found: {backupId}");
        }

        try
        {
            // Load backup data
            var json = await File.ReadAllTextAsync(backupPath);
            var backup = JsonSerializer.Deserialize<ConfigurationBackup>(json);
            
            if (backup == null)
            {
                throw new InvalidOperationException($"Invalid backup format: {backupId}");
            }

            // Restore configuration in transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Soft delete existing configuration
            await SoftDeleteExistingConfigurationAsync();

            // Restore from backup
            await RestoreConfigurationFromBackupAsync(backup);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Configuration restored successfully from backup: {BackupId}", backupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore configuration from backup: {BackupId}", backupId);
            throw;
        }
    }

    public async Task<bool> BackupExistsAsync(string backupId)
    {
        var backupPath = Path.Combine(_options.BackupPath, $"{backupId}.json");
        return File.Exists(backupPath);
    }

    public async Task<IEnumerable<BackupInfo>> GetBackupsAsync()
    {
        if (!Directory.Exists(_options.BackupPath))
        {
            return Enumerable.Empty<BackupInfo>();
        }

        var backupFiles = Directory.GetFiles(_options.BackupPath, "backup-*.json")
            .OrderByDescending(f => new FileInfo(f).CreationTime);

        var backups = new List<BackupInfo>();

        foreach (var file in backupFiles)
        {
            try
            {
                var fileInfo = new FileInfo(file);
                var backupId = Path.GetFileNameWithoutExtension(file);
                
                // Try to get counts from file content (optional)
                var json = await File.ReadAllTextAsync(file);
                var backup = JsonSerializer.Deserialize<ConfigurationBackup>(json);
                
                backups.Add(new BackupInfo(
                    backupId,
                    backup?.CreatedAt ?? fileInfo.CreationTime,
                    backup?.Routes?.Count ?? 0,
                    backup?.Clusters?.Count ?? 0,
                    backup?.Destinations?.Count ?? 0,
                    backup?.WebHosts?.Count ?? 0,
                    fileInfo.Length
                ));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read backup info from file: {File}", file);
            }
        }

        return backups;
    }

    public async Task CleanupOldBackupsAsync()
    {
        if (!Directory.Exists(_options.BackupPath))
        {
            return;
        }

        var backupFiles = Directory.GetFiles(_options.BackupPath, "backup-*.json")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .ToList();

        var cutoffDate = _dateTimeProvider.UtcNow.Subtract(_options.MaxAge);
        var filesToDelete = new List<FileInfo>();

        // Delete files beyond max count
        if (backupFiles.Count > _options.MaxBackups)
        {
            filesToDelete.AddRange(backupFiles.Skip(_options.MaxBackups));
        }

        // Delete files older than max age
        filesToDelete.AddRange(backupFiles.Where(f => f.CreationTime < cutoffDate));

        foreach (var file in filesToDelete.Distinct())
        {
            try
            {
                file.Delete();
                _logger.LogInformation("Deleted old backup: {FileName}", file.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete backup file: {FileName}", file.Name);
            }
        }

        if (filesToDelete.Any())
        {
            _logger.LogInformation("Cleanup completed: {DeletedCount} backup files removed", filesToDelete.Count);
        }
    }

    private async Task<ConfigurationBackup> CreateBackupDataAsync()
    {
        var routes = await _context.Routes
            .Include(r => r.Host)
            .Include(r => r.Match)
            .Include(r => r.Transforms)
            .Include(r => r.Metadata)
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        var clusters = await _context.Clusters
            .Include(c => c.Destinations)
            .Include(c => c.HealthCheck)
            .Include(c => c.SessionAffinity)
            .Include(c => c.HttpClient)
            .Include(c => c.HttpRequest)
            .Include(c => c.Metadata)
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        var webHosts = await _context.WebHosts
            .Where(h => !h.IsDeleted)
            .ToListAsync();

        var destinations = clusters.SelectMany(c => c.Destinations ?? Enumerable.Empty<Destination>())
            .Where(d => !d.IsDeleted)
            .ToList();

        return new ConfigurationBackup
        {
            Routes = routes,
            Clusters = clusters,
            WebHosts = webHosts,
            Destinations = destinations
        };
    }

    private async Task SoftDeleteExistingConfigurationAsync()
    {
        var routes = await _context.Routes.Where(r => !r.IsDeleted).ToListAsync();
        var clusters = await _context.Clusters.Where(c => !c.IsDeleted).ToListAsync();
        var webHosts = await _context.WebHosts.Where(h => !h.IsDeleted).ToListAsync();

        foreach (var route in routes)
        {
            route.IsDeleted = true;
            route.UpdatedAt = _dateTimeProvider.UtcNow;
        }

        foreach (var cluster in clusters)
        {
            cluster.IsDeleted = true;
            cluster.UpdatedAt = _dateTimeProvider.UtcNow;
        }

        foreach (var webHost in webHosts)
        {
            webHost.IsDeleted = true;
            webHost.UpdatedAt = _dateTimeProvider.UtcNow;
        }
    }

    private async Task RestoreConfigurationFromBackupAsync(ConfigurationBackup backup)
    {
        // Restore web hosts
        foreach (var webHost in backup.WebHosts ?? Enumerable.Empty<WebHost>())
        {
            webHost.IsDeleted = false;
            webHost.UpdatedAt = _dateTimeProvider.UtcNow;
            _context.WebHosts.Add(webHost);
        }

        // Restore clusters
        foreach (var cluster in backup.Clusters ?? Enumerable.Empty<Cluster>())
        {
            cluster.IsDeleted = false;
            cluster.UpdatedAt = _dateTimeProvider.UtcNow;
            _context.Clusters.Add(cluster);
        }

        // Restore routes
        foreach (var route in backup.Routes ?? Enumerable.Empty<Route>())
        {
            route.IsDeleted = false;
            route.UpdatedAt = _dateTimeProvider.UtcNow;
            _context.Routes.Add(route);
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Internal backup data structure
/// </summary>
internal class ConfigurationBackup
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Route>? Routes { get; set; }
    public List<Cluster>? Clusters { get; set; }
    public List<WebHost>? WebHosts { get; set; }
    public List<Destination>? Destinations { get; set; }
}