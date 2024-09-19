using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class RouteConfigConfiguration : IEntityTypeConfiguration<RouteConfig>
{
    public void Configure(EntityTypeBuilder<RouteConfig> builder)
    {
        // RouteConfig and MatchConfig
        builder.HasOne(r => r.Match)
            .WithOne(m => m.RouteConfig)
            .HasForeignKey<MatchConfig>(m => m.RouteConfigId);

        // RouteConfig and CertificateConfig
        builder.HasOne(r => r.Certificate)
            .WithOne(c => c.RouteConfig)
            .HasForeignKey<CertificateConfig>(c => c.RouteConfigId);

        // Configure properties
        builder.Property(r => r.RouteId).IsRequired();
        builder.Property(r => r.ClusterId).IsRequired();
    }
}