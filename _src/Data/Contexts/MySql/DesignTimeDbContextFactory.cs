using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data.Contexts.MySql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MySqlWriteOnlyContext>
{
    public MySqlWriteOnlyContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MySqlWriteOnlyContext>();
        const string connectionString = "server=localhost;database=devv;";

        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version("9.0.1")));

        return new MySqlWriteOnlyContext(optionsBuilder.Options);
    }
}