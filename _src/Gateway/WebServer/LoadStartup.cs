﻿﻿using Data.Contexts.Base;
using Data.Entities;
using Gateway.Configuration;
using Gateway.Proxy;
using Microsoft.EntityFrameworkCore;
using Shared.Certificates;

namespace Gateway.WebServer;

public class LoadStartup : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<LoadStartup> _logger;
    private readonly IConfiguration _configuration;
    private readonly GatewayConfigurationService _gatewayConfigurationService;
    private readonly YarpConfigurationService _yarpConfigurationService;

    public LoadStartup(
        IServiceScopeFactory serviceScopeFactory, 
        ILogger<LoadStartup> logger,
        IConfiguration configuration,
        GatewayConfigurationService gatewayConfigurationService,
        YarpConfigurationService yarpConfigurationService)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _configuration = configuration;
        _gatewayConfigurationService = gatewayConfigurationService;
        _yarpConfigurationService = yarpConfigurationService;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
            try
            {
                await CreateDatabaseAsync(scope, ct);
                await LoadDefaultsAsync(scope, ct);
                
                // Load the gateway configuration
                await _gatewayConfigurationService.LoadConfigurationAsync(ct);
                
                // Update the YARP configuration
                _yarpConfigurationService.UpdateConfig();
                
                _logger.LogInformation("Gateway configuration loaded and YARP configuration updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during startup.");
            }
        }
    }

    private async Task CreateDatabaseAsync(IServiceScope scope, CancellationToken ct)
    {
        _logger.LogInformation("Applying database migrations...");
        var context = scope.ServiceProvider.GetRequiredService<IWriteOnlyContext>();
        await context.Database.MigrateAsync(ct);
        _logger.LogInformation("Database migrations applied successfully.");
    }

    private async Task LoadDefaultsAsync(IServiceScope scope, CancellationToken ct)
    {
        _logger.LogInformation("Initializing gateway with saved hosts and certificates ...");
        var readonlyContext = scope.ServiceProvider.GetRequiredService<IReadOnlyContext>();

        _logger.LogInformation("Checking for default certificate...");
        var certificate = await readonlyContext.Certificates
            .FirstOrDefaultAsync(w => w.IsDefault, ct);
        if (certificate is null)
            certificate = await CreateDefaultCertificateAsync(scope, ct);

        var host = await readonlyContext.WebHosts
            .FirstOrDefaultAsync(w => w.IsDefault, ct);
        if (host is null)
            await CreateDefaultHostAsync(scope, host, certificate, ct);

        _logger.LogInformation("Loading saved hosts and certificates...");
        var certificateManager = scope.ServiceProvider.GetRequiredService<CertificateManager>();
        var hosts = await readonlyContext.WebHosts
            .Include(w => w.Certificate)
            .ToArrayAsync(ct);

        _logger.LogInformation("Found {count} hosts to load at startup...", hosts.Length);
        foreach (var h in hosts)
        {
            _logger.LogInformation("Loading certificate for host {HostName}...", h.HostName);
            await certificateManager.LoadCertificateAsync(h.Certificate, ct, h.HostName);
            _logger.LogInformation("Certificate loaded successfully for host {HostName}.", h.HostName);
        }

        _logger.LogInformation("Default data loaded successfully.");
    }

    private async Task<Certificate> CreateDefaultCertificateAsync(IServiceScope scope, CancellationToken ct)
    {
        _logger.LogInformation("Creating default certificate...");
        var writeContext = scope.ServiceProvider.GetRequiredService<IWriteOnlyContext>();

        var certificate = new Certificate
        {
            Name = "Default Certificate",
            CertificateSource = CertificateSources.InMemory,
            IsDefault = true
        };

        writeContext.Certificates.Add(certificate);
        await writeContext.SaveChangesAsync(ct);
        _logger.LogInformation("Default certificate created successfully.");
        return certificate;
    }

    private async Task CreateDefaultHostAsync(IServiceScope scope, WebHost? host, Certificate certificate,
        CancellationToken ct)
    {
        _logger.LogInformation("Creating default host...");
        var writeContext = scope.ServiceProvider.GetRequiredService<IWriteOnlyContext>();

        host = new WebHost
        {
            Name = "Default Host",
            IsDefault = true,
            CertificateId = certificate.Id,
            HostName = _configuration["GatewayOptions:ManagementDomain"]
        };

        writeContext.WebHosts.Add(host);
        await writeContext.SaveChangesAsync(ct);
        _logger.LogInformation("Default host created successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping LoadStartup service...");
        return Task.CompletedTask;
    }
}
