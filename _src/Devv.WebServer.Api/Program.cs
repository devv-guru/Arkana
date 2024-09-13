using System.Text.Json;
using Devv.WebServer.Api.Features.Hosts;
using FastEndpoints;
using FastEndpoints.Swagger;

namespace Devv.WebServer.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var certificateManager = new CertificateManager();
        await builder.ConfigureCertificates(certificateManager);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(8080);
            options.ListenAnyIP(8081,
                listenOptions =>
                {
                    listenOptions.UseHttps(httpsOptions =>
                    {
                        httpsOptions.ServerCertificateSelector = (context, hostName) =>
                        {
                            return certificateManager.GetCertificate(hostName);
                        };
                    });
                });
        });

        builder.Services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            options.HttpsPort = 8081;
        });

        builder.Services.AddSingleton(certificateManager);

        var configuration = builder.Configuration;

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
                    var allowedDomain = configuration["Server:VerifiedDomain"];
                    if (ep.Routes[0].StartsWith("/api") &&
                        !string.IsNullOrWhiteSpace(allowedDomain))
                    {
                        ep.Options(b => b.RequireHost(allowedDomain));
                        ep.Description(b => b.Produces<ErrorResponse>(400, "application/problem+json"));
                    }
                };
            })
            .UseSwaggerGen();

        await app.RunAsync();
    }
}