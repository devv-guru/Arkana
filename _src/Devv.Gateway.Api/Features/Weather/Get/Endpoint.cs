using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;


namespace Devv.Gateway.Api.Features.Weather.Get;

public class Endpoint : EndpointWithoutRequest<Results<Ok<Response[]>, NotFound, ProblemDetails>>
{
    public override void Configure()
    {
        Get("/weather");
        Group<GatewayGroup>();
        AllowAnonymous();
    }

    public override async Task<Results<Ok<Response[]>, NotFound, ProblemDetails>> ExecuteAsync(CancellationToken ct)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var weather = Enumerable.Range(1, 5).Select(index => new Response
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
            .ToArray();

        return TypedResults.Ok(weather);
    }
}