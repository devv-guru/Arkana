namespace Graph.User.Mcp.Client.Models;

/// <summary>
/// Represents an authenticated user token
/// </summary>
public class UserToken
{
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string AuthenticationMethod { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
    
    public bool IsValid => !string.IsNullOrEmpty(Token) && ExpiresAt > DateTime.UtcNow;
    
    public string GetDisplayInfo()
    {
        var status = IsAuthenticated ? "✅ Authenticated" : "⚠️ Demo Mode";
        var method = AuthenticationMethod;
        var expiry = IsValid ? $"expires {ExpiresAt:HH:mm}" : "⚠️ EXPIRED";
        return $"{status} via {method} ({expiry})";
    }
}