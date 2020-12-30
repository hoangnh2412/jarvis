using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.SqlServer.Migrations
{
    public partial class AddSecretKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "Core_TenantInfo",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "Core_TenantInfo");
        }
    }
}
