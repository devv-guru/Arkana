using Microsoft.EntityFrameworkCore;

namespace Devv.Data.Contexts;

public sealed  class WriteOnlyContext : DataContext
{
    public WriteOnlyContext(DbContextOptions<WriteOnlyContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        // Implement any specific logic for write operations here
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Implement any specific logic for write operations here
        return base.SaveChangesAsync(cancellationToken);
    }
}