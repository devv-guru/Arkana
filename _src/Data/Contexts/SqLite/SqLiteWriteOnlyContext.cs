using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.SqLite;

public class SqLiteWriteOnlyContext : WriteOnlyProxyContext
{
    public SqLiteWriteOnlyContext(DbContextOptions<SqLiteWriteOnlyContext> options)
        : base(options)
    {
    }
}