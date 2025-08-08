using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Authentication;
using Graph.User.Mcp.Client.Commands;
using Graph.User.Mcp.Client.Services;
using Serilog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

// Configure Serilog bootstrap logger for early logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MCP Client application");

    // Display welcome message
    ConsoleHelpService.ShowWelcomeMessage();

    var builder = Host.CreateApplicationBuilder(args);

    // Add configuration (supports both appsettings.json and environment variables)
    builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
    builder.Configuration.AddEnvironmentVariables();

    // Configure Serilog with full configuration
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext());
    
    builder.Services.AddHttpClient()
        .ConfigureHttpClientDefaults(http =>
        {
            // Accept self-signed certificates for development
            http.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            });
        });

    // Register application services
    builder.Services.AddSingleton<WindowsHelloAuthService>();
    builder.Services.AddSingleton<McpApiClient>();

    var host = builder.Build();

    // Get required services
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    var auth = host.Services.GetRequiredService<WindowsHelloAuthService>();
    var config = host.Services.GetRequiredService<IConfiguration>();
    var mcpClient = host.Services.GetRequiredService<McpApiClient>();

    // Build gateway URL
    var gatewayUrl = config["Gateway:BaseUrl"] + config["Gateway:McpEndpoint"];

    logger.LogInformation("MCP Client started successfully");

    // Get user authentication
    var userToken = await auth.GetUserTokenAsync();
    if (userToken == null)
    {
        logger.LogError("Authentication failed");
        Console.WriteLine("❌ Authentication failed. Exiting.");
        return;
    }
    
    logger.LogInformation("User authenticated successfully: {Username}", userToken.Username);
    Console.WriteLine($"✅ {userToken.GetDisplayInfo()}");
    Console.WriteLine($"   User: {userToken.Username}");
    
    // Test connectivity
    var gatewayHealthy = await mcpClient.TestConnectivityAsync();
    var mcpHealthy = await mcpClient.TestMcpConnectivityAsync();
    
    ConsoleHelpService.ShowConnectionDiagnostics(gatewayUrl, gatewayHealthy, mcpHealthy);
    
    if (!gatewayHealthy)
    {
        logger.LogError("Gateway connectivity failed, exiting application");
        return; // Exit if gateway is not accessible
    }
    
    // Show architecture and available services
    ConsoleHelpService.ShowArchitectureInfo();
    ConsoleHelpService.ShowServicesOverview();
    
    logger.LogInformation("Starting interactive command loop");
    
    // Start interactive command loop
    while (true)
    {
        Console.Write("Command: ");
        var input = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
        {
            logger.LogInformation("User requested exit");
            break;
        }
            
        try
        {
            await CommandRouter.ExecuteCommandAsync(input.Trim(), userToken, mcpClient, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing command: {Command}", input);
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    Console.WriteLine("\n👋 Goodbye!");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Fatal error during application startup");
    Console.WriteLine($"❌ Fatal error: {ex.Message}");
}
finally
{
    Log.Information("Application shutting down");
    Log.CloseAndFlush();
}