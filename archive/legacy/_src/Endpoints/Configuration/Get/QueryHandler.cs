using Endpoints.Configuration.Services;
using Mediator;

namespace Endpoints.Configuration.Get;

public class QueryHandler : IQueryHandler<Request, Response?>
{
    private readonly IConfigurationService _configurationService;

    public QueryHandler(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public ValueTask<Response?> Handle(Request request, CancellationToken ct)
    {
        var config = _configurationService.GetConfiguration();
        if (config == null)
        {
            return ValueTask.FromResult<Response?>(null);
        }

        return ValueTask.FromResult<Response?>(new Response
        {
            Configuration = Mapper.FromGatewayConfig(config)
        });
    }
}
