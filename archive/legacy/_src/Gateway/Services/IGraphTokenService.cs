using System.Security.Claims;

namespace Gateway.Services;

/// <summary>
/// Service for exchanging user tokens for Microsoft Graph access tokens
/// </summary>
public interface IGraphTokenService
{
    /// <summary>
    /// Exchange a user's OIDC token for a Microsoft Graph access token
    /// </summary>
    /// <param name="userClaims">User claims from OIDC token</param>
    /// <param name="graphScopes">Required Microsoft Graph scopes</param>
    /// <returns>Microsoft Graph access token</returns>
    Task<string?> GetGraphAccessTokenAsync(ClaimsPrincipal userClaims, string[] graphScopes);
    
    /// <summary>
    /// Exchange a JWT token for Microsoft Graph access token using On-Behalf-Of flow
    /// </summary>
    /// <param name="userAccessToken">User's access token from client</param>
    /// <param name="graphScopes">Required Microsoft Graph scopes</param>
    /// <returns>Microsoft Graph access token</returns>
    Task<string?> ExchangeTokenForGraphAsync(string userAccessToken, string[] graphScopes);
    
    /// <summary>
    /// Validate if a token has the required scopes for Microsoft Graph access
    /// </summary>
    /// <param name="token">Access token to validate</param>
    /// <param name="requiredScopes">Required scopes</param>
    /// <returns>True if token has required scopes</returns>
    Task<bool> ValidateTokenScopesAsync(string token, string[] requiredScopes);
}