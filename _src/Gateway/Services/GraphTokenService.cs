using Microsoft.Identity.Web;
using Microsoft.Graph;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Client;

namespace Gateway.Services;

/// <summary>
/// Service for exchanging user tokens for Microsoft Graph access tokens using OIDC and On-Behalf-Of flow
/// </summary>
public class GraphTokenService : IGraphTokenService
{
    private readonly ILogger<GraphTokenService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IConfidentialClientApplication _app;

    // Default Microsoft Graph scopes for MCP operations
    private static readonly string[] DefaultGraphScopes = new[]
    {
        "https://graph.microsoft.com/User.Read",
        "https://graph.microsoft.com/Mail.Read", 
        "https://graph.microsoft.com/Calendars.Read",
        "https://graph.microsoft.com/Directory.Read.All",
        "https://graph.microsoft.com/Group.Read.All"
    };

    public GraphTokenService(ILogger<GraphTokenService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Initialize MSAL Confidential Client Application
        _app = ConfidentialClientApplicationBuilder
            .Create(_configuration["AzureAd:ClientId"])
            .WithClientSecret(_configuration["AzureAd:ClientSecret"])
            .WithAuthority(_configuration["AzureAd:Instance"] + _configuration["AzureAd:TenantId"])
            .Build();

        _logger.LogInformation("GraphTokenService initialized with tenant: {TenantId}", 
            _configuration["AzureAd:TenantId"]);
    }

    public async Task<string?> GetGraphAccessTokenAsync(ClaimsPrincipal userClaims, string[] graphScopes)
    {
        try
        {
            _logger.LogInformation("Getting Graph access token for user from claims");
            
            var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = userClaims.FindFirst(ClaimTypes.Email)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No user ID found in claims");
                return null;
            }

            _logger.LogInformation("Requesting Graph token for user: {UserId} ({Email})", userId, userEmail);
            
            // Use scopes provided or default ones
            var scopes = graphScopes?.Length > 0 ? graphScopes : DefaultGraphScopes;
            
            // Try to get token silently first (from cache)
            var accounts = await _app.GetAccountsAsync();
            var account = accounts.FirstOrDefault(a => a.HomeAccountId?.Identifier?.Contains(userId) == true);
            
            AuthenticationResult? result = null;
            
            if (account != null)
            {
                try
                {
                    result = await _app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                    _logger.LogInformation("Successfully acquired Graph token from cache");
                }
                catch (MsalUiRequiredException)
                {
                    _logger.LogInformation("Token not in cache, interactive authentication required");
                    // In a real implementation, this would redirect to Azure AD for interactive login
                    // For now, return null to indicate authentication is required
                    return null;
                }
            }
            
            if (result != null)
            {
                _logger.LogInformation("Graph access token acquired for scopes: {Scopes}", string.Join(", ", scopes));
                return result.AccessToken;
            }
            
            _logger.LogWarning("Failed to acquire Graph access token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Graph access token from user claims");
            return null;
        }
    }

    public async Task<string?> ExchangeTokenForGraphAsync(string userAccessToken, string[] graphScopes)
    {
        try
        {
            _logger.LogInformation("Exchanging user token for Graph access token using On-Behalf-Of flow");
            
            // Parse the incoming token to get user information
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(userAccessToken);
            
            var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == "sub" || x.Type == "oid")?.Value;
            var userName = jsonToken.Claims.FirstOrDefault(x => x.Type == "unique_name" || x.Type == "upn")?.Value;
            
            _logger.LogInformation("Token exchange for user: {UserId} ({UserName})", userId, userName);
            
            // Use scopes provided or default ones
            var scopes = graphScopes?.Length > 0 ? graphScopes : DefaultGraphScopes;
            
            // Create UserAssertion with the incoming token
            var userAssertion = new UserAssertion(userAccessToken);
            
            // Use On-Behalf-Of flow to get Graph token
            var result = await _app.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
            
            if (result != null)
            {
                _logger.LogInformation("Successfully exchanged token for Graph access token");
                _logger.LogInformation("Graph token scopes: {Scopes}", string.Join(", ", result.Scopes));
                return result.AccessToken;
            }
            
            _logger.LogWarning("Token exchange returned null result");
            return null;
        }
        catch (MsalServiceException msalEx)
        {
            _logger.LogError(msalEx, "MSAL service error during token exchange: {ErrorCode} - {Message}", 
                msalEx.ErrorCode, msalEx.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging token for Graph access token");
            return null;
        }
    }

    public async Task<bool> ValidateTokenScopesAsync(string token, string[] requiredScopes)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            // Extract scopes from token
            var scopeClaims = jsonToken.Claims.Where(x => x.Type == "scp" || x.Type == "scope").ToList();
            var tokenScopes = new HashSet<string>();
            
            foreach (var scopeClaim in scopeClaims)
            {
                var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var scope in scopes)
                {
                    tokenScopes.Add(scope);
                }
            }
            
            // Check if all required scopes are present
            var hasAllScopes = requiredScopes.All(requiredScope => 
                tokenScopes.Any(tokenScope => tokenScope.Equals(requiredScope, StringComparison.OrdinalIgnoreCase)));
            
            _logger.LogInformation("Token scope validation: Required={RequiredScopes}, Present={TokenScopes}, Valid={Valid}", 
                string.Join(",", requiredScopes), string.Join(",", tokenScopes), hasAllScopes);
                
            return hasAllScopes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token scopes");
            return false;
        }
    }
}