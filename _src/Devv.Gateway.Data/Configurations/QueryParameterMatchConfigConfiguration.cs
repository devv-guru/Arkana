using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class QueryParameterMatchConfigConfiguration : IEntityTypeConfiguration<QueryParameterMatchConfig>
{
    public void Configure(EntityTypeBuilder<QueryParameterMatchConfig> builder)
    {
        // Configure properties
        builder.Property(q => q.Name).IsRequired();
        builder.Property(q => q.Mode).IsRequired();
    }
}