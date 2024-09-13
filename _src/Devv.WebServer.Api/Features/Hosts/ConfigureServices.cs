using Microsoft.Extensions.Options;

namespace Devv.WebServer.Api.Features.Hosts;

public static class ConfigureServices
{
    public static async Task<WebApplication> UseDynamicWebServer(this WebApplication app)
    {
        var certificateOptions = app.Services.GetRequiredService<IOptions<CertificateOptions>>().Value;
        var certificateManager = app.Services.GetRequiredService<CertificateManager>();

        foreach (var certSettings in certificateOptions.Certificates)
        {
            await certificateManager.AddOrUpdateCertificateAsync(certSettings);
        }

        return app;
    }
}