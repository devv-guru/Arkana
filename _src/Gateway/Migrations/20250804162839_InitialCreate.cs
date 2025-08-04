using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gateway.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "McpServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Endpoint = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Protocol = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthType = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthSettings = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "McpAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    McpServerId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpAuditLogs_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "McpUserAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    McpServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UserEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Roles = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpUserAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpUserAccess_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "McpServers",
                columns: new[] { "Id", "AuthSettings", "AuthType", "CreatedAt", "Description", "Endpoint", "IsEnabled", "Name", "Protocol", "UpdatedAt" },
                values: new object[] { 1, null, 1, new DateTime(2025, 8, 4, 16, 28, 39, 24, DateTimeKind.Utc).AddTicks(5639), "Basic arithmetic calculator tool", "https://localhost:7001", true, "calculator", 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_McpServerId",
                table: "McpAuditLogs",
                column: "McpServerId");

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_Timestamp",
                table: "McpAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_UserId",
                table: "McpAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_McpServers_Name",
                table: "McpServers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McpUserAccess_McpServerId_UserId",
                table: "McpUserAccess",
                columns: new[] { "McpServerId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "McpAuditLogs");

            migrationBuilder.DropTable(
                name: "McpUserAccess");

            migrationBuilder.DropTable(
                name: "McpServers");
        }
    }
}
