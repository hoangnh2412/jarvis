using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Entities;
using Platform.Orgs;
using Platform.Orgs.EntityFramework.Extensions;

namespace UnitTest.Org;

public class OrgCatalogModelTests
{
    [Fact]
    public void Model_MapsStandardColumns_AndLeavesHostColumn()
    {
        using var context = new HostOrgDbContext(
            new DbContextOptionsBuilder<HostOrgDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var entity = context.Model.FindEntityType(typeof(HostOrg));
        Assert.NotNull(entity);
        Assert.NotNull(entity.FindProperty(nameof(HostOrg.Code)));
        Assert.NotNull(entity.FindProperty(nameof(HostOrg.TaxCode)));
        Assert.Null(typeof(IOrgManagementEntity).GetProperty(nameof(HostOrg.TaxCode)));

        var uniqueOnCode = entity.GetIndexes().Any(index =>
            index.IsUnique
            && index.Properties.Select(property => property.Name).SequenceEqual(["TenantId", "Code"]));
        Assert.True(uniqueOnCode);
    }

    private sealed class HostOrg : BaseEntity<Guid>, IOrgManagementEntity
    {
        public Guid TenantId { get; set; }

        public Guid? ParentId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string? TaxCode { get; set; }
    }

    private sealed class HostOrgDbContext(DbContextOptions<HostOrgDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyPlatformOrgCatalog<HostOrg>();
        }
    }
}
