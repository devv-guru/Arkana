using Data.Contexts.Base;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Endpoints.MCP.GetAllServers;

public class QueryHandler : IRequestHandler<Request, Response>
{
    private readonly IReadOnlyContext _context;

    public QueryHandler(IReadOnlyContext context)
    {
        _context = context;
    }

    public async ValueTask<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var servers = await _context.McpServers
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Priority)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var serverDtos = servers.Select(s => new McpServerDto
        {
            Id = new Ulid(s.Id).ToString(),
            Name = s.Name,
            Description = s.Description,
            Endpoint = s.Endpoint,
            ProtocolType = (int)s.ProtocolType,
            IsEnabled = s.IsEnabled,
            Priority = s.Priority,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();

        return new Response { Servers = serverDtos };
    }
}