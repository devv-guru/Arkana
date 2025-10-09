using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.MySql;

public class MySqlWriteOnlyContext : WriteOnlyDataContext
{
    public MySqlWriteOnlyContext(DbContextOptions<MySqlWriteOnlyContext> options) 
        : base(options)
    {
    }
}