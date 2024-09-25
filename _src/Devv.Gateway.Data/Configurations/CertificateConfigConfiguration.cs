using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class CertificateConfigConfiguration : IEntityTypeConfiguration<CertificateConfig>
{
    public void Configure(EntityTypeBuilder<CertificateConfig> builder)
    {
        // Primary Key
        builder.HasKey(c => c.Id);

        // Configure relationships
        builder.HasOne(c => c.Host)
            .WithOne(h => h.Certificate)
            .HasForeignKey<Host>(h => h.CertificateId);

        // Configure properties
        builder.Property(c => c.SourceType).IsRequired();
        builder.Property(c => c.LocalPath).HasMaxLength(500);
        builder.Property(c => c.KeyVaultUri).HasMaxLength(500);
    }
}