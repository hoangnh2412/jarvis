using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Platform.Modules.ImportExport.EntityFramework.Stores;

namespace UnitTest.ImportExport.Infrastructure;

public sealed class TemplateTenantIsolationTests : IDisposable
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly string _tempRoot;
    private readonly TestTemplateDbContext _db;
    private readonly TestTemplateUnitOfWork _uow;
    private readonly StubCurrentTenant _currentTenant = new();
    private readonly DbTemplateStore<FakeExcelTemplate, TestTemplateUnitOfWork, TestTenantIdentity> _sut;

    public TemplateTenantIsolationTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "importexport-tenant-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);

        var options = new DbContextOptionsBuilder<TestTemplateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        _db = new TestTemplateDbContext(options);
        _uow = new TestTemplateUnitOfWork(_db);
        _sut = new DbTemplateStore<FakeExcelTemplate, TestTemplateUnitOfWork, TestTenantIdentity>(
            _uow,
            new TemplateFileResolver(Options.Create(new ExcelEngineOptions { TemplateFilesRoot = _tempRoot })),
            _currentTenant,
            NullLogger<DbTemplateStore<FakeExcelTemplate, TestTemplateUnitOfWork, TestTenantIdentity>>.Instance);
    }

    private void SetWorkingTenant(Guid tenantId)
    {
        _currentTenant.SetTenantId(tenantId);
        _db.SetTenantId(tenantId);
    }

    [Fact]
    public async Task TenantA_cannot_get_TenantB_template()
    {
        SetWorkingTenant(TenantA);
        await _sut.SaveAsync(CreateTemplate("CODE", "PK"u8.ToArray()));

        SetWorkingTenant(TenantB);
        Assert.Null(await _sut.GetByCodeAsync("CODE"));
    }

    [Fact]
    public async Task SoftDeleted_code_reserved_only_same_tenant()
    {
        SetWorkingTenant(TenantA);
        await _sut.SaveAsync(CreateTemplate("CODE", "PK"u8.ToArray()));
        await _sut.DeleteAsync("CODE");

        await Assert.ThrowsAsync<TemplateAlreadyExistsException>(() =>
            _sut.SaveAsync(CreateTemplate("CODE", "PK"u8.ToArray())));

        SetWorkingTenant(TenantB);
        await _sut.SaveAsync(CreateTemplate("CODE", "PK"u8.ToArray()));
        var loaded = await _sut.GetByCodeAsync("CODE");
        Assert.NotNull(loaded);
        Assert.Equal(TenantB, loaded.TenantId);
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
        Name = code,
        FileName = code.ToLowerInvariant() + ".xlsx",
        Content = content,
        HeaderRow = 1,
        StartRow = 2,
        Mappings = [new ColumnMapping("A", "A")]
    };
}
