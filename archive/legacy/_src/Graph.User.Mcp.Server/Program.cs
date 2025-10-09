using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Implementations;
using Graph.User.Mcp.Server.Middleware;
using Graph.User.Mcp.Server.HealthChecks;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add local configuration support
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// Configure options
builder.Services.Configure<McpConfiguration>(
    builder.Configuration.GetSection(McpConfiguration.SectionName));

// Add MCP server with HTTP transport
builder.Services
    .AddMcpServer()
    .WithHttpTransport(o => o.Stateless = true)
    .WithToolsFromAssembly(); // This will discover our new tool classes

// Add production security features
builder.Services.AddSecuritySettings(builder.Configuration);

// Add Microsoft Graph services
builder.Services.AddHttpClient<GraphUserService>();
builder.Services.AddHttpClient<GraphGroupService>();
builder.Services.AddHttpClient<GraphMailService>();
builder.Services.AddHttpClient<GraphCalendarService>();
builder.Services.AddHttpClient<GraphFilesService>();
builder.Services.AddHttpClient<GraphTasksService>();
builder.Services.AddHttpClient<GraphContactsService>();
builder.Services.AddHttpClient<GraphNotesService>();
builder.Services.AddHttpClient<GraphPresenceService>();

builder.Services.AddScoped<IGraphUserService, GraphUserService>();
builder.Services.AddScoped<IGraphGroupService, GraphGroupService>();
builder.Services.AddScoped<IGraphMailService, GraphMailService>();
builder.Services.AddScoped<IGraphCalendarService, GraphCalendarService>();
builder.Services.AddScoped<IGraphFilesService, GraphFilesService>();
builder.Services.AddScoped<IGraphTasksService, GraphTasksService>();
builder.Services.AddScoped<IGraphContactsService, GraphContactsService>();
builder.Services.AddScoped<IGraphNotesService, GraphNotesService>();
builder.Services.AddScoped<IGraphPresenceService, GraphPresenceService>();

// Add cross-cutting concerns
builder.Services.AddHttpContextAccessor();

// Enhanced logging with structured logging support
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}

// Production health checks with enhanced monitoring
builder.Services.AddHealthChecks()
    .AddCheck<McpServerHealthCheck>("mcp_server");

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var options = host.Services.GetRequiredService<IOptions<McpConfiguration>>().Value;

// Configure middleware pipeline
host.UseSecurityMiddleware();

// Add global exception handling
host.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Configure endpoints
host.MapMcp("/mcp").RequireRateLimiting("MCP");

// Enhanced health checks with detailed reporting
host.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var startTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
        var uptime = DateTime.UtcNow - startTime;

        var response = new
        {
            Status = report.Status.ToString(),
            Service = "Microsoft Graph MCP Server (Refactored & Production-Ready)",
            Version = "2.0.0",
            Architecture = "Modular services with focused responsibilities",
            Environment = host.Environment.EnvironmentName,
            Timestamp = DateTime.UtcNow,
            Uptime = uptime.TotalSeconds > 0 ? uptime.ToString(@"dd\.hh\:mm\:ss") : "Recently Started",
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    Status = entry.Value.Status.ToString(),
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Description = entry.Value.Description,
                    Data = entry.Value.Data.Count > 0 ? entry.Value.Data : null
                }),
            Configuration = new
            {
                RateLimitingEnabled = true,
                SecurityHeadersEnabled = true,
                StructuredLoggingEnabled = options.Logging.EnableStructuredLogging,
                ResiliencePatternsEnabled = true,
                EnabledTools = options.Features.EnabledTools
            },
            ToolArchitecture = new
            {
                UserTools =
                    "get_my_profile, list_users, search_users, get_user_by_id, get_my_manager, get_my_direct_reports",
                GroupTools =
                    "list_groups, get_group_by_id, get_group_members, get_group_owners, search_groups, get_my_group_memberships",
                Services = new[]
                {
                    "IGraphUserService - User operations",
                    "IGraphGroupService - Group operations", 
                    "IGraphMailService - Mail operations",
                    "IGraphCalendarService - Calendar operations",
                    "IGraphFilesService - OneDrive operations",
                    "IGraphTasksService - Tasks and planning operations",
                    "IGraphContactsService - Contacts operations",
                    "IGraphNotesService - OneNote operations",
                    "IGraphPresenceService - Teams presence operations",
                    "GraphServiceBase - Common functionality with resilience"
                }
            }
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            }));
    }
});

// Secure diagnostics endpoint (production-ready)
host.MapSecureDiagnostics(host.Configuration);

// Add a simple info endpoint for testing
host.MapGet("/", () => new
{
    Service = "Microsoft Graph MCP Server (Refactored & Production-Ready)",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Version = "2.0.0",
    Architecture = new
    {
        ToolOrganization = "Domain-specific tool classes (UserTools, GroupTools)",
        ServiceArchitecture = "Focused services with single responsibilities",
        SecurityModel = "Production-ready with rate limiting, headers, exception handling",
        ResiliencePatterns = "Retry policies, circuit breakers, correlation tracking"
    },
    Environment = host.Environment.EnvironmentName,
    Features = new
    {
        SecurityHeaders = "Enabled",
        RateLimiting = "Enabled (100 req/min)",
        GlobalExceptionHandling = "Enabled",
        StructuredLogging = "Enabled",
        ResiliencePatterns = "Enabled (3 retries, exponential backoff)",
        CorrelationTracking = "Enabled",
        ModularServices = "Enabled"
    },
    Endpoints = new
    {
        Health = "/health",
        MCP = "/mcp",
        Diagnostics = "/diagnostics (secured)",
        Info = "/"
    }
}).RequireRateLimiting("MCP");

// Production startup logging
logger.LogInformation("🚀 Microsoft Graph MCP Server (Refactored & Production-Ready) starting");
logger.LogInformation("📊 Environment: {Environment}", host.Environment.EnvironmentName);
logger.LogInformation("🏗️  Architecture: Modular services with focused responsibilities");
logger.LogInformation("🔧 Tool Organization: Domain-specific classes (UserTools, GroupTools)");
logger.LogInformation("⚙️  Services: 9 Microsoft Graph services (User, Group, Mail, Calendar, Files, Tasks, Contacts, Notes, Presence) with GraphServiceBase");
logger.LogInformation("🔒 Security features: Rate limiting, security headers, global exception handling");
logger.LogInformation("🔄 Resilience patterns: Retry policies, circuit breakers, timeouts");
logger.LogInformation("📝 Logging: Structured logging with correlation tracking");
logger.LogInformation("📋 Health check available at: /health");
logger.LogInformation("🔧 Diagnostics available at: /diagnostics (API key required in production)");
logger.LogInformation("ℹ️  Server info available at: /");
logger.LogInformation("🛡️  MCP tools protected with X-ARK-TOKEN validation");

// Log configuration summary
logger.LogInformation(
    "⚙️  Configuration: Rate limit: {RateLimit} req/min, Graph timeout: {Timeout}s, Max retries: {MaxRetries}",
    100, // Default from config
    options.Performance.GraphApiTimeoutSeconds,
    options.Performance.MaxRetryAttempts);

await host.RunAsync();