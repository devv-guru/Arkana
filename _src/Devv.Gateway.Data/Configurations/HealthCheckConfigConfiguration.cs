using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class HealthCheckConfigConfiguration : IEntityTypeConfiguration<HealthCheckConfig>
{
    public void Configure(EntityTypeBuilder<HealthCheckConfig> builder)
    {
        // Configure properties
        builder.HasOne(o => o.Active)
            .WithOne(o => o.HealthCheckConfig)
            .HasForeignKey<ActiveHealthCheckConfig>(f => f.HealthCheckConfigId)
            .IsRequired(false);
        
        builder.HasOne(o => o.Passive)
            .WithOne(o => o.HealthCheckConfig)
            .HasForeignKey<PassiveHealthCheckConfig>(f => f.HealthCheckConfigId)
            .IsRequired(false);
    }
}