using Devv.Gateway.Data.Entities;
using Devv.Gateway.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class MetadataConfigConfiguration : IEntityTypeConfiguration<MetadataConfig>
{
    public void Configure(EntityTypeBuilder<MetadataConfig> builder)
    {
        var converter = new DictionaryToStringConverter();
        // Configure properties
        builder.Property(m => m.Data)
            .HasConversion(converter);
    }
}