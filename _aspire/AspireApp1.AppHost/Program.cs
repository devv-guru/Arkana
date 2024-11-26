using AppHost.Common;

var builder = DistributedApplication.CreateBuilder(args);
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

var gateway = builder.AddProject<Projects.Gateway>("arkana-gateway", launchProfileName: "https")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache);

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
