namespace Data.Entities;

public class WebHost : EntityBase
{
    public string? Name { get; set; }
    public string? HostName { get; set; }
    public bool IsDefault { get; set; }

    // Azure handles SSL termination - no certificates needed in Gateway
    
    public Guid? ClusterId { get; set; }
    public Cluster? Cluster { get; set; }

    public List<Route> Routes { get; set; }
}