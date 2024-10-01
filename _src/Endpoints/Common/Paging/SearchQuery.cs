using FastEndpoints;

namespace Endpoints.Common.Paging;

public class SearchQuery : FastEndpointPagingQuery
{
    [QueryParam,BindFrom("search")]
    public string? Search { get; init; }
}