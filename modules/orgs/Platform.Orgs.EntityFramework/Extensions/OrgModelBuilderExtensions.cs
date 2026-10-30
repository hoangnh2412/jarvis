using Microsoft.EntityFrameworkCore;
using Platform.Orgs;

namespace Platform.Orgs.EntityFramework.Extensions;

/// <summary>
/// Map cột chuẩn của catalog. Property thêm trên class host do EF convention map — method này không đụng chúng.
/// </summary>
public static class OrgModelBuilderExtensions
{
    public static void ApplyPlatformOrgCatalog<TOrg>(this ModelBuilder modelBuilder)
        where TOrg : class, IOrgManagementEntity
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<TOrg>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TenantId).IsRequired();
            builder.Property(x => x.Code).HasMaxLength(OrgCatalogLimits.CodeMaxLength).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(OrgCatalogLimits.NameMaxLength).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.ParentId);
            builder.HasIndex(x => x.ParentId);
            builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        });
    }
}
