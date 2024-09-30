using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.Postgre;

public class PostgreSqlReadOnlyContext : ReadOnlyDataContext
{
    public PostgreSqlReadOnlyContext(DbContextOptions<PostgreSqlReadOnlyContext> options) 
        : base(options)
    {
    }
}