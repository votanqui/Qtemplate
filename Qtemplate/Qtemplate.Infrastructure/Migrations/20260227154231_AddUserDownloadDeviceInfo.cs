using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qtemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDownloadDeviceInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "UserDownloads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "UserDownloads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OS",
                table: "UserDownloads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "UserDownloads",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Browser",
                table: "UserDownloads");

            migrationBuilder.DropColumn(
                name: "Device",
                table: "UserDownloads");

            migrationBuilder.DropColumn(
                name: "OS",
                table: "UserDownloads");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "UserDownloads");
        }
    }
}
