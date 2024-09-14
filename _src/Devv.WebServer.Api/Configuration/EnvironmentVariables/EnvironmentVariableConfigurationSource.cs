namespace Devv.WebServer.Api.Configuration.EnvironmentVariables;

public class EnvironmentVariableConfigurationSource : IConfigurationSource
{
    private readonly Dictionary<string, string> _mappings;

    public EnvironmentVariableConfigurationSource(Dictionary<string, string> mappings)
    {
        _mappings = mappings;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new EnvironmentVariableConfigurationProvider(_mappings);
    }
}