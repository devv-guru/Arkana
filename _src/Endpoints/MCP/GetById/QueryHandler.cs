using Data.Contexts.Base;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Endpoints.MCP.GetById;

public class QueryHandler : IRequestHandler<Request, Response?>
{
    private readonly IReadOnlyContext _context;

    public QueryHandler(IReadOnlyContext context)
    {
        _context = context;
    }

    public async ValueTask<Response?> Handle(Request request, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(request.Id, out var ulid))
            return null;
            
        var guid = ulid.ToGuid();
        
        var server = await _context.McpServers
            .Where(s => s.Id == guid && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (server == null)
            return null;

        return new Response
        {
            Id = new Ulid(server.Id).ToString(),
            Name = server.Name,
            Description = server.Description,
            Endpoint = server.Endpoint,
            ProtocolType = (int)server.ProtocolType,
            IsEnabled = server.IsEnabled,
            Priority = server.Priority,
            CreatedAt = server.CreatedAt,
            UpdatedAt = server.UpdatedAt
        };
    }
}