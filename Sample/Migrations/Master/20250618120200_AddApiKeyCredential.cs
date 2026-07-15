using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddApiKeyCredential : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeyCredential",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Roles = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyCredential", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeyCredential_Key",
                table: "ApiKeyCredential",
                column: "Key",
                unique: true);

            migrationBuilder.InsertData(
                table: "ApiKeyCredential",
                columns: ["Id", "Key", "OwnerName", "Roles"],
                values: new object[]
                {
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    "sample-db-api-key",
                    "database",
                    new[] { "integration" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeyCredential");
        }
    }
}
