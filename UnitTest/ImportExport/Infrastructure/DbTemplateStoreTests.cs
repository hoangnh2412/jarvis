using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Platform.Modules.ImportExport.EntityFramework.Stores;

namespace UnitTest.ImportExport.Infrastructure;

public sealed class DbTemplateStoreTests : IDisposable
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly string _tempRoot;
    private readonly TestTemplateDbContext _db;
    private readonly TestTemplateUnitOfWork _uow;
    private readonly StubCurrentTenant _currentTenant = new();
    private readonly DbTemplateStore<FakeExcelTemplate, TestTemplateUnitOfWork, TestTenantIdentity> _sut;

    public DbTemplateStoreTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "importexport-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);

        var options = new DbContextOptionsBuilder<TestTemplateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        _db = new TestTemplateDbContext(options);
        SetWorkingTenant(TenantA);
        _uow = new TestTemplateUnitOfWork(_db);

        var resolver = new TemplateFileResolver(Options.Create(new ExcelEngineOptions
        {
            TemplateFilesRoot = _tempRoot
        }));

        _sut = new DbTemplateStore<FakeExcelTemplate, TestTemplateUnitOfWork, TestTenantIdentity>(
            _uow,
            resolver,
            _currentTenant,
            NullLogger<DbTemplateStore<FakeExcelTemplate, TestTemplateUnitOfWork, TestTenantIdentity>>.Instance);
    }

    private void SetWorkingTenant(Guid? tenantId)
    {
        _currentTenant.SetTenantId(tenantId);
        _db.SetTenantId(tenantId);
    }

    [Fact]
    public async Task SaveAsync_then_GetByCodeAsync_returns_definition()
    {
        var content = "PK"u8.ToArray();
        await _sut.SaveAsync(CreateTemplate("ORDERS", content));

        var loaded = await _sut.GetByCodeAsync("ORDERS");

        Assert.NotNull(loaded);
        Assert.Equal("ORDERS", loaded.Code);
        Assert.Equal(TenantA, loaded.TenantId);
        Assert.Equal("Orders", loaded.Name);
        Assert.NotEmpty(loaded.Content ?? []);
        Assert.Single(loaded.Mappings);
        Assert.Contains(TenantA.ToString("D"), loaded.Location, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveAsync_when_soft_deleted_code_exists_throws_TemplateAlreadyExistsException()
    {
        _db.Templates.Add(new FakeExcelTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = TenantA,
            Code = "GONE",
            Name = "Gone",
            FileName = "g.xlsx",
            MappingsJson = "[]",
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<TemplateAlreadyExistsException>(() =>
            _sut.SaveAsync(CreateTemplate("GONE", "PK"u8.ToArray())));
    }

    [Fact]
    public async Task DeleteAsync_soft_deletes()
    {
        await _sut.SaveAsync(CreateTemplate("DEL", "PK"u8.ToArray()));
        await _sut.DeleteAsync("DEL");

        Assert.Null(await _sut.GetByCodeAsync("DEL"));

        var reserved = await _db.Templates.IgnoreQueryFilters()
            .SingleAsync(x => x.Code == "DEL" && x.TenantId == TenantA);
        Assert.False(reserved.IsActive);
    }

    [Fact]
    public async Task Missing_tenant_throws()
    {
        SetWorkingTenant(null);
        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            _sut.SaveAsync(CreateTemplate("X", "PK"u8.ToArray())));
        Assert.Equal(EngineMessages.TenantRequired, ex.Message);
    }

    public void Dispose()
    {
        _uow.Dispose();
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private static TemplateDefinition CreateTemplate(string code, byte[] content) => new()
    {
        Code = code,
        Name = code == "ORDERS" ? "Orders" : code,
        FileName = code.ToLowerInvariant() + ".xlsx",
        Content = content,
        HeaderRow = 1,
        StartRow = 2,
        Mappings =
        [
            new ColumnMapping("A", "A")
        ]
    };
}
