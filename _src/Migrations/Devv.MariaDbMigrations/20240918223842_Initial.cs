#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Devv.MariaDbMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClusterId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoadBalancingPolicy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RouteId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClusterId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: true),
                    MaxRequestBodySize = table.Column<long>(type: "bigint", nullable: true),
                    AuthorizationPolicy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorsPolicy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DestinationId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Health = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClusterConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Destinations_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HealthChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClusterConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthChecks_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HttpClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SslProtocols = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DangerousAcceptAnyServerCertificate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxConnectionsPerServer = table.Column<int>(type: "int", nullable: false),
                    EnableMultipleHttp2Connections = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequestHeaderEncoding = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseHeaderEncoding = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClusterConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HttpClients_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HttpRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActivityTimeout = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Version = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VersionPolicy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllowResponseBuffering = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ClusterConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HttpRequests_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SessionAffinities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Policy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FailurePolicy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Settings = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClusterConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionAffinities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionAffinities_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    LocalPath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KeyVaultName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KeyVaultSecretName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KeyVaultUri = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AwsSecretName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AwsRegion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RouteConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Routes_RouteConfigId",
                        column: x => x.RouteConfigId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Path = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hosts = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Methods = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RouteConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Routes_RouteConfigId",
                        column: x => x.RouteConfigId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Metadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Data = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RouteConfigId = table.Column<int>(type: "int", nullable: true),
                    ClusterConfigId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metadata_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Metadata_Routes_RouteConfigId",
                        column: x => x.RouteConfigId,
                        principalTable: "Routes",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Transforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RequestHeader = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Set = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RouteConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transforms_Routes_RouteConfigId",
                        column: x => x.RouteConfigId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActiveHealthChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Interval = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Timeout = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Policy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Path = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Query = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HealthCheckConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveHealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveHealthChecks_HealthChecks_HealthCheckConfigId",
                        column: x => x.HealthCheckConfigId,
                        principalTable: "HealthChecks",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PassiveHealthChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Policy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReactivationPeriod = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HealthCheckConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassiveHealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PassiveHealthChecks_HealthChecks_HealthCheckConfigId",
                        column: x => x.HealthCheckConfigId,
                        principalTable: "HealthChecks",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HeaderMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Values = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCaseSensitive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MatchConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeaderMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeaderMatches_Matches_MatchConfigId",
                        column: x => x.MatchConfigId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QueryParameterMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Values = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCaseSensitive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MatchConfigId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryParameterMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryParameterMatches_Matches_MatchConfigId",
                        column: x => x.MatchConfigId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveHealthChecks_HealthCheckConfigId",
                table: "ActiveHealthChecks",
                column: "HealthCheckConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_RouteConfigId",
                table: "Certificates",
                column: "RouteConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_ClusterConfigId",
                table: "Destinations",
                column: "ClusterConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderMatches_MatchConfigId",
                table: "HeaderMatches",
                column: "MatchConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthChecks_ClusterConfigId",
                table: "HealthChecks",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HttpClients_ClusterConfigId",
                table: "HttpClients",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HttpRequests_ClusterConfigId",
                table: "HttpRequests",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RouteConfigId",
                table: "Matches",
                column: "RouteConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_ClusterConfigId",
                table: "Metadata",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_RouteConfigId",
                table: "Metadata",
                column: "RouteConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PassiveHealthChecks_HealthCheckConfigId",
                table: "PassiveHealthChecks",
                column: "HealthCheckConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueryParameterMatches_MatchConfigId",
                table: "QueryParameterMatches",
                column: "MatchConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAffinities_ClusterConfigId",
                table: "SessionAffinities",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transforms_RouteConfigId",
                table: "Transforms",
                column: "RouteConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveHealthChecks");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "HeaderMatches");

            migrationBuilder.DropTable(
                name: "HttpClients");

            migrationBuilder.DropTable(
                name: "HttpRequests");

            migrationBuilder.DropTable(
                name: "Metadata");

            migrationBuilder.DropTable(
                name: "PassiveHealthChecks");

            migrationBuilder.DropTable(
                name: "QueryParameterMatches");

            migrationBuilder.DropTable(
                name: "SessionAffinities");

            migrationBuilder.DropTable(
                name: "Transforms");

            migrationBuilder.DropTable(
                name: "HealthChecks");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropTable(
                name: "Routes");
        }
    }
}
