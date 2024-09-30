using System.Net.NetworkInformation;
using Devv.Gateway.Api.Common.Paging;
using Devv.Gateway.Data.Contexts.Base;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Api.Features.Certificates.Find;

public class QueryHandler : IRequestHandler<Request, PagedResult<Response, Response[]>>
{
    private readonly IReadOnlyContext _context;

    public QueryHandler(IReadOnlyContext context)
    {
        _context = context;
    }

    public async ValueTask<PagedResult<Response, Response[]>> Handle(Request request, CancellationToken ct)
    {
        var certificates = await _context.Certificates
            .Include(c => c.WebHosts)
            .Search(request.Search,
                x => x.Name,
                x => x.WebHosts,
                x => x.CreatedAt)
            .Sort(request.SortBy, request.SortAscending)
            .ToPagedArrayAsync(request.Page, request.PageSize, Mapper.FromEntity, ct);

        return certificates;
    }
}