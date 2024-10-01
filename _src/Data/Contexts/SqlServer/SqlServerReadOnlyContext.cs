using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.SqlServer;

public class SqlServerReadOnlyContext : ReadOnlyDataContext
{
    public SqlServerReadOnlyContext(DbContextOptions<SqlServerReadOnlyContext> options) 
        : base(options)
    {
    }
}