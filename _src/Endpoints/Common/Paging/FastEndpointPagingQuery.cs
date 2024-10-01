using Shared.Paging;
using FastEndpoints;

namespace Endpoints.Common.Paging;

public class FastEndpointPagingQuery : PagedQuery
{
    [QueryParam, BindFrom("page")] public override int Page { get; init; }
    [QueryParam, BindFrom("pageSize")] public override int PageSize { get; init; }
    [QueryParam, BindFrom("sortBy")] public override string? SortBy { get; init; }

    [QueryParam, BindFrom("sortAscending")]
    public override bool SortAscending { get; init; }
}