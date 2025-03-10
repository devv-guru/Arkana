using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Gateway.UI;

/// <summary>
/// Middleware for handling UI requests
/// </summary>
public class UIMiddleware
{
    private readonly RequestDelegate _next;
    private readonly UIOptions _options;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="options">The UI options</param>
    public UIMiddleware(RequestDelegate next, UIOptions options)
    {
        _next = next;
        _options = options;
    }
    
    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if authentication is required
        if (_options.RequireAuthentication && !context.User.Identity.IsAuthenticated)
        {
            context.Response.StatusCode = 401;
            return;
        }
        
        // If the request is for a file that exists, let the static files middleware handle it
        if (Path.HasExtension(context.Request.Path))
        {
            await _next(context);
            return;
        }
        
        // Otherwise, serve the index.html file
        context.Request.Path = $"{_options.Path}/index.html";
        await _next(context);
    }
}
