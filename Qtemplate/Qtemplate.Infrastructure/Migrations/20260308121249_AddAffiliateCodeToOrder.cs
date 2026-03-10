using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qtemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAffiliateCodeToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AffiliateCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffiliateCode",
                table: "Orders");
        }
    }
}
