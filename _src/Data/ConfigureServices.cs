using Data.Common;
using Data.Contexts.Base;
using Data.Contexts.Metrics;
using Data.Contexts.SqLite;
using Domain.Options;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Data;

public static class ConfigureServices
{
    public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration)
    {

        var dataContextOptions = configuration.GetSection(DataContextOptions.SectionName)
            .Get<DataContextOptions>();

        using var serviceProvider = services.BuildServiceProvider();
        var environmentOptions = serviceProvider.GetRequiredService<IOptions<EnvironmentOptions>>().Value;

        var readOnlyConnectionString = CreateConnectionString(SqliteOpenMode.ReadOnly, dataContextOptions.DatabasePassword, environmentOptions);
        var writeOnlyConnectionString = CreateConnectionString(SqliteOpenMode.ReadWriteCreate, dataContextOptions.DatabasePassword, environmentOptions);

        ConfigureDbContext<SqLiteWriteOnlyContext>(services, writeOnlyConnectionString);
        ConfigureDbContext<SqLiteReadOnlyContext>(services, readOnlyConnectionString);

        services.AddScoped<IWriteOnlyMetricsContext, WriteOnlyMetricsContext>();
        services.AddScoped<IReadOnlyMetricsContext, ReadOnlyMetricsContext>();

        services.AddScoped<IReadOnlyProxyContext, SqLiteReadOnlyContext>();
        services.AddScoped<IWriteOnlyProxyContext, SqLiteWriteOnlyContext>();
        return services;
    }

    private static SqliteConnectionStringBuilder CreateConnectionString(SqliteOpenMode mode, string password,
        EnvironmentOptions environmentOptions) =>
        new SqliteConnectionStringBuilder
        {
            Mode = mode,
            Pooling = true,
            Password = password,
            Cache = SqliteCacheMode.Shared,
            DataSource = Path.Combine(environmentOptions.DataPath, "configuration")
        };

    private static void ConfigureDbContext<TContext>(IServiceCollection services, SqliteConnectionStringBuilder connectionString)
    where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlite(connectionString.ToString(),
                x => x.MigrationsAssembly(typeof(Marker).Assembly.GetName().Name!));
        });
    }

}