using Devv.WebServer.Api.Configuration;
using Devv.WebServer.Api.Data;
using Devv.WebServer.Api.FastEndpoints;
using Devv.WebServer.Api.Features.Hosts;
using Devv.WebServer.Api.Logging;

namespace Devv.WebServer.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var host = builder.Host;
        var configuration = builder.Configuration;

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

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseGatewayFastEndpoints(configuration);
        app.UseProxy();

        await app.RunAsync();
    }
}