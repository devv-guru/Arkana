using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.SqLite;

public class SqLiteReadOnlyContext : ReadOnlyProxyContext
{
    public SqLiteReadOnlyContext(DbContextOptions<SqLiteReadOnlyContext> options)
        : base(options)
    {
    }
}