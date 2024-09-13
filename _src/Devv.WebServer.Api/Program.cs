using System.Text.Json;
using Devv.WebServer.Api.Features.Hosts;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.Extensions.Options;

namespace Devv.WebServer.Api;

public class Program
{
    public async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var certificateManager = new CertificateManager();
        var certificateOptions = new CertificateOptions();
        builder.Configuration.GetSection(CertificateOptions.SectionName).Bind(certificateOptions);

        foreach (var certSettings in certificateOptions.Certificates)
        {
            await certificateManager.AddOrUpdateCertificateAsync(certSettings);
        }

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(80);
            options.ListenAnyIP(443,
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

        builder.Services.AddSingleton(certificateManager);

        builder.Services.Configure<CertificateOptions>(
            builder.Configuration.GetSection(CertificateOptions.SectionName));

        var configuration = builder.Configuration;

        builder.Services.AddFastEndpoints()
            .SwaggerDocument()
            .AddAuthorization();

        var app = builder.Build();

        builder.Services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            options.HttpsPort = 443;
        });

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

        app.Run();
    }
}