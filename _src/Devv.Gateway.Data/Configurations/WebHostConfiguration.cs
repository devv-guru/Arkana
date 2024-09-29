using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class WebHostConfiguration : IEntityTypeConfiguration<WebHost>
{
    public void Configure(EntityTypeBuilder<WebHost> builder)
    {
        builder.ToTable("WebHosts");
        // Primary Key
        builder.HasKey(h => h.Id);

        // Configure relationships
        builder.HasOne(o => o.Certificate)
            .WithMany(o => o.Hosts)
            .HasForeignKey(m => m.CertificateId)
            .IsRequired();

        builder.HasOne(h => h.Cluster)
            .WithOne(o => o.Host)
            .HasForeignKey<Cluster>(f => f.HostId);

        builder.HasMany(m => m.Routes)
            .WithOne(o => o.Host)
            .HasForeignKey(f => f.HostId);

        // Configure properties
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(h => h.HostName)
            .IsRequired()
            .HasMaxLength(500);
    }
}