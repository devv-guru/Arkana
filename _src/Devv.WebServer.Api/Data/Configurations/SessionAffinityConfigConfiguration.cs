using Devv.WebServer.Api.Data.Entities;
using Devv.WebServer.Api.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.WebServer.Api.Data.Configurations;

public class SessionAffinityConfigConfiguration : IEntityTypeConfiguration<SessionAffinityConfig>
{
    public void Configure(EntityTypeBuilder<SessionAffinityConfig> builder)
    {
        var converter = new DictionaryToStringConverter();
        // Configure properties
        builder.Property(s => s.Policy).IsRequired();
        builder.Property(s => s.FailurePolicy).IsRequired();
        builder.Property(s => s.Settings)
            .HasConversion(converter);
    }
}