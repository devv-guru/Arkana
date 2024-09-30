using Devv.Gateway.Shared.Paging;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace Devv.Gateway.Api.Common.Paging;

public class FastEndpointPagingQuery : PagedQuery
{
    [BindFrom("page")] public override int Page { get; init; }
    [BindFrom("pageSize")] public override int PageSize { get; init; }
    [BindFrom("sortBy")] public override string? SortBy { get; init; }
    [BindFrom("sortAscending")] public override bool SortAscending { get; init; }
}