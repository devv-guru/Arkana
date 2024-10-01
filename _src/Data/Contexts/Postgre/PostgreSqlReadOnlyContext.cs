using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.Postgre;

public class PostgreSqlReadOnlyContext : ReadOnlyDataContext
{
    public PostgreSqlReadOnlyContext(DbContextOptions<PostgreSqlReadOnlyContext> options) 
        : base(options)
    {
    }
}