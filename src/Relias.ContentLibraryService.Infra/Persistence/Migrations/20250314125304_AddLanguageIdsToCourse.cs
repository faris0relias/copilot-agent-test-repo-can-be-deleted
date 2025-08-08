using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageIdsToCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LanguageIds",
                table: "Course",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageIds",
                table: "Course");
        }
    }
}
