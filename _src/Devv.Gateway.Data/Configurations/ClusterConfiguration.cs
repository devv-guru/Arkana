using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class ClusterConfiguration : IEntityTypeConfiguration<Cluster>
{
    public void Configure(EntityTypeBuilder<Cluster> builder)
    {
        builder.ToTable("Clusters");
        // Primary Key
        builder.HasKey(c => c.Id);

        // Configure relationships
        builder.HasOne(c => c.SessionAffinity)
            .WithOne(s => s.ClusterConfig)
            .HasForeignKey<SessionAffinity>(s => s.ClusterConfigId);

        builder.HasOne(c => c.HealthCheck)
            .WithOne(h => h.ClusterConfig)
            .HasForeignKey<HealthCheck>(h => h.ClusterConfigId);

        builder.HasOne(c => c.Metadata)
            .WithOne(m => m.Cluster)
            .HasForeignKey<Metadata>(m => m.ClusterConfigId);

        // Configure properties
        builder.Property(c => c.LoadBalancingPolicy)
            .IsRequired();
    }
}