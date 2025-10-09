using System.CommandLine;
using Arkana.Mcp.Bridge.Configuration;
using Arkana.Mcp.Bridge.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arkana.Mcp.Bridge;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Arkana MCP Bridge - Connect MCP clients to enterprise tools via Arkana Gateway");

        var gatewayUrlOption = new Option<string>(
            "--gateway-url",
            description: "The URL of the Arkana Gateway");

        var tenantIdOption = new Option<string>(
            "--tenant-id",
            description: "Azure AD Tenant ID");

        var clientIdOption = new Option<string>(
            "--client-id",
            description: "Azure AD Client ID");

        var verboseOption = new Option<bool>(
            "--verbose",
            description: "Enable verbose logging");

        rootCommand.AddOption(gatewayUrlOption);
        rootCommand.AddOption(tenantIdOption);
        rootCommand.AddOption(clientIdOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler(async (gatewayUrl, tenantId, clientId, verbose) =>
        {
            await RunBridgeAsync(args, gatewayUrl, tenantId, clientId, verbose);
        }, gatewayUrlOption, tenantIdOption, clientIdOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task RunBridgeAsync(
        string[] args,
        string? gatewayUrl,
        string? tenantId,
        string? clientId,
        bool verbose)
    {
        try
        {
            var host = CreateHostBuilder(args, gatewayUrl, tenantId, clientId, verbose).Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static IHostBuilder CreateHostBuilder(
        string[] args,
        string? gatewayUrl,
        string? tenantId,
        string? clientId,
        bool verbose)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables("ARKANA_");
                config.AddCommandLine(args);

                if (!string.IsNullOrEmpty(gatewayUrl))
                    config.AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Gateway:Url", gatewayUrl) });

                if (!string.IsNullOrEmpty(tenantId))
                    config.AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Authentication:TenantId", tenantId) });

                if (!string.IsNullOrEmpty(clientId))
                    config.AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Authentication:ClientId", clientId) });
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                
                if (verbose || context.Configuration.GetValue<bool>("Logging:EnableConsole", false))
                {
                    logging.AddConsole();
                }
                else
                {
                    logging.AddConsole(options =>
                    {
                        options.LogToStandardErrorThreshold = LogLevel.Warning;
                    });
                }
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<BridgeOptions>(context.Configuration.GetSection(BridgeOptions.SectionName));
                services.Configure<AuthenticationOptions>(context.Configuration.GetSection(AuthenticationOptions.SectionName));
                services.Configure<McpOptions>(context.Configuration.GetSection(McpOptions.SectionName));

                services.AddHttpClient<GatewayProxyService>();
                
                services.AddSingleton<IAuthenticationService, AuthenticationService>();
                services.AddSingleton<IGatewayProxyService, GatewayProxyService>();
                services.AddSingleton<IMcpProtocolService, McpProtocolService>();
                services.AddHostedService<BridgeHostedService>();
            })
            .UseConsoleLifetime();
    }
}

public class BridgeHostedService : BackgroundService
{
    private readonly ILogger<BridgeHostedService> _logger;
    private readonly IAuthenticationService _authService;
    private readonly IMcpProtocolService _mcpService;

    public BridgeHostedService(
        ILogger<BridgeHostedService> logger,
        IAuthenticationService authService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _authService = authService;
        _mcpService = serviceProvider.GetRequiredService<IMcpProtocolService>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting Arkana MCP Bridge");

            var isAuthenticated = await _authService.IsAuthenticatedAsync(stoppingToken);
            if (!isAuthenticated)
            {
                _logger.LogError("Failed to authenticate with Azure AD. Bridge cannot start.");
                return;
            }

            _logger.LogInformation("Authentication successful. Starting MCP protocol handler.");
            await _mcpService.StartAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in bridge service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Arkana MCP Bridge");
        await _mcpService.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}