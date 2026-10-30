using Microsoft.EntityFrameworkCore;
using Platform.Tenants;

namespace Platform.Tenants.EntityFramework.Extensions;

/// <summary>Map cột chuẩn catalog tenant. Cột host do EF convention map.</summary>
public static class TenantModelBuilderExtensions
{
    public static void ApplyPlatformTenantCatalog<TTenant>(this ModelBuilder modelBuilder)
        where TTenant : class, ITenantCatalogEntity
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<TTenant>(builder =>
        {
            builder.HasKey(tenant => tenant.Id);
            builder.Property(tenant => tenant.Code).HasMaxLength(TenantCatalogLimits.CodeMaxLength).IsRequired();
            builder.Property(tenant => tenant.Name).HasMaxLength(TenantCatalogLimits.NameMaxLength).IsRequired();
            builder.Property(tenant => tenant.IsActive).IsRequired();
            builder.Property(tenant => tenant.ParentId).HasColumnName("parent_id");
            builder.Property(tenant => tenant.ConnectionString)
                .HasMaxLength(TenantCatalogLimits.ConnectionStringMaxLength)
                .IsRequired();
            builder.HasIndex(tenant => tenant.Code).IsUnique();
            builder.HasIndex(tenant => tenant.ParentId);
        });
    }
}
