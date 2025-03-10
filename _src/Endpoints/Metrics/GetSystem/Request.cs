using Shared.Paging;
using Mediator;

namespace Endpoints.Metrics.GetSystem;

public class Request : PagedQuery, IQuery<Response>
{
    public int Count { get; set; } = 10;
}
