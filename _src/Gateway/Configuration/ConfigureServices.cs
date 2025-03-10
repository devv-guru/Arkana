using Endpoints.Configuration.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Configuration;

public static class ConfigureServices
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<GatewayConfigurationService>();
        services.AddSingleton<IConfigurationService, ConfigurationServiceAdapter>();
        
        return services;
    }
}
