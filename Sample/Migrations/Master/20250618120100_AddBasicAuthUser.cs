using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddBasicAuthUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BasicAuthUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Roles = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasicAuthUser", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BasicAuthUser_Username",
                table: "BasicAuthUser",
                column: "Username",
                unique: true);

            migrationBuilder.InsertData(
                table: "BasicAuthUser",
                columns: ["Id", "Username", "Password", "Roles"],
                values: new object[]
                {
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "sampleuser",
                    "samplepass",
                    new[] { "user", "admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BasicAuthUser");
        }
    }
}
