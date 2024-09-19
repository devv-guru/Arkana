using Microsoft.EntityFrameworkCore;

namespace Devv.Data.Contexts;

public sealed  class ReadOnlyContext : DataContext
{
    public ReadOnlyContext(DbContextOptions<ReadOnlyContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        throw new InvalidOperationException("This context is read-only and does not support modifications.");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("This context is read-only and does not support modifications.");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}