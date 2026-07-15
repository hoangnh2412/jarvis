using Jarvis.BlobStoring;
using Jarvis.BlobStoring.Configuration;
using Jarvis.BlobStoring.Extensions;
using Jarvis.BlobStoring.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class BlobStoringHostBuilderExtensionsTests
{
    [Fact]
    public void UseFileSystem_Configure_Applies_RootPath_To_Service()
    {
        var root = Path.Combine(Path.GetTempPath(), "jarvis-blob-di", Guid.NewGuid().ToString("N"));
        try
        {
            var hostBuilder = Host.CreateApplicationBuilder();
            hostBuilder.AddCoreBlobStoring()
                .UseFileSystem(fs =>
                {
                    fs.RootPath = root;
                    fs.SubPath = "tenant-a";
                });

            using var host = hostBuilder.Build();
            var fileSystemOptions = host.Services.GetRequiredService<IOptions<FileSystemBlobOptions>>().Value;

            Assert.Equal(root, fileSystemOptions.RootPath);
            Assert.Equal("tenant-a", fileSystemOptions.SubPath);
            Assert.IsType<FileSystemBlobStoringService>(host.Services.GetRequiredService<IBlobStoringService>());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
