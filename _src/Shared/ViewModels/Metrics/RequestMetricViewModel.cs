using System;

namespace Shared.ViewModels.Metrics;

/// <summary>
/// View model for request metrics
/// </summary>
public class RequestMetricViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the route identifier
    /// </summary>
    public string RouteId { get; set; }
    
    /// <summary>
    /// Gets or sets the cluster identifier
    /// </summary>
    public string ClusterId { get; set; }
    
    /// <summary>
    /// Gets or sets the destination identifier
    /// </summary>
    public string DestinationId { get; set; }
    
    /// <summary>
    /// Gets or sets the HTTP method
    /// </summary>
    public string Method { get; set; }
    
    /// <summary>
    /// Gets or sets the request path
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the host header
    /// </summary>
    public string Host { get; set; }
    
    /// <summary>
    /// Gets or sets the HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Gets or sets the elapsed time in milliseconds
    /// </summary>
    public long ElapsedMilliseconds { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the metric was recorded
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the client IP address
    /// </summary>
    public string ClientIp { get; set; }
    
    /// <summary>
    /// Gets or sets the user agent
    /// </summary>
    public string UserAgent { get; set; }
    
    /// <summary>
    /// Gets or sets the request size in bytes
    /// </summary>
    public long RequestSize { get; set; }
    
    /// <summary>
    /// Gets or sets the response size in bytes
    /// </summary>
    public long ResponseSize { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the request resulted in an error
    /// </summary>
    public bool IsError { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the request resulted in a server error
    /// </summary>
    public bool IsServerError { get; set; }
}
