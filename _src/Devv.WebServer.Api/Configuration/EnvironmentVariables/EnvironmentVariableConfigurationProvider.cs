namespace Devv.WebServer.Api.Configuration.EnvironmentVariables;

public class EnvironmentVariableConfigurationProvider : ConfigurationProvider
{
    private readonly Dictionary<string, string> _mappings;

    public EnvironmentVariableConfigurationProvider(Dictionary<string, string> mappings)
    {
        _mappings = mappings;
    }

    public override void Load()
    {
        var config = new Dictionary<string, string>();

        foreach (var mapping in _mappings)
        {
            var envVarValue = Environment.GetEnvironmentVariable(mapping.Key);
            if (!string.IsNullOrEmpty(envVarValue)) config[mapping.Value] = envVarValue;
        }

        Data = config;
    }
}