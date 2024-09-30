using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.SqlServer;

public class SqlServerReadOnlyContext : ReadOnlyDataContext
{
    public SqlServerReadOnlyContext(DbContextOptions<SqlServerReadOnlyContext> options) 
        : base(options)
    {
    }
}