using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Master
{
    /// <inheritdoc />
    public partial class KebabCaseTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessWorkflowMappings_AppUsers_UserId",
                table: "BusinessWorkflowMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessWorkflowMappings_OnboardingSteps_StepId",
                table: "BusinessWorkflowMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_OnboardingSteps_AppUsers_UserId",
                table: "OnboardingSteps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenant",
                table: "Tenant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Setting",
                table: "Setting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OnboardingSteps",
                table: "OnboardingSteps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BusinessWorkflowMappings",
                table: "BusinessWorkflowMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers");

            migrationBuilder.RenameTable(
                name: "Tenant",
                newName: "tenant");

            migrationBuilder.RenameTable(
                name: "Setting",
                newName: "setting");

            migrationBuilder.RenameTable(
                name: "OnboardingSteps",
                newName: "onboarding-steps");

            migrationBuilder.RenameTable(
                name: "BusinessWorkflowMappings",
                newName: "business-workflow-mappings");

            migrationBuilder.RenameTable(
                name: "AppUsers",
                newName: "app-users");

            migrationBuilder.RenameIndex(
                name: "IX_Tenant_Code",
                table: "tenant",
                newName: "IX_tenant_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Setting_TenantId_Key",
                table: "setting",
                newName: "IX_setting_TenantId_Key");

            migrationBuilder.RenameIndex(
                name: "IX_Setting_TenantId_Group",
                table: "setting",
                newName: "IX_setting_TenantId_Group");

            migrationBuilder.RenameIndex(
                name: "IX_OnboardingSteps_UserId",
                table: "onboarding-steps",
                newName: "IX_onboarding-steps_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessWorkflowMappings_UserId",
                table: "business-workflow-mappings",
                newName: "IX_business-workflow-mappings_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessWorkflowMappings_StepId",
                table: "business-workflow-mappings",
                newName: "IX_business-workflow-mappings_StepId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenant",
                table: "tenant",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_setting",
                table: "setting",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_onboarding-steps",
                table: "onboarding-steps",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_business-workflow-mappings",
                table: "business-workflow-mappings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_app-users",
                table: "app-users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_business-workflow-mappings_app-users_UserId",
                table: "business-workflow-mappings",
                column: "UserId",
                principalTable: "app-users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_business-workflow-mappings_onboarding-steps_StepId",
                table: "business-workflow-mappings",
                column: "StepId",
                principalTable: "onboarding-steps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_onboarding-steps_app-users_UserId",
                table: "onboarding-steps",
                column: "UserId",
                principalTable: "app-users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_business-workflow-mappings_app-users_UserId",
                table: "business-workflow-mappings");

            migrationBuilder.DropForeignKey(
                name: "FK_business-workflow-mappings_onboarding-steps_StepId",
                table: "business-workflow-mappings");

            migrationBuilder.DropForeignKey(
                name: "FK_onboarding-steps_app-users_UserId",
                table: "onboarding-steps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tenant",
                table: "tenant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_setting",
                table: "setting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_onboarding-steps",
                table: "onboarding-steps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_business-workflow-mappings",
                table: "business-workflow-mappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_app-users",
                table: "app-users");

            migrationBuilder.RenameTable(
                name: "tenant",
                newName: "Tenant");

            migrationBuilder.RenameTable(
                name: "setting",
                newName: "Setting");

            migrationBuilder.RenameTable(
                name: "onboarding-steps",
                newName: "OnboardingSteps");

            migrationBuilder.RenameTable(
                name: "business-workflow-mappings",
                newName: "BusinessWorkflowMappings");

            migrationBuilder.RenameTable(
                name: "app-users",
                newName: "AppUsers");

            migrationBuilder.RenameIndex(
                name: "IX_tenant_Code",
                table: "Tenant",
                newName: "IX_Tenant_Code");

            migrationBuilder.RenameIndex(
                name: "IX_setting_TenantId_Key",
                table: "Setting",
                newName: "IX_Setting_TenantId_Key");

            migrationBuilder.RenameIndex(
                name: "IX_setting_TenantId_Group",
                table: "Setting",
                newName: "IX_Setting_TenantId_Group");

            migrationBuilder.RenameIndex(
                name: "IX_onboarding-steps_UserId",
                table: "OnboardingSteps",
                newName: "IX_OnboardingSteps_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_business-workflow-mappings_UserId",
                table: "BusinessWorkflowMappings",
                newName: "IX_BusinessWorkflowMappings_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_business-workflow-mappings_StepId",
                table: "BusinessWorkflowMappings",
                newName: "IX_BusinessWorkflowMappings_StepId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenant",
                table: "Tenant",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Setting",
                table: "Setting",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OnboardingSteps",
                table: "OnboardingSteps",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessWorkflowMappings",
                table: "BusinessWorkflowMappings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessWorkflowMappings_AppUsers_UserId",
                table: "BusinessWorkflowMappings",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessWorkflowMappings_OnboardingSteps_StepId",
                table: "BusinessWorkflowMappings",
                column: "StepId",
                principalTable: "OnboardingSteps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OnboardingSteps_AppUsers_UserId",
                table: "OnboardingSteps",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id");
        }
    }
}
