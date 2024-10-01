using Data.Contexts.Base;
using Endpoints.Common.Paging;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Endpoints.Certificates.Find;

public class QueryHandler : IQueryHandler<Request, PagedResult<Response, Response[]>>
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
            .Where(w => w.Name.Contains(request.Search)
                        || w.CreatedAt.ToString().Contains(request.Search)
                        || w.WebHosts.Any(x => x.Name.Contains(request.Search)))
            .Sort(request.SortBy, request.SortAscending)
            .ToPagedArrayAsync(request.Page, request.PageSize, Mapper.FromEntity, ct);

        return certificates;
    }
}