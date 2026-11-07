using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class OrgStructureHostEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrgStructureJobGrade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgStructureJobGrade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrgStructureJobPosition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgStructureJobPosition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrgStructureJobTitle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgStructureJobTitle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrgStructureLegalEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DetailsJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgStructureLegalEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrgStructurePosition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationalUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobTitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportsToPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgStructurePosition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrgStructureUnit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UnitType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgStructureUnit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrgStructureUnitMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationalUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobPositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobTitleId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgStructureUnitMember", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrgStructureUnitType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Icon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgStructureUnitType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureJobGrade_OrgId_Code",
                table: "OrgStructureJobGrade",
                columns: new[] { "OrgId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureJobPosition_OrgId_Code",
                table: "OrgStructureJobPosition",
                columns: new[] { "OrgId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureJobTitle_OrgId_Code",
                table: "OrgStructureJobTitle",
                columns: new[] { "OrgId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureLegalEntity_OrgId_Code",
                table: "OrgStructureLegalEntity",
                columns: new[] { "OrgId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructurePosition_OrganizationalUnitId",
                table: "OrgStructurePosition",
                column: "OrganizationalUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructurePosition_ReportsToPositionId",
                table: "OrgStructurePosition",
                column: "ReportsToPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureUnit_OrgId_Code",
                table: "OrgStructureUnit",
                columns: new[] { "OrgId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureUnit_ParentId",
                table: "OrgStructureUnit",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureUnit_Path",
                table: "OrgStructureUnit",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureUnitMember_OrganizationalUnitId_UserId",
                table: "OrgStructureUnitMember",
                columns: new[] { "OrganizationalUnitId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgStructureUnitType_OrgId_Code",
                table: "OrgStructureUnitType",
                columns: new[] { "OrgId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrgStructureJobGrade");

            migrationBuilder.DropTable(
                name: "OrgStructureJobPosition");

            migrationBuilder.DropTable(
                name: "OrgStructureJobTitle");

            migrationBuilder.DropTable(
                name: "OrgStructureLegalEntity");

            migrationBuilder.DropTable(
                name: "OrgStructurePosition");

            migrationBuilder.DropTable(
                name: "OrgStructureUnit");

            migrationBuilder.DropTable(
                name: "OrgStructureUnitMember");

            migrationBuilder.DropTable(
                name: "OrgStructureUnitType");
        }
    }
}
