using Devv.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Data.Configurations;

public class PassiveHealthCheckConfigConfiguration : IEntityTypeConfiguration<PassiveHealthCheckConfig>
{
    public void Configure(EntityTypeBuilder<PassiveHealthCheckConfig> builder)
    {
        // Configure properties
        builder.Property(p => p.Enabled).IsRequired();
        builder.Property(p => p.Policy).IsRequired();
    }
}