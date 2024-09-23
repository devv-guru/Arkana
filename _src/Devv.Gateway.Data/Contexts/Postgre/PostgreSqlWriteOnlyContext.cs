using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.Postgre;

public class PostgreSqlWriteOnlyContext : WriteOnlyDataContext
{
    public PostgreSqlWriteOnlyContext(DbContextOptions<PostgreSqlWriteOnlyContext> options) 
        : base(options)
    {
    }
}