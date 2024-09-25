using Devv.Gateway.Data.Entities;
using Devv.Gateway.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class QueryParameterMatchConfigConfiguration : IEntityTypeConfiguration<QueryParameterMatchConfig>
{
    public void Configure(EntityTypeBuilder<QueryParameterMatchConfig> builder)
    {
        // Primary Key
        builder.HasKey(q => q.Id);

        // Value converter
        var converter = new ListToStringConverter();

        // Value comparer
        var stringCollectionComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1.SequenceEqual(c2), // Comparison logic
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Hash code generation
            c => c.ToList()); // Cloning logic, deep copy

        // Configure properties
        builder.Property(q => q.Name).IsRequired();
        builder.Property(q => q.Mode).IsRequired();
        builder.Property(q => q.Values)
            .HasConversion(converter)
            .Metadata.SetValueComparer(stringCollectionComparer);
    }
}