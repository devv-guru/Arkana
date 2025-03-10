﻿﻿using Gateway.Configuration;
using Yarp.ReverseProxy.Configuration;

namespace Gateway.Proxy;

public static class ConfigureServices
{
    public static IServiceCollection AddProxy(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the GatewayConfigurationService
        services.AddSingleton<GatewayConfigurationService>();
        
        // Register the YarpConfigurationService as the IProxyConfigProvider
        services.AddSingleton<IProxyConfigProvider, YarpConfigurationService>();
        
        // Add YARP reverse proxy
        services.AddReverseProxy();
        
        return services;
    }

    public static IApplicationBuilder UseProxy(this WebApplication app)
    {
        // Configure the pipeline for the reverse proxy
        app.MapReverseProxy(pipeline =>
        {
            pipeline.UseSessionAffinity();
            pipeline.UseLoadBalancing();
        });

        // Load the gateway configuration and update YARP
        var gatewayConfigService = app.Services.GetRequiredService<GatewayConfigurationService>();
        var yarpConfigService = app.Services.GetRequiredService<YarpConfigurationService>();
        
        // Load the configuration asynchronously
        Task.Run(async () =>
        {
            await gatewayConfigService.LoadConfigurationAsync();
            
            // Update the YARP configuration
            ((YarpConfigurationService)yarpConfigService).UpdateConfig();
        });

        return app;
    }
}
