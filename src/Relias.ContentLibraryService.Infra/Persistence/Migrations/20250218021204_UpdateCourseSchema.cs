using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    public partial class UpdateCourseSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Module_ModuleId",
                table: "Course");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Course",
                table: "Course");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Course",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Course",
                type: "int",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ContentId",
                table: "Course",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "ContentCode",
                table: "Course",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Course",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Course",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BriefDescription",
                table: "Course",
                type: "nvarchar(140)",
                maxLength: 140,
                nullable: false);

            migrationBuilder.AddColumn<byte>(
                name: "StatusId",
                table: "Course",
                type: "tinyint",
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Course",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Course",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Course",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Course",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.DropColumn(name: "ModuleId", table: "Course");
            migrationBuilder.DropColumn(name: "Outline", table: "Course");
            migrationBuilder.DropColumn(name: "LearningObjectives", table: "Course");
            migrationBuilder.DropColumn(name: "Is508Compliant", table: "Course");
            migrationBuilder.DropColumn(name: "DisclosureStatement", table: "Course");
            migrationBuilder.DropColumn(name: "TopicIds", table: "Course");
            migrationBuilder.DropColumn(name: "ContributorIds", table: "Course");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Course",
                table: "Course",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Content_ContentId",
                table: "Course",
                column: "ContentId",
                principalTable: "Content",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Status_StatusId",
                table: "Course",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Content_ContentId",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Status_StatusId",
                table: "Course");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Course",
                table: "Course");

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "Course",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<string>(
                name: "Outline",
                table: "Course",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LearningObjectives",
                table: "Course",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Is508Compliant",
                table: "Course",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DisclosureStatement",
                table: "Course",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TopicIds",
                table: "Course",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContributorIds",
                table: "Course",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(name: "CourseId", table: "Course");
            migrationBuilder.DropColumn(name: "OrganizationId", table: "Course");
            migrationBuilder.DropColumn(name: "ContentId", table: "Course");
            migrationBuilder.DropColumn(name: "ContentCode", table: "Course");
            migrationBuilder.DropColumn(name: "Title", table: "Course");
            migrationBuilder.DropColumn(name: "Description", table: "Course");
            migrationBuilder.DropColumn(name: "BriefDescription", table: "Course");
            migrationBuilder.DropColumn(name: "StatusId", table: "Course");
            migrationBuilder.DropColumn(name: "Created", table: "Course");
            migrationBuilder.DropColumn(name: "CreatedBy", table: "Course");
            migrationBuilder.DropColumn(name: "LastModified", table: "Course");
            migrationBuilder.DropColumn(name: "LastModifiedBy", table: "Course");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Course",
                table: "Course",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Module_ModuleId",
                table: "Course",
                column: "ModuleId",
                principalTable: "Module",
                principalColumn: "ModuleId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
