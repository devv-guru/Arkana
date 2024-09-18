using Devv.WebServer.Api.Configuration;
using Devv.WebServer.Api.Data;
using Devv.WebServer.Api.FastEndpoints;
using Devv.WebServer.Api.Logging;
using Devv.WebServer.Api.Proxy;
using Devv.WebServer.Api.WebServer;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Devv.WebServer.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var host = builder.Host;
        var configuration = builder.Configuration;

        builder.Services.Configure<EnvironmentOptions>(config =>
        {
            config.CertificatePath = Environment.GetEnvironmentVariable("CERT_PATH") ?? "/etc/app/certs";
            config.LogsPath = Environment.GetEnvironmentVariable("LOGS_PATH") ?? "/var/log/app";
            config.ConfigPath = Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "/etc/app-config";
            config.StaticFilesPath = Environment.GetEnvironmentVariable("STATIC_CONTENT_PATH") ?? "/var/www/app/static";
        });

        host.AddLogging(configuration);
        configuration.AddConfigurationSources(configuration, args);
        builder.Services.AddLazyCache();
        builder.Services.AddScoped<HostCertificateCache>();
        builder.WebHost.ConfigureWebServer();
        builder.Services.AddDataContext(configuration);
        builder.Services.AddGatewayHttpsRedirection();
        builder.Services.AddGatewayFastEndpoints();
        builder.Services.AddProxy();

        var app = builder.Build();

        var envOptions = app.Services.GetRequiredService<IOptions<EnvironmentOptions>>().Value;

        app.UseWhen(context => !IsStaticFileRequest(context), appBuilder => { appBuilder.UseHttpsRedirection(); });
        app.UseAuthorization();
        // Serve static files from the path
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(envOptions.StaticFilesPath),
            RequestPath = EnvironmentOptions.StaticRequestPath
        });

        app.UseGatewayFastEndpoints(configuration);
        app.UseProxy();

        await app.RunAsync();
    }

    private static bool IsStaticFileRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments($"/{EnvironmentOptions.StaticRequestPath}")
               && context.Request.Method == "GET"
               && !context.Request.IsHttps;
    }
}