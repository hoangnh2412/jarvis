using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddAppUserAndUserWorkflowRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Department = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Position = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Username",
                schema: "public",
                table: "AppUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessWorkflowMappings_UserId",
                schema: "public",
                table: "BusinessWorkflowMappings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessWorkflowMappings_AppUsers_UserId",
                schema: "public",
                table: "BusinessWorkflowMappings",
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
                name: "FK_BusinessWorkflowMappings_AppUsers_UserId",
                schema: "public",
                table: "BusinessWorkflowMappings");

            migrationBuilder.DropIndex(
                name: "IX_BusinessWorkflowMappings_UserId",
                schema: "public",
                table: "BusinessWorkflowMappings");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "public",
                table: "BusinessWorkflowMappings");

            migrationBuilder.DropTable(
                name: "AppUsers",
                schema: "public");
        }
    }
}
