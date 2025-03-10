using Endpoints;
using Gateway.Configuration;
using Gateway.Data;
using Gateway.Logging;
using Gateway.Metrics;
using Gateway.Proxy;
using Gateway.UI;
using Gateway.WebServer;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

namespace Gateway;

public class Program
{
    public static async Task Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        var host = builder.Host;

        // Clear the default configuration sources
        builder.Configuration.Sources.Clear();
#if DEBUG
        builder.Configuration.AddJsonFile("appsettings.Development.json", false, true);
#else
        builder.Configuration.SetBasePath(Environment.GetEnvironmentVariable("CONFIG_PATH"))
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables();
#endif
        var configuration = builder.Configuration;

        var envOptions = new EnvironmentOptions
        {
            CertificatePath = !string.IsNullOrEmpty(configuration["CERT_PATH"]) ? configuration["CERT_PATH"] : "/etc/app/certs",
            LogsPath = !string.IsNullOrEmpty(configuration["LOG_PATH"]) ? configuration["LOG_PATH"] : "/var/log/app",
            ConfigPath = !string.IsNullOrEmpty(configuration["CONFIG_PATH"]) ? configuration["CONFIG_PATH"] : "/etc/app-config",
            StaticFilesPath = !string.IsNullOrEmpty(configuration["STATIC_CONTENT_PATH"]) ? configuration["STATIC_CONTENT_PATH"] : "/var/www/app/static",
            DataPath = !string.IsNullOrEmpty(configuration["DATA_PATH"]) ? configuration["DATA_PATH"] : "/var/lib/app/data",
        };

        // Ensure all paths are created
        Directory.CreateDirectory(envOptions.CertificatePath);
        Directory.CreateDirectory(envOptions.LogsPath);
        Directory.CreateDirectory(envOptions.ConfigPath);
        Directory.CreateDirectory(envOptions.StaticFilesPath);
        Directory.CreateDirectory(envOptions.DataPath);

        builder.Services.Configure<EnvironmentOptions>(_ =>
        {
            _.CertificatePath = envOptions.CertificatePath;
            _.LogsPath = envOptions.LogsPath;
            _.ConfigPath = envOptions.ConfigPath;
            _.StaticFilesPath = envOptions.StaticFilesPath;
            _.DataPath = envOptions.DataPath;
        });

        host.AddLogging(builder.Configuration);

        builder.Services.AddLazyCache();
        builder.Services.AddSingleton<HostCertificateCache>();
        builder.Services.AddSingleton<CertificateManager>();
        builder.WebHost.ConfigureWebServer();
        builder.AddDataContext(configuration);
        builder.Services.AddGatewayHttpsRedirection();
        builder.Services.AddGatewayFastEndpoints();
        builder.Services.AddProxy(configuration);
        builder.Services.AddConfiguration();
        builder.Services.AddMetrics(configuration);
        builder.Services.AddUI(configuration);
        
        builder.Services.AddHostedService<LoadStartup>();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        app.UseWhen(context => !IsStaticFileRequest(context), appBuilder => { appBuilder.UseHttpsRedirection(); });
        app.UseAuthorization();
        // Serve static files from the path
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(envOptions.StaticFilesPath),
            RequestPath = $"/{EnvironmentOptions.StaticRequestPath}"
        });

        app.UseMetricsMiddleware();
        app.UseGatewayFastEndpoints(configuration);
        app.UseProxy();
        app.UseUI(builder.Environment);

        await app.RunAsync();
    }

    private static bool IsStaticFileRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments($"/{EnvironmentOptions.StaticRequestPath}")
               && context.Request.Method == "GET"
               && !context.Request.IsHttps;
    }
}
