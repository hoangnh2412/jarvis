using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Entities;
using Platform.ORM.EntityFramework.DataStorages;

namespace UnitTest.Org;

public class OrgEntityFilterTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OrgA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrgB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task T8_WhenOrgFilterSet_ReturnsMatchingRowsAndSharedNullOrg()
    {
        var options = new DbContextOptionsBuilder<FilterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new FilterTestDbContext(options);
        db.SetTenantId(TenantId);
        db.Rows.AddRange(
            new ScopedRow { Id = Guid.NewGuid(), TenantId = TenantId, OrgId = OrgA, Code = "A" },
            new ScopedRow { Id = Guid.NewGuid(), TenantId = TenantId, OrgId = OrgB, Code = "B" },
            new ScopedRow { Id = Guid.NewGuid(), TenantId = TenantId, OrgId = null, Code = "Shared" });
        await db.SaveChangesAsync();

        db.SetOrgId(OrgA);
        var visible = await db.Rows.Select(x => x.Code).OrderBy(x => x).ToListAsync();

        Assert.Equal(["A", "Shared"], visible);
    }

    [Fact]
    public async Task T8_WhenOrgFilterNotSet_ReturnsAllTenantRows()
    {
        var options = new DbContextOptionsBuilder<FilterTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new FilterTestDbContext(options);
        db.SetTenantId(TenantId);
        db.Rows.AddRange(
            new ScopedRow { Id = Guid.NewGuid(), TenantId = TenantId, OrgId = OrgA, Code = "A" },
            new ScopedRow { Id = Guid.NewGuid(), TenantId = TenantId, OrgId = OrgB, Code = "B" });
        await db.SaveChangesAsync();

        var visible = await db.Rows.Select(x => x.Code).OrderBy(x => x).ToListAsync();

        Assert.Equal(["A", "B"], visible);
    }

    private sealed class FilterTestDbContext(DbContextOptions<FilterTestDbContext> options)
        : BaseStorageContext(options)
    {
        public DbSet<ScopedRow> Rows => Set<ScopedRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ScopedRow>(e =>
            {
                e.HasKey(x => x.Id);
            });
        }
    }

    private sealed class ScopedRow : ITenantEntity, IOrgEntity
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public Guid? OrgId { get; set; }

        public string Code { get; set; } = string.Empty;
    }
}
