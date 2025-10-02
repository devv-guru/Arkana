using Data.Contexts.Base;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Services;

namespace Endpoints.MCP.Delete;

public class CommandHandler : IRequestHandler<Request, bool>
{
    private readonly IWriteOnlyContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CommandHandler(IWriteOnlyContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async ValueTask<bool> Handle(Request request, CancellationToken cancellationToken)
    {
        if (!Ulid.TryParse(request.Id, out var ulid))
            return false;
            
        var guid = ulid.ToGuid();
        
        var server = await _context.McpServers
            .Where(s => s.Id == guid && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (server == null)
            return false;

        // Soft delete
        server.IsDeleted = true;
        server.UpdatedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}