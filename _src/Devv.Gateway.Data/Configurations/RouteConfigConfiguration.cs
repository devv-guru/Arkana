using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class RouteConfigConfiguration : IEntityTypeConfiguration<RouteConfig>
{
    public void Configure(EntityTypeBuilder<RouteConfig> builder)
    {
        // Primary Key
        builder.HasKey(r => r.Id);
        
        // Configure relationships
        builder.HasOne(r => r.Match)
            .WithOne(m => m.RouteConfig)
            .HasForeignKey<MatchConfig>(m => m.RouteConfigId);

        // Configure properties
        builder.Property(r => r.ClusterId)
            .IsRequired();
        builder.Property(r => r.AuthorizationPolicy)
            .HasMaxLength(100);
        builder.Property(r => r.CorsPolicy)
            .HasMaxLength(100);
    }
}