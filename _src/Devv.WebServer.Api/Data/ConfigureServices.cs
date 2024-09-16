using Microsoft.EntityFrameworkCore;

namespace Devv.WebServer.Api.Data;

public static class ConfigureServices
{
    public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration)
    {
        var dataContextOptions = configuration.GetSection(DataContextOptions.SectionName).Get<DataContextOptions>();

        services.AddDbContext<DataContext>(options =>
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
                {
                    if (dataContextOptions.MySqlVersion != "AutoDetect")
                        options.UseMySql(dataContextOptions.ConnectionString,
                            new MySqlServerVersion(new Version(dataContextOptions.MySqlVersion)));
                    else
                        options.UseMySql(dataContextOptions.ConnectionString,
                            ServerVersion.AutoDetect(dataContextOptions.ConnectionString));
                    break;
                }
                case DataContextProviders.MariaDb:
                {
                    if (dataContextOptions.MariaDbVersion != "AutoDetect")
                        options.UseMySql(dataContextOptions.ConnectionString,
                            new MariaDbServerVersion(new Version(dataContextOptions.MariaDbVersion)));
                    else
                        options.UseMySql(dataContextOptions.ConnectionString,
                            ServerVersion.AutoDetect(dataContextOptions.ConnectionString));

                    break;
                }

                default:
                    throw new NotSupportedException($"The provider '{dataContextOptions.Provider}' is not supported.");
            }
        });

        return services;
    }
}