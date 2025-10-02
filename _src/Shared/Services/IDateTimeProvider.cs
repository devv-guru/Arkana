namespace Shared.Services;

/// <summary>
/// Provides abstraction over DateTime operations for testability
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }
    
    /// <summary>
    /// Gets the current local date and time
    /// </summary>
    DateTime Now { get; }
}

/// <summary>
/// Default implementation using system DateTime
/// </summary>
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}