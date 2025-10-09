using Endpoints.Configuration.Services;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Endpoints.Configuration.Reload;

public class CommandHandler : ICommandHandler<Request, Response>
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        IConfigurationService configurationService,
        ILogger<CommandHandler> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    public async ValueTask<Response> Handle(Request request, CancellationToken ct)
    {
        try
        {
            await _configurationService.ReloadConfigurationAsync(ct);
            
            return new Response
            {
                Success = true,
                Message = "Configuration reloaded successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading configuration");
            
            return new Response
            {
                Success = false,
                Message = $"Error reloading configuration: {ex.Message}"
            };
        }
    }
}
