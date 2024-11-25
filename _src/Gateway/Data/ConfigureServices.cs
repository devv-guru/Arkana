using Data.Common;
using Data.Contexts.Base;
using Data.Contexts.Metrics;
using Data.Contexts.SqLite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Data;

public static class ConfigureServices
{
    public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration)
    {
        var dataContextOptions = configuration.GetSection(DataContextOptions.SectionName).Get<DataContextOptions>();

        var readOnlyConnectionString = new SqliteConnectionStringBuilder()
        {
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = true,
            Password = dataContextOptions.DatabasePassword,
            Cache = SqliteCacheMode.Shared
        };

        var writeOnlyConnectionString = new SqliteConnectionStringBuilder()
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true,
            Password = dataContextOptions.DatabasePassword,
            Cache = SqliteCacheMode.Shared
        };

        services.AddDbContext<SqLiteWriteOnlyContext>(options =>
        {
            options.UseSqlite(writeOnlyConnectionString.ToString(),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
        });

        services.AddDbContext<SqLiteReadOnlyContext>(options =>
        {
            options.UseSqlite(readOnlyConnectionString.ToString(),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
        });

        services.AddScoped<IWriteOnlyMetricsContext, WriteOnlyMetricsContext>();
        services.AddScoped<IReadOnlyMetricsContext, ReadOnlyMetricsContext>();

        services.AddScoped<IReadOnlyProxyContext, SqLiteReadOnlyContext>();
        services.AddScoped<IWriteOnlyProxyContext, SqLiteWriteOnlyContext>();

        return services;
    }
}