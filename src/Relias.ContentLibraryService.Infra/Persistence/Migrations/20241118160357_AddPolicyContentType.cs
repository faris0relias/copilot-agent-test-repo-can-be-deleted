using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM dbo.ContentType WHERE ContentTypeDescription = 'Policy') INSERT INTO dbo.ContentType (ContentTypeDescription) VALUES ('Policy')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM dbo.ContentType WHERE ContentTypeDescription = 'Policy'");
        }
    }
}
