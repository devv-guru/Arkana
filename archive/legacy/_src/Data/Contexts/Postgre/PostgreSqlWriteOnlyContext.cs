using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.Postgre;

public class PostgreSqlWriteOnlyContext : WriteOnlyDataContext
{
    public PostgreSqlWriteOnlyContext(DbContextOptions<PostgreSqlWriteOnlyContext> options) 
        : base(options)
    {
    }
}