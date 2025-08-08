using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LanguageSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModuleLanguage_Language",
                table: "ModuleLanguage");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Language");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Language");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Language");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Language");

            migrationBuilder.RenameColumn(
                name: "LanguageName",
                table: "Language",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "LanguageCode",
                table: "Language",
                newName: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleLanguage_Language_LanguageId",
                table: "ModuleLanguage",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "LanguageId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModuleLanguage_Language_LanguageId",
                table: "ModuleLanguage");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Language",
                newName: "LanguageName");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Language",
                newName: "LanguageCode");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Language",
                type: "DateTime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Language",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Language",
                type: "DateTime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Language",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleLanguage_Language",
                table: "ModuleLanguage",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "LanguageId");
        }
    }
}
