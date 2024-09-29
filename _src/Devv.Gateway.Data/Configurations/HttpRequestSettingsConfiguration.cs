using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class HttpRequestSettingsConfiguration : IEntityTypeConfiguration<HttpRequestSettings>
{
    public void Configure(EntityTypeBuilder<HttpRequestSettings> builder)
    {
        builder.ToTable("HttpRequestSettings");
        // Primary Key
        builder.HasKey(h => h.Id);
        // Configure properties
        builder.Property(h => h.Version).IsRequired();
        builder.Property(h => h.ActivityTimeout).IsRequired();
    }
}