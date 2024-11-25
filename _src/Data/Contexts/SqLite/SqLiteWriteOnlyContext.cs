using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.SqLite;

public class SqLiteWriteOnlyContext : WriteOnlyDataContext
{
    public SqLiteWriteOnlyContext(DbContextOptions<SqLiteWriteOnlyContext> options) 
        : base(options)
    {
    }
}