using System;
using Infrastructure.Database;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Entities;
using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Jarvis.Core.Database.Oracle
{
    public class CoreDbContext : IdentityDbContext<User, Role, Guid>, IStorageContext
    {
        // private DatabaseOption DbOptions;
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {
        }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     IConfigurationRoot configuration = new ConfigurationBuilder()
        //         .SetBasePath(System.IO.Directory.GetCurrentDirectory())
        //         .AddJsonFile("appsettings.json")
        //         .Build();

        //     DbOptions = new DatabaseOption();
        //     configuration.GetSection("Jarvis:Database").Bind(DbOptions);

        //     base.OnConfiguring(optionsBuilder);
        // }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // modelBuilder.HasDefaultSchema(DbOptions.Schema);

            modelBuilder.Entity<File>(builder =>
            {
                builder.ToTable("CORE_FILE_MEDIA");
                builder.HasKey(x => x.Id);

                // builder.Property(x => x.Id).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.TenantCode).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.CreatedBy).HasColumnType("VARCHAR2(50)").IsRequired();

                builder.Property(x => x.Name).HasMaxLength(500).IsUnicode(false);
                builder.Property(x => x.ContentType).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.FileName).HasMaxLength(500).IsUnicode(true);

                builder.HasIndex(x => x.Name).IsUnique();
                builder.HasIndex(x => x.TenantCode);
            });

            modelBuilder.Entity<OrganizationUnit>(builder =>
            {
                builder.ToTable("CORE_ORGANIZATION_UNIT");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                // builder.Property(x => x.Code).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.IdParent).HasColumnType("VARCHAR2(50)");
                // builder.Property(x => x.TenantCode).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.CreatedBy).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.UpdatedBy).HasColumnType("VARCHAR2(50)");
                // builder.Property(x => x.DeletedBy).HasColumnType("VARCHAR2(50)");

                builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
                builder.Property(x => x.FullName).IsRequired().HasMaxLength(250);
                builder.Property(x => x.Description).HasMaxLength(250);

                builder.HasIndex(x => x.Code).IsUnique();
                builder.HasIndex(x => new { x.Name, x.TenantCode, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<OrganizationUser>(builder =>
            {
                builder.ToTable("CORE_ORGANIZATION_USER");

                // builder.Property(x => x.IdUser).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.OrganizationCode).HasColumnType("VARCHAR2(50)").IsRequired();

                builder.HasKey(x => new { x.IdUser, x.OrganizationCode });
                builder.HasIndex(x => new { x.IdUser, x.OrganizationCode }).IsUnique();
            });

            modelBuilder.Entity<OrganizationRole>(builder =>
            {
                builder.ToTable("CORE_ORGANIZATION_ROLE");

                // builder.Property(x => x.IdRole).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.OrganizationCode).HasColumnType("VARCHAR2(50)").IsRequired();

                builder.HasKey(x => new { x.IdRole, x.OrganizationCode });
                builder.HasIndex(x => new { x.IdRole, x.OrganizationCode }).IsUnique();
            });

            modelBuilder.Entity<Tenant>(builder =>
            {
                builder.ToTable("CORE_TENANT");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                // builder.Property(x => x.Code).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.IdParent).HasColumnType("VARCHAR2(50)");
                // builder.Property(x => x.CreatedBy).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.UpdatedBy).HasColumnType("VARCHAR2(50)");
                // builder.Property(x => x.DeletedBy).HasColumnType("VARCHAR2(50)");

                builder.Property(x => x.Code).IsRequired();
                builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Server).HasMaxLength(250);
                builder.Property(x => x.Database).HasMaxLength(250);
                builder.Property(x => x.DbConnectionString).HasMaxLength(1000);
                builder.Property(x => x.Theme).HasMaxLength(250);
                builder.Property(x => x.IsEnable).IsRequired().HasDefaultValue(false);

                builder.HasIndex(x => x.Code).IsUnique();
                builder.HasIndex(x => new { x.Name, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<TenantInfo>(builder =>
            {
                builder.ToTable("CORE_TENANT_INFO");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                // builder.Property(x => x.Code).HasColumnType("VARCHAR2(50)").IsRequired();

                builder.Property(x => x.Code).IsRequired();
                builder.Property(x => x.TaxCode).IsRequired().HasMaxLength(50);
                builder.Property(x => x.FullNameVi).IsRequired().HasMaxLength(250);
                builder.Property(x => x.FullNameEn).HasMaxLength(250);
                builder.Property(x => x.Address).IsRequired().HasMaxLength(500);
                builder.Property(x => x.District).IsRequired().HasMaxLength(250);
                builder.Property(x => x.City).IsRequired().HasMaxLength(250);
                builder.Property(x => x.Country).IsRequired().HasMaxLength(250);
                builder.Property(x => x.LegalName).HasMaxLength(250);
                builder.Property(x => x.Fax).HasMaxLength(50);
                builder.Property(x => x.Phones).HasMaxLength(500);
                builder.Property(x => x.Emails).HasMaxLength(500);
            });

            modelBuilder.Entity<TenantHost>(builder =>
            {
                builder.ToTable("CORE_TENANT_HOST");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                // builder.Property(x => x.Code).HasColumnType("VARCHAR2(50)").IsRequired();

                builder.Property(x => x.Code).IsRequired();
                builder.Property(x => x.HostName).IsRequired().HasMaxLength(250);

                builder.HasIndex(x => new { x.HostName, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<TokenInfo>(builder =>
            {
                builder.ToTable("CORE_TOKEN_INFO");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                // builder.Property(x => x.Code).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.TenantCode).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.IdUser).HasColumnType("VARCHAR2(50)").IsRequired();

                builder.Property(x => x.Source).HasMaxLength(100);
                builder.Property(x => x.LocalIpAddress).HasMaxLength(100);
                builder.Property(x => x.PublicIpAddress).HasMaxLength(100);
                builder.Property(x => x.AccessToken).IsRequired();

                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<Setting>(builder =>
            {
                builder.ToTable("CORE_SETTING");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                // builder.Property(x => x.Code).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.TenantCode).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.CreatedBy).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.UpdatedBy).HasColumnType("VARCHAR2(50)");
                // builder.Property(x => x.DeletedBy).HasColumnType("VARCHAR2(50)");

                builder.Property(x => x.Key).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Name).HasMaxLength(250).IsUnicode(true);
                builder.Property(x => x.Description).HasMaxLength(500).IsUnicode(true);

                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique();
                builder.HasIndex(x => new { x.Key, x.TenantCode, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<Label>(builder =>
            {
                builder.ToTable("CORE_LABEL");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                // builder.Property(x => x.Code).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.TenantCode).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.CreatedBy).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.UpdatedBy).HasColumnType("VARCHAR2(50)");
                // builder.Property(x => x.DeletedBy).HasColumnType("VARCHAR2(50)");

                builder.Property(x => x.Name).HasMaxLength(250).IsUnicode(true);
                builder.Property(x => x.Description).HasMaxLength(500).IsUnicode(true);
                builder.Property(x => x.Icon).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Color).HasMaxLength(50).IsUnicode(false);

                builder.HasIndex(x => new { x.Name, x.TenantCode, x.DeletedVersion }).IsUnique();
                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("CORE_USER");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();

                // builder.Property(x => x.TenantCode).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.CreatedBy).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.UpdatedBy).HasColumnType("VARCHAR2(50)");
                // builder.Property(x => x.DeletedBy).HasColumnType("VARCHAR2(50)");

                builder.HasIndex(x => x.NormalizedUserName).IsUnique(false);
                builder.HasIndex(x => x.UserName).IsUnique(false);
                builder.HasIndex(x => x.Email).IsUnique(false);
                builder.HasIndex(x => new
                {
                    x.NormalizedUserName,
                    x.TenantCode,
                    x.DeletedVersion
                }).IsUnique();
            });

            modelBuilder.Entity<UserInfo>(builder =>
            {
                builder.ToTable("CORE_USER_INFO");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();

                // builder.Property(x => x.Id).HasColumnType("VARCHAR2(50)").IsRequired();
            });

            modelBuilder.Entity<Role>(builder =>
            {
                builder.ToTable("CORE_ROLE");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();

                // builder.Property(x => x.TenantCode).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.CreatedBy).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.UpdatedBy).HasColumnType("VARCHAR2(50)");
                // builder.Property(x => x.DeletedBy).HasColumnType("VARCHAR2(50)");

                builder.HasIndex(x => x.NormalizedName).IsUnique(false);
                builder.HasIndex(x => new
                {
                    x.NormalizedName,
                    x.TenantCode,
                    x.DeletedVersion
                }).IsUnique();
            });

            modelBuilder.Entity<IdentityUserClaim<Guid>>(builder =>
            {
                builder.ToTable("CORE_USER_CLAIM");

                // builder.Property(x => x.UserId).HasColumnType("VARCHAR2(50)").IsRequired();
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
            {
                builder.ToTable("CORE_USER_ROLE");

                // builder.Property(x => x.RoleId).HasColumnType("VARCHAR2(50)").IsRequired();
                // builder.Property(x => x.UserId).HasColumnType("VARCHAR2(50)").IsRequired();
            });

            modelBuilder.Entity<IdentityUserLogin<Guid>>(builder =>
            {
                builder.ToTable("CORE_USER_LOGIN");

                // builder.Property(x => x.UserId).HasColumnType("VARCHAR2(50)").IsRequired();
            });

            modelBuilder.Entity<IdentityRoleClaim<Guid>>(builder =>
            {
                builder.ToTable("CORE_ROLE_CLAIM");

                // builder.Property(x => x.RoleId).HasColumnType("VARCHAR2(50)").IsRequired();
            });

            modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
            {
                builder.ToTable("CORE_USER_TOKEN");
                
                // builder.Property(x => x.UserId).HasColumnType("VARCHAR2(50)").IsRequired();
            });

            modelBuilder.Entity<Country>(builder =>
            {
                builder.ToTable("CORE_COUNTRY");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<City>(builder =>
            {
                builder.ToTable("CORE_CITY");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<Disctrict>(builder =>
            {
                builder.ToTable("CORE_DISTRICT");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<Ward>(builder =>
            {
                builder.ToTable("CORE_WARD");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });
        }
    }
}



