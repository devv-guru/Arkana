using Data.Contexts.Base;
using Mediator;

namespace Endpoints.Certificates.Create;

public class CommandHandler : ICommandHandler<Request, Response>
{
    private readonly IWriteOnlyContext _context;

    public CommandHandler(IWriteOnlyContext context)
    {
        _context = context;
    }

    public async ValueTask<Response> Handle(Request command, CancellationToken ct)
    {
        var certificate = Mapper.ToEntity(command);
        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync(ct);
        return Mapper.FromEntity(certificate);
    }
}