using Devv.WebServer.Api.Data;
using Devv.WebServer.Api.Data.Contexts;

namespace Devv.WebServer.Api.WebServer;

public class ServerInitializer : IHostedService
{
    private readonly ILogger<ServerInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ServerInitializer(ILogger<ServerInitializer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServerInitializer StartAsync");

        await using (var scope = _serviceProvider.CreateAsyncScope())
        {
            _logger.LogInformation("ServerInitializer StartAsync scope");
            
            var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}