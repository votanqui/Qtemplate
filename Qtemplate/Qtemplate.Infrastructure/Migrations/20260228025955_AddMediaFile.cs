using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qtemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Folder",
                table: "MediaFiles",
                newName: "StorageType");

            migrationBuilder.AddColumn<int>(
                name: "MediaFileId",
                table: "Templates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageType",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "MediaFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MediaFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                table: "MediaFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId1",
                table: "MediaFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_TemplateId1",
                table: "MediaFiles",
                column: "TemplateId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Templates_TemplateId1",
                table: "MediaFiles",
                column: "TemplateId1",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Templates_TemplateId1",
                table: "MediaFiles");

            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_TemplateId1",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "MediaFileId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "TemplateId1",
                table: "MediaFiles");

            migrationBuilder.RenameColumn(
                name: "StorageType",
                table: "MediaFiles",
                newName: "Folder");
        }
    }
}
