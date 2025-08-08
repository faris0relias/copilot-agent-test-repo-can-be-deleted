using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPrimaryKeyOnPolicyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PolicyTag_Policy_PolicyId",
                table: "PolicyTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Policy",
                table: "Policy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Policy",
                table: "Policy",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Policy_ContentId",
                table: "Policy",
                column: "ContentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PolicyTag_Policy_PolicyId",
                table: "PolicyTag",
                column: "PolicyId",
                principalTable: "Policy",
                principalColumn: "PolicyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PolicyTag_Policy_PolicyId",
                table: "PolicyTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Policy",
                table: "Policy");

            migrationBuilder.DropIndex(
                name: "IX_Policy_ContentId",
                table: "Policy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Policy",
                table: "Policy",
                column: "ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PolicyTag_Policy_PolicyId",
                table: "PolicyTag",
                column: "PolicyId",
                principalTable: "Policy",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
