using System.Text.Json;
using Graph.User.Mcp.Server.Services.Models;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Enhanced interface for Microsoft Graph user profile and identity operations
/// Based on User.Read, User.ReadWrite, Profile.Read permissions
/// </summary>
public interface IGraphUserService
{
    // Enhanced Profile & Identity Operations
    ValueTask<JsonDocument?> GetCurrentUserAsync(string accessToken, GraphUserQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetCurrentUserProfileAsync(string accessToken, GraphQueryOptions? options = null, string? correlationId = null);
    ValueTask<Stream?> GetCurrentUserPhotoAsync(string accessToken, string size = "48x48", string? correlationId = null);
    ValueTask<JsonDocument?> UpdateCurrentUserAsync(string accessToken, object updates, string? correlationId = null);
    
    // Enhanced Organizational Structure
    ValueTask<JsonDocument?> GetCurrentUserManagerAsync(string accessToken, GraphQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetCurrentUserDirectReportsAsync(string accessToken, GraphUserQueryOptions? options = null, string? correlationId = null);
    
    // Enhanced User Directory Operations with flexible queries
    ValueTask<JsonDocument?> GetUsersAsync(string accessToken, GraphUserQueryOptions? options = null, string? correlationId = null);
    ValueTask<GraphPagedResponse<JsonElement>?> GetUsersPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphUserQueryOptions? queryOptions = null, string? correlationId = null);
    ValueTask<JsonDocument?> SearchUsersAsync(string accessToken, string searchTerm, GraphUserQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetUserByIdAsync(string accessToken, string userId, GraphQueryOptions? options = null, string? correlationId = null);
    ValueTask<Stream?> GetUserPhotoAsync(string accessToken, string userId, string size = "48x48", string? correlationId = null);
    
    // Enhanced People & Insights
    ValueTask<JsonDocument?> GetRelevantPeopleAsync(string accessToken, GraphQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetUserInsightsAsync(string accessToken, string insightType, GraphQueryOptions? options = null, string? correlationId = null);
    
    // Pagination Support
    ValueTask<JsonDocument?> GetNextPageAsync(string accessToken, string nextLink, string? correlationId = null);
    
    // Delta Queries for Incremental Sync
    ValueTask<GraphDeltaResponse<JsonElement>?> GetUsersDeltaAsync(string accessToken, string? deltaLink = null, GraphUserQueryOptions? options = null, string? correlationId = null);
    
    // Batch Operations
    ValueTask<BatchResponse[]> BatchGetUsersAsync(string accessToken, BatchRequest[] requests, string? correlationId = null);
    
    // Advanced Filtering
    ValueTask<JsonDocument?> GetUsersByDepartmentAsync(string accessToken, string department, GraphUserQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetUsersByJobTitleAsync(string accessToken, string jobTitle, GraphUserQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetUsersByLocationAsync(string accessToken, string location, GraphUserQueryOptions? options = null, string? correlationId = null);
}