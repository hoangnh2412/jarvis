using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jarvis.Core.Database.SQLite.Migrations
{
    public partial class InitCoreDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "country",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "email_history",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    TenantCode = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<Guid>(nullable: true),
                    Code = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Cc = table.Column<string>(nullable: true),
                    Bcc = table.Column<string>(nullable: true),
                    Attachments = table.Column<string>(nullable: true),
                    Metadata = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    From = table.Column<string>(nullable: true),
                    FromName = table.Column<string>(nullable: true),
                    To = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    ConcurrencyStamp = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "email_template",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
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
                    DeletedVersion = table.Column<int>(nullable: false),
                    Code = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    To = table.Column<string>(nullable: true),
                    Cc = table.Column<string>(nullable: true),
                    Bcc = table.Column<string>(nullable: true),
                    Attachments = table.Column<string>(nullable: true),
                    Metadata = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_template", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "file",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    TenantCode = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    Path = table.Column<string>(nullable: true),
                    Extension = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    FileName = table.Column<string>(maxLength: 500, nullable: true),
                    BucketName = table.Column<string>(nullable: true),
                    Length = table.Column<long>(nullable: false),
                    Metadata = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "label",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                    Key = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: true),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Icon = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Color = table.Column<string>(unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_label", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_unit",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    FullName = table.Column<string>(maxLength: 250, nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: true),
                    IdParent = table.Column<Guid>(nullable: true),
                    Left = table.Column<int>(nullable: false),
                    Right = table.Column<int>(nullable: false),
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
                    table.PrimaryKey("PK_organization_unit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_user",
                schema: "core",
                columns: table => new
                {
                    IdUser = table.Column<Guid>(nullable: false),
                    OrganizationCode = table.Column<Guid>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    Key = table.Column<Guid>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_user", x => new { x.IdUser, x.OrganizationCode });
                });

            migrationBuilder.CreateTable(
                name: "role",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Key = table.Column<Guid>(nullable: false),
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
                    table.PrimaryKey("PK_role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "setting",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                    Key = table.Column<Guid>(nullable: false),
                    Group = table.Column<string>(nullable: true),
                    Code = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    Name = table.Column<string>(maxLength: 250, nullable: true),
                    Value = table.Column<string>(nullable: true),
                    Options = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Note = table.Column<string>(nullable: true),
                    IsReadOnly = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    IdParent = table.Column<Guid>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    Server = table.Column<string>(maxLength: 250, nullable: true),
                    Database = table.Column<string>(maxLength: 250, nullable: true),
                    DbConnectionString = table.Column<string>(maxLength: 1000, nullable: true),
                    Theme = table.Column<string>(maxLength: 250, nullable: true),
                    Skin = table.Column<string>(nullable: true),
                    SecretKey = table.Column<string>(nullable: true),
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
                    table.PrimaryKey("PK_tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_host",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    HostName = table.Column<string>(maxLength: 250, nullable: false),
                    DeletedVersion = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_host", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_info",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    IsCurrent = table.Column<bool>(nullable: false),
                    TaxCode = table.Column<string>(maxLength: 50, nullable: false),
                    Address = table.Column<string>(maxLength: 500, nullable: false),
                    City = table.Column<string>(maxLength: 250, nullable: false),
                    Country = table.Column<string>(maxLength: 250, nullable: false),
                    District = table.Column<string>(maxLength: 250, nullable: false),
                    FullNameVi = table.Column<string>(maxLength: 250, nullable: false),
                    FullNameEn = table.Column<string>(maxLength: 250, nullable: true),
                    ShortName = table.Column<string>(nullable: true),
                    Logo = table.Column<string>(nullable: true),
                    LegalName = table.Column<string>(maxLength: 250, nullable: true),
                    Fax = table.Column<string>(maxLength: 50, nullable: true),
                    BusinessType = table.Column<string>(nullable: true),
                    Emails = table.Column<string>(maxLength: 500, nullable: true),
                    Phones = table.Column<string>(maxLength: 500, nullable: true),
                    BranchName = table.Column<string>(nullable: true),
                    Metadata = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_info", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "token_info",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
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
                    table.PrimaryKey("PK_token_info", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "core",
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
                    Key = table.Column<Guid>(nullable: false),
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
                    DeletedVersion = table.Column<Guid>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_info",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    FullName = table.Column<string>(nullable: true),
                    AvatarPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_info", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "city",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdCountry = table.Column<int>(nullable: false),
                    CountryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_city", x => x.Id);
                    table.ForeignKey(
                        name: "FK_city_country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "core",
                        principalTable: "country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_claim",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_claim_role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "core",
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claim",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_claim_user_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_login",
                schema: "core",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_login", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_user_login_user_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "core",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_role_role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "core",
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_role_user_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_token",
                schema: "core",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_token", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_user_token_user_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "district",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdCity = table.Column<int>(nullable: false),
                    CityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_district", x => x.Id);
                    table.ForeignKey(
                        name: "FK_district_city_CityId",
                        column: x => x.CityId,
                        principalSchema: "core",
                        principalTable: "city",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ward",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Longitude = table.Column<string>(nullable: true),
                    Latitude = table.Column<string>(nullable: true),
                    IdDistrict = table.Column<int>(nullable: false),
                    DisctrictId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ward_district_DisctrictId",
                        column: x => x.DisctrictId,
                        principalSchema: "core",
                        principalTable: "district",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_city_CountryId",
                schema: "core",
                table: "city",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_district_CityId",
                schema: "core",
                table: "district",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_email_history_Key",
                schema: "core",
                table: "email_history",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_history_Code_TenantCode",
                schema: "core",
                table: "email_history",
                columns: new[] { "Code", "TenantCode" });

            migrationBuilder.CreateIndex(
                name: "IX_email_history_CreatedAt_TenantCode",
                schema: "core",
                table: "email_history",
                columns: new[] { "CreatedAt", "TenantCode" });

            migrationBuilder.CreateIndex(
                name: "IX_email_history_Status_TenantCode",
                schema: "core",
                table: "email_history",
                columns: new[] { "Status", "TenantCode" });

            migrationBuilder.CreateIndex(
                name: "IX_email_history_To_TenantCode",
                schema: "core",
                table: "email_history",
                columns: new[] { "To", "TenantCode" });

            migrationBuilder.CreateIndex(
                name: "IX_email_history_Type_TenantCode",
                schema: "core",
                table: "email_history",
                columns: new[] { "Type", "TenantCode" });

            migrationBuilder.CreateIndex(
                name: "IX_email_template_Key",
                schema: "core",
                table: "email_template",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_template_Code_TenantCode",
                schema: "core",
                table: "email_template",
                columns: new[] { "Code", "TenantCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_file_FileName",
                schema: "core",
                table: "file",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_file_TenantCode",
                schema: "core",
                table: "file",
                column: "TenantCode");

            migrationBuilder.CreateIndex(
                name: "IX_label_Key",
                schema: "core",
                table: "label",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_label_Name",
                schema: "core",
                table: "label",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_label_Name_TenantCode_DeletedVersion",
                schema: "core",
                table: "label",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_unit_Key",
                schema: "core",
                table: "organization_unit",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_unit_Name_TenantCode_DeletedVersion",
                schema: "core",
                table: "organization_unit",
                columns: new[] { "Name", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_user_IdUser_OrganizationCode",
                schema: "core",
                table: "organization_user",
                columns: new[] { "IdUser", "OrganizationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "core",
                table: "role",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_role_NormalizedName_TenantCode_DeletedVersion",
                schema: "core",
                table: "role",
                columns: new[] { "NormalizedName", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_claim_RoleId",
                schema: "core",
                table: "role_claim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_setting_Key",
                schema: "core",
                table: "setting",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_setting_Code_TenantCode_DeletedVersion",
                schema: "core",
                table: "setting",
                columns: new[] { "Code", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_Key",
                schema: "core",
                table: "tenant",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_Name_DeletedVersion",
                schema: "core",
                table: "tenant",
                columns: new[] { "Name", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_host_HostName_DeletedVersion",
                schema: "core",
                table: "tenant_host",
                columns: new[] { "HostName", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_token_info_Key",
                schema: "core",
                table: "token_info",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                schema: "core",
                table: "user",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "core",
                table: "user",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "core",
                table: "user",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_user_UserName",
                schema: "core",
                table: "user",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_user_NormalizedUserName_TenantCode_DeletedVersion",
                schema: "core",
                table: "user",
                columns: new[] { "NormalizedUserName", "TenantCode", "DeletedVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_claim_UserId",
                schema: "core",
                table: "user_claim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_login_UserId",
                schema: "core",
                table: "user_login",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_RoleId",
                schema: "core",
                table: "user_role",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ward_DisctrictId",
                schema: "core",
                table: "ward",
                column: "DisctrictId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_history",
                schema: "core");

            migrationBuilder.DropTable(
                name: "email_template",
                schema: "core");

            migrationBuilder.DropTable(
                name: "file",
                schema: "core");

            migrationBuilder.DropTable(
                name: "label",
                schema: "core");

            migrationBuilder.DropTable(
                name: "organization_unit",
                schema: "core");

            migrationBuilder.DropTable(
                name: "organization_user",
                schema: "core");

            migrationBuilder.DropTable(
                name: "role_claim",
                schema: "core");

            migrationBuilder.DropTable(
                name: "setting",
                schema: "core");

            migrationBuilder.DropTable(
                name: "tenant",
                schema: "core");

            migrationBuilder.DropTable(
                name: "tenant_host",
                schema: "core");

            migrationBuilder.DropTable(
                name: "tenant_info",
                schema: "core");

            migrationBuilder.DropTable(
                name: "token_info",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_claim",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_info",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_login",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user_token",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ward",
                schema: "core");

            migrationBuilder.DropTable(
                name: "role",
                schema: "core");

            migrationBuilder.DropTable(
                name: "user",
                schema: "core");

            migrationBuilder.DropTable(
                name: "district",
                schema: "core");

            migrationBuilder.DropTable(
                name: "city",
                schema: "core");

            migrationBuilder.DropTable(
                name: "country",
                schema: "core");
        }
    }
}
