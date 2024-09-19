namespace Devv.Gateway.Api.Configuration;

public class GatewayOptions
{
    public const string SectionName = "GatewayOptions";
    public string? ManagementDomain { get; init; }
}