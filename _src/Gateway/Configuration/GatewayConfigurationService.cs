using System.Text.Json;
using Data.Entities;
using Gateway.WebServer;
using Microsoft.Extensions.Options;
using Shared.Certificates;

namespace Gateway.Configuration;

public class GatewayConfigurationService
{
    // Event for configuration changes
    public delegate void ConfigurationChangedEventHandler(object sender, GatewayConfigurationOptions configuration);
    public event ConfigurationChangedEventHandler? ConfigurationChanged;

    private readonly ILogger<GatewayConfigurationService> _logger;
    private readonly IHostEnvironment _environment;
    private readonly EnvironmentOptions _environmentOptions;
    private readonly CertificateManager? _certificateManager;
    private readonly HostCertificateCache? _hostCertificateCache;
    private GatewayConfigurationOptions? _configuration;
    private readonly JsonSerializerOptions _jsonOptions;
    private string _configPath;

    public GatewayConfigurationService(
        ILogger<GatewayConfigurationService> logger,
        IHostEnvironment environment,
        IOptions<EnvironmentOptions>? environmentOptions = null,
        CertificateManager? certificateManager = null,
        HostCertificateCache? hostCertificateCache = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _environmentOptions = environmentOptions?.Value ?? new EnvironmentOptions();
        _certificateManager = certificateManager;
        _hostCertificateCache = hostCertificateCache;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        
        // Set the config path
        if (_environment.IsDevelopment())
        {
            _configPath = Path.Combine(Directory.GetCurrentDirectory(), "_config", "gateway-config.json");
        }
        else
        {
            _configPath = Path.Combine(_environmentOptions.ConfigPath, "gateway-config.json");
        }
    }

    public virtual async Task LoadConfigurationAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading gateway configuration from {ConfigPath}", _configPath);

        if (!File.Exists(_configPath))
        {
            _logger.LogWarning("Configuration file not found at {ConfigPath}", _configPath);
            _configuration = null;
            return;
        }

        try
        {
            var jsonString = await File.ReadAllTextAsync(_configPath, cancellationToken);
            _configuration = JsonSerializer.Deserialize<GatewayConfigurationOptions>(jsonString, _jsonOptions);
            
            if (_configuration == null)
            {
                _logger.LogError("Failed to deserialize configuration from {ConfigPath}", _configPath);
                return;
            }

            _logger.LogInformation("Successfully loaded gateway configuration with {HostCount} hosts and {RuleCount} proxy rules", 
                _configuration.Hosts.Count, _configuration.ProxyRules.Count);

            // Load certificates for all hosts
            await LoadCertificatesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading gateway configuration from {ConfigPath}", _configPath);
        }
    }

    public virtual async Task<bool> SaveConfigurationAsync(GatewayConfigurationOptions configuration, CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        try
        {
            _logger.LogInformation("Saving gateway configuration to {ConfigPath}", _configPath);
            
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Serialize the configuration to JSON
            var jsonString = JsonSerializer.Serialize(configuration, _jsonOptions);
            
            // Write the JSON to the file
            await File.WriteAllTextAsync(_configPath, jsonString, cancellationToken);
            
            // Update the in-memory configuration
            _configuration = configuration;
            
            // Notify subscribers of the configuration change
            OnConfigurationChanged(configuration);
            
            _logger.LogInformation("Gateway configuration saved successfully");
            
            // Load certificates for all hosts
            await LoadCertificatesAsync(cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving gateway configuration to {ConfigPath}", _configPath);
            return false;
        }
    }

    public virtual async Task<bool> UpdateConfigurationAsync(Action<GatewayConfigurationOptions> updateAction, CancellationToken cancellationToken = default)
    {
        if (_configuration == null)
        {
            _logger.LogError("Cannot update configuration because it has not been loaded");
            return false;
        }

        try
        {
            // Create a deep copy of the configuration
            var jsonString = JsonSerializer.Serialize(_configuration, _jsonOptions);
            var configCopy = JsonSerializer.Deserialize<GatewayConfigurationOptions>(jsonString, _jsonOptions);
            
            if (configCopy == null)
            {
                _logger.LogError("Failed to create a copy of the configuration");
                return false;
            }
            
            // Apply the update action to the copy
            updateAction(configCopy);
            
            // Save the updated configuration
            return await SaveConfigurationAsync(configCopy, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating gateway configuration");
            return false;
        }
    }

    protected virtual void OnConfigurationChanged(GatewayConfigurationOptions configuration)
    {
        ConfigurationChanged?.Invoke(this, configuration);
    }

    private async Task LoadCertificatesAsync(CancellationToken cancellationToken)
    {
        if (_configuration == null || _configuration.Hosts.Count == 0)
        {
            _logger.LogWarning("No hosts configured, skipping certificate loading");
            return;
        }

        if (_certificateManager == null)
        {
            _logger.LogWarning("Certificate manager not available, skipping certificate loading");
            return;
        }

        foreach (var host in _configuration.Hosts)
        {
            foreach (var hostName in host.HostNames)
            {
                try
                {
                    _logger.LogInformation("Loading certificate for host {HostName}", hostName);
                    
                    var certificate = new Certificate
                    {
                        Name = host.Certificate.Name,
                        CertificateSource = MapCertificateSource(host.Certificate.Source),
                        SubjectAlternativeNames = host.Certificate.SubjectAlternativeNames?.ToArray(),
                        KeyVaultUri = host.Certificate.KeyVaultUri,
                        KeyVaultCertificateName = host.Certificate.KeyVaultCertificateName,
                        KeyVaultCertificatePasswordName = host.Certificate.KeyVaultCertificatePasswordName,
                        FilePath = host.Certificate.FilePath,
                        FilePassword = host.Certificate.FilePassword
                    };

                    await _certificateManager.LoadCertificateAsync(certificate, cancellationToken, hostName);
                    _logger.LogInformation("Certificate loaded successfully for host {HostName}", hostName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading certificate for host {HostName}", hostName);
                }
            }
        }
    }

    private string MapCertificateSource(string source)
    {
        return source.ToLowerInvariant() switch
        {
            "inmemory" => CertificateSources.InMemory,
            "azurekeyvault" => CertificateSources.AzureKeyVault,
            "file" => CertificateSources.File,
            _ => CertificateSources.InMemory
        };
    }

    public virtual GatewayConfigurationOptions? GetConfiguration() => _configuration;
}
