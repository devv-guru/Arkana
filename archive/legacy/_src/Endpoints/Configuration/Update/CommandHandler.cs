using Endpoints.Configuration.Services;
using Mediator;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Endpoints.Configuration.Update;

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
        if (request.Configuration == null)
        {
            return new Response
            {
                Success = false,
                Message = "Configuration is required"
            };
        }

        try
        {
            // Convert the configuration model to JSON and then to the target object
            var json = JsonSerializer.Serialize(request.Configuration);
            var config = JsonSerializer.Deserialize(json, _configurationService.GetConfiguration()?.GetType() ?? typeof(object));

            if (config == null)
            {
                return new Response
                {
                    Success = false,
                    Message = "Failed to convert configuration"
                };
            }

            var result = await _configurationService.SaveConfigurationAsync(config, ct);

            return new Response
            {
                Success = result,
                Message = result ? "Configuration updated successfully" : "Failed to update configuration"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration");
            
            return new Response
            {
                Success = false,
                Message = $"Error updating configuration: {ex.Message}"
            };
        }
    }
}
