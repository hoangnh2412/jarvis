using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.Postgres.Migrations
{
    public partial class UpdateTableTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Skin",
                schema: "core",
                table: "tenant",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BucketName",
                schema: "core",
                table: "file",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skin",
                schema: "core",
                table: "tenant");

            migrationBuilder.DropColumn(
                name: "BucketName",
                schema: "core",
                table: "file");
        }
    }
}
