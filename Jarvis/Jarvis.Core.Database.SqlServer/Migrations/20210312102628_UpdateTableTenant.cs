using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.SqlServer.Migrations
{
    public partial class UpdateTableTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Core_OrganizationRole");

            migrationBuilder.DropIndex(
                name: "IX_Core_User_NormalizedUserName_TenantCode_DeleteIndex",
                table: "Core_User");

            migrationBuilder.DropIndex(
                name: "IX_Core_TenantHost_HostName_DeleteIndex",
                table: "Core_TenantHost");

            migrationBuilder.DropIndex(
                name: "IX_Core_Tenant_Name_DeleteIndex",
                table: "Core_Tenant");

            migrationBuilder.DropIndex(
                name: "IX_Core_Setting_Key_TenantCode_DeleteIndex",
                table: "Core_Setting");

            migrationBuilder.DropIndex(
                name: "IX_Core_Role_NormalizedName_TenantCode_DeleteIndex",
                table: "Core_Role");

            migrationBuilder.DropIndex(
                name: "IX_Core_OrganizationUnit_Name_TenantCode_DeleteIndex",
                table: "Core_OrganizationUnit");

            migrationBuilder.DropIndex(
                name: "IX_Core_Label_Name_TenantCode_DeleteIndex",
                table: "Core_Label");

            migrationBuilder.DropColumn(
                name: "DeleteIndex",
                table: "Core_User");

            migrationBuilder.DropColumn(
                name: "DeleteIndex",
                table: "Core_TenantHost");

            migrationBuilder.DropColumn(
                name: "DeleteIndex",
                table: "Core_Tenant");

            migrationBuilder.DropColumn(
                name: "DeleteIndex",
                table: "Core_Setting");

            migrationBuilder.DropColumn(
                name: "DeleteIndex",
                table: "Core_Role");

            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "Core_OrganizationUser");

            migrationBuilder.DropColumn(
                name: "DeleteIndex",
                table: "Core_OrganizationUnit");

            migrationBuilder.DropColumn(
                name: "DeleteIndex",
                table: "Core_Label");

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedVersion",
                table: "Core_User",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedVersion",
                table: "Core_TenantHost",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedVersion",
                table: "Core_Tenant",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "Core_Tenant",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedVersion",
                table: "Core_Setting",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedVersion",
                table: "Core_Role",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Core_OrganizationUser",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Core_OrganizationUser",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Core_OrganizationUser",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Core_OrganizationUser",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "IdParent",
                table: "Core_OrganizationUnit",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "DeletedVersion",
                table: "Core_OrganizationUnit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Left",
                table: "Core_OrganizationUnit",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Right",
                table: "Core_OrganizationUnit",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeletedVersion",
                table: "Core_Label",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_User_NormalizedUserName_TenantCode_DeletedVersion",
                table: "Core_User",
                columns: new[] { "NormalizedUserName", "TenantCode", "DeletedVersion" },
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL AND [DeletedVersion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_TenantHost_HostName_DeletedVersion",
                table: "Core_TenantHost",
                columns: new[] { "HostName", "DeletedVersion" },
                unique: true,
                filter: "[DeletedVersion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Tenant_Name_DeletedVersion",
                table: "Core_Tenant",
                columns: new[] { "Name", "DeletedVersion" },
                unique: true,
                filter: "[DeletedVersion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Setting_Key_TenantCode_DeletedVersion",
                table: "Core_Setting",
                columns: new[] { "Key", "TenantCode", "DeletedVersion" },
                unique: true,
                filter: "[Key] IS NOT NULL AND [DeletedVersion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Role_NormalizedName_TenantCode_DeletedVersion",
                table: "Core_Role",
                columns: new[] { "NormalizedName", "TenantCode", "DeletedVersion" },
                unique: true,
                filter: "[NormalizedName] IS NOT NULL AND [DeletedVersion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_OrganizationUnit_Name_TenantCode_DeletedVersion",
                table: "Core_OrganizationUnit",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true,
                filter: "[DeletedVersion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Label_Name_TenantCode_DeletedVersion",
                table: "Core_Label",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true,
                filter: "[Name] IS NOT NULL AND [DeletedVersion] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Core_User_NormalizedUserName_TenantCode_DeletedVersion",
                table: "Core_User");

            migrationBuilder.DropIndex(
                name: "IX_Core_TenantHost_HostName_DeletedVersion",
                table: "Core_TenantHost");

            migrationBuilder.DropIndex(
                name: "IX_Core_Tenant_Name_DeletedVersion",
                table: "Core_Tenant");

            migrationBuilder.DropIndex(
                name: "IX_Core_Setting_Key_TenantCode_DeletedVersion",
                table: "Core_Setting");

            migrationBuilder.DropIndex(
                name: "IX_Core_Role_NormalizedName_TenantCode_DeletedVersion",
                table: "Core_Role");

            migrationBuilder.DropIndex(
                name: "IX_Core_OrganizationUnit_Name_TenantCode_DeletedVersion",
                table: "Core_OrganizationUnit");

            migrationBuilder.DropIndex(
                name: "IX_Core_Label_Name_TenantCode_DeletedVersion",
                table: "Core_Label");

            migrationBuilder.DropColumn(
                name: "DeletedVersion",
                table: "Core_User");

            migrationBuilder.DropColumn(
                name: "DeletedVersion",
                table: "Core_TenantHost");

            migrationBuilder.DropColumn(
                name: "DeletedVersion",
                table: "Core_Tenant");

            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "Core_Tenant");

            migrationBuilder.DropColumn(
                name: "DeletedVersion",
                table: "Core_Setting");

            migrationBuilder.DropColumn(
                name: "DeletedVersion",
                table: "Core_Role");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Core_OrganizationUser");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Core_OrganizationUser");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Core_OrganizationUser");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Core_OrganizationUser");

            migrationBuilder.DropColumn(
                name: "DeletedVersion",
                table: "Core_OrganizationUnit");

            migrationBuilder.DropColumn(
                name: "Left",
                table: "Core_OrganizationUnit");

            migrationBuilder.DropColumn(
                name: "Right",
                table: "Core_OrganizationUnit");

            migrationBuilder.DropColumn(
                name: "DeletedVersion",
                table: "Core_Label");

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteIndex",
                table: "Core_User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "DeleteIndex",
                table: "Core_TenantHost",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeleteIndex",
                table: "Core_Tenant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeleteIndex",
                table: "Core_Setting",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteIndex",
                table: "Core_Role",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "Core_OrganizationUser",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "IdParent",
                table: "Core_OrganizationUnit",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeleteIndex",
                table: "Core_OrganizationUnit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeleteIndex",
                table: "Core_Label",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Core_OrganizationRole",
                columns: table => new
                {
                    IdRole = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationCode = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_OrganizationRole", x => new { x.IdRole, x.OrganizationCode });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Core_User_NormalizedUserName_TenantCode_DeleteIndex",
                table: "Core_User",
                columns: new[] { "NormalizedUserName", "TenantCode", "DeleteIndex" },
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_TenantHost_HostName_DeleteIndex",
                table: "Core_TenantHost",
                columns: new[] { "HostName", "DeleteIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Tenant_Name_DeleteIndex",
                table: "Core_Tenant",
                columns: new[] { "Name", "DeleteIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Setting_Key_TenantCode_DeleteIndex",
                table: "Core_Setting",
                columns: new[] { "Key", "TenantCode", "DeleteIndex" },
                unique: true,
                filter: "[Key] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Role_NormalizedName_TenantCode_DeleteIndex",
                table: "Core_Role",
                columns: new[] { "NormalizedName", "TenantCode", "DeleteIndex" },
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_OrganizationUnit_Name_TenantCode_DeleteIndex",
                table: "Core_OrganizationUnit",
                columns: new[] { "Name", "TenantCode", "DeleteIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Label_Name_TenantCode_DeleteIndex",
                table: "Core_Label",
                columns: new[] { "Name", "TenantCode", "DeleteIndex" },
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Core_OrganizationRole_IdRole_OrganizationCode",
                table: "Core_OrganizationRole",
                columns: new[] { "IdRole", "OrganizationCode" },
                unique: true);
        }
    }
}
