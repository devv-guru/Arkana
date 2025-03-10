using System.Threading;
using System.Threading.Tasks;
using Gateway.Configuration;
using Gateway.WebServer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gateway.Tests.Helpers;

public class TestGatewayConfigurationService : GatewayConfigurationService
{
    private readonly GatewayConfigurationOptions? _configuration;
    private readonly bool _fileExists;

    public TestGatewayConfigurationService(
        ILogger<GatewayConfigurationService> logger,
        IHostEnvironment environment,
        GatewayConfigurationOptions? configuration = null,
        bool fileExists = true)
        : base(logger, environment)
    {
        _configuration = configuration;
        _fileExists = fileExists;
    }

    public override async Task LoadConfigurationAsync(CancellationToken cancellationToken = default)
    {
        if (!_fileExists)
        {
            // Simulate file not found
            SetConfiguration(null);
            return;
        }

        // Simulate file found and loaded
        SetConfiguration(_configuration);
        await Task.CompletedTask;
    }

    public override GatewayConfigurationOptions? GetConfiguration() => _configuration;

    // Helper method to set the configuration directly
    public void SetConfiguration(GatewayConfigurationOptions? configuration)
    {
        // Use reflection to set the private field
        var field = typeof(GatewayConfigurationService).GetField("_configuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(this, configuration);
    }
}
