using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestrictContentTypeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Content_ContentType_ContentTypeId",
                table: "Content");

            migrationBuilder.AddForeignKey(
                name: "FK_Content_ContentType_ContentTypeId",
                table: "Content",
                column: "ContentTypeId",
                principalTable: "ContentType",
                principalColumn: "ContentTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Content_ContentType_ContentTypeId",
                table: "Content");

            migrationBuilder.AddForeignKey(
                name: "FK_Content_ContentType_ContentTypeId",
                table: "Content",
                column: "ContentTypeId",
                principalTable: "ContentType",
                principalColumn: "ContentTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
