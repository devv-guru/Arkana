using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class HeaderMatchConfigConfiguration : IEntityTypeConfiguration<HeaderMatchConfig>
{
    public void Configure(EntityTypeBuilder<HeaderMatchConfig> builder)
    {
        // Configure properties
        builder.Property(h => h.Name).IsRequired();
        builder.Property(h => h.Mode).IsRequired();
    }
}