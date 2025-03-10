using Endpoints.Configuration.Get;
using Endpoints.Configuration.Services;

namespace Gateway.Configuration;

/// <summary>
/// Adapter for GatewayConfigurationService to implement IConfigurationService
/// </summary>
public class ConfigurationServiceAdapter : IConfigurationService
{
    private readonly GatewayConfigurationService _gatewayConfigurationService;

    public ConfigurationServiceAdapter(GatewayConfigurationService gatewayConfigurationService)
    {
        _gatewayConfigurationService = gatewayConfigurationService;
    }

    public object? GetConfiguration()
    {
        return _gatewayConfigurationService.GetConfiguration();
    }

    public async Task<bool> SaveConfigurationAsync(object configuration, CancellationToken ct = default)
    {
        if (configuration is GatewayConfigurationOptions options)
        {
            return await _gatewayConfigurationService.SaveConfigurationAsync(options, ct);
        }
        
        return false;
    }

    public async Task<bool> UpdateConfigurationAsync(Action<object> updateAction, CancellationToken ct = default)
    {
        var config = _gatewayConfigurationService.GetConfiguration();
        if (config == null)
        {
            return false;
        }
        
        updateAction(config);
        return await _gatewayConfigurationService.SaveConfigurationAsync(config, ct);
    }

    public async Task ReloadConfigurationAsync(CancellationToken ct = default)
    {
        await _gatewayConfigurationService.LoadConfigurationAsync(ct);
    }
}
