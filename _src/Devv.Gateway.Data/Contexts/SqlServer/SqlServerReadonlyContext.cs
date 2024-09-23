using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.SqlServer;

public class SqlServerReadonlyContext : ReadonlyDataContext
{
    public SqlServerReadonlyContext(DbContextOptions<SqlServerReadonlyContext> options) 
        : base(options)
    {
    }
}