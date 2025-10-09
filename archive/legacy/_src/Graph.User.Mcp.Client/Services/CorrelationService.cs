using System.Diagnostics;

namespace Graph.User.Mcp.Client.Services;

/// <summary>
/// Service for managing correlation IDs across requests
/// </summary>
public static class CorrelationService
{
    private static readonly AsyncLocal<string?> _correlationId = new();
    
    /// <summary>
    /// Gets the current correlation ID, creating one if none exists
    /// </summary>
    public static string GetOrCreate()
    {
        if (string.IsNullOrEmpty(_correlationId.Value))
        {
            _correlationId.Value = Activity.Current?.Id ?? Guid.NewGuid().ToString("N")[..8];
        }
        
        return _correlationId.Value;
    }
    
    /// <summary>
    /// Sets a specific correlation ID for the current context
    /// </summary>
    public static void Set(string correlationId)
    {
        _correlationId.Value = correlationId;
    }
    
    /// <summary>
    /// Clears the correlation ID for the current context
    /// </summary>
    public static void Clear()
    {
        _correlationId.Value = null;
    }
}