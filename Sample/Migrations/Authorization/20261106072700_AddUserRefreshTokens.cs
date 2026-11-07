using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sample.AuthorizationDemo;

#nullable disable

namespace Sample.Migrations.Authorization;

/// <summary>Bảng refresh token của identity.</summary>
[DbContext(typeof(SampleAuthorizationDbContext))]
[Migration("20261106072700_AddUserRefreshTokens")]
public class AddUserRefreshTokens : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserRefreshTokens",
            schema: "identity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                RevokedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRefreshTokens", column => column.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserRefreshTokens_TokenHash",
            schema: "identity",
            table: "UserRefreshTokens",
            column: "TokenHash",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserRefreshTokens",
            schema: "identity");
    }
}
