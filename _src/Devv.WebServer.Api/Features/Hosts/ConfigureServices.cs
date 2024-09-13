namespace Devv.WebServer.Api.Features.Hosts;

public static class ConfigureServices
{
    public static async Task ConfigureCertificates(this WebApplicationBuilder builder,
        CertificateManager certificateManager)
    {
        var certificateOptions = new CertificateOptions();
        builder.Configuration.GetSection(CertificateOptions.SectionName).Bind(certificateOptions);

        if (certificateOptions?.Certificates == null || !certificateOptions.Certificates.Any())
        {
            await certificateManager.AddOrUpdateCertificateAsync(new CertificateSettings
            {
                HostName = "localhost",
                CertificateSource = CertificateSources.Fallback,
                Location = "Generated",
                Password = null
            });
        }
        else
        {
            foreach (var certSettings in certificateOptions.Certificates)
            {
                await certificateManager.AddOrUpdateCertificateAsync(certSettings);
            }
        }
    }
}