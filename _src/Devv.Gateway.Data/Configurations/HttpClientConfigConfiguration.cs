using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class HttpClientConfigConfiguration : IEntityTypeConfiguration<HttpClientConfig>
{
    public void Configure(EntityTypeBuilder<HttpClientConfig> builder)
    {
        // Configure properties
        builder.Property(h => h.SslProtocols).IsRequired();
        builder.Property(h => h.MaxConnectionsPerServer).IsRequired();
    }
}