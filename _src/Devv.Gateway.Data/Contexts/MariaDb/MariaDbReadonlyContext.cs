using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.MariaDb;

public class MariaDbReadonlyContext : ReadonlyDataContext
{
    public MariaDbReadonlyContext(DbContextOptions<MariaDbReadonlyContext> options) 
        : base(options)
    {
    }
}