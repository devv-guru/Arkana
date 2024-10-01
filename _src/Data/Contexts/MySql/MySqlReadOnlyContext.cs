using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.MySql;

public class MySqlReadOnlyContext : ReadOnlyDataContext
{
    public MySqlReadOnlyContext(DbContextOptions<MySqlReadOnlyContext> options) 
        : base(options)
    {
    }
}