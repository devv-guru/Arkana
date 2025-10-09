using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data.Contexts.Postgre;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlWriteOnlyContext>
{
    public PostgreSqlWriteOnlyContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlWriteOnlyContext>();
        const string connectionString = "server=localhost;database=devv;";

        optionsBuilder.UseNpgsql(connectionString);

        return new PostgreSqlWriteOnlyContext(optionsBuilder.Options);
    }
}