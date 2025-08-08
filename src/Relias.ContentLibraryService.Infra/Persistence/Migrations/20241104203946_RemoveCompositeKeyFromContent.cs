using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompositeKeyFromContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policy_Content_ContentId_ContentTypeId",
                table: "Policy");

            migrationBuilder.DropIndex(
                name: "IX_Policy_ContentId_ContentTypeId",
                table: "Policy");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Content_ContentId",
                table: "Content");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Content",
                table: "Content");

            migrationBuilder.DropColumn(
                name: "ContentTypeId",
                table: "Policy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Content",
                table: "Content",
                column: "ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policy_Content_ContentId",
                table: "Policy",
                column: "ContentId",
                principalTable: "Content",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policy_Content_ContentId",
                table: "Policy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Content",
                table: "Content");

            migrationBuilder.AddColumn<int>(
                name: "ContentTypeId",
                table: "Policy",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Content_ContentId",
                table: "Content",
                column: "ContentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Content",
                table: "Content",
                columns: new[] { "ContentId", "ContentTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Policy_ContentId_ContentTypeId",
                table: "Policy",
                columns: new[] { "ContentId", "ContentTypeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Policy_Content_ContentId_ContentTypeId",
                table: "Policy",
                columns: new[] { "ContentId", "ContentTypeId" },
                principalTable: "Content",
                principalColumns: new[] { "ContentId", "ContentTypeId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
