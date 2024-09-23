using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.SqlServer;

public class SqlServerWriteOnlyContext : WriteOnlyDataContext
{
    public SqlServerWriteOnlyContext(DbContextOptions<SqlServerWriteOnlyContext> options) 
        : base(options)
    {
    }
}