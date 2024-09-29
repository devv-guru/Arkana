using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class HttpClientSettingsConfiguration : IEntityTypeConfiguration<HttpClientSettings>
{
    public void Configure(EntityTypeBuilder<HttpClientSettings> builder)
    {
        builder.ToTable("HttpClientSettings");
        // Primary Key
        builder.HasKey(h => h.Id);
        
        // Configure properties
        builder.Property(h => h.SslProtocols).IsRequired();
        builder.Property(h => h.MaxConnectionsPerServer).IsRequired();
    }
}