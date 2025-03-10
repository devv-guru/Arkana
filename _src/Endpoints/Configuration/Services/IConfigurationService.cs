namespace Endpoints.Configuration.Services;

/// <summary>
/// Interface for configuration service
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the current configuration
    /// </summary>
    /// <returns>The configuration object or null if not found</returns>
    object? GetConfiguration();
    
    /// <summary>
    /// Saves the configuration
    /// </summary>
    /// <param name="configuration">The configuration to save</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SaveConfigurationAsync(object configuration, CancellationToken ct = default);
    
    /// <summary>
    /// Updates the configuration
    /// </summary>
    /// <param name="updateAction">Action to update the configuration</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdateConfigurationAsync(Action<object> updateAction, CancellationToken ct = default);
    
    /// <summary>
    /// Reloads the configuration from the source
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    Task ReloadConfigurationAsync(CancellationToken ct = default);
}
