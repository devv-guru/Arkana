using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Graph.User.Mcp.Client.Models;

namespace Graph.User.Mcp.Client.Authentication;

/// <summary>
/// Windows Hello authentication service using Windows APIs and Azure AD
/// </summary>
public class WindowsHelloAuthService
{
    private readonly ILogger<WindowsHelloAuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes;
    
    public WindowsHelloAuthService(ILogger<WindowsHelloAuthService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Load configuration values
        var clientId = _configuration["AzureAd:ClientId"] ?? throw new InvalidOperationException("AzureAd:ClientId not configured");
        var tenantId = _configuration["AzureAd:TenantId"] ?? throw new InvalidOperationException("AzureAd:TenantId not configured");
        var instance = _configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
        var apiScope = _configuration["Gateway:ApiScope"] ?? throw new InvalidOperationException("Gateway:ApiScope not configured");
        
        _scopes = new[] { apiScope };
        
        _logger.LogInformation("Initializing MSAL with ClientId: {ClientId}, TenantId: {TenantId}", clientId, tenantId);
        
        // Initialize MSAL Public Client Application for device/Windows Hello authentication
        _app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority(instance + tenantId)
            .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
            .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
            {
                Title = "Arkana MCP Client Authentication",
                ListOperatingSystemAccounts = true
            })
            .Build();
            
        _logger.LogInformation("MSAL Public Client initialized for Gateway API access with scope: {Scope}", apiScope);
    }
    
    public async Task<UserToken?> GetUserTokenAsync()
    {
        try
        {
            _logger.LogInformation("Starting Azure AD authentication with Windows Hello...");
            
            // Check if running on Windows
            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine("❌ Windows Hello is only available on Windows.");
                return await GetFallbackTokenAsync();
            }
            
            Console.WriteLine("🔐 Authenticating with Azure AD...");
            Console.WriteLine($"   Requesting access to Gateway API scope: {_scopes[0]}");
            
            AuthenticationResult? result = null;
            
            try
            {
                // Try to get token silently first (from cache)
                var accounts = await _app.GetAccountsAsync();
                if (accounts.Any())
                {
                    result = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
                    _logger.LogInformation("Successfully acquired token from cache");
                    Console.WriteLine("✅ Token retrieved from cache");
                }
            }
            catch (MsalUiRequiredException)
            {
                _logger.LogInformation("Silent authentication failed, requiring interactive login");
                Console.WriteLine("🔐 Interactive authentication required...");
            }
            
            // If silent authentication failed, try interactive authentication
            if (result == null)
            {
                try
                {
                    Console.WriteLine("🔐 Opening Azure AD sign-in (Windows Hello may be prompted)...");
                    result = await _app.AcquireTokenInteractive(_scopes)
                        .WithPrompt(Prompt.SelectAccount)
                        .WithUseEmbeddedWebView(false) // Use system browser for better Windows Hello integration
                        .ExecuteAsync();
                        
                    _logger.LogInformation("Successfully acquired token via interactive authentication");
                    Console.WriteLine("✅ Azure AD authentication successful!");
                }
                catch (MsalException msalEx)
                {
                    _logger.LogError(msalEx, "MSAL authentication failed: {ErrorCode}", msalEx.ErrorCode);
                    Console.WriteLine($"❌ Azure AD authentication failed: {msalEx.Message}");
                    return await GetFallbackTokenAsync();
                }
            }
            
            if (result != null)
            {
                Console.WriteLine($"✅ Authenticated as: {result.Account.Username}");
                Console.WriteLine($"   Account: {result.Account.HomeAccountId?.Identifier}");
                Console.WriteLine($"   Scopes: {string.Join(", ", result.Scopes)}");
                Console.WriteLine($"   Token expires: {result.ExpiresOn:yyyy-MM-dd HH:mm:ss} UTC");
                
                var token = new UserToken
                {
                    Username = result.Account.Username,
                    Token = result.AccessToken,
                    ExpiresAt = result.ExpiresOn.UtcDateTime,
                    AuthenticationMethod = "Azure AD with Windows Hello",
                    IsAuthenticated = true
                };
                
                _logger.LogInformation($"Generated Azure AD access token for user: {result.Account.Username}");
                return token;
            }
            
            _logger.LogWarning("Authentication failed - no token received");
            return await GetFallbackTokenAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure AD authentication failed with exception");
            Console.WriteLine($"❌ Authentication error: {ex.Message}");
            
            // Fall back to demo mode
            return await GetFallbackTokenAsync();
        }
    }
    
    private async Task<bool> CheckWindowsHelloCapabilityAsync()
    {
        try
        {
            // Check if Windows Hello is potentially available
            // This is a simplified check - in a real implementation, you would use proper Windows APIs
            await Task.Delay(100);
            
            // Check Windows version and hardware capabilities (simplified)
            var version = Environment.OSVersion.Version;
            var isWindows10OrLater = version.Major >= 10;
            
            _logger.LogInformation($"Windows version: {version}, Hello capable: {isWindows10OrLater}");
            return isWindows10OrLater;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<UserToken?> GetFallbackTokenAsync()
    {
        try
        {
            Console.WriteLine("\n💡 Falling back to username input (demo mode)");
            Console.Write("Username: ");
            var username = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("No username provided in fallback mode");
                return null;
            }
            
            Console.WriteLine("⚠️  Using demo authentication (not secure)");
            await Task.Delay(500);
            
            var token = new UserToken
            {
                Username = username,
                Token = await GenerateMockGraphTokenAsync(username),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                AuthenticationMethod = "Fallback",
                IsAuthenticated = false // Not truly authenticated
            };
            
            _logger.LogInformation($"Generated fallback token for user: {username}");
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback authentication failed");
            return null;
        }
    }
    
    private async Task<string> GenerateMockGraphTokenAsync(string username)
    {
        // In a real implementation, this would:
        // 1. Use Windows Integrated Authentication with Azure AD
        // 2. Request Microsoft Graph scopes (User.Read, Mail.Read, etc.)
        // 3. Use MSAL.NET to obtain real OAuth 2.0 Bearer token
        // 4. Return the actual token from Azure AD
        
        await Task.Delay(100); // Simulate token generation
        
        // Generate a realistic-looking JWT structure (still mock but more realistic)
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"typ\":\"JWT\",\"alg\":\"RS256\",\"kid\":\"mock-key-id\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{{" +
            $"\"sub\":\"{username}\"," +
            $"\"aud\":\"https://graph.microsoft.com\"," +
            $"\"iss\":\"https://sts.windows.net/mock-tenant-id/\"," +
            $"\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}," +
            $"\"iat\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}," +
            $"\"scp\":\"User.Read Mail.Read Calendars.Read Directory.Read.All\"," +
            $"\"unique_name\":\"{username}\"" +
            $"}}"));
        var signature = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        
        return $"{header}.{payload}.{signature}";
    }
}