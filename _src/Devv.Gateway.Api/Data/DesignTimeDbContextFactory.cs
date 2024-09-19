using Devv.Gateway.Data.Common;
using Devv.Gateway.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Devv.Gateway.Api.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        // Get DataContextOptions
        var dataContextOptions = configuration.GetSection(DataContextOptions.SectionName).Get<DataContextOptions>();

        // Configure DbContextOptionsBuilder
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        ConfigureDatabaseProvider(optionsBuilder, dataContextOptions);

        return new DataContext(optionsBuilder.Options);
    }

    private static void ConfigureDatabaseProvider(DbContextOptionsBuilder options,
        DataContextOptions dataContextOptions)
    {
        switch (dataContextOptions.Provider)
        {
            case DataContextProviders.SqlServer:
                options.UseSqlServer(dataContextOptions.ConnectionString);
                break;

            case DataContextProviders.PostgreSql:
                options.UseNpgsql(dataContextOptions.ConnectionString);
                break;

            case DataContextProviders.SqLite:
                options.UseSqlite(dataContextOptions.ConnectionString);
                break;

            case DataContextProviders.MySql:
                ConfigureMySql(options, dataContextOptions.ConnectionString, dataContextOptions.MySqlVersion);
                break;

            case DataContextProviders.MariaDb:
                ConfigureMariaDb(options, dataContextOptions.ConnectionString, dataContextOptions.MariaDbVersion);
                break;

            default:
                throw new NotSupportedException($"The provider '{dataContextOptions.Provider}' is not supported.");
        }
    }

    private static void ConfigureMySql(DbContextOptionsBuilder options, string connectionString, string version)
    {
        if (version != "AutoDetect")
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(version)));
        else
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }

    private static void ConfigureMariaDb(DbContextOptionsBuilder options, string connectionString, string version)
    {
        if (version != "AutoDetect")
            options.UseMySql(connectionString, new MariaDbServerVersion(new Version(version)));
        else
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}