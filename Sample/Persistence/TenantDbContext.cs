using Jarvis.ORM.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;

namespace Sample.Persistence;

public class TenantDbContext(
    DbContextOptions<TenantDbContext> options)
    : BaseStorageContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<BasicAuthUser> BasicAuthUsers => Set<BasicAuthUser>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new StudentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BasicAuthUserEntityConfiguration());
    }
}
