using Devv.WebServer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.WebServer.Api.Data.Configurations;

public class CertificateConfigConfiguration : IEntityTypeConfiguration<CertificateConfig>
{
    public void Configure(EntityTypeBuilder<CertificateConfig> builder)
    {
        // Configure properties
        builder.Property(c => c.SourceType).IsRequired();
        builder.Property(c => c.LocalPath).HasMaxLength(500);
        builder.Property(c => c.KeyVaultUri).HasMaxLength(500);
    }
}