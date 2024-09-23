using Devv.Gateway.Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.SqLite;

public class SqLiteWriteOnlyContext : WriteOnlyDataContext
{
    public SqLiteWriteOnlyContext(DbContextOptions<SqLiteWriteOnlyContext> options) 
        : base(options)
    {
    }
}