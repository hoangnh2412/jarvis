using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Entities;
using UnitTest.Querying;

namespace UnitTest.Querying;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; }
    public DbSet<NoAuditEntity> NoAuditEntities { get; set; }
    public DbSet<OnlyCreatedEntity> OnlyCreatedEntities { get; set; }
}
