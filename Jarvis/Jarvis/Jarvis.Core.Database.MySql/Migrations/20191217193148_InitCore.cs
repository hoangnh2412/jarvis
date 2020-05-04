using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.MySql.Migrations
{
    public partial class InitCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "core_country",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_file",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantCode = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(unicode: false, maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    FileName = table.Column<string>(maxLength: 500, nullable: true),
                    Length = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_file", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_label",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenantCode = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedAt = table.Column<DateTime>(nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    DeletedVersion = table.Column<int>(nullable: true),
                    Code = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: true),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Icon = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Color = table.Column<string>(unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_label", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_organization_role",
                columns: table => new
                {
                    IdRole = table.Column<Guid>(nullable: false),
                    OrganizationCode = table.Column<Guid>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_organization_role", x => new { x.IdRole, x.OrganizationCode });
                });

            migrationBuilder.CreateTable(
                name: "core_organization_unit",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    FullName = table.Column<string>(maxLength: 250, nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: true),
                    IdParent = table.Column<Guid>(nullable: false),
                    TenantCode = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedAt = table.Column<DateTime>(nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    DeletedVersion = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_organization_unit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_organization_user",
                columns: table => new
                {
                    IdUser = table.Column<Guid>(nullable: false),
                    OrganizationCode = table.Column<Guid>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    IsManager = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_organization_user", x => new { x.IdUser, x.OrganizationCode });
                });

            migrationBuilder.CreateTable(
                name: "core_role",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    TenantCode = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedAt = table.Column<DateTime>(nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    DeletedVersion = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_setting",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenantCode = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedAt = table.Column<DateTime>(nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    DeletedVersion = table.Column<int>(nullable: true),
                    Code = table.Column<Guid>(nullable: false),
                    Group = table.Column<string>(nullable: true),
                    Key = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Name = table.Column<string>(maxLength: 250, nullable: true),
                    Value = table.Column<string>(nullable: true),
                    Options = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    IsReadOnly = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_tenant",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    IdParent = table.Column<Guid>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    Server = table.Column<string>(maxLength: 250, nullable: true),
                    Database = table.Column<string>(maxLength: 250, nullable: true),
                    DbConnectionString = table.Column<string>(maxLength: 1000, nullable: true),
                    Theme = table.Column<string>(maxLength: 250, nullable: true),
                    IsEnable = table.Column<bool>(nullable: false, defaultValue: false),
                    ExpireDate = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedAt = table.Column<DateTime>(nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    DeletedVersion = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_tenant_host",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<Guid>(nullable: false),
                    HostName = table.Column<string>(maxLength: 250, nullable: false),
                    DeletedVersion = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_tenant_host", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_tenant_info",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<Guid>(nullable: false),
                    IsCurrent = table.Column<bool>(nullable: false),
                    TaxCode = table.Column<string>(maxLength: 50, nullable: false),
                    Address = table.Column<string>(maxLength: 500, nullable: false),
                    City = table.Column<string>(maxLength: 250, nullable: false),
                    Country = table.Column<string>(maxLength: 250, nullable: false),
                    District = table.Column<string>(maxLength: 250, nullable: false),
                    FullNameVi = table.Column<string>(maxLength: 250, nullable: false),
                    FullNameEn = table.Column<string>(maxLength: 250, nullable: true),
                    LegalName = table.Column<string>(maxLength: 250, nullable: true),
                    Fax = table.Column<string>(maxLength: 50, nullable: true),
                    BusinessType = table.Column<string>(nullable: true),
                    Emails = table.Column<string>(maxLength: 500, nullable: true),
                    Phones = table.Column<string>(maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_tenant_info", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_token_info",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<Guid>(nullable: false),
                    TenantCode = table.Column<Guid>(nullable: false),
                    IdUser = table.Column<Guid>(nullable: false),
                    AccessToken = table.Column<string>(nullable: false),
                    RefreshToken = table.Column<string>(nullable: true),
                    Metadata = table.Column<string>(nullable: true),
                    TimeToLife = table.Column<double>(nullable: false),
                    LocalIpAddress = table.Column<string>(maxLength: 100, nullable: true),
                    PublicIpAddress = table.Column<string>(maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(nullable: true),
                    Source = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    ExpireAt = table.Column<DateTime>(nullable: false),
                    ExpireAtUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_token_info", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_user",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    TenantCode = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    DeletedAt = table.Column<DateTime>(nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<Guid>(nullable: true),
                    DeletedVersion = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_user_info",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FullName = table.Column<string>(nullable: true),
                    AvatarPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_user_info", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "core_city",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdCountry = table.Column<int>(nullable: false),
                    CountryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_city", x => x.Id);
                    table.ForeignKey(
                        name: "FK_core_city_core_country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "core_country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "core_role_claim",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_role_claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_core_role_claim_core_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "core_role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "core_user_claim",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_user_claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_core_user_claim_core_user_UserId",
                        column: x => x.UserId,
                        principalTable: "core_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "core_user_login",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_user_login", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_core_user_login_core_user_UserId",
                        column: x => x.UserId,
                        principalTable: "core_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "core_user_role",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_user_role", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_core_user_role_core_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "core_role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_core_user_role_core_user_UserId",
                        column: x => x.UserId,
                        principalTable: "core_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "core_user_token",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_user_token", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_core_user_token_core_user_UserId",
                        column: x => x.UserId,
                        principalTable: "core_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "core_district",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdCity = table.Column<int>(nullable: false),
                    CityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_district", x => x.Id);
                    table.ForeignKey(
                        name: "FK_core_district_core_city_CityId",
                        column: x => x.CityId,
                        principalTable: "core_city",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "core_ward",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdDistrict = table.Column<int>(nullable: false),
                    DisctrictId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core_ward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_core_ward_core_district_DisctrictId",
                        column: x => x.DisctrictId,
                        principalTable: "core_district",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_core_city_CountryId",
                table: "core_city",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_core_district_CityId",
                table: "core_district",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_core_file_Name",
                table: "core_file",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_file_TenantCode",
                table: "core_file",
                column: "TenantCode");

            migrationBuilder.CreateIndex(
                name: "IX_core_label_Code",
                table: "core_label",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_label_Name_TenantCode_DeletedVersion",
                table: "core_label",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_organization_role_IdRole_OrganizationCode",
                table: "core_organization_role",
                columns: new[] { "IdRole", "OrganizationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_organization_unit_Code",
                table: "core_organization_unit",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_organization_unit_Name_TenantCode_DeletedVersion",
                table: "core_organization_unit",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_organization_user_IdUser_OrganizationCode",
                table: "core_organization_user",
                columns: new[] { "IdUser", "OrganizationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "core_role",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_core_role_NormalizedName_TenantCode_DeletedVersion",
                table: "core_role",
                columns: new[] { "NormalizedName", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_role_claim_RoleId",
                table: "core_role_claim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_core_setting_Code",
                table: "core_setting",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_setting_Key_TenantCode_DeletedVersion",
                table: "core_setting",
                columns: new[] { "Key", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_tenant_Code",
                table: "core_tenant",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_tenant_Name_DeletedVersion",
                table: "core_tenant",
                columns: new[] { "Name", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_tenant_host_HostName_DeletedVersion",
                table: "core_tenant_host",
                columns: new[] { "HostName", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_token_info_Code",
                table: "core_token_info",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_user_Email",
                table: "core_user",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "core_user",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "core_user",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_core_user_UserName",
                table: "core_user",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_core_user_NormalizedUserName_TenantCode_DeletedVersion",
                table: "core_user",
                columns: new[] { "NormalizedUserName", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_core_user_claim_UserId",
                table: "core_user_claim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_core_user_login_UserId",
                table: "core_user_login",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_core_user_role_RoleId",
                table: "core_user_role",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_core_ward_DisctrictId",
                table: "core_ward",
                column: "DisctrictId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "core_file");

            migrationBuilder.DropTable(
                name: "core_label");

            migrationBuilder.DropTable(
                name: "core_organization_role");

            migrationBuilder.DropTable(
                name: "core_organization_unit");

            migrationBuilder.DropTable(
                name: "core_organization_user");

            migrationBuilder.DropTable(
                name: "core_role_claim");

            migrationBuilder.DropTable(
                name: "core_setting");

            migrationBuilder.DropTable(
                name: "core_tenant");

            migrationBuilder.DropTable(
                name: "core_tenant_host");

            migrationBuilder.DropTable(
                name: "core_tenant_info");

            migrationBuilder.DropTable(
                name: "core_token_info");

            migrationBuilder.DropTable(
                name: "core_user_claim");

            migrationBuilder.DropTable(
                name: "core_user_info");

            migrationBuilder.DropTable(
                name: "core_user_login");

            migrationBuilder.DropTable(
                name: "core_user_role");

            migrationBuilder.DropTable(
                name: "core_user_token");

            migrationBuilder.DropTable(
                name: "core_ward");

            migrationBuilder.DropTable(
                name: "core_role");

            migrationBuilder.DropTable(
                name: "core_user");

            migrationBuilder.DropTable(
                name: "core_district");

            migrationBuilder.DropTable(
                name: "core_city");

            migrationBuilder.DropTable(
                name: "core_country");
        }
    }
}
