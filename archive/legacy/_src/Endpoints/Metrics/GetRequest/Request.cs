using Mediator;
using Shared.Paging;

namespace Endpoints.Metrics.GetRequest;

public class Request : PagedQuery, IQuery<Response>
{
    public int Count { get; set; } = 10;
}
