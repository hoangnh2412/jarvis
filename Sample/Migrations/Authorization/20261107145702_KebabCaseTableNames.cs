using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Authorization
{
    /// <inheritdoc />
    public partial class KebabCaseTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_policy_rules_policies_PolicyId",
                schema: "identity",
                table: "policy_rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_policy_rules",
                schema: "identity",
                table: "policy_rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRefreshTokens",
                schema: "identity",
                table: "UserRefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SampleExpenses",
                table: "SampleExpenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                schema: "identity",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                schema: "identity",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                schema: "identity",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                schema: "identity",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "policy_rules",
                schema: "identity",
                newName: "policy-rules",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "UserRefreshTokens",
                schema: "identity",
                newName: "user-refresh-tokens",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "SampleExpenses",
                newName: "sample-expenses");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "identity",
                newName: "asp-net-users",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "identity",
                newName: "asp-net-user-roles",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "identity",
                newName: "asp-net-user-claims",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "identity",
                newName: "asp-net-roles",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "identity",
                newName: "asp-net-role-claims",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_policy_rules_PolicyId_Order",
                schema: "identity",
                table: "policy-rules",
                newName: "IX_policy-rules_PolicyId_Order");

            migrationBuilder.RenameIndex(
                name: "IX_UserRefreshTokens_TokenHash",
                schema: "identity",
                table: "user-refresh-tokens",
                newName: "IX_user-refresh-tokens_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoles_RoleLevel",
                schema: "identity",
                table: "asp-net-roles",
                newName: "IX_asp-net-roles_RoleLevel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_policy-rules",
                schema: "identity",
                table: "policy-rules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user-refresh-tokens",
                schema: "identity",
                table: "user-refresh-tokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sample-expenses",
                table: "sample-expenses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_asp-net-users",
                schema: "identity",
                table: "asp-net-users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_asp-net-user-roles",
                schema: "identity",
                table: "asp-net-user-roles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_asp-net-user-claims",
                schema: "identity",
                table: "asp-net-user-claims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_asp-net-roles",
                schema: "identity",
                table: "asp-net-roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_asp-net-role-claims",
                schema: "identity",
                table: "asp-net-role-claims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_policy-rules_policies_PolicyId",
                schema: "identity",
                table: "policy-rules",
                column: "PolicyId",
                principalSchema: "identity",
                principalTable: "policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_policy-rules_policies_PolicyId",
                schema: "identity",
                table: "policy-rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user-refresh-tokens",
                schema: "identity",
                table: "user-refresh-tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sample-expenses",
                table: "sample-expenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_policy-rules",
                schema: "identity",
                table: "policy-rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_asp-net-users",
                schema: "identity",
                table: "asp-net-users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_asp-net-user-roles",
                schema: "identity",
                table: "asp-net-user-roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_asp-net-user-claims",
                schema: "identity",
                table: "asp-net-user-claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_asp-net-roles",
                schema: "identity",
                table: "asp-net-roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_asp-net-role-claims",
                schema: "identity",
                table: "asp-net-role-claims");

            migrationBuilder.RenameTable(
                name: "user-refresh-tokens",
                schema: "identity",
                newName: "UserRefreshTokens",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "sample-expenses",
                newName: "SampleExpenses");

            migrationBuilder.RenameTable(
                name: "policy-rules",
                schema: "identity",
                newName: "policy_rules",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "asp-net-users",
                schema: "identity",
                newName: "AspNetUsers",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "asp-net-user-roles",
                schema: "identity",
                newName: "AspNetUserRoles",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "asp-net-user-claims",
                schema: "identity",
                newName: "AspNetUserClaims",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "asp-net-roles",
                schema: "identity",
                newName: "AspNetRoles",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "asp-net-role-claims",
                schema: "identity",
                newName: "AspNetRoleClaims",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_user-refresh-tokens_TokenHash",
                schema: "identity",
                table: "UserRefreshTokens",
                newName: "IX_UserRefreshTokens_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_policy-rules_PolicyId_Order",
                schema: "identity",
                table: "policy_rules",
                newName: "IX_policy_rules_PolicyId_Order");

            migrationBuilder.RenameIndex(
                name: "IX_asp-net-roles_RoleLevel",
                schema: "identity",
                table: "AspNetRoles",
                newName: "IX_AspNetRoles_RoleLevel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRefreshTokens",
                schema: "identity",
                table: "UserRefreshTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SampleExpenses",
                table: "SampleExpenses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_policy_rules",
                schema: "identity",
                table: "policy_rules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                schema: "identity",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                schema: "identity",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                schema: "identity",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                schema: "identity",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                schema: "identity",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_policy_rules_policies_PolicyId",
                schema: "identity",
                table: "policy_rules",
                column: "PolicyId",
                principalSchema: "identity",
                principalTable: "policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
