using Devv.WebServer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.WebServer.Api.Data.Configurations;

public class HealthCheckConfigConfiguration : IEntityTypeConfiguration<HealthCheckConfig>
{
    public void Configure(EntityTypeBuilder<HealthCheckConfig> builder)
    {
        // Configure properties
        builder.Property(h => h.Active).IsRequired();
        builder.Property(h => h.Passive).IsRequired();
    }
}