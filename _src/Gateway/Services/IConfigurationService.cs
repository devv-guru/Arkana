using Data.Entities;
using Gateway.Models;

namespace Gateway.Services;

public interface IConfigurationService
{
    // Route Management
    Task<IEnumerable<Route>> GetRoutesAsync();
    Task<Route?> GetRouteAsync(Guid id);
    Task<Route> CreateRouteAsync(Route route);
    Task<Route> UpdateRouteAsync(Route route);
    Task DeleteRouteAsync(Guid id);
    
    // Cluster Management  
    Task<IEnumerable<Cluster>> GetClustersAsync();
    Task<Cluster?> GetClusterAsync(Guid id);
    Task<Cluster> CreateClusterAsync(Cluster cluster);
    Task<Cluster> UpdateClusterAsync(Cluster cluster);
    Task DeleteClusterAsync(Guid id);
    
    // Destination Management
    Task<IEnumerable<Destination>> GetDestinationsAsync(Guid clusterId);
    Task<Destination?> GetDestinationAsync(Guid id);
    Task<Destination> CreateDestinationAsync(Destination destination);
    Task<Destination> UpdateDestinationAsync(Destination destination);
    Task DeleteDestinationAsync(Guid id);
    
    // WebHost Management
    Task<IEnumerable<WebHost>> GetWebHostsAsync();
    Task<WebHost?> GetWebHostAsync(Guid id);
    Task<WebHost> CreateWebHostAsync(WebHost webHost);
    Task<WebHost> UpdateWebHostAsync(WebHost webHost);
    Task DeleteWebHostAsync(Guid id);
    
    // Configuration Operations
    Task ReloadProxyConfigurationAsync();
    Task<bool> ValidateConfigurationAsync();
    Task ImportConfigurationAsync(GatewayConfigurationModel configuration);
    Task<GatewayConfigurationModel> ExportConfigurationAsync();
    
    // Health and Status
    Task<ConfigurationStatus> GetConfigurationStatusAsync();
}