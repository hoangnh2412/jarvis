using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Platform.Modules.ImportExport.EntityFramework.Stores;

namespace UnitTest.ImportExport.Infrastructure;

public sealed class FolderTemplateStoreTests : IDisposable
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly string _tempRoot;
    private readonly StubCurrentTenant _currentTenant = new();
    private readonly FolderTemplateStore<TestTenantIdentity> _sut;

    public FolderTemplateStoreTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "importexport-folder-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
        _sut = new FolderTemplateStore<TestTenantIdentity>(
            Options.Create(new ExcelEngineOptions { TemplateFilesRoot = _tempRoot }),
            new TemplateFileResolver(Options.Create(new ExcelEngineOptions { TemplateFilesRoot = _tempRoot })),
            _currentTenant,
            NullLogger<FolderTemplateStore<TestTenantIdentity>>.Instance);
    }

    [Fact]
    public async Task Save_and_get_under_tenant_folder()
    {
        using (_currentTenant.Change(TenantA))
        {
            await _sut.SaveAsync(CreateTemplate("T1", "PK"u8.ToArray()));
            var loaded = await _sut.GetByCodeAsync("T1");
            Assert.NotNull(loaded);
            Assert.Equal(TenantA, loaded.TenantId);
            Assert.True(File.Exists(Path.Combine(_tempRoot, TenantA.ToString("D"), "T1.json")));
        }
    }

    [Fact]
    public async Task TenantB_does_not_see_TenantA_meta()
    {
        using (_currentTenant.Change(TenantA))
            await _sut.SaveAsync(CreateTemplate("T1", "PK"u8.ToArray()));

        using (_currentTenant.Change(TenantB))
            Assert.Null(await _sut.GetByCodeAsync("T1"));
    }

    [Fact]
    public async Task Missing_tenant_throws()
    {
        _currentTenant.SetTenantId(null);
        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            _sut.SaveAsync(CreateTemplate("T1", "PK"u8.ToArray())));
        Assert.Equal(EngineMessages.TenantRequired, ex.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private static TemplateDefinition CreateTemplate(string code, byte[] content) => new()
    {
        Code = code,
        Name = code,
        FileName = code + ".xlsx",
        Content = content,
        HeaderRow = 1,
        StartRow = 2,
        Mappings = [new ColumnMapping("A", "A")]
    };
}
