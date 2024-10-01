using Endpoints.Common.Paging;
using Mediator;

namespace Endpoints.Certificates.Find;

public class Request : SearchQuery, IQuery<PagedResult<Response, Response[]>>
{
}