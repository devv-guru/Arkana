using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

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
    }
}