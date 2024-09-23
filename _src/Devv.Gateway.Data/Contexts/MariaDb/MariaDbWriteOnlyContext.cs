using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.MariaDb;

public class MariaDbWriteOnlyContext : WriteOnlyDataContext
{
    public MariaDbWriteOnlyContext(DbContextOptions<MariaDbWriteOnlyContext> options) 
        : base(options)
    {
    }
}