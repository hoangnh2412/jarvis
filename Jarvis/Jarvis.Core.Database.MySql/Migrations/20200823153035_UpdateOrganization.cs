using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.MySql.Migrations
{
    public partial class UpdateOrganization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "core_organization_role");

            migrationBuilder.DropColumn(
                name: "IsManager",
                table: "core_organization_user");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "core_organization_user",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "core_organization_user");

            migrationBuilder.AddColumn<bool>(
                name: "IsManager",
                table: "core_organization_user",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "core_organization_role",
                columns: table => new
                {
                    IdRole = table.Column<string>(type: "char(36)", nullable: false),
                    OrganizationCode = table.Column<string>(type: "char(36)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_organization_role", x => new { x.IdRole, x.OrganizationCode });
                });

            migrationBuilder.CreateIndex(
                name: "IX_core_organization_role_IdRole_OrganizationCode",
                table: "core_organization_role",
                columns: new[] { "IdRole", "OrganizationCode" },
                unique: true);
        }
    }
}
