using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qtemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderReminderSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "Orders");
        }
    }
}
