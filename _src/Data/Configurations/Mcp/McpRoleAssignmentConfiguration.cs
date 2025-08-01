using Data.Entities.Mcp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations.Mcp;

public class McpRoleAssignmentConfiguration : IEntityTypeConfiguration<McpRoleAssignment>
{
    public void Configure(EntityTypeBuilder<McpRoleAssignment> builder)
    {
        builder.ToTable("McpRoleAssignments");
        
        // Primary Key
        builder.HasKey(r => r.Id);
        
        // Properties
        builder.Property(r => r.RoleName)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("OIDC role/group name");
            
        builder.Property(r => r.RoleDisplayName)
            .HasMaxLength(300)
            .HasComment("Human-readable role name");
            
        builder.Property(r => r.IsEnabled)
            .HasDefaultValue(true)
            .HasComment("Whether the assignment is active");
            
        builder.Property(r => r.ExpiresAt)
            .HasComment("Optional expiration date for the assignment");

        // Indexes
        builder.HasIndex(r => new { r.McpServerId, r.RoleName })
            .IsUnique()
            .HasFilter("IsDeleted = 0")
            .HasDatabaseName("IX_McpRoleAssignments_ServerId_RoleName");
            
        builder.HasIndex(r => r.RoleName)
            .HasDatabaseName("IX_McpRoleAssignments_RoleName");
            
        builder.HasIndex(r => r.IsEnabled);
        builder.HasIndex(r => r.ExpiresAt);

        // Soft Delete Query Filter
        builder.HasQueryFilter(r => !r.IsDeleted);
        builder.HasIndex(r => r.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}