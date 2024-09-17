using Devv.WebServer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.WebServer.Api.Data.Configurations;

public class HttpClientConfigConfiguration : IEntityTypeConfiguration<HttpClientConfig>
{
    public void Configure(EntityTypeBuilder<HttpClientConfig> builder)
    {
        // Configure properties
        builder.Property(h => h.SslProtocols).IsRequired();
        builder.Property(h => h.MaxConnectionsPerServer).IsRequired();
    }
}