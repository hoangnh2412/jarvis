using System;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Entities;
using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Database.SqlServer
{
    public class CoreDbContext : IdentityDbContext<User, Role, Guid>, IStorageContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<File>(builder =>
            {
                builder.ToTable("Core_File");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Name).HasMaxLength(500).IsUnicode(false);
                builder.Property(x => x.ContentType).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.FileName).HasMaxLength(500).IsUnicode(true);

                builder.HasIndex(x => x.Name).IsUnique();
                builder.HasIndex(x => x.TenantCode);
            });

            modelBuilder.Entity<OrganizationUnit>(builder =>
            {
                builder.ToTable("Core_OrganizationUnit");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
                builder.Property(x => x.FullName).IsRequired().HasMaxLength(250);
                builder.Property(x => x.Description).HasMaxLength(250);

                builder.HasIndex(x => x.Code).IsUnique();
                builder.HasIndex(x => new { x.Name, x.TenantCode, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<OrganizationUser>(builder =>
            {
                builder.ToTable("Core_OrganizationUser");
                builder.HasKey(x => new { x.IdUser, x.OrganizationCode });
                builder.HasIndex(x => new { x.IdUser, x.OrganizationCode }).IsUnique();
            });

            modelBuilder.Entity<Tenant>(builder =>
            {
                builder.ToTable("Core_Tenant");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

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
                builder.ToTable("Core_TenantInfo");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

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
                builder.ToTable("Core_TenantHost");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Code).IsRequired();
                builder.Property(x => x.HostName).IsRequired().HasMaxLength(250);

                builder.HasIndex(x => new { x.HostName, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<TokenInfo>(builder =>
            {
                builder.ToTable("Core_TokenInfo");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Source).HasMaxLength(100);
                builder.Property(x => x.LocalIpAddress).HasMaxLength(100);
                builder.Property(x => x.PublicIpAddress).HasMaxLength(100);
                builder.Property(x => x.AccessToken).IsRequired();

                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<Setting>(builder =>
            {
                builder.ToTable("Core_Setting");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Key).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Name).HasMaxLength(250).IsUnicode(true);
                builder.Property(x => x.Description).HasMaxLength(500).IsUnicode(true);


                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique();
                builder.HasIndex(x => new { x.Key, x.TenantCode, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<Label>(builder =>
            {
                builder.ToTable("Core_Label");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

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
                builder.ToTable("Core_User");
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
                builder.ToTable("Core_UserInfo");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
            });

            modelBuilder.Entity<Role>(builder =>
            {
                builder.ToTable("Core_Role");
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
                builder.ToTable("Core_UserClaim");
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
            {
                builder.ToTable("Core_UserRole");
            });

            modelBuilder.Entity<IdentityUserLogin<Guid>>(builder =>
            {
                builder.ToTable("Core_UserLogin");
            });

            modelBuilder.Entity<IdentityRoleClaim<Guid>>(builder =>
            {
                builder.ToTable("Core_RoleClaim");
            });

            modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
            {
                builder.ToTable("Core_UserToken");
            });

            modelBuilder.Entity<Country>(builder =>
            {
                builder.ToTable("Core_Country");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<City>(builder =>
            {
                builder.ToTable("Core_City");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<Disctrict>(builder =>
            {
                builder.ToTable("Core_District");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<Ward>(builder =>
            {
                builder.ToTable("Core_Ward");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });
        }
    }
}
