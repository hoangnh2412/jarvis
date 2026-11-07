using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sample.AuthorizationDemo;

#nullable disable

namespace Sample.Migrations.Authorization;

/// <summary>Cột refresh token lưu UTC, kiểu timestamp không kèm timezone.</summary>
[DbContext(typeof(SampleAuthorizationDbContext))]
[Migration("20261106144600_UserRefreshTokenTimestampWithoutTimeZone")]
public class UserRefreshTokenTimestampWithoutTimeZone : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpiresAtUtc",
            schema: "identity",
            table: "UserRefreshTokens",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAtUtc",
            schema: "identity",
            table: "UserRefreshTokens",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "RevokedAtUtc",
            schema: "identity",
            table: "UserRefreshTokens",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpiresAtUtc",
            schema: "identity",
            table: "UserRefreshTokens",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAtUtc",
            schema: "identity",
            table: "UserRefreshTokens",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "RevokedAtUtc",
            schema: "identity",
            table: "UserRefreshTokens",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true);
    }
}
