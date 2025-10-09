using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class ActiveHealthCheckConfiguration : IEntityTypeConfiguration<ActiveHealthCheck>
{
    public void Configure(EntityTypeBuilder<ActiveHealthCheck> builder)
    {
        builder.ToTable("ActiveHealthChecks");
        // Primary Key
        builder.HasKey(a => a.Id);
        // Configure properties
        builder.Property(a => a.Enabled).IsRequired();
        builder.Property(a => a.Interval).IsRequired();
        builder.Property(a => a.Path).IsRequired();
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}