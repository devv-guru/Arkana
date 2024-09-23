using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.MySql;

public class MySqlReadonlyContext : ReadonlyDataContext
{
    public MySqlReadonlyContext(DbContextOptions<MySqlReadonlyContext> options) 
        : base(options)
    {
    }
}