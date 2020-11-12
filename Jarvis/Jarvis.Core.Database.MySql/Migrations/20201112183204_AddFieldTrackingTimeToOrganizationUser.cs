using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.MySql.Migrations
{
    public partial class AddFieldTrackingTimeToOrganizationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "core_organization_user",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "core_organization_user",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "core_organization_user",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "core_organization_user");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "core_organization_user");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "core_organization_user");
        }
    }
}
