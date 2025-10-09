using Data.Entities.Mcp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations.Mcp;

public class McpServerConfiguration : IEntityTypeConfiguration<McpServer>
{
    public void Configure(EntityTypeBuilder<McpServer> builder)
    {
        builder.ToTable("McpServers");
        
        // Primary Key
        builder.HasKey(m => m.Id);
        
        // Properties
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Unique name for the MCP server");
            
        builder.Property(m => m.Description)
            .HasMaxLength(1000)
            .HasComment("Description of the MCP server functionality");
            
        builder.Property(m => m.Endpoint)
            .IsRequired()
            .HasMaxLength(2000)
            .HasComment("WebSocket or SSE endpoint URL");
            
        builder.Property(m => m.ProtocolType)
            .HasComment("Protocol type: WebSocket, SSE, or HTTP");
            
        builder.Property(m => m.IsEnabled)
            .HasDefaultValue(true)
            .HasComment("Whether the MCP server is enabled");
            
        builder.Property(m => m.Priority)
            .HasDefaultValue(0)
            .HasComment("Priority for routing (lower = higher priority)");

        // Indexes
        builder.HasIndex(m => m.Name)
            .IsUnique()
            .HasFilter("IsDeleted = 0");
            
        builder.HasIndex(m => m.IsEnabled);
        builder.HasIndex(m => m.Priority);

        // Relationships
        builder.HasOne(m => m.BackendAuth)
            .WithOne(b => b.McpServer)
            .HasForeignKey<McpBackendAuth>(b => b.McpServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.UserAssignments)
            .WithOne(u => u.McpServer)
            .HasForeignKey(u => u.McpServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.RoleAssignments)
            .WithOne(r => r.McpServer)
            .HasForeignKey(r => r.McpServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.AuditLogs)
            .WithOne(a => a.McpServer)
            .HasForeignKey(a => a.McpServerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Soft Delete Query Filter
        builder.HasQueryFilter(m => !m.IsDeleted);
        builder.HasIndex(m => m.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}