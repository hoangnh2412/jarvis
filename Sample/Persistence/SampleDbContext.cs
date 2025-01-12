using Jarvis.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;

namespace Sample.Persistence;

public class SampleDbContext(
    DbContextOptions<SampleDbContext> options)
    : BaseStorageContext<SampleDbContext>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new StudentEntityConfiguration());
    }
}