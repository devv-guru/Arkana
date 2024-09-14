namespace Devv.WebServer.Api.Configuration.StartParameters;

public class StartupParametersConfigurationSource : IConfigurationSource
{
    private readonly Dictionary<string, string> _mappings;
    private readonly string[] _args;

    public StartupParametersConfigurationSource(Dictionary<string, string> mappings, string[] args)
    {
        _mappings = mappings;
        _args = args;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new StartParametersConfigurationProvider(_mappings, _args);
    }
}