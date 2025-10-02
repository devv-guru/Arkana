using Gateway.Models;
using Data.Entities;

namespace Gateway.Services;

public interface IConfigurationTranslationService
{
    /// <summary>
    /// Translates application configuration to database entities
    /// </summary>
    Task<TranslationResult> TranslateConfigurationAsync(GatewayConfigurationModel configuration);
    
    /// <summary>
    /// Translates a single proxy rule to route and cluster entities
    /// </summary>
    (Route route, Cluster cluster) TranslateProxyRule(ProxyRuleConfigurationModel proxyRule, Dictionary<string, WebHost> hostLookup);
    
    /// <summary>
    /// Translates host configuration to WebHost entity
    /// </summary>
    WebHost TranslateHost(HostConfigurationModel hostConfig);
}

public class TranslationResult
{
    public bool IsSuccess { get; set; }
    public List<WebHost> WebHosts { get; set; } = new();
    public List<Route> Routes { get; set; } = new();
    public List<Cluster> Clusters { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static TranslationResult Success(List<WebHost> webHosts, List<Route> routes, List<Cluster> clusters)
    {
        return new TranslationResult
        {
            IsSuccess = true,
            WebHosts = webHosts,
            Routes = routes,
            Clusters = clusters
        };
    }

    public static TranslationResult Failure(params string[] errors)
    {
        return new TranslationResult
        {
            IsSuccess = false,
            Errors = errors.ToList()
        };
    }

    public void AddError(string error)
    {
        Errors.Add(error);
        IsSuccess = false;
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}