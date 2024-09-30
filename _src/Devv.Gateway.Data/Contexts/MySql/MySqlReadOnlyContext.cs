using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.MySql;

public class MySqlReadOnlyContext : ReadOnlyDataContext
{
    public MySqlReadOnlyContext(DbContextOptions<MySqlReadOnlyContext> options) 
        : base(options)
    {
    }
}