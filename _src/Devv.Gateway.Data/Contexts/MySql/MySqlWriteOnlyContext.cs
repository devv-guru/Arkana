using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.MySql;

public class MySqlWriteOnlyContext : WriteOnlyDataContext
{
    public MySqlWriteOnlyContext(DbContextOptions<MySqlWriteOnlyContext> options) 
        : base(options)
    {
    }
}