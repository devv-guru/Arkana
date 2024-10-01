namespace Gateway.WebServer;

public static class ConfigureServices
{
    public static ConfigureWebHostBuilder ConfigureWebServer(this ConfigureWebHostBuilder builder)
    {
        builder.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.ListenAnyIP(8080);
            options.ListenAnyIP(8081,
                listenOptions =>
                {
                    var hostCache = options.ApplicationServices.GetRequiredService<HostCertificateCache>();
                    listenOptions.UseHttps(httpsOptions =>
                    {
                        httpsOptions.ServerCertificateSelector =
                            (context, hostName) => hostCache.GetCertificate(hostName);
                    });
                });
        });

        return builder;
    }

    public static IServiceCollection AddGatewayHttpsRedirection(this IServiceCollection services)
    {
        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            options.HttpsPort = 8081;
        });

        return services;
    }
}