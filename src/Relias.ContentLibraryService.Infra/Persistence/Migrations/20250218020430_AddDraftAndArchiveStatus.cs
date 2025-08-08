using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    public partial class AddDraftAndArchiveStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Status ON;
                
                IF NOT EXISTS (SELECT 1 FROM Status WHERE StatusId = 1 AND Name = 'Draft')
                INSERT INTO Status (StatusId, Name) VALUES (1, 'Draft');
                
                IF NOT EXISTS (SELECT 1 FROM Status WHERE StatusId = 2 AND Name = 'Archived')
                INSERT INTO Status (StatusId, Name) VALUES (2, 'Archived');

                SET IDENTITY_INSERT Status OFF;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Status WHERE Name = 'Draft';");
            migrationBuilder.Sql("DELETE FROM Status WHERE Name = 'Archived';");
        }
    }
}
