using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class OrgStructureSnakeCaseTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_org-structure-unit-type",
                table: "org-structure-unit-type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org-structure-unit-member",
                table: "org-structure-unit-member");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org-structure-unit",
                table: "org-structure-unit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org-structure-position",
                table: "org-structure-position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org-structure-legal-entity",
                table: "org-structure-legal-entity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org-structure-job-title",
                table: "org-structure-job-title");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org-structure-job-position",
                table: "org-structure-job-position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org-structure-job-grade",
                table: "org-structure-job-grade");

            migrationBuilder.RenameTable(
                name: "org-structure-unit-type",
                newName: "org_structure_unit_type");

            migrationBuilder.RenameTable(
                name: "org-structure-unit-member",
                newName: "org_structure_unit_member");

            migrationBuilder.RenameTable(
                name: "org-structure-unit",
                newName: "org_structure_unit");

            migrationBuilder.RenameTable(
                name: "org-structure-position",
                newName: "org_structure_position");

            migrationBuilder.RenameTable(
                name: "org-structure-legal-entity",
                newName: "org_structure_legal_entity");

            migrationBuilder.RenameTable(
                name: "org-structure-job-title",
                newName: "org_structure_job_title");

            migrationBuilder.RenameTable(
                name: "org-structure-job-position",
                newName: "org_structure_job_position");

            migrationBuilder.RenameTable(
                name: "org-structure-job-grade",
                newName: "org_structure_job_grade");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit-type_OrgId_Code",
                table: "org_structure_unit_type",
                newName: "IX_org_structure_unit_type_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit-member_OrganizationalUnitId_UserId",
                table: "org_structure_unit_member",
                newName: "IX_org_structure_unit_member_OrganizationalUnitId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit_Path",
                table: "org_structure_unit",
                newName: "IX_org_structure_unit_Path");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit_ParentId",
                table: "org_structure_unit",
                newName: "IX_org_structure_unit_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit_OrgId_Code",
                table: "org_structure_unit",
                newName: "IX_org_structure_unit_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-position_ReportsToPositionId",
                table: "org_structure_position",
                newName: "IX_org_structure_position_ReportsToPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-position_OrganizationalUnitId",
                table: "org_structure_position",
                newName: "IX_org_structure_position_OrganizationalUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-legal-entity_OrgId_Code",
                table: "org_structure_legal_entity",
                newName: "IX_org_structure_legal_entity_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-job-title_OrgId_Code",
                table: "org_structure_job_title",
                newName: "IX_org_structure_job_title_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-job-position_OrgId_Code",
                table: "org_structure_job_position",
                newName: "IX_org_structure_job_position_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-job-grade_OrgId_Code",
                table: "org_structure_job_grade",
                newName: "IX_org_structure_job_grade_OrgId_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org_structure_unit_type",
                table: "org_structure_unit_type",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org_structure_unit_member",
                table: "org_structure_unit_member",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org_structure_unit",
                table: "org_structure_unit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org_structure_position",
                table: "org_structure_position",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org_structure_legal_entity",
                table: "org_structure_legal_entity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org_structure_job_title",
                table: "org_structure_job_title",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org_structure_job_position",
                table: "org_structure_job_position",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org_structure_job_grade",
                table: "org_structure_job_grade",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_org_structure_unit_type",
                table: "org_structure_unit_type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org_structure_unit_member",
                table: "org_structure_unit_member");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org_structure_unit",
                table: "org_structure_unit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org_structure_position",
                table: "org_structure_position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org_structure_legal_entity",
                table: "org_structure_legal_entity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org_structure_job_title",
                table: "org_structure_job_title");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org_structure_job_position",
                table: "org_structure_job_position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org_structure_job_grade",
                table: "org_structure_job_grade");

            migrationBuilder.RenameTable(
                name: "org_structure_unit_type",
                newName: "org-structure-unit-type");

            migrationBuilder.RenameTable(
                name: "org_structure_unit_member",
                newName: "org-structure-unit-member");

            migrationBuilder.RenameTable(
                name: "org_structure_unit",
                newName: "org-structure-unit");

            migrationBuilder.RenameTable(
                name: "org_structure_position",
                newName: "org-structure-position");

            migrationBuilder.RenameTable(
                name: "org_structure_legal_entity",
                newName: "org-structure-legal-entity");

            migrationBuilder.RenameTable(
                name: "org_structure_job_title",
                newName: "org-structure-job-title");

            migrationBuilder.RenameTable(
                name: "org_structure_job_position",
                newName: "org-structure-job-position");

            migrationBuilder.RenameTable(
                name: "org_structure_job_grade",
                newName: "org-structure-job-grade");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_unit_type_OrgId_Code",
                table: "org-structure-unit-type",
                newName: "IX_org-structure-unit-type_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_unit_member_OrganizationalUnitId_UserId",
                table: "org-structure-unit-member",
                newName: "IX_org-structure-unit-member_OrganizationalUnitId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_unit_Path",
                table: "org-structure-unit",
                newName: "IX_org-structure-unit_Path");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_unit_ParentId",
                table: "org-structure-unit",
                newName: "IX_org-structure-unit_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_unit_OrgId_Code",
                table: "org-structure-unit",
                newName: "IX_org-structure-unit_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_position_ReportsToPositionId",
                table: "org-structure-position",
                newName: "IX_org-structure-position_ReportsToPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_position_OrganizationalUnitId",
                table: "org-structure-position",
                newName: "IX_org-structure-position_OrganizationalUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_legal_entity_OrgId_Code",
                table: "org-structure-legal-entity",
                newName: "IX_org-structure-legal-entity_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_job_title_OrgId_Code",
                table: "org-structure-job-title",
                newName: "IX_org-structure-job-title_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_job_position_OrgId_Code",
                table: "org-structure-job-position",
                newName: "IX_org-structure-job-position_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org_structure_job_grade_OrgId_Code",
                table: "org-structure-job-grade",
                newName: "IX_org-structure-job-grade_OrgId_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org-structure-unit-type",
                table: "org-structure-unit-type",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org-structure-unit-member",
                table: "org-structure-unit-member",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org-structure-unit",
                table: "org-structure-unit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org-structure-position",
                table: "org-structure-position",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org-structure-legal-entity",
                table: "org-structure-legal-entity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org-structure-job-title",
                table: "org-structure-job-title",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org-structure-job-position",
                table: "org-structure-job-position",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org-structure-job-grade",
                table: "org-structure-job-grade",
                column: "Id");
        }
    }
}
