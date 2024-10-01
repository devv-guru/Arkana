using Data.Common;
using Data.Contexts;
using Data.Contexts.Base;
using Data.Contexts.MariaDb;
using Data.Contexts.MySql;
using Data.Contexts.Postgre;
using Data.Contexts.SqLite;
using Data.Contexts.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Data;

public static class ConfigureServices
{
    public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration)
    {
        var dataContextOptions = configuration.GetSection(DataContextOptions.SectionName).Get<DataContextOptions>();

        switch (dataContextOptions.Provider)
        {
            case DataContextProviders.SqlServer:
            {
                services.AddDbContext<SqlServerWriteOnlyContext>(options =>
                {
                    options.UseSqlServer(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddDbContext<SqlServerReadOnlyContext>(options =>
                {
                    options.UseSqlServer(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddScoped<IReadOnlyContext, SqlServerReadOnlyContext>();
                services.AddScoped<IWriteOnlyContext, SqlServerWriteOnlyContext>();
                break;
            }
            case DataContextProviders.PostgreSql:
            {
                services.AddDbContext<PostgreSqlWriteOnlyContext>(options =>
                {
                    options.UseNpgsql(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddDbContext<PostgreSqlReadOnlyContext>(options =>
                {
                    options.UseNpgsql(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddScoped<IReadOnlyContext, PostgreSqlReadOnlyContext>();
                services.AddScoped<IWriteOnlyContext, PostgreSqlWriteOnlyContext>();
                break;
            }
            case DataContextProviders.SqLite:
            {
                services.AddDbContext<SqLiteWriteOnlyContext>(options =>
                {
                    options.UseSqlite(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddDbContext<SqLiteReadOnlyContext>(options =>
                {
                    options.UseSqlite(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddScoped<IReadOnlyContext, SqLiteReadOnlyContext>();
                services.AddScoped<IWriteOnlyContext, SqLiteWriteOnlyContext>();
                break;
            }
            case DataContextProviders.MySql:
            {
                services.AddDbContext<MySqlWriteOnlyContext>(options =>
                {
                    ConfigureMySql(options, dataContextOptions.ConnectionString, dataContextOptions.MySqlVersion);
                });

                services.AddDbContext<MySqlReadOnlyContext>(options =>
                {
                    ConfigureMySql(options, dataContextOptions.ConnectionString, dataContextOptions.MySqlVersion);
                });

                services.AddScoped<IReadOnlyContext, MySqlReadOnlyContext>();
                services.AddScoped<IWriteOnlyContext, MySqlWriteOnlyContext>();
                break;
            }
            case DataContextProviders.MariaDb:
            {
                services.AddDbContext<MariaDbWriteOnlyContext>(options =>
                {
                    ConfigureMariaDb(options, dataContextOptions.ConnectionString,
                        dataContextOptions.MariaDbVersion);
                });

                services.AddDbContext<MariaDbReadOnlyContext>(options =>
                {
                    ConfigureMariaDb(options, dataContextOptions.ConnectionString,
                        dataContextOptions.MariaDbVersion);
                });

                services.AddScoped<IReadOnlyContext, MariaDbReadOnlyContext>();
                services.AddScoped<IWriteOnlyContext, MariaDbWriteOnlyContext>();
                break;
            }
            default:
                throw new NotSupportedException($"The provider '{dataContextOptions.Provider}' is not supported.");
        }

        return services;
    }

    private static void ConfigureMySql(DbContextOptionsBuilder options, string connectionString, string version)
    {
        if (version != "AutoDetect")
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(version)),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
        else
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
    }

    private static void ConfigureMariaDb(DbContextOptionsBuilder options, string connectionString, string version)
    {
        if (version != "AutoDetect")
            options.UseMySql(connectionString, new MariaDbServerVersion(new Version(version)),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
        else
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
    }
}