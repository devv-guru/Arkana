using Devv.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Data.Configurations;

public class QueryParameterMatchConfigConfiguration : IEntityTypeConfiguration<QueryParameterMatchConfig>
{
    public void Configure(EntityTypeBuilder<QueryParameterMatchConfig> builder)
    {
        // Configure properties
        builder.Property(q => q.Name).IsRequired();
        builder.Property(q => q.Mode).IsRequired();
    }
}