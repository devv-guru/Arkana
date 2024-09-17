using Devv.WebServer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.WebServer.Api.Data.Configurations;

public class MetadataConfigConfiguration : IEntityTypeConfiguration<MetadataConfig>
{
    public void Configure(EntityTypeBuilder<MetadataConfig> builder)
    {
        // Configure properties
        builder.Property(m => m.Data).IsRequired();
    }
}