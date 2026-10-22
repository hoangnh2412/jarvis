using Platform.DDD.Domain.Entities;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Orgs;
using Platform.Orgs.Hooks;
using Platform.Orgs.Models;
using Platform.Orgs.Persistence;
using Platform.Orgs.Mapping;
using Platform.Orgs.Abstractions.Events;
using Platform.Orgs.Events;
using Platform.Orgs.Services;
using UnitTest.SignalR;

namespace UnitTest.Org;

public class OrgManagerTests
{
    private static readonly Guid TenantX = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantY = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task Create_PersistsTenantFromAmbient_AndHostColumnViaPrepareHook()
    {
        var fixture = CreateFixture(TenantX);

        var created = await fixture.Manager.CreateAsync(new HostCreateOrgInput
        {
            Code = " acme ",
            Name = "Acme",
            TaxCode = "0101234567",
        });

        Assert.Equal(TenantX, created.TenantId);
        Assert.Equal("acme", created.Code);
        Assert.True(created.IsActive);

        var stored = fixture.Store.Find(created.Id);
        Assert.NotNull(stored);
        Assert.Equal("0101234567", stored.TaxCode);
    }

    [Fact]
    public async Task List_ReturnsOnlyAmbientTenant()
    {
        var fixture = CreateFixture(TenantX);
        await fixture.Manager.CreateAsync(new HostCreateOrgInput { Code = "A", Name = "Alpha" });
        fixture.Store.Seed(new HostOrg
        {
            Id = Guid.NewGuid(),
            TenantId = TenantY,
            Code = "C",
            Name = "Charlie",
            IsActive = true,
        });

        var listed = await fixture.Manager.ListAsync();

        Assert.Single(listed);
        Assert.Equal("a", listed[0].Code);
        Assert.Equal(TenantX, listed[0].TenantId);
    }

    [Fact]
    public async Task Validate_RejectsInactive_AndOrgOutsideTenant()
    {
        var fixture = CreateFixture(TenantX);
        var created = await fixture.Manager.CreateAsync(new HostCreateOrgInput { Code = "A", Name = "Alpha" });
        await fixture.Manager.SetActiveAsync(created.Id, false);

        var inactive = await Assert.ThrowsAsync<ConflictException>(() =>
            fixture.Catalog.ValidateForAssignmentAsync(created.Id, TenantX));
        Assert.Equal(OrgErrorCode.Inactive, inactive.Code);

        var otherTenant = await Assert.ThrowsAsync<NotFoundException>(() =>
            fixture.Catalog.ValidateForAssignmentAsync(created.Id, TenantY));
        Assert.Equal(OrgErrorCode.NotFound, otherTenant.Code);
    }

    [Fact]
    public async Task CatalogReader_ReturnsHostFieldsViaMapper()
    {
        var fixture = CreateFixture(TenantX);
        var created = await fixture.Manager.CreateAsync(new HostCreateOrgInput
        {
            Code = "acme",
            Name = "Acme",
            TaxCode = "0101234567",
        });

        var reader = new OrgCatalogReader<HostOrg, TestTenantInfo, HostOrgDetail>(
            fixture.Store,
            new FakeCurrentTenant(TenantX),
            new HostOrgReadMapper());

        var detail = await reader.GetByIdAsync(created.Id);
        Assert.NotNull(detail);
        Assert.Equal("0101234567", detail.TaxCode);
        Assert.Equal("acme", detail.Code);
    }

    [Fact]
    public async Task Create_DuplicateCodeInTenant_Conflicts()
    {
        var fixture = CreateFixture(TenantX);
        await fixture.Manager.CreateAsync(new HostCreateOrgInput { Code = "A", Name = "Alpha" });

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            fixture.Manager.CreateAsync(new HostCreateOrgInput { Code = "a", Name = "Other" }));

        Assert.Equal(OrgErrorCode.CodeAlreadyExists, error.Code);
    }

    private static Fixture CreateFixture(Guid tenantId)
    {
        var store = new MemoryOrgStore();
        var manager = new OrgManager<HostOrg, TestTenantInfo>(
            store,
            new FakeCurrentTenant(tenantId),
            new HostOrgHooks(),
            new NullOrgCatalogEventPublisher());
        return new Fixture(store, manager, new OrgCatalog<HostOrg>(store));
    }

    private sealed record Fixture(MemoryOrgStore Store, OrgManager<HostOrg, TestTenantInfo> Manager, OrgCatalog<HostOrg> Catalog);

    private sealed class HostOrg : BaseEntity<Guid>, IOrgCatalogEntity
    {
        public Guid TenantId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string? TaxCode { get; set; }
    }

    private sealed class HostCreateOrgInput : CreateOrgInput
    {
        public string? TaxCode { get; set; }
    }

    private sealed class HostOrgDetail
    {
        public Guid Id { get; init; }

        public Guid TenantId { get; init; }

        public string Code { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public string? TaxCode { get; init; }
    }

    private sealed class HostOrgReadMapper : IOrgCatalogReadMapper<HostOrg, HostOrgDetail>
    {
        public HostOrgDetail Map(HostOrg entity) =>
            new()
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Code = entity.Code,
                Name = entity.Name,
                IsActive = entity.IsActive,
                TaxCode = entity.TaxCode,
            };
    }

    private sealed class HostOrgHooks : DefaultOrgCatalogHooks<HostOrg>
    {
        public override Task PrepareCreateAsync(
            OrgCatalogCreateHookContext<HostOrg> context,
            CancellationToken cancellationToken = default)
        {
            if (context.Input is HostCreateOrgInput host)
                context.Entity.TaxCode = host.TaxCode;

            return Task.CompletedTask;
        }
    }

    private sealed class MemoryOrgStore : IOrgStore<HostOrg>
    {
        private readonly List<HostOrg> _items = [];

        public HostOrg? Find(Guid id) => _items.FirstOrDefault(x => x.Id == id);

        public void Seed(HostOrg entity) => _items.Add(entity);

        public Task<HostOrg?> FindTrackedAsync(Guid tenantId, Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(_items.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id));

        public Task<HostOrg?> FindByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken)
        {
            var match = _items.FirstOrDefault(x =>
                x.TenantId == tenantId && x.Code == code);
            return Task.FromResult(match);
        }

        public Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? exceptId, CancellationToken cancellationToken)
        {
            var exists = _items.Any(x =>
                x.TenantId == tenantId
                && x.Code == code
                && (exceptId == null || x.Id != exceptId));
            return Task.FromResult(exists);
        }

        public Task<IReadOnlyList<HostOrg>> ListAsync(
            Guid tenantId,
            bool? isActive,
            string? search,
            CancellationToken cancellationToken)
        {
            IEnumerable<HostOrg> query = _items.Where(x => x.TenantId == tenantId);
            if (isActive is bool active)
                query = query.Where(x => x.IsActive == active);

            IReadOnlyList<HostOrg> items = query.OrderBy(x => x.Code).ToArray();
            return Task.FromResult(items);
        }

        public Task AddAsync(HostOrg entity, CancellationToken cancellationToken)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
