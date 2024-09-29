using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");
        // Primary Key
        builder.HasKey(c => c.Id);
        // Configure properties
        builder.Property(c => c.Name).HasMaxLength(500);
        builder.Property(c => c.CertificateSource).IsRequired();
        builder.Property(c => c.KeyVaultUri).HasMaxLength(1000);
        builder.Property(c => c.KeyVaultName).HasMaxLength(500);
        builder.Property(c => c.KeyVaultCertificateName).HasMaxLength(500);
        builder.Property(c => c.KeyVaultCertificatePasswordName).HasMaxLength(500);
        builder.Property(c => c.AwsRegion).HasMaxLength(500);
        builder.Property(c => c.AwsCertificateName).HasMaxLength(500);
        builder.Property(c => c.AwsCertificatePasswordName).HasMaxLength(500);
    }
}