using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qtemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityScanLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityScanLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Violation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAdminOverride = table.Column<bool>(type: "bit", nullable: false),
                    OverrideByEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverrideNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverrideAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityScanLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityScanLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityScanLogs_UserId",
                table: "SecurityScanLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityScanLogs_Violation_IpAddress_UserId_ScannedAt",
                table: "SecurityScanLogs",
                columns: new[] { "Violation", "IpAddress", "UserId", "ScannedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityScanLogs");
        }
    }
}
