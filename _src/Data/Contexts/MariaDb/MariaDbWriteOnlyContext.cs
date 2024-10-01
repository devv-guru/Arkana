using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.MariaDb;

public class MariaDbWriteOnlyContext : WriteOnlyDataContext
{
    public MariaDbWriteOnlyContext(DbContextOptions<MariaDbWriteOnlyContext> options) 
        : base(options)
    {
    }
}