using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data.Contexts.SqLite;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqLiteWriteOnlyContext>
{
    public SqLiteWriteOnlyContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqLiteWriteOnlyContext>();
        optionsBuilder.UseSqlite("Data Source=devv.db");

        return new SqLiteWriteOnlyContext(optionsBuilder.Options);
    }
}