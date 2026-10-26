using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.OrgStructure;
using Platform.OrgStructure.Models;
using Platform.OrgStructure.Persistence;
using Platform.OrgStructure.Services;
using Platform.Orgs.Abstractions;

namespace UnitTest.OrgStructureCatalog;

public class OrgStructureManagerTests
{
    [Fact]
    public async Task Preview_DoesNotWrite_ApplyWritesConfirmedRows()
    {
        var store = new MemoryStore();
        var manager = CreateManager(store);
        var preview = manager.PreviewTemplate(OrgStructureLimits.SimpleCompanyTemplate, "Công ty");

        Assert.Empty(store.Units);
        Assert.Equal(4, preview.Nodes.Count);
        Assert.Contains(preview.Nodes, node => node.Path == "/cong-ty/ke-toan");

        var written = await manager.ApplyTemplateAsync(new ApplyOrgStructureTemplateInput(
            OrgId: OrgA,
            TenantId: TenantA,
            TemplateCode: preview.TemplateCode,
            RootName: preview.RootName,
            Confirmed: true,
            Nodes: preview.Nodes));

        Assert.Equal(4, written.Count);
        Assert.All(written, unit => Assert.Equal(OrgA, unit.OrgId));
        Assert.Equal(4, store.Units.Count);
    }

    [Fact]
    public async Task Apply_RejectsUnconfirmedMismatchAndSecondApply()
    {
        var manager = CreateManager(new MemoryStore());
        var preview = manager.PreviewTemplate(OrgStructureLimits.SimpleCompanyTemplate, "Công ty");

        var missingConfirm = await Assert.ThrowsAsync<BadRequestException>(() => manager.ApplyTemplateAsync(
            new ApplyOrgStructureTemplateInput(OrgA, TenantA, preview.TemplateCode, preview.RootName, false, preview.Nodes)));
        Assert.Equal(OrgStructureErrorCode.InvalidInput, missingConfirm.Code);

        var tampered = preview.Nodes.Select(node => node with { Name = node.Name + " x" }).ToArray();
        var mismatch = await Assert.ThrowsAsync<BadRequestException>(() => manager.ApplyTemplateAsync(
            new ApplyOrgStructureTemplateInput(OrgA, TenantA, preview.TemplateCode, preview.RootName, true, tampered)));
        Assert.Equal(OrgStructureErrorCode.InvalidInput, mismatch.Code);

        await manager.ApplyTemplateAsync(new ApplyOrgStructureTemplateInput(
            OrgA, TenantA, preview.TemplateCode, preview.RootName, true, preview.Nodes));
        var second = await Assert.ThrowsAsync<ConflictException>(() => manager.ApplyTemplateAsync(
            new ApplyOrgStructureTemplateInput(OrgA, TenantA, preview.TemplateCode, preview.RootName, true, preview.Nodes)));
        Assert.Equal(OrgStructureErrorCode.TemplateNotEmpty, second.Code);
    }

    [Fact]
    public async Task Move_RewritesDescendantPaths_AndRejectsOtherOrgOrCycle()
    {
        var manager = CreateManager(new MemoryStore());
        var root = await manager.CreateUnitAsync(Unit(OrgA, "ha-noi", "Hà Nội", SuggestedUnitTypes.Branch, null));
        var child = await manager.CreateUnitAsync(Unit(OrgA, "phong-ve", "Phòng vé", SuggestedUnitTypes.Department, root.Id));
        var grandchild = await manager.CreateUnitAsync(Unit(OrgA, "quay-01", "Quầy 01", SuggestedUnitTypes.Section, child.Id));
        var sibling = await manager.CreateUnitAsync(Unit(OrgA, "da-nang", "Đà Nẵng", SuggestedUnitTypes.Branch, null));
        var other = await manager.CreateUnitAsync(Unit(OrgB, "sai-gon", "Sài Gòn", SuggestedUnitTypes.Branch, null));

        var moved = await manager.MoveUnitAsync(child.Id, sibling.Id);
        var descendants = await manager.GetDescendantsAsync(sibling.Id);

        Assert.Equal("/da-nang/phong-ve", moved.Path);
        Assert.Contains(descendants, unit => unit.Id == grandchild.Id && unit.Path == "/da-nang/phong-ve/quay-01");

        var crossOrg = await Assert.ThrowsAsync<BadRequestException>(() => manager.MoveUnitAsync(moved.Id, other.Id));
        Assert.Equal(OrgStructureErrorCode.InvalidParent, crossOrg.Code);

        var cycle = await Assert.ThrowsAsync<BadRequestException>(() => manager.MoveUnitAsync(sibling.Id, grandchild.Id));
        Assert.Equal(OrgStructureErrorCode.InvalidParent, cycle.Code);
    }

    [Fact]
    public async Task SetInactive_RejectsWhenBranchHasActivePosition()
    {
        var manager = CreateManager(new MemoryStore());
        var branch = await manager.CreateUnitAsync(Unit(OrgA, "ha-noi", "Hà Nội", SuggestedUnitTypes.Branch, null));
        var room = await manager.CreateUnitAsync(Unit(OrgA, "phong-ve", "Phòng vé", SuggestedUnitTypes.Department, branch.Id));
        await manager.CreatePositionAsync(new CreatePositionInput(OrgA, room.Id, null, null));

        var error = await Assert.ThrowsAsync<ConflictException>(() => manager.SetUnitActiveAsync(branch.Id, false));
        Assert.Equal(OrgStructureErrorCode.HasActivePositions, error.Code);

        await manager.SetPositionActiveAsync((await manager.ListPositionsAsync(OrgA)).Single().Id, false);
        var inactive = await manager.SetUnitActiveAsync(branch.Id, false);
        Assert.False(inactive.IsActive);
    }

    [Fact]
    public async Task GetDescendants_ReturnsEveryLowerLevel()
    {
        var manager = CreateManager(new MemoryStore());
        Guid? parentId = null;
        Guid rootId = Guid.Empty;
        foreach (var code in new[] { "n1", "n2", "n3", "n4", "n5" })
        {
            var created = await manager.CreateUnitAsync(Unit(OrgA, code, code, SuggestedUnitTypes.Department, parentId));
            if (parentId is null)
                rootId = created.Id;
            parentId = created.Id;
        }

        var descendants = await manager.GetDescendantsAsync(rootId);
        Assert.Equal(4, descendants.Count);
        Assert.Equal("/n1/n2/n3/n4/n5", descendants[^1].Path);
    }

    private static readonly Guid OrgA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OrgB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid TenantA = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static CreateOrganizationalUnitInput Unit(Guid orgId, string code, string name, string unitType, Guid? parentId) =>
        new(orgId, TenantA, code, name, unitType, parentId, 0, null);

    private static OrgStructureManager<TestUnit, TestLegal, TestJob, TestPosition> CreateManager(MemoryStore store) =>
        new(store, new AllowOrgCatalog());

    private sealed class AllowOrgCatalog : IOrgCatalog
    {
        public Task<OrgDescriptor?> GetAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default) =>
            Task.FromResult<OrgDescriptor?>(null);

        public Task<OrgDescriptor?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default) =>
            Task.FromResult<OrgDescriptor?>(null);

        public Task<IReadOnlyList<OrgDescriptor>> ListAsync(
            Guid tenantId,
            bool? isActive = null,
            string? search = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OrgDescriptor>>([]);

        public Task ValidateForAssignmentAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class TestUnit : IOrganizationalUnitEntity
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? ParentId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public Guid? LegalEntityId { get; set; }
        public string Path { get; set; } = string.Empty;
    }

    private sealed class TestLegal : ILegalEntityEntity
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class TestJob : IJobTitleEntity
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestPosition : IPositionEntity
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public Guid OrganizationalUnitId { get; set; }
        public Guid? JobTitleId { get; set; }
        public Guid? ReportsToPositionId { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class MemoryStore : IOrgStructureStore<TestUnit, TestLegal, TestJob, TestPosition>
    {
        public List<TestUnit> Units { get; } = [];
        public List<TestLegal> Legals { get; } = [];
        public List<TestJob> Jobs { get; } = [];
        public List<TestPosition> Positions { get; } = [];

        public Task<TestUnit?> FindUnitAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Units.FirstOrDefault(unit => unit.Id == id));

        public Task<IReadOnlyList<TestUnit>> ListUnitsAsync(Guid orgId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TestUnit>>(Units.Where(unit => unit.OrgId == orgId).ToArray());

        public Task AddUnitAsync(TestUnit entity, CancellationToken cancellationToken)
        {
            Units.Add(entity);
            return Task.CompletedTask;
        }

        public Task<TestLegal?> FindLegalEntityAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Legals.FirstOrDefault(entity => entity.Id == id));

        public Task<IReadOnlyList<TestLegal>> ListLegalEntitiesAsync(Guid orgId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TestLegal>>(Legals.Where(entity => entity.OrgId == orgId).ToArray());

        public Task AddLegalEntityAsync(TestLegal entity, CancellationToken cancellationToken)
        {
            Legals.Add(entity);
            return Task.CompletedTask;
        }

        public Task<TestJob?> FindJobTitleAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Jobs.FirstOrDefault(entity => entity.Id == id));

        public Task<IReadOnlyList<TestJob>> ListJobTitlesAsync(Guid orgId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TestJob>>(Jobs.Where(entity => entity.OrgId == orgId).ToArray());

        public Task AddJobTitleAsync(TestJob entity, CancellationToken cancellationToken)
        {
            Jobs.Add(entity);
            return Task.CompletedTask;
        }

        public Task<TestPosition?> FindPositionAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Positions.FirstOrDefault(entity => entity.Id == id));

        public Task<IReadOnlyList<TestPosition>> ListPositionsAsync(Guid orgId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TestPosition>>(Positions.Where(entity => entity.OrgId == orgId).ToArray());

        public Task AddPositionAsync(TestPosition entity, CancellationToken cancellationToken)
        {
            Positions.Add(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
