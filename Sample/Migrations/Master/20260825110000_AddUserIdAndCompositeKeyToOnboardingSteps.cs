using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddUserIdAndCompositeKeyToOnboardingSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OnboardingSteps_StepCode",
                schema: "public",
                table: "OnboardingSteps");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "public",
                table: "OnboardingSteps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingSteps_UserId_StepCode",
                schema: "public",
                table: "OnboardingSteps",
                columns: new[] { "UserId", "StepCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OnboardingSteps_AppUsers_UserId",
                schema: "public",
                table: "OnboardingSteps",
                column: "UserId",
                principalSchema: "public",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnboardingSteps_AppUsers_UserId",
                schema: "public",
                table: "OnboardingSteps");

            migrationBuilder.DropIndex(
                name: "IX_OnboardingSteps_UserId_StepCode",
                schema: "public",
                table: "OnboardingSteps");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "public",
                table: "OnboardingSteps");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingSteps_StepCode",
                schema: "public",
                table: "OnboardingSteps",
                column: "StepCode",
                unique: true);
        }
    }
}
