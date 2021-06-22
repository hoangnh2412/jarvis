using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;

namespace Jarvis.Core.Database.Oracle.Migrations
{
    public partial class InitCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_City",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_Country",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_Disctrict",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_File",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_Label",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_OrganizationRole",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_OrganizationUnit",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_OrganizationUser",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_Role",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_Setting",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_Tenant",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_TenantHost",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_TenantInfo",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_TokenInfo",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_User",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_UserInfo",
                schema: "INEINVOICE");

            migrationBuilder.CreateSequence<int>(
                name: "SEQ_Ward",
                schema: "INEINVOICE");

            migrationBuilder.CreateTable(
                name: "Core_Country",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_File",
                schema: "INEINVOICE",
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
                    table.PrimaryKey("PK_Core_File", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_Label",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("PK_Core_Label", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_OrganizationRole",
                schema: "INEINVOICE",
                columns: table => new
                {
                    IdRole = table.Column<Guid>(nullable: false),
                    OrganizationCode = table.Column<Guid>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_OrganizationRole", x => new { x.IdRole, x.OrganizationCode });
                });

            migrationBuilder.CreateTable(
                name: "Core_OrganizationUnit",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("PK_Core_OrganizationUnit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_OrganizationUser",
                schema: "INEINVOICE",
                columns: table => new
                {
                    IdUser = table.Column<Guid>(nullable: false),
                    OrganizationCode = table.Column<Guid>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    IsManager = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_OrganizationUser", x => new { x.IdUser, x.OrganizationCode });
                });

            migrationBuilder.CreateTable(
                name: "Core_Role",
                schema: "INEINVOICE",
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
                    table.PrimaryKey("PK_Core_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_Setting",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("PK_Core_Setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_Tenant",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("PK_Core_Tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_TenantHost",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<Guid>(nullable: false),
                    HostName = table.Column<string>(maxLength: 250, nullable: false),
                    DeletedVersion = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_TenantHost", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_TenantInfo",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("PK_Core_TenantInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_TokenInfo",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("PK_Core_TokenInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_User",
                schema: "INEINVOICE",
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
                    table.PrimaryKey("PK_Core_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserInfo",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FullName = table.Column<string>(nullable: true),
                    AvatarPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Core_City",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdCountry = table.Column<int>(nullable: false),
                    CountryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_City", x => x.Id);
                    table.ForeignKey(
                        name: "FK_City_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Core_RoleClaim",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_RoleClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaim_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserClaim",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaim_UserId",
                        column: x => x.UserId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserLogin",
                schema: "INEINVOICE",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogin_UserId",
                        column: x => x.UserId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserRole",
                schema: "INEINVOICE",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_UserId",
                        column: x => x.UserId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_UserToken",
                schema: "INEINVOICE",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_UserToken", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserToken_UserId",
                        column: x => x.UserId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Core_District",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdCity = table.Column<int>(nullable: false),
                    CityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_District", x => x.Id);
                    table.ForeignKey(
                        name: "FK_District_CityId",
                        column: x => x.CityId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Core_Ward",
                schema: "INEINVOICE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdDistrict = table.Column<int>(nullable: false),
                    DisctrictId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Core_Ward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ward_DisctrictId",
                        column: x => x.DisctrictId,
                        principalSchema: "INEINVOICE",
                        principalTable: "Core_District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Core_City_CountryId",
                schema: "INEINVOICE",
                table: "Core_City",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_District_CityId",
                schema: "INEINVOICE",
                table: "Core_District",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_File_1",
                schema: "INEINVOICE",
                table: "Core_File",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_File_2",
                schema: "INEINVOICE",
                table: "Core_File",
                column: "TenantCode");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Label_2",
                schema: "INEINVOICE",
                table: "Core_Label",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Label_1",
                schema: "INEINVOICE",
                table: "Core_Label",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_OrganizationUnit_1",
                schema: "INEINVOICE",
                table: "Core_OrganizationUnit",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_OrganizationUnit_2",
                schema: "INEINVOICE",
                table: "Core_OrganizationUnit",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Role_1",
                schema: "INEINVOICE",
                table: "Core_Role",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Role_2",
                schema: "INEINVOICE",
                table: "Core_Role",
                columns: new[] { "NormalizedName", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_RoleClaim_RoleId",
                schema: "INEINVOICE",
                table: "Core_RoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Setting_1",
                schema: "INEINVOICE",
                table: "Core_Setting",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Setting_2",
                schema: "INEINVOICE",
                table: "Core_Setting",
                columns: new[] { "Key", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Tenant_1",
                schema: "INEINVOICE",
                table: "Core_Tenant",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_Tenant_2",
                schema: "INEINVOICE",
                table: "Core_Tenant",
                columns: new[] { "Name", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_TenantHost_1",
                schema: "INEINVOICE",
                table: "Core_TenantHost",
                columns: new[] { "HostName", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_TokenInfo_1",
                schema: "INEINVOICE",
                table: "Core_TokenInfo",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_User_3",
                schema: "INEINVOICE",
                table: "Core_User",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "INEINVOICE",
                table: "Core_User",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Core_User_1",
                schema: "INEINVOICE",
                table: "Core_User",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_Core_User_2",
                schema: "INEINVOICE",
                table: "Core_User",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_Core_User_4",
                schema: "INEINVOICE",
                table: "Core_User",
                columns: new[] { "NormalizedUserName", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Core_UserClaim_UserId",
                schema: "INEINVOICE",
                table: "Core_UserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_UserLogin_UserId",
                schema: "INEINVOICE",
                table: "Core_UserLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_UserRole_RoleId",
                schema: "INEINVOICE",
                table: "Core_UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Core_Ward_DisctrictId",
                schema: "INEINVOICE",
                table: "Core_Ward",
                column: "DisctrictId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Core_File",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_Label",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_OrganizationRole",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_OrganizationUnit",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_OrganizationUser",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_RoleClaim",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_Setting",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_Tenant",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_TenantHost",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_TenantInfo",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_TokenInfo",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_UserClaim",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_UserInfo",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_UserLogin",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_UserRole",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_UserToken",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_Ward",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_Role",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_User",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_District",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_City",
                schema: "INEINVOICE");

            migrationBuilder.DropTable(
                name: "Core_Country",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_City",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_Country",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_Disctrict",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_File",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_Label",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_OrganizationRole",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_OrganizationUnit",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_OrganizationUser",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_Role",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_Setting",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_Tenant",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_TenantHost",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_TenantInfo",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_TokenInfo",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_User",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_UserInfo",
                schema: "INEINVOICE");

            migrationBuilder.DropSequence(
                name: "SEQ_Ward",
                schema: "INEINVOICE");
        }
    }
}
