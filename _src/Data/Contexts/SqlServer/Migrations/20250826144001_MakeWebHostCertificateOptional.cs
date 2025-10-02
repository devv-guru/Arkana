using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Contexts.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class MakeWebHostCertificateOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebHosts_Certificates_CertificateId",
                table: "WebHosts");

            migrationBuilder.AlterColumn<Guid>(
                name: "CertificateId",
                table: "WebHosts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_WebHosts_Certificates_CertificateId",
                table: "WebHosts",
                column: "CertificateId",
                principalTable: "Certificates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebHosts_Certificates_CertificateId",
                table: "WebHosts");

            migrationBuilder.AlterColumn<Guid>(
                name: "CertificateId",
                table: "WebHosts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WebHosts_Certificates_CertificateId",
                table: "WebHosts",
                column: "CertificateId",
                principalTable: "Certificates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
