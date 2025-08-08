using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishedAndScheduledForPublishStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            SET IDENTITY_INSERT Status ON;

            IF NOT EXISTS (SELECT 1 FROM Status WHERE StatusId = 3 AND Name = 'ScheduledForPublish')
            INSERT INTO Status (StatusId, Name) VALUES (3, 'ScheduledForPublish');

            IF NOT EXISTS (SELECT 1 FROM Status WHERE StatusId = 4 AND Name = 'Published')
            INSERT INTO Status (StatusId, Name) VALUES (4, 'Published');

            SET IDENTITY_INSERT Status OFF;
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Status WHERE Name = 'ScheduledForPublish';");
            migrationBuilder.Sql("DELETE FROM Status WHERE Name = 'Published';");
        }
    }
}
