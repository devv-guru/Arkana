using Data.Contexts.Base;
using Data.Entities.Mcp;
using Mediator;
using Shared.Services;

namespace Endpoints.MCP.Create;

public class CommandHandler : IRequestHandler<Request, Response>
{
    private readonly IWriteOnlyContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CommandHandler(IWriteOnlyContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async ValueTask<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var server = new McpServer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Endpoint = request.Endpoint,
            ProtocolType = request.Protocol,
            IsEnabled = request.IsEnabled,
            Priority = 0, // Default priority
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        _context.McpServers.Add(server);
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