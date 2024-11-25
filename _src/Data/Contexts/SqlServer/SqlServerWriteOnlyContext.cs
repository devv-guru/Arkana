using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.SqlServer;

public class SqlServerWriteOnlyContext : WriteOnlyDataContext
{
    public SqlServerWriteOnlyContext(DbContextOptions<SqlServerWriteOnlyContext> options) 
        : base(options)
    {
    }
}