namespace Devv.WebServer.Api.Configuration.StartParameters;

public class StartParametersConfigurationProvider : ConfigurationProvider
{
    private readonly Dictionary<string, string> _mappings;
    private readonly string[] _args;

    public StartParametersConfigurationProvider(Dictionary<string, string> mappings, string[] args)
    {
        _mappings = mappings;
        _args = args;
    }

    public override void Load()
    {
        var config = new Dictionary<string, string>();

        foreach (var mapping in _mappings)
        {
            var argValue = _args.FirstOrDefault(x => x.StartsWith(mapping.Key));
            if (!string.IsNullOrEmpty(argValue))
            {
                config[mapping.Value] = argValue.Split("=")[1];
            }
        }

        Data = config;
    }
}