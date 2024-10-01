using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class HealthCheckConfiguration : IEntityTypeConfiguration<HealthCheck>
{
    public void Configure(EntityTypeBuilder<HealthCheck> builder)
    {
        builder.ToTable("HealthChecks");
        // Primary Key
        builder.HasKey(h => h.Id);

        // Configure properties
        builder.HasOne(o => o.Active)
            .WithOne(o => o.HealthCheckConfig)
            .HasForeignKey<ActiveHealthCheck>(f => f.HealthCheckConfigId)
            .IsRequired(false);

        builder.HasOne(o => o.Passive)
            .WithOne(o => o.HealthCheckConfig)
            .HasForeignKey<PassiveHealthCheck>(f => f.HealthCheckConfigId)
            .IsRequired(false);
        
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}