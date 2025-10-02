using EndpointsConfigService = Endpoints.Configuration.Services.IConfigurationService;

namespace Gateway.Services;

/// <summary>
/// Implementation of the Endpoints configuration service
/// </summary>
public class EndpointsConfigurationService : EndpointsConfigService
{
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public EndpointsConfigurationService(Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public object? GetConfiguration()
    {
        return _configuration;
    }

    public Task<bool> SaveConfigurationAsync(object configuration, CancellationToken ct = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> UpdateConfigurationAsync(Action<object> updateAction, CancellationToken ct = default)
    {
        return Task.FromResult(false);
    }

    public Task ReloadConfigurationAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}