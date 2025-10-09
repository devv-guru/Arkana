using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.MariaDb;

public class MariaDbReadOnlyContext : ReadOnlyDataContext
{
    public MariaDbReadOnlyContext(DbContextOptions<MariaDbReadOnlyContext> options) 
        : base(options)
    {
    }
}