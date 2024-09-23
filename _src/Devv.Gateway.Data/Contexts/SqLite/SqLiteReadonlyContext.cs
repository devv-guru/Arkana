using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.SqLite;

public class SqLiteReadonlyContext : ReadonlyDataContext
{
    public SqLiteReadonlyContext(DbContextOptions<SqLiteReadonlyContext> options) 
        : base(options)
    {
    }
}