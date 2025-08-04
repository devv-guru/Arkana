using FluentValidation;
using Gateway.Data;
using Gateway.Services;
using Gateway.Endpoints;
using Gateway.Middleware;
using Microsoft.EntityFrameworkCore;
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
            var context = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
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
        services.AddDbContext<GatewayDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=arkana-gateway.db";
            options.UseSqlite(connectionString);
        });

        // JWT Authentication
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = configuration["Authentication:Jwt:Authority"];
                options.Audience = configuration["Authentication:Jwt:Audience"];
                options.RequireHttpsMetadata = false; // For development
            });

        services.AddAuthorization();

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<GatewayDbContext>();

        // YARP Reverse Proxy
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        // Business Services
        services.AddScoped<IMcpServerService, McpServerService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPromptSafetyService, PromptSafetyService>();

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

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Custom middleware - TODO: Fix scoped service resolution
        // app.UseMiddleware<PromptInjectionMiddleware>();

        // Routing
        app.UseRouting();

        // Minimal API endpoints
        app.MapMcpServerEndpoints();
        app.MapUserEndpoints();
        app.MapHealthChecks("/health");

        // YARP Reverse Proxy (last in pipeline)
        app.MapReverseProxy();
    }
}