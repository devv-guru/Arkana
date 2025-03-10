using Endpoints.Metrics.Services;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Endpoints.Metrics.GetRequest;

public class QueryHandler : IQueryHandler<Request, Response>
{
    private readonly IMetricsServiceAdapter _metricsService;
    private readonly ILogger<QueryHandler> _logger;

    public QueryHandler(
        IMetricsServiceAdapter metricsService,
        ILogger<QueryHandler> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    public async ValueTask<Response> Handle(Request req, CancellationToken ct)
    {
        try
        {
            var metrics = await _metricsService.GetLatestRequestMetricsAsync(req.Count, ct);
            
            return new Response
            {
                Metrics = metrics.Select(m => m.ToDto()).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request metrics");
            return new Response();
        }
    }
}
