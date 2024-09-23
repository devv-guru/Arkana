﻿using Devv.Gateway.Data.Common;
using Devv.Gateway.Data.Contexts;
using Devv.Gateway.Data.Contexts.Base;
using Devv.Gateway.Data.Contexts.MariaDb;
using Devv.Gateway.Data.Contexts.MySql;
using Devv.Gateway.Data.Contexts.Postgre;
using Devv.Gateway.Data.Contexts.SqLite;
using Devv.Gateway.Data.Contexts.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Api.Data;

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

                services.AddDbContext<SqlServerReadonlyContext>(options =>
                {
                    options.UseSqlServer(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddScoped<IReadonlyContext, SqlServerReadonlyContext>();
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

                services.AddDbContext<PostgreSqlReadonlyContext>(options =>
                {
                    options.UseNpgsql(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddScoped<IReadonlyContext, PostgreSqlReadonlyContext>();
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

                services.AddDbContext<SqLiteReadonlyContext>(options =>
                {
                    options.UseSqlite(dataContextOptions.ConnectionString,
                        x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
                });

                services.AddScoped<IReadonlyContext, SqLiteReadonlyContext>();
                services.AddScoped<IWriteOnlyContext, SqLiteWriteOnlyContext>();
                break;
            }
            case DataContextProviders.MySql:
            {
                services.AddDbContext<MySqlWriteOnlyContext>(options =>
                {
                    ConfigureMySql(options, dataContextOptions.ConnectionString, dataContextOptions.MySqlVersion);
                });

                services.AddDbContext<MySqlReadonlyContext>(options =>
                {
                    ConfigureMySql(options, dataContextOptions.ConnectionString, dataContextOptions.MySqlVersion);
                });

                services.AddScoped<IReadonlyContext, MySqlReadonlyContext>();
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

                services.AddDbContext<MariaDbReadonlyContext>(options =>
                {
                    ConfigureMariaDb(options, dataContextOptions.ConnectionString,
                        dataContextOptions.MariaDbVersion);
                });

                services.AddScoped<IReadonlyContext, MariaDbReadonlyContext>();
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