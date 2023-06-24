using System;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Entities;
using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Database.Oracle
{
    public class CoreDbContext : IdentityDbContext<User, Role, Guid>, IStorageContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("core");

            modelBuilder.Entity<EmailTemplate>(builder =>
            {
                builder.ToTable("email_template");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Code).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Name).HasMaxLength(200).IsUnicode(true);

                builder.HasIndex(x => x.Key).IsUnique();
                builder.HasIndex(x => new { x.Code, x.TenantCode }).IsUnique();
            });

            modelBuilder.Entity<EmailHistory>(builder =>
            {
                builder.ToTable("email_history");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Code).HasMaxLength(50).IsUnicode(false);

                builder.HasIndex(x => x.Key).IsUnique();
                builder.HasIndex(x => new { x.Code, x.TenantCode });
                builder.HasIndex(x => new { x.Type, x.TenantCode });
                builder.HasIndex(x => new { x.To, x.TenantCode });
                builder.HasIndex(x => new { x.Status, x.TenantCode });
                builder.HasIndex(x => new { x.CreatedAt, x.TenantCode });
            });

            modelBuilder.Entity<File>(builder =>
            {
                builder.ToTable("FILE_MEDIA");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Extension).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.FileName).HasMaxLength(500).IsUnicode(true);

                builder.HasIndex(x => x.FileName).IsUnique();
                builder.HasIndex(x => x.TenantCode);
            });

            modelBuilder.Entity<OrganizationUnit>(builder =>
            {
                builder.ToTable("ORGANIZATION_UNIT");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
                builder.Property(x => x.FullName).IsRequired().HasMaxLength(250);
                builder.Property(x => x.Description).HasMaxLength(250);

                builder.HasIndex(x => x.Key).IsUnique();
                builder.HasIndex(x => new { x.Name, x.TenantCode, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<OrganizationUser>(builder =>
            {
                builder.ToTable("ORGANIZATION_USER");
                builder.HasKey(x => new { x.IdUser, x.OrganizationCode });
                builder.HasIndex(x => new { x.IdUser, x.OrganizationCode }).IsUnique();
            });

            modelBuilder.Entity<Tenant>(builder =>
            {
                builder.ToTable("TENANT");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Key).IsRequired();
                builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Server).HasMaxLength(250);
                builder.Property(x => x.Database).HasMaxLength(250);
                builder.Property(x => x.DbConnectionString).HasMaxLength(1000);
                builder.Property(x => x.Theme).HasMaxLength(250);
                builder.Property(x => x.IsEnable).IsRequired().HasDefaultValue(false);

                builder.HasIndex(x => x.Key).IsUnique();
                builder.HasIndex(x => new { x.Name, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<TenantInfo>(builder =>
            {
                builder.ToTable("TENANT_INFO");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Key).IsRequired();
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
                builder.ToTable("TENANT_HOST");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Key).IsRequired();
                builder.Property(x => x.HostName).IsRequired().HasMaxLength(250);

                builder.HasIndex(x => new { x.HostName, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<TokenInfo>(builder =>
            {
                builder.ToTable("TOKEN_INFO");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Source).HasMaxLength(100);
                builder.Property(x => x.LocalIpAddress).HasMaxLength(100);
                builder.Property(x => x.PublicIpAddress).HasMaxLength(100);
                builder.Property(x => x.AccessToken).IsRequired();

                builder.Property(x => x.Key).IsRequired();
                builder.HasIndex(x => x.Key).IsUnique();
            });

            modelBuilder.Entity<Setting>(builder =>
            {
                builder.ToTable("SETTING");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Code).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Name).HasMaxLength(250).IsUnicode(true);
                builder.Property(x => x.Description).HasMaxLength(500).IsUnicode(true);


                builder.Property(x => x.Key).IsRequired();
                builder.HasIndex(x => x.Key).IsUnique();
                builder.HasIndex(x => new { x.Code, x.TenantCode, x.DeletedVersion }).IsUnique();
            });

            modelBuilder.Entity<Label>(builder =>
            {
                builder.ToTable("LABEL");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Name).HasMaxLength(250).IsUnicode(true);
                builder.Property(x => x.Description).HasMaxLength(500).IsUnicode(true);
                builder.Property(x => x.Icon).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Color).HasMaxLength(50).IsUnicode(false);

                builder.Property(x => x.Key).IsRequired();

                builder.HasIndex(x => new { x.Name, x.TenantCode, x.DeletedVersion }).IsUnique();
                builder.HasIndex(x => x.Key).IsUnique();
                builder.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("USER");
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
                builder.ToTable("USER_INFO");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
            });

            modelBuilder.Entity<Role>(builder =>
            {
                builder.ToTable("ROLE");
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
                builder.ToTable("USER_CLAIM");
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
            {
                builder.ToTable("USER_ROLE");
            });

            modelBuilder.Entity<IdentityUserLogin<Guid>>(builder =>
            {
                builder.ToTable("USER_LOGIN");
            });

            modelBuilder.Entity<IdentityRoleClaim<Guid>>(builder =>
            {
                builder.ToTable("ROLE_CLAIM");
            });

            modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
            {
                builder.ToTable("USER_TOKEN");
            });

            modelBuilder.Entity<Country>(builder =>
            {
                builder.ToTable("COUNTRY");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<City>(builder =>
            {
                builder.ToTable("CITY");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<Disctrict>(builder =>
            {
                builder.ToTable("DISTRICT");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });

            modelBuilder.Entity<Ward>(builder =>
            {
                builder.ToTable("WARD");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });
        }
    }
}



