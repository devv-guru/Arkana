using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.Postgre;

public class PostgreSqlReadonlyContext : ReadonlyDataContext
{
    public PostgreSqlReadonlyContext(DbContextOptions<PostgreSqlReadonlyContext> options) 
        : base(options)
    {
    }
}