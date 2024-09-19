using Devv.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Data.Configurations;

public class ActiveHealthCheckConfigConfiguration : IEntityTypeConfiguration<ActiveHealthCheckConfig>
{
    public void Configure(EntityTypeBuilder<ActiveHealthCheckConfig> builder)
    {
        // Configure properties
        builder.Property(a => a.Enabled).IsRequired();
        builder.Property(a => a.Interval).IsRequired();
        builder.Property(a => a.Path).IsRequired();
    }
}