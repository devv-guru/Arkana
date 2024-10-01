using FastEndpoints;
using Mediator;

namespace Endpoints.Certificates.Get;

public class Request : IQuery<Response?>
{
    [BindFrom("id")] public Guid Id { get; set; }
}