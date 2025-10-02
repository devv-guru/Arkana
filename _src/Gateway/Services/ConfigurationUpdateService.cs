using Gateway.Models;
using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Shared.Services;

namespace Gateway.Services;

public class ConfigurationUpdateService : IConfigurationUpdateService
{
    private readonly IWriteOnlyContext _context;
    private readonly IConfigurationValidationService _validationService;
    private readonly IConfigurationTranslationService _translationService;
    private readonly DynamicProxyConfigProvider _proxyConfigProvider;
    private readonly IConfigurationBackupService _backupService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ConfigurationUpdateService> _logger;

    public ConfigurationUpdateService(
        IWriteOnlyContext context,
        IConfigurationValidationService validationService,
        IConfigurationTranslationService translationService,
        DynamicProxyConfigProvider proxyConfigProvider,
        IConfigurationBackupService backupService,
        IDateTimeProvider dateTimeProvider,
        ILogger<ConfigurationUpdateService> logger)
    {
        _context = context;
        _validationService = validationService;
        _translationService = translationService;
        _proxyConfigProvider = proxyConfigProvider;
        _backupService = backupService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<ConfigurationUpdateResult> UpdateConfigurationAsync(
        GatewayConfigurationModel configuration, 
        ConfigurationUpdateOptions? options = null)
    {
        options ??= new ConfigurationUpdateOptions();
        var stopwatch = Stopwatch.StartNew();
        var result = new ConfigurationUpdateResult();
        string? backupId = null;

        try
        {
            _logger.LogInformation("Starting configuration update with options: ValidateHealth={ValidateHealth}, ForceUpdate={ForceUpdate}, DryRun={DryRun}",
                options.ValidateDestinationHealth, options.ForceUpdate, options.DryRun);

            // Step 1: Validation (unless forced)
            if (!options.ForceUpdate)
            {
                _logger.LogInformation("Validating configuration...");
                var validationResult = await _validationService.ValidateConfigurationAsync(configuration);
                
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Configuration validation failed with {ErrorCount} errors", validationResult.Errors.Count);
                    result.Errors.AddRange(validationResult.Errors);
                    result.IsSuccess = false;
                    return result;
                }

                result.Warnings.AddRange(validationResult.Warnings);

                // Optional destination health validation
                if (options.ValidateDestinationHealth)
                {
                    _logger.LogInformation("Validating destination health...");
                    var destinations = configuration.ProxyRules
                        .SelectMany(rule => rule.Cluster.Destinations)
                        .ToList();
                    
                    var healthResult = await _validationService.ValidateDestinationHealthAsync(destinations);
                    result.Warnings.AddRange(healthResult.Warnings);
                    
                    if (!healthResult.IsValid)
                    {
                        result.Errors.AddRange(healthResult.Errors);
                        result.IsSuccess = false;
                        return result;
                    }
                }
            }

            // Step 2: Translation
            _logger.LogInformation("Translating configuration to database entities...");
            var translationResult = await _translationService.TranslateConfigurationAsync(configuration);
            
            if (!translationResult.IsSuccess)
            {
                _logger.LogError("Configuration translation failed");
                result.Errors.AddRange(translationResult.Errors);
                result.IsSuccess = false;
                return result;
            }

            result.Warnings.AddRange(translationResult.Warnings);

            // Stop here if dry run
            if (options.DryRun)
            {
                _logger.LogInformation("Dry run completed successfully. No changes committed.");
                result.IsSuccess = true;
                result.Status = await GetConfigurationStatusAsync();
                return result;
            }

            // Step 3: Create backup (if requested)
            if (options.CreateBackup)
            {
                _logger.LogInformation("Creating configuration backup...");
                backupId = await _backupService.CreateBackupAsync();
                result.BackupId = backupId;
            }

            // Step 4: Transactional update
            _logger.LogInformation("Committing configuration changes to database...");
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Clear existing configuration (soft delete)
                await SoftDeleteExistingConfigurationAsync();

                // Add new configuration
                await AddNewConfigurationAsync(translationResult);

                // Save changes
                await _context.SaveChangesAsync();

                // Step 5: Update YARP configuration
                _logger.LogInformation("Reloading YARP proxy configuration...");
                await _proxyConfigProvider.ReloadConfigurationAsync();

                // Commit transaction
                await transaction.CommitAsync();

                _logger.LogInformation("Configuration update completed successfully");
                result.IsSuccess = true;
                result.Status = await GetConfigurationStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transactional update, rolling back...");
                await transaction.RollbackAsync();
                
                // If we have a backup, we could restore it here
                if (!string.IsNullOrEmpty(backupId))
                {
                    _logger.LogInformation("Attempting to restore from backup {BackupId}...", backupId);
                    try
                    {
                        await _backupService.RestoreFromBackupAsync(backupId);
                    }
                    catch (Exception restoreEx)
                    {
                        _logger.LogError(restoreEx, "Failed to restore from backup");
                        result.AddWarning($"Failed to restore from backup: {restoreEx.Message}");
                    }
                }

                result.AddError($"Configuration update failed: {ex.Message}");
                throw;
            }
        }
        catch (Exception ex) when (!options.DryRun)
        {
            _logger.LogError(ex, "Configuration update failed");
            result.AddError($"Update failed: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            _logger.LogInformation("Configuration update completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
        }

        return result;
    }

    public async Task<ValidationResult> ValidateConfigurationAsync(GatewayConfigurationModel configuration)
    {
        _logger.LogInformation("Validating configuration without applying changes");
        return await _validationService.ValidateConfigurationAsync(configuration);
    }

    public async Task<ConfigurationStatus> GetConfigurationStatusAsync()
    {
        var routeCount = await _context.Routes.CountAsync(r => !r.IsDeleted);
        var clusterCount = await _context.Clusters.CountAsync(c => !c.IsDeleted);
        var destinationCount = await _context.Destinations.CountAsync(d => !d.IsDeleted);
        var webHostCount = await _context.WebHosts.CountAsync(h => !h.IsDeleted);

        var lastUpdated = await _context.Routes
            .Where(r => !r.IsDeleted)
            .MaxAsync(r => (DateTime?)r.UpdatedAt) ?? DateTime.MinValue;

        return new ConfigurationStatus
        {
            IsValid = true, // Could add more sophisticated validation
            RouteCount = routeCount,
            ClusterCount = clusterCount,
            DestinationCount = destinationCount,
            WebHostCount = webHostCount,
            LastUpdated = lastUpdated
        };
    }

    public async Task<ConfigurationUpdateResult> RollbackConfigurationAsync()
    {
        _logger.LogInformation("Rolling back to previous configuration...");
        
        try
        {
            // Get the most recent backup
            var backups = await _backupService.GetBackupsAsync();
            var latestBackup = backups.OrderByDescending(b => b.CreatedAt).FirstOrDefault();
            
            if (latestBackup == null)
            {
                return ConfigurationUpdateResult.Failure("No backup available for rollback");
            }
            
            _logger.LogInformation("Rolling back to backup: {BackupId}", latestBackup.Id);
            
            // Restore from backup
            await _backupService.RestoreFromBackupAsync(latestBackup.Id);
            
            // Reload YARP configuration
            await _proxyConfigProvider.ReloadConfigurationAsync();
            
            var status = await GetConfigurationStatusAsync();
            
            _logger.LogInformation("Successfully rolled back to backup: {BackupId}", latestBackup.Id);
            
            return ConfigurationUpdateResult.Success(status, latestBackup.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback configuration");
            return ConfigurationUpdateResult.Failure($"Rollback failed: {ex.Message}");
        }
    }

    private async Task SoftDeleteExistingConfigurationAsync()
    {
        _logger.LogDebug("Soft deleting existing configuration");

        // Mark all existing entities as deleted
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

    private async Task AddNewConfigurationAsync(TranslationResult translationResult)
    {
        _logger.LogDebug("Adding new configuration entities");

        // Add web hosts
        foreach (var webHost in translationResult.WebHosts)
        {
            _context.WebHosts.Add(webHost);
        }

        // Add clusters
        foreach (var cluster in translationResult.Clusters)
        {
            _context.Clusters.Add(cluster);
        }

        // Add routes
        foreach (var route in translationResult.Routes)
        {
            _context.Routes.Add(route);
        }

        await Task.CompletedTask;
    }

}