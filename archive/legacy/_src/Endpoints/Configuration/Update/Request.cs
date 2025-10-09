using Endpoints.Configuration.Get;
using Mediator;

namespace Endpoints.Configuration.Update;

public class Request : ICommand<Response>
{
    public ConfigurationModel? Configuration { get; set; }
}
