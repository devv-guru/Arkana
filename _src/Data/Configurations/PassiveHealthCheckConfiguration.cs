using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class PassiveHealthCheckConfiguration : IEntityTypeConfiguration<PassiveHealthCheck>
{
    public void Configure(EntityTypeBuilder<PassiveHealthCheck> builder)
    {
        builder.ToTable("PassiveHealthChecks");
        // Primary Key
        builder.HasKey(p => p.Id);
        // Configure properties
        builder.Property(p => p.Enabled).IsRequired();
        builder.Property(p => p.Policy).IsRequired();
        
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}