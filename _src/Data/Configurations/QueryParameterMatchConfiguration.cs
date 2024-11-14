using Data.Entities.Proxy;
using Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class QueryParameterMatchConfiguration : IEntityTypeConfiguration<QueryParameterMatch>
{
    public void Configure(EntityTypeBuilder<QueryParameterMatch> builder)
    {
        builder.ToTable("QueryParameterMatches");
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

        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}