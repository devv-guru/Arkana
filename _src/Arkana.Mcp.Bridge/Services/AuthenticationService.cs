using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace Arkana.Mcp.Bridge.Services;

public interface IAuthenticationService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TokenCredential _credential;
    private readonly string[] _scopes;
    private readonly string _tenantId;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        _tenantId = configuration["Authentication:TenantId"] 
            ?? throw new InvalidOperationException("Authentication:TenantId is required");
        
        var clientId = configuration["Authentication:ClientId"] 
            ?? throw new InvalidOperationException("Authentication:ClientId is required");
            
        _scopes = configuration.GetSection("Authentication:Scopes").Get<string[]>() 
            ?? ["api://arkana-gateway/.default"];

        _credential = CreateCredential(clientId, _tenantId);
    }

    private TokenCredential CreateCredential(string clientId, string tenantId)
    {
        var options = new DefaultAzureCredentialOptions
        {
            TenantId = tenantId
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _logger.LogInformation("Using Windows-specific authentication chain");
            return new ChainedTokenCredential(
                new ManagedIdentityCredential(),
                new VisualStudioCredential(new VisualStudioCredentialOptions { TenantId = tenantId }),
                new VisualStudioCodeCredential(new VisualStudioCodeCredentialOptions { TenantId = tenantId }),
                new AzureCliCredential(),
                new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions 
                { 
                    ClientId = clientId,
                    TenantId = tenantId 
                })
            );
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _logger.LogInformation("Using cross-platform authentication chain");
            return new ChainedTokenCredential(
                new ManagedIdentityCredential(),
                new AzureCliCredential(),
                new DeviceCodeCredential(new DeviceCodeCredentialOptions
                {
                    ClientId = clientId,
                    TenantId = tenantId,
                    DeviceCodeCallback = (code, cancellation) =>
                    {
                        Console.WriteLine();
                        Console.WriteLine("=== Azure Authentication Required ===");
                        Console.WriteLine($"Please visit: {code.VerificationUri}");
                        Console.WriteLine($"And enter code: {code.UserCode}");
                        Console.WriteLine("=====================================");
                        Console.WriteLine();
                        return Task.FromResult(0);
                    }
                })
            );
        }
        else
        {
            _logger.LogWarning("Unknown platform, using default credential chain");
            return new DefaultAzureCredential(options);
        }
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Requesting access token for scopes: {Scopes}", string.Join(", ", _scopes));
            
            var tokenRequest = new TokenRequestContext(_scopes);
            var tokenResult = await _credential.GetTokenAsync(tokenRequest, cancellationToken);
            
            _logger.LogDebug("Successfully obtained access token, expires at: {ExpiresOn}", tokenResult.ExpiresOn);
            return tokenResult.Token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain access token");
            throw;
        }
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await GetAccessTokenAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Authentication check failed");
            return false;
        }
    }
}