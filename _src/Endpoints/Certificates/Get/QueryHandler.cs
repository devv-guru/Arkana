using Data.Contexts.Base;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Endpoints.Certificates.Get;

public class QueryHandler : IQueryHandler<Request, Response?>
{
    private readonly IReadOnlyProxyContext _context;

    public QueryHandler(IReadOnlyProxyContext context)
    {
        _context = context;
    }

    public async ValueTask<Response?> Handle(Request request, CancellationToken ct)
    {
        var certificate = await _context.Certificates
            .Include(i => i.WebHosts)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        return Mapper.FromEntity(certificate);
    }
}