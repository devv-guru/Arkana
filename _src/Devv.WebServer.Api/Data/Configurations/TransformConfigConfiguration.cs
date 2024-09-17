using Devv.WebServer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.WebServer.Api.Data.Configurations;

public class TransformConfigConfiguration : IEntityTypeConfiguration<TransformConfig>
{
    public void Configure(EntityTypeBuilder<TransformConfig> builder)
    {
        // Configure properties
        builder.Property(t => t.RequestHeader).IsRequired();
        builder.Property(t => t.Set).IsRequired();
    }
}