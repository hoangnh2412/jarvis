using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.Oracle.Migrations
{
    public partial class InitCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "DBDEV");

            migrationBuilder.CreateTable(
                name: "CORE_COUNTRY",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_COUNTRY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_FILE_MEDIA",
                schema: "DBDEV",
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
                    table.PrimaryKey("PK_CORE_FILE_MEDIA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_LABEL",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
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
                    table.PrimaryKey("PK_CORE_LABEL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_ORGANIZATION_ROLE",
                schema: "DBDEV",
                columns: table => new
                {
                    IdRole = table.Column<Guid>(nullable: false),
                    OrganizationCode = table.Column<Guid>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_ORGANIZATION_ROLE", x => new { x.IdRole, x.OrganizationCode });
                });

            migrationBuilder.CreateTable(
                name: "CORE_ORGANIZATION_UNIT",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
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
                    table.PrimaryKey("PK_CORE_ORGANIZATION_UNIT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_ORGANIZATION_USER",
                schema: "DBDEV",
                columns: table => new
                {
                    IdUser = table.Column<Guid>(nullable: false),
                    OrganizationCode = table.Column<Guid>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    IsManager = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_ORGANIZATION_USER", x => new { x.IdUser, x.OrganizationCode });
                });

            migrationBuilder.CreateTable(
                name: "CORE_ROLE",
                schema: "DBDEV",
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
                    table.PrimaryKey("PK_CORE_ROLE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_SETTING",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
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
                    table.PrimaryKey("PK_CORE_SETTING", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_TENANT",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
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
                    table.PrimaryKey("PK_CORE_TENANT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_TENANT_HOST",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<Guid>(nullable: false),
                    HostName = table.Column<string>(maxLength: 250, nullable: false),
                    DeletedVersion = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_TENANT_HOST", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_TENANT_INFO",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
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
                    table.PrimaryKey("PK_CORE_TENANT_INFO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_TOKEN_INFO",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
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
                    table.PrimaryKey("PK_CORE_TOKEN_INFO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_USER",
                schema: "DBDEV",
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
                    table.PrimaryKey("PK_CORE_USER", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_USER_INFO",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FullName = table.Column<string>(nullable: true),
                    AvatarPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_USER_INFO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CORE_CITY",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdCountry = table.Column<int>(nullable: false),
                    CountryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_CITY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CORE_CITY_CORE_COUNTRY_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_COUNTRY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CORE_ROLE_CLAIM",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RoleId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_ROLE_CLAIM", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CORE_ROLE_CLAIM_CORE_ROLE_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_ROLE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CORE_USER_CLAIM",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_USER_CLAIM", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CORE_USER_CLAIM_CORE_USER_UserId",
                        column: x => x.UserId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CORE_USER_LOGIN",
                schema: "DBDEV",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_USER_LOGIN", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_CORE_USER_LOGIN_CORE_USER_UserId",
                        column: x => x.UserId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CORE_USER_ROLE",
                schema: "DBDEV",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_USER_ROLE", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_CORE_USER_ROLE_CORE_ROLE_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_ROLE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CORE_USER_ROLE_CORE_USER_UserId",
                        column: x => x.UserId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CORE_USER_TOKEN",
                schema: "DBDEV",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_USER_TOKEN", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_CORE_USER_TOKEN_CORE_USER_UserId",
                        column: x => x.UserId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CORE_DISTRICT",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdCity = table.Column<int>(nullable: false),
                    CityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_DISTRICT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CORE_DISTRICT_CORE_CITY_CityId",
                        column: x => x.CityId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_CITY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CORE_WARD",
                schema: "DBDEV",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdDistrict = table.Column<int>(nullable: false),
                    DisctrictId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CORE_WARD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CORE_WARD_CORE_DISTRICT_DisctrictId",
                        column: x => x.DisctrictId,
                        principalSchema: "DBDEV",
                        principalTable: "CORE_DISTRICT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CORE_CITY_CountryId",
                schema: "DBDEV",
                table: "CORE_CITY",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_DISTRICT_CityId",
                schema: "DBDEV",
                table: "CORE_DISTRICT",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_FILE_MEDIA_Name",
                schema: "DBDEV",
                table: "CORE_FILE_MEDIA",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_FILE_MEDIA_TenantCode",
                schema: "DBDEV",
                table: "CORE_FILE_MEDIA",
                column: "TenantCode");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_LABEL_Code",
                schema: "DBDEV",
                table: "CORE_LABEL",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_LABEL_Name_TenantCode_DeletedVersion",
                schema: "DBDEV",
                table: "CORE_LABEL",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_ORGANIZATION_ROLE_IdRole_OrganizationCode",
                schema: "DBDEV",
                table: "CORE_ORGANIZATION_ROLE",
                columns: new[] { "IdRole", "OrganizationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_ORGANIZATION_UNIT_Code",
                schema: "DBDEV",
                table: "CORE_ORGANIZATION_UNIT",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_ORGANIZATION_UNIT_Name_TenantCode_DeletedVersion",
                schema: "DBDEV",
                table: "CORE_ORGANIZATION_UNIT",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_ORGANIZATION_USER_IdUser_OrganizationCode",
                schema: "DBDEV",
                table: "CORE_ORGANIZATION_USER",
                columns: new[] { "IdUser", "OrganizationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "DBDEV",
                table: "CORE_ROLE",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_ROLE_NormalizedName_TenantCode_DeletedVersion",
                schema: "DBDEV",
                table: "CORE_ROLE",
                columns: new[] { "NormalizedName", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_ROLE_CLAIM_RoleId",
                schema: "DBDEV",
                table: "CORE_ROLE_CLAIM",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_SETTING_Code",
                schema: "DBDEV",
                table: "CORE_SETTING",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_SETTING_Key_TenantCode_DeletedVersion",
                schema: "DBDEV",
                table: "CORE_SETTING",
                columns: new[] { "Key", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_TENANT_Code",
                schema: "DBDEV",
                table: "CORE_TENANT",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_TENANT_Name_DeletedVersion",
                schema: "DBDEV",
                table: "CORE_TENANT",
                columns: new[] { "Name", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_TENANT_HOST_HostName_DeletedVersion",
                schema: "DBDEV",
                table: "CORE_TENANT_HOST",
                columns: new[] { "HostName", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_TOKEN_INFO_Code",
                schema: "DBDEV",
                table: "CORE_TOKEN_INFO",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_USER_Email",
                schema: "DBDEV",
                table: "CORE_USER",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "DBDEV",
                table: "CORE_USER",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "DBDEV",
                table: "CORE_USER",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_USER_UserName",
                schema: "DBDEV",
                table: "CORE_USER",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_USER_NormalizedUserName_TenantCode_DeletedVersion",
                schema: "DBDEV",
                table: "CORE_USER",
                columns: new[] { "NormalizedUserName", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CORE_USER_CLAIM_UserId",
                schema: "DBDEV",
                table: "CORE_USER_CLAIM",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_USER_LOGIN_UserId",
                schema: "DBDEV",
                table: "CORE_USER_LOGIN",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_USER_ROLE_RoleId",
                schema: "DBDEV",
                table: "CORE_USER_ROLE",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CORE_WARD_DisctrictId",
                schema: "DBDEV",
                table: "CORE_WARD",
                column: "DisctrictId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CORE_FILE_MEDIA",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_LABEL",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_ORGANIZATION_ROLE",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_ORGANIZATION_UNIT",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_ORGANIZATION_USER",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_ROLE_CLAIM",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_SETTING",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_TENANT",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_TENANT_HOST",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_TENANT_INFO",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_TOKEN_INFO",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_USER_CLAIM",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_USER_INFO",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_USER_LOGIN",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_USER_ROLE",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_USER_TOKEN",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_WARD",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_ROLE",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_USER",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_DISTRICT",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_CITY",
                schema: "DBDEV");

            migrationBuilder.DropTable(
                name: "CORE_COUNTRY",
                schema: "DBDEV");
        }
    }
}
