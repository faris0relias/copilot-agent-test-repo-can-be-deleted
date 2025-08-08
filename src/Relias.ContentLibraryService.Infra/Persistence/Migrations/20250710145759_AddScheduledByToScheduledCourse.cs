using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledByToScheduledCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduledCourse_CourseId",
                table: "ScheduledCourse");

            migrationBuilder.AddColumn<string>(
                name: "ScheduledBy",
                table: "ScheduledCourse",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledCourse_CourseId_StatusId",
                table: "ScheduledCourse",
                columns: new[] { "CourseId", "StatusId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduledCourse_CourseId_StatusId",
                table: "ScheduledCourse");

            migrationBuilder.DropColumn(
                name: "ScheduledBy",
                table: "ScheduledCourse");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledCourse_CourseId",
                table: "ScheduledCourse",
                column: "CourseId");
        }
    }
}
