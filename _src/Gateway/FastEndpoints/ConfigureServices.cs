using FastEndpoints;
using FastEndpoints.Swagger;
using System.Text.Json;

namespace Gateway.FastEndpoints;

public static class ConfigureServices
{
    public static IServiceCollection AddGatewayFastEndpoints(this IServiceCollection services)
    {
        services.AddFastEndpoints()
            .SwaggerDocument(o =>
            {
                o.AutoTagPathSegmentIndex = 0;
                o.ShortSchemaNames = true;
                o.RemoveEmptyRequestSchema = true;
                o.DocumentSettings = s =>
                {
                    s.Title = "Devv Gateway API";
                    s.Version = "v1";
                };
            })
            .AddAuthorization();

        return services;
    }

    public static IApplicationBuilder UseGatewayFastEndpoints(this WebApplication? app, IConfiguration configuration)
    {
        app.UseFastEndpoints(c =>
            {
                c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                c.Endpoints.Configurator = ep =>
                {
                    var allowedDomain = configuration["GatewayOptions:ManagementDomain"];

                    if (string.IsNullOrWhiteSpace(allowedDomain) || !ep.Routes[0].StartsWith("/gateway"))
                        return;

                    ep.Options(b => b.RequireHost(allowedDomain));
                    ep.Description(b => b.Produces<ErrorResponse>(400, "application/problem+json"));
                };
            })
            .UseSwaggerGen();

        return app;
    }
}