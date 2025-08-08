using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnToPolicyAndEditContentKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Policy",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Content_ContentTypeId",
                table: "Content",
                column: "ContentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Content_ContentType_ContentTypeId",
                table: "Content",
                column: "ContentTypeId",
                principalTable: "ContentType",
                principalColumn: "ContentTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Content_ContentType_ContentTypeId",
                table: "Content");

            migrationBuilder.DropIndex(
                name: "IX_Content_ContentTypeId",
                table: "Content");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Policy");
        }
    }
}
