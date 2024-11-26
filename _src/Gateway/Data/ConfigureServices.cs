using Data.Common;
using Data.Contexts.Base;
using Data.Contexts.MariaDb;
using Data.Contexts.MySql;
using Data.Contexts.Postgre;
using Data.Contexts.SqLite;
using Data.Contexts.SqlServer;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using Shared.Options;

namespace Gateway.Data;

public static class ConfigureServices
{
    public static WebApplicationBuilder? AddDataContext(this WebApplicationBuilder? builder, IConfiguration configuration)
    {
        var dataContextOptions = configuration.GetSection(DataContextOptions.SectionName).Get<DataContextOptions>();
        string connectionName = "Arkana";

        switch (dataContextOptions.Provider)
        {
            case DataContextProviders.SqlServer:
                {
                    builder.AddSqlServerDbContext<SqlServerWriteOnlyContext>(connectionName, configureDbContextOptions: options =>
                    {
                        options.UseSqlServer(x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                    });

                    builder.AddSqlServerDbContext<SqlServerReadOnlyContext>(connectionName, configureDbContextOptions: options =>
                    {
                        options.UseSqlServer(x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                    });

                    builder.Services.AddScoped<IReadOnlyContext, SqlServerReadOnlyContext>();
                    builder.Services.AddScoped<IWriteOnlyContext, SqlServerWriteOnlyContext>();
                    break;
                }
            case DataContextProviders.PostgreSql:
                {
                    builder.AddNpgsqlDbContext<PostgreSqlWriteOnlyContext>(connectionName, configureDbContextOptions: options =>
                    {
                        options.UseNpgsql(x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                    });

                    builder.AddNpgsqlDbContext<PostgreSqlReadOnlyContext>(connectionName, configureDbContextOptions: options =>
                    {
                        options.UseNpgsql(x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                    });

                    builder.Services.AddScoped<IReadOnlyContext, PostgreSqlReadOnlyContext>();
                    builder.Services.AddScoped<IWriteOnlyContext, PostgreSqlWriteOnlyContext>();
                    break;
                }
            case DataContextProviders.SqLite:
                {
                    builder.Services.AddDbContext<SqLiteWriteOnlyContext>(options =>
                    {
                        options.UseSqlite(dataContextOptions.ConnectionString,
                            x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                    });

                    builder.Services.AddDbContext<SqLiteReadOnlyContext>(options =>
                    {
                        options.UseSqlite(dataContextOptions.ConnectionString,
                            x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                    });

                    builder.Services.AddScoped<IReadOnlyContext, SqLiteReadOnlyContext>();
                    builder.Services.AddScoped<IWriteOnlyContext, SqLiteWriteOnlyContext>();
                    break;
                }
            case DataContextProviders.MySql:
                {
                    builder.AddMySqlDbContext<MySqlWriteOnlyContext>(connectionName,
                        settings =>
                        {
                            settings.ServerVersion = new Version(dataContextOptions.MySqlVersion).ToString();
                        },
                        options =>
                        {
                            ConfigureMySql(options, dataContextOptions.ConnectionString, dataContextOptions.MySqlVersion);
                        });

                    builder.AddMySqlDbContext<MySqlReadOnlyContext>(connectionName,
                        settings =>
                        {
                            settings.ServerVersion = new Version(dataContextOptions.MySqlVersion).ToString();
                        },
                        options =>
                        {
                            ConfigureMySql(options, dataContextOptions.ConnectionString, dataContextOptions.MySqlVersion);
                        });

                    builder.Services.AddScoped<IReadOnlyContext, MySqlReadOnlyContext>();
                    builder.Services.AddScoped<IWriteOnlyContext, MySqlWriteOnlyContext>();
                    break;
                }
            case DataContextProviders.MariaDb:
                {
                    builder.AddMySqlDbContext<MariaDbWriteOnlyContext>(connectionName, configureDbContextOptions: options =>
                    {
                        ConfigureMariaDb(options, dataContextOptions.ConnectionString,
                            dataContextOptions.MariaDbVersion);
                    });

                    builder.AddMySqlDbContext<MariaDbReadOnlyContext>(connectionName, configureDbContextOptions: options =>
                    {
                        ConfigureMariaDb(options, dataContextOptions.ConnectionString,
                            dataContextOptions.MariaDbVersion);
                    });

                    builder.Services.AddScoped<IReadOnlyContext, MariaDbReadOnlyContext>();
                    builder.Services.AddScoped<IWriteOnlyContext, MariaDbWriteOnlyContext>();
                    break;
                }
            default:
                throw new NotSupportedException($"The provider '{dataContextOptions.Provider}' is not supported.");
        }

        return builder;
    }

    private static void ConfigureMySql(DbContextOptionsBuilder options, string connectionString, string version)
    {
        if (version != "AutoDetect")
            options.UseMySql(new MySqlServerVersion(new Version(version)),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
        else
            options.UseMySql(ServerVersion.AutoDetect(connectionString),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
    }

    private static void ConfigureMariaDb(DbContextOptionsBuilder options, string connectionString, string version)
    {
        if (version != "AutoDetect")
            options.UseMySql(new MariaDbServerVersion(new Version(version)),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
        else
            options.UseMySql(ServerVersion.AutoDetect(connectionString),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
    }
}