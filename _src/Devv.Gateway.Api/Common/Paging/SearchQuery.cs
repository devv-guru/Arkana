using FastEndpoints;

namespace Devv.Gateway.Api.Common.Paging;

public class SearchQuery : FastEndpointPagingQuery
{
    [BindFrom("search")]
    public string? Search { get; init; }
}