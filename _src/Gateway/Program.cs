using FluentValidation;
using Data.Contexts.SqLite;
using Data.Contexts.Base;
using Gateway.Services;
using Gateway.Endpoints;
using Gateway.Middleware;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json;

namespace Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/arkana-gateway-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting Arkana MCP Security Gateway");
            CreateApplication(args).Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static WebApplication CreateApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add local configuration support
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

        // Add Serilog
        builder.Host.UseSerilog();

        // Add services
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Configure pipeline
        ConfigurePipeline(app);

        // Ensure database is created
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SqLiteWriteOnlyContext>();
            context.Database.EnsureCreated();
        }

        return app;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

        // API Documentation
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { 
                Title = "Arkana MCP Gateway API", 
                Version = "v1",
                Description = "Enterprise MCP Security Gateway"
            });
        });

        // Database - SQLite for MVP
        services.AddDbContext<SqLiteWriteOnlyContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=arkana-gateway.db";
            options.UseSqlite(connectionString);
        });
        
        // Register the interface
        services.AddScoped<IWriteOnlyContext>(provider => 
            provider.GetRequiredService<SqLiteWriteOnlyContext>());

        // Azure AD Authentication
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = configuration["AzureAd:Instance"] + configuration["AzureAd:TenantId"] + "/v2.0";
                options.Audience = "api://0e8c11c8-1fd7-42ec-8532-e02711fa5b5b";
                options.RequireHttpsMetadata = false; // For development
                
                // Accept both v1.0 and v2.0 token issuers
                options.TokenValidationParameters.ValidIssuers = new[]
                {
                    $"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}/v2.0",
                    $"https://sts.windows.net/{configuration["AzureAd:TenantId"]}/"
                };
            });

        services.AddAuthorization();

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Health Checks
        services.AddHttpClient<Gateway.HealthChecks.McpServerHealthCheck>();
        services.AddHealthChecks()
            .AddDbContextCheck<SqLiteWriteOnlyContext>()
            .AddCheck<Gateway.HealthChecks.McpServerHealthCheck>("mcp_servers");

        // YARP Reverse Proxy with dynamic configuration
        services.AddSingleton<DynamicProxyConfigProvider>();
        services.AddSingleton<IProxyConfigProvider>(provider => 
            provider.GetRequiredService<DynamicProxyConfigProvider>());
        services.AddReverseProxy()
            .LoadFromMemory([], []);

        // Business Services
        services.AddScoped<IMcpServerService, McpServerService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPromptSafetyService, PromptSafetyService>();
        services.AddScoped<IGraphTokenService, GraphTokenService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();

        // Configure JSON options
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        // Development tools
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arkana MCP Gateway v1"));
        }

        // Security headers
        app.UseHsts();
        app.UseHttpsRedirection();

        // Routing (must come before Authorization)
        app.UseRouting();

        // Authentication & Authorization (must come after UseRouting)
        app.UseAuthentication();
        app.UseAuthorization();

        // Custom middleware - TODO: Fix scoped service resolution
        // app.UseMiddleware<PromptInjectionMiddleware>();
        app.UseMiddleware<GraphTokenExchangeMiddleware>();

        // Minimal API endpoints
        app.MapMcpServerEndpoints();
        app.MapUserEndpoints();
        app.MapConfigurationEndpoints();
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    Status = report.Status.ToString(),
                    Service = "Arkana MCP Gateway",
                    Version = "1.0.0",
                    Timestamp = DateTime.UtcNow,
                    TotalDuration = report.TotalDuration.TotalMilliseconds,
                    Checks = report.Entries.ToDictionary(
                        entry => entry.Key,
                        entry => new
                        {
                            Status = entry.Value.Status.ToString(),
                            Duration = entry.Value.Duration.TotalMilliseconds,
                            Description = entry.Value.Description,
                            Data = entry.Value.Data.Count > 0 ? entry.Value.Data : null,
                            Exception = entry.Value.Exception?.Message
                        })
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, 
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
        });

        // YARP Reverse Proxy (last in pipeline)
        app.MapReverseProxy();
    }
}