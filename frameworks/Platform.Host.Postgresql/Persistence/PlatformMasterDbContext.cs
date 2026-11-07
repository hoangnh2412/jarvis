using Microsoft.EntityFrameworkCore;
using Platform.Host.Entities;
using Platform.ORM.EntityFramework.DataStorages;
using Platform.Tenants.EntityFramework.Extensions;

namespace Platform.Host.Postgresql.Persistence;

/// <summary>
/// DbContext master mặc định: catalog tenant. Product thêm bảng riêng thì kế thừa class này.
/// </summary>
public class PlatformMasterDbContext : BaseStorageContext
{
    public PlatformMasterDbContext(DbContextOptions<PlatformMasterDbContext> options)
        : base(options)
    {
    }

    protected PlatformMasterDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyPlatformTenantCatalog<Tenant>();
        modelBuilder.Entity<Tenant>().ToTable("Tenant");
    }
}
