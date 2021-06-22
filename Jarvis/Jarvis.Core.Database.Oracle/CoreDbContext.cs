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
            modelBuilder.HasDefaultSchema("INEINVOICE");
            modelBuilder.HasSequence<int>("SEQ_File", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
            modelBuilder.Entity<File>(builder =>
            {
                builder.ToTable("Core_File");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Name).HasMaxLength(500).IsUnicode(false);
                builder.Property(x => x.ContentType).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.FileName).HasMaxLength(500).IsUnicode(true);

                builder.HasIndex(x => x.Name).IsUnique().HasName("IX_Core_File_1");
                builder.HasIndex(x => x.TenantCode).HasName("IX_Core_File_2");
            });
            modelBuilder.HasSequence<int>("SEQ_OrganizationUnit", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
            modelBuilder.Entity<OrganizationUnit>(builder =>
            {
                builder.ToTable("Core_OrganizationUnit");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
                builder.Property(x => x.FullName).IsRequired().HasMaxLength(250);
                builder.Property(x => x.Description).HasMaxLength(250);

                builder.HasIndex(x => x.Code).IsUnique().HasName("IX_Core_OrganizationUnit_1");
                builder.HasIndex(x => new { x.Name, x.TenantCode, x.DeletedVersion }).IsUnique().HasName("IX_Core_OrganizationUnit_2");
            });
            modelBuilder.HasSequence<int>("SEQ_OrganizationUser", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
            modelBuilder.Entity<OrganizationUser>(builder =>
            {
                builder.ToTable("Core_OrganizationUser");
                builder.HasKey(x => new { x.IdUser, x.OrganizationCode });
            });
            modelBuilder.HasSequence<int>("SEQ_OrganizationRole", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
            modelBuilder.Entity<OrganizationRole>(builder =>
            {
                builder.ToTable("Core_OrganizationRole");
                builder.HasKey(x => new { x.IdRole, x.OrganizationCode });
            });
            modelBuilder.HasSequence<int>("SEQ_Tenant", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
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

                builder.HasIndex(x => x.Code).IsUnique().HasName("IX_Core_Tenant_1");
                builder.HasIndex(x => new { x.Name, x.DeletedVersion }).IsUnique().HasName("IX_Core_Tenant_2");
            });
            modelBuilder.HasSequence<int>("SEQ_TenantInfo", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
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
            modelBuilder.HasSequence<int>("SEQ_TenantHost", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
            modelBuilder.Entity<TenantHost>(builder =>
            {
                builder.ToTable("Core_TenantHost");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Code).IsRequired();
                builder.Property(x => x.HostName).IsRequired().HasMaxLength(250);

                builder.HasIndex(x => new { x.HostName, x.DeletedVersion }).IsUnique().HasName("IX_Core_TenantHost_1");
            });
            modelBuilder.HasSequence<int>("SEQ_TokenInfo", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
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
                builder.HasIndex(x => x.Code).IsUnique().HasName("IX_Core_TokenInfo_1");
            });
            modelBuilder.HasSequence<int>("SEQ_Setting", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
            modelBuilder.Entity<Setting>(builder =>
            {
                builder.ToTable("Core_Setting");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Key).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Name).HasMaxLength(250).IsUnicode(true);
                builder.Property(x => x.Description).HasMaxLength(500).IsUnicode(true);


                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique().HasName("IX_Core_Setting_1");
                builder.HasIndex(x => new { x.Key, x.TenantCode, x.DeletedVersion }).IsUnique().HasName("IX_Core_Setting_2");
            });
            modelBuilder.HasSequence<int>("SEQ_Label", schema: "INEINVOICE")
                        .StartsAt(1)
                        .IncrementsBy(1);
            modelBuilder.Entity<Label>(builder =>
            {
                builder.ToTable("Core_Label");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired().UseIdentityColumn();

                builder.Property(x => x.Name).HasMaxLength(250).IsUnicode(true);
                builder.Property(x => x.Description).HasMaxLength(500).IsUnicode(true);
                builder.Property(x => x.Icon).HasMaxLength(50).IsUnicode(false);
                builder.Property(x => x.Color).HasMaxLength(50).IsUnicode(false);

                builder.HasIndex(x => new { x.Name, x.TenantCode, x.DeletedVersion }).IsUnique().HasName("IX_Core_Label_1");
                builder.Property(x => x.Code).IsRequired();
                builder.HasIndex(x => x.Code).IsUnique().HasName("IX_Core_Label_2");
            });
            modelBuilder.HasSequence<int>("SEQ_User", schema: "INEINVOICE")
                       .StartsAt(1)
                       .IncrementsBy(1);
            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("Core_User");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();

                builder.HasIndex(x => x.NormalizedUserName).IsUnique(false).HasName("IX_Core_User_1");
                builder.HasIndex(x => x.UserName).IsUnique(false).HasName("IX_Core_User_2");
                builder.HasIndex(x => x.Email).IsUnique(false).HasName("IX_Core_User_3");
                builder.HasIndex(x => new
                {
                    x.NormalizedUserName,
                    x.TenantCode,
                    x.DeletedVersion
                }).IsUnique().HasName("IX_Core_User_4");
            });
            modelBuilder.HasSequence<int>("SEQ_UserInfo", schema: "INEINVOICE")
                       .StartsAt(1)
                       .IncrementsBy(1);
            modelBuilder.Entity<UserInfo>(builder =>
            {
                builder.ToTable("Core_UserInfo");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
            });
            modelBuilder.HasSequence<int>("SEQ_Role", schema: "INEINVOICE")
                       .StartsAt(1)
                       .IncrementsBy(1);
            modelBuilder.Entity<Role>(builder =>
            {
                builder.ToTable("Core_Role");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();

                builder.HasIndex(x => x.NormalizedName).IsUnique(false).HasName("IX_Core_Role_1");
                builder.HasIndex(x => new
                {
                    x.NormalizedName,
                    x.TenantCode,
                    x.DeletedVersion
                }).IsUnique().HasName("IX_Core_Role_2");
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
            modelBuilder.HasSequence<int>("SEQ_Country", schema: "INEINVOICE")
                       .StartsAt(1)
                       .IncrementsBy(1);
            modelBuilder.Entity<Country>(builder =>
            {
                builder.ToTable("Core_Country");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });
            modelBuilder.HasSequence<int>("SEQ_City", schema: "INEINVOICE")
                       .StartsAt(1)
                       .IncrementsBy(1);
            modelBuilder.Entity<City>(builder =>
            {
                builder.ToTable("Core_City");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });
            modelBuilder.HasSequence<int>("SEQ_Disctrict", schema: "INEINVOICE")
                       .StartsAt(1)
                       .IncrementsBy(1);
            modelBuilder.Entity<Disctrict>(builder =>
            {
                builder.ToTable("Core_District");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
                builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
            });
            modelBuilder.HasSequence<int>("SEQ_Ward", schema: "INEINVOICE")
                       .StartsAt(1)
                       .IncrementsBy(1);
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
