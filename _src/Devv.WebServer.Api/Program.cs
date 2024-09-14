using System.Text.Json;
using Devv.WebServer.Api.Configuration;
using Devv.WebServer.Api.Features.Hosts;
using Devv.WebServer.Api.Logging;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.Azure;

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
        builder.Services.AddScoped<IHostCache, HostCache>();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.ListenAnyIP(8080);
            options.ListenAnyIP(8081,
                listenOptions =>
                {
                    var hostCache = options.ApplicationServices.GetRequiredService<IHostCache>();
                    listenOptions.UseHttps(httpsOptions =>
                    {
                        httpsOptions.ServerCertificateSelector =
                            (context, hostName) => hostCache.GetCertificate(hostName);
                    });
                });
        });

        builder.Services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            options.HttpsPort = 8081;
        });

        builder.Services.AddFastEndpoints()
            .SwaggerDocument()
            .AddAuthorization();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseFastEndpoints(c =>
            {
                c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                c.Endpoints.Configurator = ep =>
                {
                    var allowedDomain = configuration["Server:ManagementDomain"];

                    if (string.IsNullOrWhiteSpace(allowedDomain) || !ep.Routes[0].StartsWith("/api"))
                        return;

                    ep.Options(b => b.RequireHost(allowedDomain));
                    ep.Description(b => b.Produces<ErrorResponse>(400, "application/problem+json"));
                };
            })
            .UseSwaggerGen();

        await app.RunAsync();
    }
}