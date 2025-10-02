using Data.Contexts.Base;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Services;

namespace Endpoints.MCP.Update;

public class CommandHandler : IRequestHandler<Request, Response?>
{
    private readonly IWriteOnlyContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CommandHandler(IWriteOnlyContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
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

        server.Name = request.Name;
        server.Description = request.Description;
        server.Endpoint = request.Endpoint;
        server.ProtocolType = request.Protocol;
        server.IsEnabled = request.IsEnabled;
        server.UpdatedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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