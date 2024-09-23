using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Devv.Gateway.Data.Contexts.MariaDb;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MariaDbWriteOnlyContext>
{
    public MariaDbWriteOnlyContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MariaDbWriteOnlyContext>();
        const string connectionString = "server=localhost;database=devv;";
        
        optionsBuilder.UseMySql(connectionString, new MariaDbServerVersion(new Version("11.6.1")));

        return new MariaDbWriteOnlyContext(optionsBuilder.Options);
    }
}