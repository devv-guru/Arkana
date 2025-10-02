namespace Gateway.Services;

/// <summary>
/// Provides abstraction over GUID generation for testability
/// </summary>
public interface IGuidProvider
{
    /// <summary>
    /// Creates a new GUID
    /// </summary>
    Guid NewGuid();
}

/// <summary>
/// Default implementation using system Guid
/// </summary>
public class SystemGuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}