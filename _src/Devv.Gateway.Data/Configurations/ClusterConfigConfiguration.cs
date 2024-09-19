using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class ClusterConfigConfiguration : IEntityTypeConfiguration<ClusterConfig>
{
    public void Configure(EntityTypeBuilder<ClusterConfig> builder)
    {
        // ClusterConfig and SessionAffinityConfig
        builder.HasOne(c => c.SessionAffinity)
            .WithOne(s => s.ClusterConfig)
            .HasForeignKey<SessionAffinityConfig>(s => s.ClusterConfigId);

        // ClusterConfig and HealthCheckConfig
        builder.HasOne(c => c.HealthCheck)
            .WithOne(h => h.ClusterConfig)
            .HasForeignKey<HealthCheckConfig>(h => h.ClusterConfigId);

        // Configure properties
        builder.Property(c => c.ClusterId).IsRequired();
        builder.Property(c => c.LoadBalancingPolicy).IsRequired();
    }
}