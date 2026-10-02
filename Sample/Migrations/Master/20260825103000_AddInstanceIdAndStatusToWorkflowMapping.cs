using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddInstanceIdAndStatusToWorkflowMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstanceId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowStatus",
                schema: "public",
                table: "BusinessWorkflowMappings",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "NotStarted");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessWorkflowMappings_WorkflowDefinitionId_InstanceId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                columns: new[] { "WorkflowDefinitionId", "InstanceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusinessWorkflowMappings_WorkflowDefinitionId_InstanceId",
                schema: "public",
                table: "BusinessWorkflowMappings");

            migrationBuilder.DropColumn(
                name: "InstanceId",
                schema: "public",
                table: "BusinessWorkflowMappings");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "BusinessWorkflowMappings");
        }
    }
}
