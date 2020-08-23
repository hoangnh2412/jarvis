using System;
using Infrastructure;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Entities;
using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Database.MySql
{
    public class CoreDbContext : IdentityDbContext<User, Role, Guid>, IStorageContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {
            Counter.CurrentCoreContext++;
            // Console.WriteLine($"CurrentCoreContext: {Counter.CurrentCoreContext}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<File>(builder =>
            {
                builder.ToTable("core_file");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Name).HasMaxLength(500).IsUnicode(false);
                builder.Property(x => x.ContentType).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.FileName).HasMaxLength(500).IsUnicode(true);

                builder.HasIndex(x => x.Name).IsUnique();
                builder.HasIndex(x => x.TenantCode);
            });

            modelBuilder.Entity<OrganizationUnit>(builder =>
            {
                builder.ToTable("core_organization_unit");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseMySqlIdentityColumn();

                builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
                builder.Property(x => x.FullName).IsRequired().HasMaxLength(250);
                builder.Property(x => x.Description).HasMaxLength(250);

                builder.HasIndex(x => x.Code).IsUnique();
                builder.HasIndex(x => new { x.Name, x.TenantCode, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<OrganizationUser>(builder =>
            {
                builder.ToTable("core_organization_user");
                builder.HasKey(x => new { x.IdUser, x.OrganizationCode });
                builder.HasIndex(x => new { x.IdUser, x.OrganizationCode }).IsUnique();
            });

            modelBuilder.Entity<Tenant>(builder =>
            {
                builder.ToTable("core_tenant");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseMySqlIdentityColumn();

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
                builder.ToTable("core_tenant_info");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseMySqlIdentityColumn();

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
                builder.ToTable("core_tenant_host");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseMySqlIdentityColumn();

                builder.Property(x => x.Code).IsRequired();
                builder.Property(x => x.HostName).IsRequired().HasMaxLength(250);

                builder.HasIndex(x => new { x.HostName, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<TokenInfo>(builder =>
            {
                builder.ToTable("core_token_info");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseMySqlIdentityColumn();

                builder.Property(x => x.Source).HasMaxLength(100);
                builder.Property(x => x.LocalIpAddress).HasMaxLength(100);
                builder.Property(x => x.PublicIpAddress).HasMaxLength(100);
                builder.Property(x => x.AccessToken).IsRequired();

                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<Setting>(builder =>
            {
                builder.ToTable("core_setting");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseMySqlIdentityColumn();

                builder.Property(x => x.Key).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Name).HasMaxLength(250).IsUnicode(true);
                builder.Property(x => x.Description).HasMaxLength(500).IsUnicode(true);


                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique();
                builder.HasIndex(x => new { x.Key, x.TenantCode, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<Label>(builder =>
            {
                builder.ToTable("core_label");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseMySqlIdentityColumn();

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
                builder.ToTable("core_user");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();

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
                builder.ToTable("core_user_info");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
            });

            modelBuilder.Entity<Role>(builder =>
            {
                builder.ToTable("core_role");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();

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
                builder.ToTable("core_user_claim");
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
            {
                builder.ToTable("core_user_role");
            });

            modelBuilder.Entity<IdentityUserLogin<Guid>>(builder =>
            {
                builder.ToTable("core_user_login");
            });

            modelBuilder.Entity<IdentityRoleClaim<Guid>>(builder =>
            {
                builder.ToTable("core_role_claim");
            });

            modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
            {
                builder.ToTable("core_user_token");
            });

            modelBuilder.Entity<Country>(builder =>
            {
                builder.ToTable("core_country");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<City>(builder =>
            {
                builder.ToTable("core_city");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<Disctrict>(builder =>
            {
                builder.ToTable("core_district");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<Ward>(builder =>
            {
                builder.ToTable("core_ward");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });
        }
    }
}
