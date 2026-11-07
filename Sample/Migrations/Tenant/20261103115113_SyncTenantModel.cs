using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class SyncTenantModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Org",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Org_ParentId",
                table: "Org",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Org_ParentId",
                table: "Org");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Org");
        }
    }
}
