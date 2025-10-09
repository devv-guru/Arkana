using System.Text.Json;

namespace Endpoints.Configuration.Get;

public static class Mapper
{
    public static ConfigurationModel? FromGatewayConfig(object? config)
    {
        if (config == null)
            return null;
        
        // Convert the object to JSON and then back to our model
        var json = JsonSerializer.Serialize(config);
        return JsonSerializer.Deserialize<ConfigurationModel>(json);
    }
}
