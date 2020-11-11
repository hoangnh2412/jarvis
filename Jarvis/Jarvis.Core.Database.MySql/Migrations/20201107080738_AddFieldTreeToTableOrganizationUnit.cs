using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.MySql.Migrations
{
    public partial class AddFieldTreeToTableOrganizationUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "IdParent",
                table: "core_organization_unit",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AddColumn<int>(
                name: "Left",
                table: "core_organization_unit",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Right",
                table: "core_organization_unit",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Left",
                table: "core_organization_unit");

            migrationBuilder.DropColumn(
                name: "Right",
                table: "core_organization_unit");

            migrationBuilder.AlterColumn<Guid>(
                name: "IdParent",
                table: "core_organization_unit",
                type: "char(36)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
