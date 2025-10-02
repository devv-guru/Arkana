using Data.Contexts.Base;
using Data.Contexts.SqLite;
using Data.Contexts.SqlServer;
using Data.Contexts.Postgre;
using Data.Contexts.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Data.Common;

namespace Data.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["DataContextOptions:Provider"] ?? DataContextProviders.SqLite;
        
        // Try multiple connection string names that Aspire might use
        var connectionString = configuration.GetConnectionString("Arkana") 
                            ?? configuration.GetConnectionString("DefaultConnection")
                            ?? configuration.GetConnectionString("Database");

        // Log available connection strings for debugging
        var connectionStringSection = configuration.GetSection("ConnectionStrings");
        if (connectionStringSection.Exists())
        {
            Console.WriteLine("Available ConnectionStrings:");
            foreach (var child in connectionStringSection.GetChildren())
            {
                Console.WriteLine($"  {child.Key}: {child.Value?[..Math.Min(50, child.Value?.Length ?? 0)]}...");
            }
        }
        
        Console.WriteLine($"Database Provider: {databaseProvider}");
        Console.WriteLine($"Connection String: {connectionString?[..Math.Min(50, connectionString?.Length ?? 0)]}...");

        switch (databaseProvider)
        {
            case DataContextProviders.SqlServer:
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("SQL Server connection string is required when using SQL Server provider");
                
                services.AddDbContext<SqlServerWriteOnlyContext>(options =>
                    options.UseSqlServer(connectionString));
                services.AddDbContext<SqlServerReadOnlyContext>(options =>
                    options.UseSqlServer(connectionString));
                services.AddScoped<IWriteOnlyContext>(provider => 
                    provider.GetRequiredService<SqlServerWriteOnlyContext>());
                services.AddScoped<IReadOnlyContext>(provider => 
                    provider.GetRequiredService<SqlServerReadOnlyContext>());
                break;

            case DataContextProviders.PostgreSql:
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("PostgreSQL connection string is required when using PostgreSQL provider");
                
                services.AddDbContext<PostgreSqlWriteOnlyContext>(options =>
                    options.UseNpgsql(connectionString));
                services.AddDbContext<PostgreSqlReadOnlyContext>(options =>
                    options.UseNpgsql(connectionString));
                services.AddScoped<IWriteOnlyContext>(provider => 
                    provider.GetRequiredService<PostgreSqlWriteOnlyContext>());
                services.AddScoped<IReadOnlyContext>(provider => 
                    provider.GetRequiredService<PostgreSqlReadOnlyContext>());
                break;

            case DataContextProviders.MySql:
            case DataContextProviders.MariaDb:
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("MySQL connection string is required when using MySQL provider");
                
                services.AddDbContext<MySqlWriteOnlyContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                services.AddDbContext<MySqlReadOnlyContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                services.AddScoped<IWriteOnlyContext>(provider => 
                    provider.GetRequiredService<MySqlWriteOnlyContext>());
                services.AddScoped<IReadOnlyContext>(provider => 
                    provider.GetRequiredService<MySqlReadOnlyContext>());
                break;

            case DataContextProviders.SqLite:
            default:
                connectionString ??= "Data Source=arkana-gateway.db";
                services.AddDbContext<SqLiteWriteOnlyContext>(options =>
                    options.UseSqlite(connectionString));
                services.AddDbContext<SqLiteReadOnlyContext>(options =>
                    options.UseSqlite(connectionString));
                services.AddScoped<IWriteOnlyContext>(provider => 
                    provider.GetRequiredService<SqLiteWriteOnlyContext>());
                services.AddScoped<IReadOnlyContext>(provider => 
                    provider.GetRequiredService<SqLiteReadOnlyContext>());
                break;
        }

        return services;
    }
}