using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations


{
    /// <inheritdoc />
    public partial class RemoveUnwantedPolicyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Policy");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Policy");
            
            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Policy");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Policy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Policy",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Policy",
                type: "nvarchar(max)",
                nullable: true);
            
            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Policy",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Policy",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
