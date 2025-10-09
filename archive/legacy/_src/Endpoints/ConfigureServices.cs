using Endpoints.Metrics.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoints;

public static class ConfigureServices
{
    public static IServiceCollection AddGatewayFastEndpoints(this IServiceCollection services)
    {
        services.AddFastEndpoints();
        
        // Register metrics service adapter
        services.AddScoped<IMetricsServiceAdapter, MetricsServiceAdapter>();
        
        return services;
    }
    
    public static WebApplication UseGatewayFastEndpoints(this WebApplication app, IConfiguration configuration)
    {
        app.UseFastEndpoints();
        
        return app;
    }
}
