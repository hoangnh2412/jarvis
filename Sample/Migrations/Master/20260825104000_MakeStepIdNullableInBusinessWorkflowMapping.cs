using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Master
{
    /// <inheritdoc />
    public partial class MakeStepIdNullableInBusinessWorkflowMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessWorkflowMappings_OnboardingSteps_StepId",
                schema: "public",
                table: "BusinessWorkflowMappings");

            migrationBuilder.AlterColumn<Guid>(
                name: "StepId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "StepCode",
                schema: "public",
                table: "BusinessWorkflowMappings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessWorkflowMappings_OnboardingSteps_StepId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                column: "StepId",
                principalSchema: "public",
                principalTable: "OnboardingSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessWorkflowMappings_OnboardingSteps_StepId",
                schema: "public",
                table: "BusinessWorkflowMappings");

            migrationBuilder.AlterColumn<Guid>(
                name: "StepId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StepCode",
                schema: "public",
                table: "BusinessWorkflowMappings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessWorkflowMappings_OnboardingSteps_StepId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                column: "StepId",
                principalSchema: "public",
                principalTable: "OnboardingSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
