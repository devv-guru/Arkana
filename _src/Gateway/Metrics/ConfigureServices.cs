using Data.Contexts.Base;
using Domain.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Metrics;

public static class ConfigureServices
{
    public static IServiceCollection AddMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        // Register metrics service
        services.AddSingleton<IMetricsService, MetricsService>();
        
        // Register metrics queue for async processing
        services.AddHostedService<MetricsQueue>();
        
        // Register system metrics collector
        services.AddHostedService<SystemMetricsCollector>();
        
        return services;
    }
    
    public static IApplicationBuilder UseMetricsMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MetricsMiddleware>();
    }
}
