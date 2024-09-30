using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.SqLite;

public class SqLiteReadOnlyContext : ReadOnlyDataContext
{
    public SqLiteReadOnlyContext(DbContextOptions<SqLiteReadOnlyContext> options) 
        : base(options)
    {
    }
}