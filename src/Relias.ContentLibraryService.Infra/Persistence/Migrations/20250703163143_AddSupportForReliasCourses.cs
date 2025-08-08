using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relias.ContentLibraryService.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportForReliasCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Topic");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "TargetAudience");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TargetAudience");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TargetAudience");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "TargetAudience");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TargetAudience");

            migrationBuilder.DropColumn(
                name: "About",
                table: "Contributor");

            migrationBuilder.DropColumn(
                name: "Designation",
                table: "Contributor");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Contributor");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Contributor");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TargetAudience",
                newName: "Audience");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "TargetAudience",
                newName: "Active");

            // Manually added to drop existing PK constraint
            migrationBuilder.DropPrimaryKey(
                name: "PK_TargetAudience",
                table: "TargetAudience");

            // Manually added to drop the old GUID column
            migrationBuilder.DropColumn(
                name: "TargetAudienceId",
                table: "TargetAudience");

            // Manually added to add new int identity column
            migrationBuilder.AddColumn<int>(
                name: "TargetAudienceId",
                table: "TargetAudience",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Manually added to recreate primary key on new column
            migrationBuilder.AddPrimaryKey(
                name: "PK_TargetAudience",
                table: "TargetAudience",
                column: "TargetAudienceId");

            migrationBuilder.AddColumn<int>(
                name: "QuickbaseRecordId",
                table: "TargetAudience",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ArchiveBy",
                table: "Course",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchiveDate",
                table: "Course",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRelias",
                table: "Course",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewDate",
                table: "Course",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishBy",
                table: "Course",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishDate",
                table: "Course",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeToCompleteInMinutes",
                table: "Course",
                type: "int",
                nullable: true);

            // Manually added to drop existing PK constraint
            migrationBuilder.DropPrimaryKey(
                name: "PK_Contributor",
                table: "Contributor");

            // Manually added to drop the old GUID column
            migrationBuilder.DropColumn(
                name: "ContributorId",
                table: "Contributor");

            // Manually added to add new int identity column
            migrationBuilder.AddColumn<int>(
                name: "ContributorId",
                table: "Contributor",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Manually added to recreate primary key on new column
            migrationBuilder.AddPrimaryKey(
                name: "PK_Contributor",
                table: "Contributor",
                column: "ContributorId");

            migrationBuilder.AddColumn<int>(
                name: "ContributorIdNum",
                table: "Contributor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisclosureStatement",
                table: "Contributor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAndCredentials",
                table: "Contributor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortBio",
                table: "Contributor",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CareSetting",
                columns: table => new
                {
                    CareSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Setting = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuickbaseRecordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareSetting", x => x.CareSettingId);
                });

            migrationBuilder.CreateTable(
                name: "CommercialProductDisclaimer",
                columns: table => new
                {
                    CommercialProductDisclaimerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Disclaimer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialProductDisclaimer", x => x.CommercialProductDisclaimerId);
                });

            migrationBuilder.CreateTable(
                name: "CompletionRequirement",
                columns: table => new
                {
                    CompletionRequirementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Requirement = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletionRequirement", x => x.CompletionRequirementId);
                });

            migrationBuilder.CreateTable(
                name: "ContentDisclaimer",
                columns: table => new
                {
                    ContentDisclaimerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Disclaimer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentDisclaimer", x => x.ContentDisclaimerId);
                });

            migrationBuilder.CreateTable(
                name: "CulturalAwarenessStatement",
                columns: table => new
                {
                    CulturalAwarenessStatementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Statement = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CulturalAwarenessStatement", x => x.CulturalAwarenessStatementId);
                });

            migrationBuilder.CreateTable(
                name: "Disclosure",
                columns: table => new
                {
                    DisclosureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Statement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    QuickbaseRecordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disclosure", x => x.DisclosureId);
                });

            migrationBuilder.CreateTable(
                name: "RequestForAccommodations",
                columns: table => new
                {
                    RequestForAccommodationsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Request = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestForAccommodations", x => x.RequestForAccommodationsId);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledCourse",
                columns: table => new
                {
                    ScheduledCourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledCourse", x => x.ScheduledCourseId);
                    table.ForeignKey(
                        name: "FK_ScheduledCourse_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingTopic",
                columns: table => new
                {
                    TrainingTopicId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    QuickbaseRecordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingTopic", x => x.TrainingTopicId);
                });

            migrationBuilder.CreateTable(
                name: "ReliasCourseProperties",
                columns: table => new
                {
                    ReliasCoursePropertiesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Outline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Disclosure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommercialProductDisclaimerId = table.Column<int>(type: "int", nullable: true),
                    CompletionRequirementId = table.Column<int>(type: "int", nullable: true),
                    ContentDisclaimerId = table.Column<int>(type: "int", nullable: true),
                    CulturalAwarenessStatementId = table.Column<int>(type: "int", nullable: true),
                    RequestForAccommodationsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliasCourseProperties", x => x.ReliasCoursePropertiesId);
                    table.UniqueConstraint("AK_ReliasCourseProperties_CourseId", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_ReliasCourseProperties_CommercialProductDisclaimer_CommercialProductDisclaimerId",
                        column: x => x.CommercialProductDisclaimerId,
                        principalTable: "CommercialProductDisclaimer",
                        principalColumn: "CommercialProductDisclaimerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliasCourseProperties_CompletionRequirement_CompletionRequirementId",
                        column: x => x.CompletionRequirementId,
                        principalTable: "CompletionRequirement",
                        principalColumn: "CompletionRequirementId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliasCourseProperties_ContentDisclaimer_ContentDisclaimerId",
                        column: x => x.ContentDisclaimerId,
                        principalTable: "ContentDisclaimer",
                        principalColumn: "ContentDisclaimerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliasCourseProperties_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReliasCourseProperties_CulturalAwarenessStatement_CulturalAwarenessStatementId",
                        column: x => x.CulturalAwarenessStatementId,
                        principalTable: "CulturalAwarenessStatement",
                        principalColumn: "CulturalAwarenessStatementId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliasCourseProperties_RequestForAccommodations_RequestForAccommodationsId",
                        column: x => x.RequestForAccommodationsId,
                        principalTable: "RequestForAccommodations",
                        principalColumn: "RequestForAccommodationsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseCareSetting",
                columns: table => new
                {
                    CourseCareSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareSettingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCareSetting", x => x.CourseCareSettingId);
                    table.ForeignKey(
                        name: "FK_CourseCareSetting_CareSetting_CareSettingId",
                        column: x => x.CareSettingId,
                        principalTable: "CareSetting",
                        principalColumn: "CareSettingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseCareSetting_ReliasCourseProperties_CourseId",
                        column: x => x.CourseId,
                        principalTable: "ReliasCourseProperties",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseContributor",
                columns: table => new
                {
                    CourseContributorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContributorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseContributor", x => x.CourseContributorId);
                    table.ForeignKey(
                        name: "FK_CourseContributor_Contributor_ContributorId",
                        column: x => x.ContributorId,
                        principalTable: "Contributor",
                        principalColumn: "ContributorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseContributor_ReliasCourseProperties_CourseId",
                        column: x => x.CourseId,
                        principalTable: "ReliasCourseProperties",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseDisclosure",
                columns: table => new
                {
                    CourseDisclosureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisclosureId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseDisclosure", x => x.CourseDisclosureId);
                    table.ForeignKey(
                        name: "FK_CourseDisclosure_Disclosure_DisclosureId",
                        column: x => x.DisclosureId,
                        principalTable: "Disclosure",
                        principalColumn: "DisclosureId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseDisclosure_ReliasCourseProperties_CourseId",
                        column: x => x.CourseId,
                        principalTable: "ReliasCourseProperties",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseTargetAudience",
                columns: table => new
                {
                    CourseTargetAudienceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetAudienceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTargetAudience", x => x.CourseTargetAudienceId);
                    table.ForeignKey(
                        name: "FK_CourseTargetAudience_ReliasCourseProperties_CourseId",
                        column: x => x.CourseId,
                        principalTable: "ReliasCourseProperties",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseTargetAudience_TargetAudience_TargetAudienceId",
                        column: x => x.TargetAudienceId,
                        principalTable: "TargetAudience",
                        principalColumn: "TargetAudienceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseTrainingTopic",
                columns: table => new
                {
                    CourseTrainingTopicsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingTopicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTrainingTopic", x => x.CourseTrainingTopicsId);
                    table.ForeignKey(
                        name: "FK_CourseTrainingTopic_ReliasCourseProperties_CourseId",
                        column: x => x.CourseId,
                        principalTable: "ReliasCourseProperties",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseTrainingTopic_TrainingTopic_TrainingTopicId",
                        column: x => x.TrainingTopicId,
                        principalTable: "TrainingTopic",
                        principalColumn: "TrainingTopicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningObjective",
                columns: table => new
                {
                    LearningObjectiveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuickbaseRecordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningObjective", x => x.LearningObjectiveId);
                    table.ForeignKey(
                        name: "FK_LearningObjective_ReliasCourseProperties_CourseId",
                        column: x => x.CourseId,
                        principalTable: "ReliasCourseProperties",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseCareSetting_CareSettingId",
                table: "CourseCareSetting",
                column: "CareSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseCareSetting_CourseId_CareSettingId",
                table: "CourseCareSetting",
                columns: new[] { "CourseId", "CareSettingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseContributor_ContributorId",
                table: "CourseContributor",
                column: "ContributorId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseContributor_CourseId_ContributorId",
                table: "CourseContributor",
                columns: new[] { "CourseId", "ContributorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseDisclosure_CourseId_DisclosureId",
                table: "CourseDisclosure",
                columns: new[] { "CourseId", "DisclosureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseDisclosure_DisclosureId",
                table: "CourseDisclosure",
                column: "DisclosureId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseTargetAudience_CourseId_TargetAudienceId",
                table: "CourseTargetAudience",
                columns: new[] { "CourseId", "TargetAudienceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseTargetAudience_TargetAudienceId",
                table: "CourseTargetAudience",
                column: "TargetAudienceId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseTrainingTopic_CourseId_TrainingTopicId",
                table: "CourseTrainingTopic",
                columns: new[] { "CourseId", "TrainingTopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseTrainingTopic_TrainingTopicId",
                table: "CourseTrainingTopic",
                column: "TrainingTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningObjective_CourseId",
                table: "LearningObjective",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliasCourseProperties_CommercialProductDisclaimerId",
                table: "ReliasCourseProperties",
                column: "CommercialProductDisclaimerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliasCourseProperties_CompletionRequirementId",
                table: "ReliasCourseProperties",
                column: "CompletionRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliasCourseProperties_ContentDisclaimerId",
                table: "ReliasCourseProperties",
                column: "ContentDisclaimerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliasCourseProperties_CulturalAwarenessStatementId",
                table: "ReliasCourseProperties",
                column: "CulturalAwarenessStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliasCourseProperties_RequestForAccommodationsId",
                table: "ReliasCourseProperties",
                column: "RequestForAccommodationsId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledCourse_CourseId",
                table: "ScheduledCourse",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseCareSetting");

            migrationBuilder.DropTable(
                name: "CourseContributor");

            migrationBuilder.DropTable(
                name: "CourseDisclosure");

            migrationBuilder.DropTable(
                name: "CourseTargetAudience");

            migrationBuilder.DropTable(
                name: "CourseTrainingTopic");

            migrationBuilder.DropTable(
                name: "LearningObjective");

            migrationBuilder.DropTable(
                name: "ScheduledCourse");

            migrationBuilder.DropTable(
                name: "CareSetting");

            migrationBuilder.DropTable(
                name: "Disclosure");

            migrationBuilder.DropTable(
                name: "TrainingTopic");

            migrationBuilder.DropTable(
                name: "ReliasCourseProperties");

            migrationBuilder.DropTable(
                name: "CommercialProductDisclaimer");

            migrationBuilder.DropTable(
                name: "CompletionRequirement");

            migrationBuilder.DropTable(
                name: "ContentDisclaimer");

            migrationBuilder.DropTable(
                name: "CulturalAwarenessStatement");

            migrationBuilder.DropTable(
                name: "RequestForAccommodations");

            migrationBuilder.DropColumn(
                name: "QuickbaseRecordId",
                table: "TargetAudience");

            migrationBuilder.DropColumn(
                name: "ArchiveBy",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ArchiveDate",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "IsRelias",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "NextReviewDate",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "PublishBy",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "PublishDate",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "TimeToCompleteInMinutes",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ContributorIdNum",
                table: "Contributor");

            migrationBuilder.DropColumn(
                name: "DisclosureStatement",
                table: "Contributor");

            migrationBuilder.DropColumn(
                name: "NameAndCredentials",
                table: "Contributor");

            migrationBuilder.DropColumn(
                name: "ShortBio",
                table: "Contributor");

            migrationBuilder.RenameColumn(
                name: "Audience",
                table: "TargetAudience",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "TargetAudience",
                newName: "IsActive");

            // Manually added to drop PK
            migrationBuilder.DropPrimaryKey(
                name: "PK_TargetAudience",
                table: "TargetAudience");

            // Manually added to drop int column
            migrationBuilder.DropColumn(
                name: "TargetAudienceId",
                table: "TargetAudience");

            // Manually added to add Guid column with default
            migrationBuilder.AddColumn<Guid>(
                name: "TargetAudienceId",
                table: "TargetAudience",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            // Manually added to recreate PK
            migrationBuilder.AddPrimaryKey(
                name: "PK_TargetAudience",
                table: "TargetAudience",
                column: "TargetAudienceId");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "TargetAudience",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TargetAudience",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TargetAudience",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "TargetAudience",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TargetAudience",
                type: "nvarchar(max)",
                nullable: true);

            // Manually added to drop PK
            migrationBuilder.DropPrimaryKey(
                name: "PK_Contributor",
                table: "Contributor");

            // Manually added to drop int column
            migrationBuilder.DropColumn(
                name: "ContributorId",
                table: "Contributor");

            // Manually added to add Guid column with default
            migrationBuilder.AddColumn<Guid>(
                name: "ContributorId",
                table: "Contributor",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            // Manually added to recreate PK
            migrationBuilder.AddPrimaryKey(
                name: "PK_Contributor",
                table: "Contributor",
                column: "ContributorId");

            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "Contributor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "Contributor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Contributor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Contributor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topic", x => x.TopicId);
                });
        }
    }
}
