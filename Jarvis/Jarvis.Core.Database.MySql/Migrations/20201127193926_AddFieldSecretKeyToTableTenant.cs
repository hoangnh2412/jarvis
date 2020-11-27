using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.MySql.Migrations
{
    public partial class AddFieldSecretKeyToTableTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "core_tenant",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "core_tenant");
        }
    }
}
