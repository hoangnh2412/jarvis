using Jarvis.ORM.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Jarvis.Modules.Setting.EntityFramework.Entities;
using Jarvis.Modules.Setting.EntityFramework.Extensions;
using Sample.Entities;

namespace Sample.Persistence;

public class MasterDbContext(
    DbContextOptions<MasterDbContext> options)
    : BaseStorageContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
        modelBuilder.ConfigureSetting();
    }
}
