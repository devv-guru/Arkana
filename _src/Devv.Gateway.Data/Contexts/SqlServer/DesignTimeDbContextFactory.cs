using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Devv.Gateway.Data.Contexts.SqlServer;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqlServerWriteOnlyContext>
{
    public SqlServerWriteOnlyContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerWriteOnlyContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=temp;User Id=sa;Password=Password1234;");

        return new SqlServerWriteOnlyContext(optionsBuilder.Options);
    }
}