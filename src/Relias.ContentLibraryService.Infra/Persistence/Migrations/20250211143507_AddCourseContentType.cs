using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    public partial class AddCourseContentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM dbo.ContentType WHERE ContentTypeDescription = 'Course') " +
                                "INSERT INTO dbo.ContentType (ContentTypeDescription) VALUES ('Course')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM dbo.ContentType WHERE ContentTypeDescription = 'Course'");
        }
    }
}
