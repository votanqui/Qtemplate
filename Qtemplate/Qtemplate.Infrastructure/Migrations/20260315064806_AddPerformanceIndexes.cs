using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qtemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "RequestLogs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "EmailLogs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "AuditLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PageUrl",
                table: "Analytics",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "Analytics",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AffiliateTransactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_CreatedAt",
                table: "Templates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_IsFeatured",
                table: "Templates",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_IsNew",
                table: "Templates",
                column: "IsNew");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_SalesCount",
                table: "Templates",
                column: "SalesCount");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Status",
                table: "Templates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Status_CategoryId",
                table: "Templates",
                columns: new[] { "Status", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_CreatedAt",
                table: "RequestLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_IpAddress",
                table: "RequestLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_IsRevoked_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "IsRevoked", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_CreatedAt",
                table: "Payments",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CouponCode",
                table: "Orders",
                column: "CouponCode",
                filter: "[CouponCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_IpBlacklists_IsActive",
                table: "IpBlacklists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_IpBlacklists_IsActive_ExpiredAt",
                table: "IpBlacklists",
                columns: new[] { "IsActive", "ExpiredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_CreatedAt",
                table: "EmailLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_Status",
                table: "EmailLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserEmail",
                table: "AuditLogs",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_CreatedAt",
                table: "Analytics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_CreatedAt_IpAddress",
                table: "Analytics",
                columns: new[] { "CreatedAt", "IpAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_PageUrl",
                table: "Analytics",
                column: "PageUrl");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateTransactions_Status_CreatedAt",
                table: "AffiliateTransactions",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Templates_CreatedAt",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_IsFeatured",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_IsNew",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_SalesCount",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Status",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Status_CategoryId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_CreatedAt",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_IpAddress",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_IsRevoked_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status_CreatedAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CouponCode",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_IpBlacklists_IsActive",
                table: "IpBlacklists");

            migrationBuilder.DropIndex(
                name: "IX_IpBlacklists_IsActive_ExpiredAt",
                table: "IpBlacklists");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_CreatedAt",
                table: "EmailLogs");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_Status",
                table: "EmailLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserEmail",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Analytics_CreatedAt",
                table: "Analytics");

            migrationBuilder.DropIndex(
                name: "IX_Analytics_CreatedAt_IpAddress",
                table: "Analytics");

            migrationBuilder.DropIndex(
                name: "IX_Analytics_PageUrl",
                table: "Analytics");

            migrationBuilder.DropIndex(
                name: "IX_AffiliateTransactions_Status_CreatedAt",
                table: "AffiliateTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "RequestLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "EmailLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "PageUrl",
                table: "Analytics",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "Analytics",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AffiliateTransactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }
    }
}
