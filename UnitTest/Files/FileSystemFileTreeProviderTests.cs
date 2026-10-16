using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Platform.BlobStoring;
using Platform.BlobStoring.Configuration;
using Platform.BlobStoring.Extensions;
using Platform.BlobStoring.Hosting;
using Platform.Modules.Files;
using Platform.Modules.Files.API.Extensions;
using Platform.Modules.Files.Extensions;
using Platform.Modules.Files.Services;
using Platform.Modules.Files.Storage;

namespace UnitTest.Files;

public class FileSystemFileTreeProviderTests
{
    [Fact]
    public async Task GetTreeAsync_Returns_Immediate_Child_Folders_Including_Empty_Ones()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "platform-fs-tree", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);

        try
        {
            var hostBuilder = Host.CreateApplicationBuilder();
            hostBuilder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.FileSystem))
                .UseFileSystem(fs => fs.RootPath = rootPath);
            hostBuilder.AddCoreFiles().UseFileSystemTree();

            using var host = hostBuilder.Build();
            var manager = host.Services.GetRequiredService<IFileManager>();

            const string bucket = "docs";
            Directory.CreateDirectory(Path.Combine(rootPath, bucket, "EmptyFolder"));
            Directory.CreateDirectory(Path.Combine(rootPath, bucket, "WithFile"));
            await File.WriteAllTextAsync(Path.Combine(rootPath, bucket, "WithFile", "note.txt"), "x");

            var root = await manager.GetTreeAsync(bucket, cancellationToken: CancellationToken.None);
            Assert.Equal(string.Empty, root.Path);
            Assert.Equal(2, root.Children.Count);
            Assert.Contains(root.Children, node => node is { IsFolder: true, Name: "EmptyFolder", Path: "EmptyFolder" });
            Assert.Contains(root.Children, node => node is { IsFolder: true, Name: "WithFile", Path: "WithFile" });
            Assert.All(root.Children.Where(node => node.IsFolder), node => Assert.Empty(node.Children));

            var nested = await manager.GetTreeAsync(bucket, "WithFile", CancellationToken.None);
            Assert.Equal("WithFile", nested.Path);
            Assert.Single(nested.Children);
            Assert.Equal("note.txt", nested.Children[0].Name);
            Assert.False(nested.Children[0].IsFolder);
        }
        finally
        {
            if (Directory.Exists(rootPath))
                Directory.Delete(rootPath, recursive: true);
        }
    }

    [Fact]
    public async Task GetTreeAsync_Throws_When_No_Tree_Provider_Registered()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.FileSystem));
        hostBuilder.AddCoreFiles();

        using var host = hostBuilder.Build();
        var manager = host.Services.GetRequiredService<IFileManager>();

        var ex = await Assert.ThrowsAsync<NotSupportedException>(() =>
            manager.GetTreeAsync(cancellationToken: CancellationToken.None));

        Assert.Contains(FileErrorCode.Message(FileErrorCode.TreeNotSupported), ex.Message, StringComparison.Ordinal);
    }
}

public class FileManagerTests
{
    [Fact]
    public async Task GetTreeAsync_Resolves_FileSystem_Provider_When_Registered()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "platform-fs-tree", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);

        try
        {
            var hostBuilder = Host.CreateApplicationBuilder();
            hostBuilder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.FileSystem))
                .UseFileSystem(fs => fs.RootPath = rootPath);
            hostBuilder.AddCoreFiles().UseFileSystemTree();

            using var host = hostBuilder.Build();
            var manager = host.Services.GetRequiredService<IFileManager>();

            var tree = await manager.GetTreeAsync(cancellationToken: CancellationToken.None);
            Assert.NotNull(tree);
            Assert.True(tree.IsFolder);
        }
        finally
        {
            if (Directory.Exists(rootPath))
                Directory.Delete(rootPath, recursive: true);
        }
    }
}

public class PlatformFilesExtensionsTests
{
    [Fact]
    public void AddCoreFiles_With_AddCoreBlobStoring_Registers_FileManager()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.AddCoreBlobStoring();
        hostBuilder.AddCoreFiles();

        using var host = hostBuilder.Build();

        Assert.IsType<FileManager>(host.Services.GetRequiredService<IFileManager>());
        Assert.NotNull(host.Services.GetService<BlobStoringProviderRegistry>());
        Assert.NotNull(host.Services.GetService<IOptions<PlatformBlobStoringOptions>>());
        Assert.NotNull(host.Services.GetService<IFileStorageResolver>());
    }
}
