using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

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
    }
}