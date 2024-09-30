using Devv.Gateway.Api.Common.Paging;
using Mediator;

namespace Devv.Gateway.Api.Features.Certificates.Find;

public class Request : SearchQuery, IRequest<PagedResult<Response, Response[]>>
{
}