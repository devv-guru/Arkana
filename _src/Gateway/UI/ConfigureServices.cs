using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.IO;

namespace Gateway.UI;

/// <summary>
/// Extension methods for configuring UI services
/// </summary>
public static class ConfigureServices
{
    /// <summary>
    /// Adds UI services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddUI(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure UI options
        services.Configure<UIOptions>(configuration.GetSection("UI"));
        
        return services;
    }
    
    /// <summary>
    /// Uses UI middleware
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="env">The web hosting environment</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseUI(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        var uiOptions = app.ApplicationServices.GetRequiredService<IOptions<UIOptions>>().Value;
        
        // If UI is not enabled, return
        if (!uiOptions.Enabled)
        {
            return app;
        }
        
        // Serve static files from the UI directory
        var uiPath = Path.Combine(env.ContentRootPath, uiOptions.PhysicalPath);
        
        if (Directory.Exists(uiPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uiPath),
                RequestPath = uiOptions.Path
            });
            
            // Configure SPA fallback
            app.UseWhen(
                context => context.Request.Path.StartsWithSegments(uiOptions.Path),
                appBuilder =>
                {
                    appBuilder.UseMiddleware<UIMiddleware>(uiOptions);
                });
        }
        
        return app;
    }
}
