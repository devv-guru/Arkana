using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class PassiveHealthCheckConfigConfiguration : IEntityTypeConfiguration<PassiveHealthCheckConfig>
{
    public void Configure(EntityTypeBuilder<PassiveHealthCheckConfig> builder)
    {
        // Primary Key
        builder.HasKey(p => p.Id);
        // Configure properties
        builder.Property(p => p.Enabled).IsRequired();
        builder.Property(p => p.Policy).IsRequired();
    }
}