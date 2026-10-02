using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddOnboardingAndWorkflowMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "OnboardingSteps",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StepCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Department = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedTo = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingSteps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessWorkflowMappings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StepId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WorkflowDefinitionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WorkflowName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessWorkflowMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessWorkflowMappings_OnboardingSteps_StepId",
                        column: x => x.StepId,
                        principalSchema: "public",
                        principalTable: "OnboardingSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessWorkflowMappings_StepId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                column: "StepId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingSteps_StepCode",
                schema: "public",
                table: "OnboardingSteps",
                column: "StepCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessWorkflowMappings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OnboardingSteps",
                schema: "public");
        }
    }
}
