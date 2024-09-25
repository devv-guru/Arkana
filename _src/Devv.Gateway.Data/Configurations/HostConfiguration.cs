using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class HostConfiguration : IEntityTypeConfiguration<Host>
{
    public void Configure(EntityTypeBuilder<Host> builder)
    {
        // Primary Key
        builder.HasKey(h => h.Id);

        // Configure relationships
        builder.HasOne(o => o.Certificate)
            .WithOne(o => o.Host)
            .HasForeignKey<CertificateConfig>(m => m.HostId);

        builder.HasOne(h => h.Cluster)
            .WithOne(o => o.Host)
            .HasForeignKey<ClusterConfig>(f => f.HostId);

        builder.HasMany(m => m.Routes)
            .WithOne(o => o.Host)
            .HasForeignKey(f => f.HostId);

        // Configure properties
        builder.Property(h => h.HostName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(h => h.Url)
            .IsRequired()
            .HasMaxLength(500);
    }
}