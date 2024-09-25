using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class HttpClientConfigConfiguration : IEntityTypeConfiguration<HttpClientConfig>
{
    public void Configure(EntityTypeBuilder<HttpClientConfig> builder)
    {
        // Primary Key
        builder.HasKey(h => h.Id);
        
        // Configure properties
        builder.Property(h => h.SslProtocols).IsRequired();
        builder.Property(h => h.MaxConnectionsPerServer).IsRequired();
    }
}