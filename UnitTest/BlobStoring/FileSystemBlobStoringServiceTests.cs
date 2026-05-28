using Jarvis.BlobStoring.Configuration;
using Jarvis.BlobStoring.FileSystem;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class FileSystemBlobStoringServiceTests : IDisposable
{
    private readonly string _root;
    private readonly FileSystemBlobStoringService _service;

    public FileSystemBlobStoringServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "jarvis-blob-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        _service = new FileSystemBlobStoringService(
            Options.Create(new FileSystemBlobOptions
            {
                RootPath = _root,
                SubPath = string.Empty
            }),
            NullLogger<FileSystemBlobStoringService>.Instance);
    }

    [Fact]
    public async Task UploadAsync_Then_DeleteAsync_Removes_File()
    {
        await _service.UploadAsync("bucket-a", "folder/file.txt", [1, 2, 3]);
        var path = Path.Combine(_root, "bucket-a", "folder", "file.txt");
        Assert.True(File.Exists(path));

        await _service.DeleteAsync("bucket-a", "folder/file.txt");
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task DeletesAsync_Removes_All_Files()
    {
        await _service.UploadAsync("bucket-b", "a.txt", [1]);
        await _service.UploadAsync("bucket-b", "b.txt", [2]);

        await _service.DeletesAsync("bucket-b", ["a.txt", "b.txt"]);

        Assert.False(File.Exists(Path.Combine(_root, "bucket-b", "a.txt")));
        Assert.False(File.Exists(Path.Combine(_root, "bucket-b", "b.txt")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
