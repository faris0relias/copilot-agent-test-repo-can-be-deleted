using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StatusSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Module_Status", table: "Module");
            migrationBuilder.DropForeignKey(name: "FK_Certificate_Status", table: "Certificate");
            migrationBuilder.DropForeignKey(name: "FK_Lesson_Status", table: "Lesson");

            migrationBuilder.DropIndex(name: "IX_Module_StatusId", table: "Module");
            migrationBuilder.DropIndex(name: "IX_Certificate_Status", table: "Certificate");
            migrationBuilder.DropIndex(name: "IX_Lesson_StatusId", table: "Lesson");

            migrationBuilder.DropColumn(name: "StatusId", table: "Module");
            migrationBuilder.DropColumn(name: "Status", table: "Certificate");
            migrationBuilder.DropColumn(name: "StatusId", table: "Lesson");

            migrationBuilder.DropColumn(name: "Created", table: "Status");
            migrationBuilder.DropColumn(name: "CreatedBy", table: "Status");
            migrationBuilder.DropColumn(name: "LastModified", table: "Status");
            migrationBuilder.DropColumn(name: "LastModifiedBy", table: "Status");

            migrationBuilder.DropPrimaryKey(name: "PK_Status", table: "Status");

            migrationBuilder.DropColumn(name: "StatusId", table: "Status");
            migrationBuilder.AddColumn<byte>(
                name: "StatusId",
                table: "Status",
                type: "tinyint",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(name: "PK_Status", table: "Status", column: "StatusId");

            migrationBuilder.AddColumn<byte>(
                name: "StatusId",
                table: "Module",
                type: "tinyint",
                nullable: false);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "Certificate",
                type: "tinyint",
                nullable: false);

            migrationBuilder.AddColumn<byte>(
                name: "StatusId",
                table: "Lesson",
                type: "tinyint",
                nullable: false);

            migrationBuilder.CreateIndex(name: "IX_Module_StatusId", table: "Module", column: "StatusId");
            migrationBuilder.CreateIndex(name: "IX_Certificate_Status", table: "Certificate", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Lesson_StatusId", table: "Lesson", column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Module_Status",
                table: "Module",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_Status",
                table: "Certificate",
                column: "Status",
                principalTable: "Status",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_Status",
                table: "Lesson",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Module_Status", table: "Module");
            migrationBuilder.DropForeignKey(name: "FK_Certificate_Status", table: "Certificate");
            migrationBuilder.DropForeignKey(name: "FK_Lesson_Status", table: "Lesson");

            migrationBuilder.DropIndex(name: "IX_Module_StatusId", table: "Module");
            migrationBuilder.DropIndex(name: "IX_Certificate_Status", table: "Certificate");
            migrationBuilder.DropIndex(name: "IX_Lesson_StatusId", table: "Lesson");

            migrationBuilder.DropColumn(name: "StatusId", table: "Module");
            migrationBuilder.DropColumn(name: "Status", table: "Certificate");
            migrationBuilder.DropColumn(name: "StatusId", table: "Lesson");

            migrationBuilder.DropPrimaryKey(name: "PK_Status", table: "Status");

            migrationBuilder.DropColumn(name: "StatusId", table: "Status");
            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Status",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(name: "PK_Status", table: "Status", column: "StatusId");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Status",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Status",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Status",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Status",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Module",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "Status",
                table: "Certificate",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Lesson",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateIndex(name: "IX_Module_StatusId", table: "Module", column: "StatusId");
            migrationBuilder.CreateIndex(name: "IX_Certificate_Status", table: "Certificate", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Lesson_StatusId", table: "Lesson", column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Module_Status",
                table: "Module",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_Status",
                table: "Certificate",
                column: "Status",
                principalTable: "Status",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lesson_Status",
                table: "Lesson",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
