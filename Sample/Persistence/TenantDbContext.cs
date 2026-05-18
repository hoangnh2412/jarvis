using Jarvis.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;

namespace Sample.Persistence;

public class TenantDbContext(
    DbContextOptions<TenantDbContext> options)
    : BaseStorageContext<TenantDbContext>(options)
{
    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new StudentEntityConfiguration());
    }
}
