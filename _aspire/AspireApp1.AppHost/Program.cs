using AppHost.Common;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Add local configuration support
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

var configuration = builder.Configuration;

var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: true)
                     .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                     .WithHttpEndpoint(targetPort: 3000, name: "http")
                     .WithLifetime(ContainerLifetime.Persistent);

var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
       .WithBindMount("../prometheus", "/etc/prometheus", isReadOnly: true)
       .WithHttpEndpoint(port: 9090, targetPort: 9090)
       .WithLifetime(ContainerLifetime.Persistent);

var mcpGraphUserServer = builder.AddProject<Projects.Graph_User_Mcp_Server>("mcp-graph-user-server",launchProfileName: "https")
    .WithReplicas(1);

var gateway = builder.AddProject<Projects.Gateway>("arkana-gateway", launchProfileName: "https")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(mcpGraphUserServer)
    .WaitFor(cache)
    .WaitFor(mcpGraphUserServer);

var databaseProvider = configuration["DataContextOptions:Provider"];

switch (databaseProvider)
{
    case DataContextProviders.SqlServer:
        {
            var db = builder.AddSqlServer("mssql")
               .WithDataVolume(isReadOnly: false)
               .WithLifetime(ContainerLifetime.Persistent)
               .AddDatabase("Arkana");

            gateway.WithReference(db)
                   .WaitFor(db);
            break;
        }
    case DataContextProviders.PostgreSql:
        {
            var db = builder.AddPostgres("postgres")
               .WithDataVolume(isReadOnly: false)
               .WithLifetime(ContainerLifetime.Persistent)
               .AddDatabase("Arkana");

            gateway.WithReference(db)
                   .WaitFor(db);
            break;
        }
    case DataContextProviders.MySql:
    case DataContextProviders.MariaDb:
        {
            var db = builder.AddMySql("mysql")
               .WithDataVolume(isReadOnly: false)
               .WithLifetime(ContainerLifetime.Persistent)
               .AddDatabase("Arkana");

            gateway.WithReference(db)
                   .WaitFor(db);
            break;
        }
}


builder.Build().Run();
