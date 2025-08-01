using Data.Entities.Mcp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations.Mcp;

public class McpUserAssignmentConfiguration : IEntityTypeConfiguration<McpUserAssignment>
{
    public void Configure(EntityTypeBuilder<McpUserAssignment> builder)
    {
        builder.ToTable("McpUserAssignments");
        
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Properties
        builder.Property(u => u.UserId)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("OIDC subject/user identifier");
            
        builder.Property(u => u.UserEmail)
            .IsRequired()
            .HasMaxLength(320)
            .HasComment("User email address");
            
        builder.Property(u => u.UserDisplayName)
            .HasMaxLength(200)
            .HasComment("User display name");
            
        builder.Property(u => u.IsEnabled)
            .HasDefaultValue(true)
            .HasComment("Whether the assignment is active");
            
        builder.Property(u => u.ExpiresAt)
            .HasComment("Optional expiration date for the assignment");

        // Indexes
        builder.HasIndex(u => new { u.McpServerId, u.UserId })
            .IsUnique()
            .HasFilter("IsDeleted = 0")
            .HasDatabaseName("IX_McpUserAssignments_ServerId_UserId");
            
        builder.HasIndex(u => u.UserId)
            .HasDatabaseName("IX_McpUserAssignments_UserId");
            
        builder.HasIndex(u => u.UserEmail)
            .HasDatabaseName("IX_McpUserAssignments_UserEmail");
            
        builder.HasIndex(u => u.IsEnabled);
        builder.HasIndex(u => u.ExpiresAt);

        // Soft Delete Query Filter
        builder.HasQueryFilter(u => !u.IsDeleted);
        builder.HasIndex(u => u.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}