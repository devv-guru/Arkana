using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class HttpRequestConfigConfiguration : IEntityTypeConfiguration<HttpRequestConfig>
{
    public void Configure(EntityTypeBuilder<HttpRequestConfig> builder)
    {
        // Configure properties
        builder.Property(h => h.Version).IsRequired();
        builder.Property(h => h.ActivityTimeout).IsRequired();
    }
}