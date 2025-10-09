using Endpoints.Configuration.Services;
using Shared.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Tests.Helpers;

/// <summary>
/// Test implementation of IConfigurationService for unit testing
/// </summary>
public class TestConfigurationService : IConfigurationService
{
    private object? _configuration;

    public TestConfigurationService(object? configuration = null)
    {
        _configuration = configuration;
    }

    public object? GetConfiguration() => _configuration;

    public Task<bool> SaveConfigurationAsync(object configuration, CancellationToken ct = default)
    {
        _configuration = configuration;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateConfigurationAsync(Action<object> updateAction, CancellationToken ct = default)
    {
        if (_configuration != null)
        {
            updateAction(_configuration);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task ReloadConfigurationAsync(CancellationToken ct = default)
    {
        // Simulate reload - in a real implementation, this would reload from the source
        return Task.CompletedTask;
    }

    /// <summary>
    /// Test helper method to set configuration directly
    /// </summary>
    public void SetConfiguration(object? configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Test helper method to simulate file not found scenario
    /// </summary>
    public void SimulateFileNotFound()
    {
        _configuration = null;
    }

    /// <summary>
    /// Test helper method to check if service has configuration
    /// </summary>
    public bool HasConfiguration => _configuration != null;
}
