using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.MariaDb;

public class MariaDbReadOnlyContext : ReadOnlyDataContext
{
    public MariaDbReadOnlyContext(DbContextOptions<MariaDbReadOnlyContext> options) 
        : base(options)
    {
    }
}