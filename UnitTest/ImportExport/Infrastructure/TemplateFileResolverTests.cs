using Microsoft.Extensions.Options;
using Platform.Modules.ImportExport.EntityFramework.Stores;

namespace UnitTest.ImportExport.Infrastructure;

public sealed class TemplateFileResolverTests
{
    [Fact]
    public async Task WriteAsync_prefixes_tenant_segment()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var sut = new TemplateFileResolver(Options.Create(new ExcelEngineOptions { TemplateFilesRoot = root }));
            var tenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var relative = await sut.WriteAsync(tenantId, "CODE", "f.xlsx", [1, 2, 3]);
            Assert.StartsWith(tenantId.ToString("D") + "/", relative);
            Assert.True(File.Exists(sut.ResolvePath(relative)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task WriteAsync_rejects_empty_tenant()
    {
        var sut = new TemplateFileResolver(Options.Create(new ExcelEngineOptions { TemplateFilesRoot = Path.GetTempPath() }));
        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            sut.WriteAsync(Guid.Empty, "CODE", "f.xlsx", [1]));
        Assert.Equal(EngineMessages.TenantRequired, ex.Message);
    }
}
