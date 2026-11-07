using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class KebabCaseTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Positions_PositionId",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Student",
                table: "Student");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Org",
                table: "Org");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Departments",
                table: "Departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_excel_templates",
                table: "excel_templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantPasswordPolicy",
                table: "TenantPasswordPolicy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgStructureUnitType",
                table: "OrgStructureUnitType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgStructureUnitMember",
                table: "OrgStructureUnitMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgStructureUnit",
                table: "OrgStructureUnit");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgStructurePosition",
                table: "OrgStructurePosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgStructureLegalEntity",
                table: "OrgStructureLegalEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgStructureJobTitle",
                table: "OrgStructureJobTitle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgStructureJobPosition",
                table: "OrgStructureJobPosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrgStructureJobGrade",
                table: "OrgStructureJobGrade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BasicAuthUser",
                table: "BasicAuthUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiKeyCredential",
                table: "ApiKeyCredential");

            migrationBuilder.RenameTable(
                name: "Student",
                newName: "student");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "positions");

            migrationBuilder.RenameTable(
                name: "Org",
                newName: "org");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "employees");

            migrationBuilder.RenameTable(
                name: "Departments",
                newName: "departments");

            migrationBuilder.RenameTable(
                name: "excel_templates",
                newName: "excel-templates");

            migrationBuilder.RenameTable(
                name: "TenantPasswordPolicy",
                newName: "tenant-password-policy");

            migrationBuilder.RenameTable(
                name: "OrgStructureUnitType",
                newName: "org-structure-unit-type");

            migrationBuilder.RenameTable(
                name: "OrgStructureUnitMember",
                newName: "org-structure-unit-member");

            migrationBuilder.RenameTable(
                name: "OrgStructureUnit",
                newName: "org-structure-unit");

            migrationBuilder.RenameTable(
                name: "OrgStructurePosition",
                newName: "org-structure-position");

            migrationBuilder.RenameTable(
                name: "OrgStructureLegalEntity",
                newName: "org-structure-legal-entity");

            migrationBuilder.RenameTable(
                name: "OrgStructureJobTitle",
                newName: "org-structure-job-title");

            migrationBuilder.RenameTable(
                name: "OrgStructureJobPosition",
                newName: "org-structure-job-position");

            migrationBuilder.RenameTable(
                name: "OrgStructureJobGrade",
                newName: "org-structure-job-grade");

            migrationBuilder.RenameTable(
                name: "BasicAuthUser",
                newName: "basic-auth-user");

            migrationBuilder.RenameTable(
                name: "ApiKeyCredential",
                newName: "api-key-credential");

            migrationBuilder.RenameIndex(
                name: "IX_Org_TenantId_Code",
                table: "org",
                newName: "IX_org_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Org_ParentId",
                table: "org",
                newName: "IX_org_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_PositionId",
                table: "employees",
                newName: "IX_employees_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DepartmentId",
                table: "employees",
                newName: "IX_employees_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_excel_templates_TenantId_Code",
                table: "excel-templates",
                newName: "IX_excel-templates_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_TenantPasswordPolicy_TenantId",
                table: "tenant-password-policy",
                newName: "IX_tenant-password-policy_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureUnitType_OrgId_Code",
                table: "org-structure-unit-type",
                newName: "IX_org-structure-unit-type_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureUnitMember_OrganizationalUnitId_UserId",
                table: "org-structure-unit-member",
                newName: "IX_org-structure-unit-member_OrganizationalUnitId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureUnit_Path",
                table: "org-structure-unit",
                newName: "IX_org-structure-unit_Path");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureUnit_ParentId",
                table: "org-structure-unit",
                newName: "IX_org-structure-unit_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureUnit_OrgId_Code",
                table: "org-structure-unit",
                newName: "IX_org-structure-unit_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructurePosition_ReportsToPositionId",
                table: "org-structure-position",
                newName: "IX_org-structure-position_ReportsToPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructurePosition_OrganizationalUnitId",
                table: "org-structure-position",
                newName: "IX_org-structure-position_OrganizationalUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureLegalEntity_OrgId_Code",
                table: "org-structure-legal-entity",
                newName: "IX_org-structure-legal-entity_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureJobTitle_OrgId_Code",
                table: "org-structure-job-title",
                newName: "IX_org-structure-job-title_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureJobPosition_OrgId_Code",
                table: "org-structure-job-position",
                newName: "IX_org-structure-job-position_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_OrgStructureJobGrade_OrgId_Code",
                table: "org-structure-job-grade",
                newName: "IX_org-structure-job-grade_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_BasicAuthUser_TenantId_Username",
                table: "basic-auth-user",
                newName: "IX_basic-auth-user_TenantId_Username");

            migrationBuilder.RenameIndex(
                name: "IX_ApiKeyCredential_TenantId_Profile_KeyHash",
                table: "api-key-credential",
                newName: "IX_api-key-credential_TenantId_Profile_KeyHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_student",
                table: "student",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_positions",
                table: "positions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_org",
                table: "org",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employees",
                table: "employees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_departments",
                table: "departments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_excel-templates",
                table: "excel-templates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenant-password-policy",
                table: "tenant-password-policy",
                column: "Id");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_basic-auth-user",
                table: "basic-auth-user",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_api-key-credential",
                table: "api-key-credential",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_employees_departments_DepartmentId",
                table: "employees",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employees_positions_PositionId",
                table: "employees",
                column: "PositionId",
                principalTable: "positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employees_departments_DepartmentId",
                table: "employees");

            migrationBuilder.DropForeignKey(
                name: "FK_employees_positions_PositionId",
                table: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_student",
                table: "student");

            migrationBuilder.DropPrimaryKey(
                name: "PK_positions",
                table: "positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_org",
                table: "org");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employees",
                table: "employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_departments",
                table: "departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tenant-password-policy",
                table: "tenant-password-policy");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_excel-templates",
                table: "excel-templates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_basic-auth-user",
                table: "basic-auth-user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_api-key-credential",
                table: "api-key-credential");

            migrationBuilder.RenameTable(
                name: "student",
                newName: "Student");

            migrationBuilder.RenameTable(
                name: "positions",
                newName: "Positions");

            migrationBuilder.RenameTable(
                name: "org",
                newName: "Org");

            migrationBuilder.RenameTable(
                name: "employees",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "departments",
                newName: "Departments");

            migrationBuilder.RenameTable(
                name: "tenant-password-policy",
                newName: "TenantPasswordPolicy");

            migrationBuilder.RenameTable(
                name: "org-structure-unit-type",
                newName: "OrgStructureUnitType");

            migrationBuilder.RenameTable(
                name: "org-structure-unit-member",
                newName: "OrgStructureUnitMember");

            migrationBuilder.RenameTable(
                name: "org-structure-unit",
                newName: "OrgStructureUnit");

            migrationBuilder.RenameTable(
                name: "org-structure-position",
                newName: "OrgStructurePosition");

            migrationBuilder.RenameTable(
                name: "org-structure-legal-entity",
                newName: "OrgStructureLegalEntity");

            migrationBuilder.RenameTable(
                name: "org-structure-job-title",
                newName: "OrgStructureJobTitle");

            migrationBuilder.RenameTable(
                name: "org-structure-job-position",
                newName: "OrgStructureJobPosition");

            migrationBuilder.RenameTable(
                name: "org-structure-job-grade",
                newName: "OrgStructureJobGrade");

            migrationBuilder.RenameTable(
                name: "excel-templates",
                newName: "excel_templates");

            migrationBuilder.RenameTable(
                name: "basic-auth-user",
                newName: "BasicAuthUser");

            migrationBuilder.RenameTable(
                name: "api-key-credential",
                newName: "ApiKeyCredential");

            migrationBuilder.RenameIndex(
                name: "IX_org_TenantId_Code",
                table: "Org",
                newName: "IX_Org_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org_ParentId",
                table: "Org",
                newName: "IX_Org_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_employees_PositionId",
                table: "Employees",
                newName: "IX_Employees_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_employees_DepartmentId",
                table: "Employees",
                newName: "IX_Employees_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_tenant-password-policy_TenantId",
                table: "TenantPasswordPolicy",
                newName: "IX_TenantPasswordPolicy_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit-type_OrgId_Code",
                table: "OrgStructureUnitType",
                newName: "IX_OrgStructureUnitType_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit-member_OrganizationalUnitId_UserId",
                table: "OrgStructureUnitMember",
                newName: "IX_OrgStructureUnitMember_OrganizationalUnitId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit_Path",
                table: "OrgStructureUnit",
                newName: "IX_OrgStructureUnit_Path");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit_ParentId",
                table: "OrgStructureUnit",
                newName: "IX_OrgStructureUnit_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-unit_OrgId_Code",
                table: "OrgStructureUnit",
                newName: "IX_OrgStructureUnit_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-position_ReportsToPositionId",
                table: "OrgStructurePosition",
                newName: "IX_OrgStructurePosition_ReportsToPositionId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-position_OrganizationalUnitId",
                table: "OrgStructurePosition",
                newName: "IX_OrgStructurePosition_OrganizationalUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-legal-entity_OrgId_Code",
                table: "OrgStructureLegalEntity",
                newName: "IX_OrgStructureLegalEntity_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-job-title_OrgId_Code",
                table: "OrgStructureJobTitle",
                newName: "IX_OrgStructureJobTitle_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-job-position_OrgId_Code",
                table: "OrgStructureJobPosition",
                newName: "IX_OrgStructureJobPosition_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_org-structure-job-grade_OrgId_Code",
                table: "OrgStructureJobGrade",
                newName: "IX_OrgStructureJobGrade_OrgId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_excel-templates_TenantId_Code",
                table: "excel_templates",
                newName: "IX_excel_templates_TenantId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_basic-auth-user_TenantId_Username",
                table: "BasicAuthUser",
                newName: "IX_BasicAuthUser_TenantId_Username");

            migrationBuilder.RenameIndex(
                name: "IX_api-key-credential_TenantId_Profile_KeyHash",
                table: "ApiKeyCredential",
                newName: "IX_ApiKeyCredential_TenantId_Profile_KeyHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Student",
                table: "Student",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Org",
                table: "Org",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Departments",
                table: "Departments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantPasswordPolicy",
                table: "TenantPasswordPolicy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgStructureUnitType",
                table: "OrgStructureUnitType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgStructureUnitMember",
                table: "OrgStructureUnitMember",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgStructureUnit",
                table: "OrgStructureUnit",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgStructurePosition",
                table: "OrgStructurePosition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgStructureLegalEntity",
                table: "OrgStructureLegalEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgStructureJobTitle",
                table: "OrgStructureJobTitle",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgStructureJobPosition",
                table: "OrgStructureJobPosition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrgStructureJobGrade",
                table: "OrgStructureJobGrade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_excel_templates",
                table: "excel_templates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BasicAuthUser",
                table: "BasicAuthUser",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiKeyCredential",
                table: "ApiKeyCredential",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Positions_PositionId",
                table: "Employees",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
