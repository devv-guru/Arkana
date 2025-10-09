using Data.Entities.Mcp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations.Mcp;

public class McpAuditLogConfiguration : IEntityTypeConfiguration<McpAuditLog>
{
    public void Configure(EntityTypeBuilder<McpAuditLog> builder)
    {
        builder.ToTable("McpAuditLogs");
        
        // Primary Key
        builder.HasKey(a => a.Id);
        
        // Properties
        builder.Property(a => a.UserId)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("OIDC subject/user identifier");
            
        builder.Property(a => a.UserEmail)
            .IsRequired()
            .HasMaxLength(320)
            .HasComment("User email address");
            
        builder.Property(a => a.EventType)
            .HasComment("Type of audit event");
            
        builder.Property(a => a.EventDescription)
            .IsRequired()
            .HasMaxLength(2000)
            .HasComment("Description of the event");
            
        builder.Property(a => a.IpAddress)
            .HasMaxLength(45)
            .HasComment("Client IP address (supports IPv6)");
            
        builder.Property(a => a.UserAgent)
            .HasMaxLength(1000)
            .HasComment("Client user agent string");
            
        builder.Property(a => a.SessionId)
            .HasMaxLength(100)
            .HasComment("Session identifier");
            
        builder.Property(a => a.AdditionalData)
            .HasMaxLength(8000)
            .HasComment("JSON serialized additional event data");
            
        builder.Property(a => a.IsSuccess)
            .HasDefaultValue(true)
            .HasComment("Whether the event was successful");
            
        builder.Property(a => a.ErrorMessage)
            .HasMaxLength(2000)
            .HasComment("Error message if event failed");
            
        builder.Property(a => a.Duration)
            .HasComment("Duration of the operation");

        // Indexes for performance (audit logs can be high volume)
        builder.HasIndex(a => a.McpServerId)
            .HasDatabaseName("IX_McpAuditLogs_McpServerId");
            
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_McpAuditLogs_UserId");
            
        builder.HasIndex(a => a.UserEmail)
            .HasDatabaseName("IX_McpAuditLogs_UserEmail");
            
        builder.HasIndex(a => a.EventType)
            .HasDatabaseName("IX_McpAuditLogs_EventType");
            
        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_McpAuditLogs_CreatedAt");
            
        builder.HasIndex(a => a.IsSuccess)
            .HasDatabaseName("IX_McpAuditLogs_IsSuccess");
            
        // Composite indexes for common queries
        builder.HasIndex(a => new { a.McpServerId, a.CreatedAt })
            .HasDatabaseName("IX_McpAuditLogs_ServerId_CreatedAt");
            
        builder.HasIndex(a => new { a.UserId, a.CreatedAt })
            .HasDatabaseName("IX_McpAuditLogs_UserId_CreatedAt");
            
        builder.HasIndex(a => new { a.EventType, a.CreatedAt })
            .HasDatabaseName("IX_McpAuditLogs_EventType_CreatedAt");

        // Note: Audit logs typically don't use soft delete - they're permanent records
        // No soft delete query filter or IsDeleted index
    }
}