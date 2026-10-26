using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Tenants;
using Platform.Tenants.Models;
using Platform.Tenants.Persistence;
using Platform.Tenants.Services;

namespace UnitTest.TenantCatalog;

public class TenantManagerTests
{
    private const string Connection = "Host=tenant-db";

    [Fact]
    public async Task Create_RequiresConnectionString()
    {
        var manager = CreateManager();

        var error = await Assert.ThrowsAsync<BadRequestException>(() => manager.CreateAsync(new CreateTenantInput
        {
            Code = "vna",
            Name = "VNA",
            ConnectionString = "  ",
        }));

        Assert.Equal(TenantErrorCode.InvalidInput, error.Code);
    }

    [Fact]
    public async Task Create_NormalizesCode_AndStoresParent()
    {
        var manager = CreateManager();
        var parent = await manager.CreateAsync(new CreateTenantInput
        {
            Code = "Minvoice",
            Name = "Minvoice",
            ConnectionString = Connection,
        });

        var child = await manager.CreateAsync(new CreateTenantInput
        {
            Code = " HCM ",
            Name = "Minvoice HCM",
            ConnectionString = Connection,
            ParentTenantId = parent.Id,
        });

        Assert.Equal("hcm", child.Code);
        Assert.Equal(parent.Id, child.ParentTenantId);
        Assert.Equal(Connection, child.ConnectionString);
        Assert.True(child.IsActive);
    }

    [Fact]
    public async Task Create_DuplicateCode_Conflicts()
    {
        var manager = CreateManager();
        await manager.CreateAsync(new CreateTenantInput
        {
            Code = "vna",
            Name = "VNA",
            ConnectionString = Connection,
        });

        var error = await Assert.ThrowsAsync<ConflictException>(() => manager.CreateAsync(new CreateTenantInput
        {
            Code = "VNA",
            Name = "Khác",
            ConnectionString = Connection,
        }));

        Assert.Equal(TenantErrorCode.CodeAlreadyExists, error.Code);
    }

    [Fact]
    public async Task SetParent_RejectsCycle()
    {
        var manager = CreateManager();
        var root = await manager.CreateAsync(Node("minvoice"));
        var hcm = await manager.CreateAsync(Node("hcm", root.Id));

        var error = await Assert.ThrowsAsync<BadRequestException>(() => manager.SetParentAsync(root.Id, hcm.Id));

        Assert.Equal(TenantErrorCode.InvalidParent, error.Code);
    }

    [Fact]
    public async Task SetActive_SoftDisables_AndBlocksConnection()
    {
        var manager = CreateManager();
        var created = await manager.CreateAsync(Node("vna"));

        var disabled = await manager.SetActiveAsync(created.Id, false);

        Assert.False(disabled.IsActive);
        var error = await Assert.ThrowsAsync<ConflictException>(() => manager.GetConnectionStringAsync(created.Id));
        Assert.Equal(TenantErrorCode.Inactive, error.Code);
    }

    [Fact]
    public async Task ListSubtree_IncludesDescendants_ExcludesSiblings()
    {
        var manager = CreateManager();
        var minvoice = await manager.CreateAsync(Node("minvoice"));
        var hcm = await manager.CreateAsync(Node("hcm", minvoice.Id));
        await manager.CreateAsync(Node("hn", minvoice.Id));
        await manager.CreateAsync(Node("epay"));
        var agent = await manager.CreateAsync(Node("agent", hcm.Id));

        var hcmTree = await manager.ListSubtreeAsync(hcm.Id);
        var codes = hcmTree.Select(tenant => tenant.Code).OrderBy(code => code).ToArray();

        Assert.Equal(["agent", "hcm"], codes);
        Assert.Contains(hcmTree, tenant => tenant.Id == agent.Id);
        Assert.All(hcmTree, tenant => Assert.Null(tenant.ConnectionString));
    }

    [Fact]
    public async Task List_OmitsConnectionString()
    {
        var manager = CreateManager();
        await manager.CreateAsync(Node("vna"));

        var listed = await manager.ListAsync();

        Assert.Single(listed);
        Assert.Null(listed[0].ConnectionString);
    }

    private static CreateTenantInput Node(string code, Guid? parentId = null) => new()
    {
        Code = code,
        Name = code,
        ConnectionString = Connection,
        ParentTenantId = parentId,
    };

    private static TenantManager<TestTenant> CreateManager() => new(new MemoryTenantStore());

    private sealed class TestTenant : ITenantCatalogEntity
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public Guid? ParentTenantId { get; set; }

        public string ConnectionString { get; set; } = string.Empty;
    }

    private sealed class MemoryTenantStore : ITenantStore<TestTenant>
    {
        private readonly List<TestTenant> _rows = [];

        public Task<TestTenant?> FindTrackedAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(_rows.FirstOrDefault(tenant => tenant.Id == id));

        public Task<TestTenant?> FindByCodeAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult(_rows.FirstOrDefault(tenant => tenant.Code == code));

        public Task<bool> CodeExistsAsync(string code, Guid? exceptId, CancellationToken cancellationToken) =>
            Task.FromResult(_rows.Any(tenant => tenant.Code == code && tenant.Id != exceptId));

        public Task<IReadOnlyList<TestTenant>> ListAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TestTenant>>(_rows.ToArray());

        public Task AddAsync(TestTenant entity, CancellationToken cancellationToken)
        {
            _rows.Add(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
