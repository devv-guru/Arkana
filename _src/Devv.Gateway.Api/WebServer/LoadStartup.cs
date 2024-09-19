using Devv.Gateway.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Api.WebServer;

public class LoadStartup : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<LoadStartup> _logger;

    public LoadStartup(IServiceScopeFactory serviceScopeFactory, ILogger<LoadStartup> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            try
            {
                await CreateDatabaseAsync(scope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying database migrations.");
            }
        }
    }

    private async Task CreateDatabaseAsync(IServiceScope scope)
    {
        _logger.LogInformation("Applying database migrations...");
        var context = scope.ServiceProvider.GetRequiredService<WriteOnlyContext>();
        await context.Database.MigrateAsync();
        _logger.LogInformation("Database migrations applied successfully.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping LoadStartup service...");
    }
}