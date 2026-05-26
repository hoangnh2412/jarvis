using Jarvis.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;

namespace Sample.Persistence;

public class MasterDbContext(
    DbContextOptions<MasterDbContext> options)
    : BaseStorageContext<MasterDbContext>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
    }
}
