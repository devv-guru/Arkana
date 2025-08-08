using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Graph.User.Mcp.Server.Configuration;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecuritySettings(this IServiceCollection services, IConfiguration configuration)
    {
        // Rate limiting
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("MCP", limiterOptions =>
            {
                limiterOptions.PermitLimit = configuration.GetValue<int>("RateLimit:PermitLimit", 100);
                limiterOptions.Window = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimit:WindowMinutes", 1));
                limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = configuration.GetValue<int>("RateLimit:QueueLimit", 10);
            });
            
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
            };
        });

        // Forward headers for proxy scenarios
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static IApplicationBuilder UseSecurityMiddleware(this WebApplication app)
    {
        // Security headers
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Only add HSTS in production with HTTPS
            if (app.Environment.IsProduction() && context.Request.IsHttps)
            {
                context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }

            await next();
        });

        // Rate limiting
        app.UseRateLimiter();

        // Forward headers
        app.UseForwardedHeaders();

        return app;
    }

    public static IEndpointRouteBuilder MapSecureDiagnostics(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var enableDiagnostics = configuration.GetValue<bool>("Features:EnableDiagnostics", false);
        
        if (enableDiagnostics)
        {
            endpoints.MapGet("/diagnostics", async (HttpContext context) =>
            {
                // Require specific API key for diagnostics in production
                var apiKey = configuration.GetValue<string>("Diagnostics:ApiKey");
                var providedKey = context.Request.Headers["X-Diagnostics-Key"].FirstOrDefault();
                
                if (string.IsNullOrEmpty(apiKey) || providedKey != apiKey)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }

                var startTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
                var uptime = DateTime.UtcNow - startTime;
                
                return Results.Json(new
                {
                    Service = "Microsoft Graph MCP Server",
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    Timestamp = DateTime.UtcNow,
                    Uptime = uptime.ToString(@"dd\.hh\:mm\:ss"),
                    ProcessId = Environment.ProcessId,
                    MachineName = Environment.MachineName,
                    WorkingSet = $"{Environment.WorkingSet / 1024 / 1024} MB",
                    GcMemory = $"{GC.GetTotalMemory(false) / 1024 / 1024} MB",
                    AvailableTools = new[]
                    {
                        "get_my_profile", "list_users", "list_groups", 
                        "get_group_members", "search_users", "get_my_messages", "get_my_events"
                    }
                });
            })
            .WithName("SecureDiagnosticsEndpoint")
            .RequireRateLimiting("MCP");
        }

        return endpoints;
    }
}