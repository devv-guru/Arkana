using Devv.WebServer.Api.Data.Common;
using Devv.WebServer.Api.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Devv.WebServer.Api.Data;

public static class ConfigureServices
{
    public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration)
    {
        var dataContextOptions = configuration.GetSection(DataContextOptions.SectionName).Get<DataContextOptions>();

        // Register the base context (used for migrations)
        services.AddDbContext<DbContext>(options =>
            ConfigureDatabaseProvider(options, dataContextOptions));

        // Register ReadOnlyContext and WriteOnlyContext
        services.AddDbContext<ReadOnlyContext>(options => { ConfigureDatabaseProvider(options, dataContextOptions); });
        services.AddDbContext<WriteOnlyContext>(options => { ConfigureDatabaseProvider(options, dataContextOptions); });

        return services;
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
        {
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(version)));
        }
        else
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

    private static void ConfigureMariaDb(DbContextOptionsBuilder options, string connectionString, string version)
    {
        if (version != "AutoDetect")
        {
            options.UseMySql(connectionString, new MariaDbServerVersion(new Version(version)));
        }
        else
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}