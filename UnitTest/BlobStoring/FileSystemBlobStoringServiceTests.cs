using System.Text;
using Platform.BlobStoring.Configuration;
using Platform.BlobStoring.FileSystem;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class FileSystemBlobStoringServiceTests : IDisposable
{
    private readonly string _root;
    private readonly FileSystemBlobStoringService _service;

    public FileSystemBlobStoringServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "platform-blob-test", Guid.NewGuid().ToString("N"));
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
    public async Task CreateBucketAsync_Creates_Empty_Directory()
    {
        var bucketPath = Path.Combine(_root, "empty-bucket");

        await _service.CreateBucketAsync("empty-bucket");

        Assert.True(Directory.Exists(bucketPath));
    }

    [Fact]
    public async Task CreateBucketAsync_With_Path_Creates_Nested_Directory()
    {
        var nestedPath = Path.Combine(_root, "nested-bucket", "2024", "Q1");

        await _service.CreateBucketAsync("nested-bucket", "2024/Q1");

        Assert.True(Directory.Exists(nestedPath));
    }

    [Fact]
    public async Task CreateBucketAsync_With_Path_Is_Idempotent()
    {
        await _service.CreateBucketAsync("idem-bucket", "a/b");
        await _service.CreateBucketAsync("idem-bucket", "a/b");

        Assert.True(Directory.Exists(Path.Combine(_root, "idem-bucket", "a", "b")));
    }

    [Fact]
    public async Task CreateBucketAsync_Throws_On_Path_Traversal_In_Path()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateBucketAsync("bucket-a", "../escape"));
    }

    [Fact]
    public async Task UploadStreamAsync_Then_OpenReadAsync_Returns_Same_Content()
    {
        var payload = Encoding.UTF8.GetBytes("stream-round-trip-" + Guid.NewGuid().ToString("N"));
        await using var uploadStream = new MemoryStream(payload);

        await _service.UploadStreamAsync("bucket-stream", "folder/file.txt", uploadStream, payload.Length);

        await using var readResult = await _service.OpenReadAsync("bucket-stream", "folder/file.txt");
        using var output = new MemoryStream();
        await readResult.Content.CopyToAsync(output);

        Assert.Equal(payload, output.ToArray());
    }

    [Fact]
    public async Task DownloadAsync_ByteArray_Shim_Matches_OpenReadAsync()
    {
        var payload = Encoding.UTF8.GetBytes("byte-shim-" + Guid.NewGuid().ToString("N"));
        await _service.UploadAsync("bucket-shim", "shim.txt", payload);

        var bytes = await _service.DownloadAsync("bucket-shim", "shim.txt");

        await using var readResult = await _service.OpenReadAsync("bucket-shim", "shim.txt");
        using var output = new MemoryStream();
        await readResult.Content.CopyToAsync(output);

        Assert.Equal(payload, bytes);
        Assert.Equal(payload, output.ToArray());
    }

    [Fact]
    public async Task UploadAsync_Then_DeleteFileAsync_Removes_File()
    {
        await _service.UploadAsync("bucket-a", "folder/file.txt", [1, 2, 3]);
        var path = Path.Combine(_root, "bucket-a", "folder", "file.txt");
        Assert.True(File.Exists(path));

        await _service.DeleteFileAsync("bucket-a", "folder/file.txt");
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task DeleteFilesAsync_Removes_All_Files()
    {
        await _service.UploadAsync("bucket-b", "a.txt", [1]);
        await _service.UploadAsync("bucket-b", "b.txt", [2]);

        await _service.DeleteFilesAsync("bucket-b", ["a.txt", "b.txt"]);

        Assert.False(File.Exists(Path.Combine(_root, "bucket-b", "a.txt")));
        Assert.False(File.Exists(Path.Combine(_root, "bucket-b", "b.txt")));
    }

    [Fact]
    public async Task DeleteBucketAsync_Removes_Bucket_And_All_Files()
    {
        await _service.UploadAsync("bucket-d", "a.txt", [1]);
        await _service.UploadAsync("bucket-d", "nested/b.txt", [2]);

        await _service.DeleteBucketAsync("bucket-d");

        Assert.False(Directory.Exists(Path.Combine(_root, "bucket-d")));
    }

    [Fact]
    public async Task DeleteBucketAsync_With_Prefix_Removes_Subtree_Only()
    {
        await _service.UploadAsync("bucket-e", "2024/a.txt", [1]);
        await _service.UploadAsync("bucket-e", "2024/nested/b.txt", [2]);
        await _service.UploadAsync("bucket-e", "other.txt", [3]);

        await _service.DeleteBucketAsync("bucket-e", "2024");

        Assert.False(Directory.Exists(Path.Combine(_root, "bucket-e", "2024")));
        Assert.True(File.Exists(Path.Combine(_root, "bucket-e", "other.txt")));
    }

    [Fact]
    public async Task UploadAsync_Throws_When_Bytes_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.UploadAsync("bucket-a", "file.txt", null!));
    }

    [Fact]
    public async Task DownloadAsync_Throws_When_File_Missing()
    {
        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _service.DownloadAsync("bucket-a", "missing.txt"));

        Assert.Contains("missing.txt", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetFileNamesAsync_Filters_By_Prefix_On_Relative_Path()
    {
        await _service.UploadAsync("bucket-c", "2024/a.pdf", [1]);
        await _service.UploadAsync("bucket-c", "2024/b.pdf", [2]);
        await _service.UploadAsync("bucket-c", "other.txt", [3]);

        var names = await _service.GetFileNamesAsync("bucket-c", "2024/");

        Assert.Equal(2, names.Count);
        Assert.All(names, name => Assert.StartsWith("2024/", name, StringComparison.Ordinal));
    }

    [Fact]
    public async Task RenameAsync_Renames_File_In_Bucket()
    {
        await _service.UploadAsync("bucket-f", "old-name.txt", [1, 2, 3]);

        await _service.RenameAsync("bucket-f", "old-name.txt", "new-name.txt");

        Assert.False(File.Exists(Path.Combine(_root, "bucket-f", "old-name.txt")));
        Assert.True(File.Exists(Path.Combine(_root, "bucket-f", "new-name.txt")));
    }

    [Fact]
    public async Task MoveAsync_Moves_File_To_Destination_Folder_Without_Rename()
    {
        await _service.UploadAsync("bucket-g", "2024/report.pdf", [1, 2, 3]);

        await _service.MoveAsync("bucket-g", "2024/report.pdf", "archive");

        Assert.False(File.Exists(Path.Combine(_root, "bucket-g", "2024", "report.pdf")));
        Assert.True(File.Exists(Path.Combine(_root, "bucket-g", "archive", "report.pdf")));
    }

    [Fact]
    public async Task MoveAsync_Moves_Folder_With_Subtree_Without_Rename()
    {
        await _service.UploadAsync("bucket-h", "2024/Q1/draft.docx", [1]);
        Directory.CreateDirectory(Path.Combine(_root, "bucket-h", "archive"));

        await _service.MoveAsync("bucket-h", "2024/Q1", "archive");

        Assert.False(Directory.Exists(Path.Combine(_root, "bucket-h", "2024", "Q1")));
        Assert.True(File.Exists(Path.Combine(_root, "bucket-h", "archive", "Q1", "draft.docx")));
    }

    [Fact]
    public async Task MoveAsync_Moves_File_To_Bucket_Root()
    {
        await _service.UploadAsync("bucket-i", "2024/report.pdf", [1]);

        await _service.MoveAsync("bucket-i", "2024/report.pdf", string.Empty);

        Assert.False(File.Exists(Path.Combine(_root, "bucket-i", "2024", "report.pdf")));
        Assert.True(File.Exists(Path.Combine(_root, "bucket-i", "report.pdf")));
    }

    [Fact]
    public async Task MoveAsync_Throws_When_Moving_Into_Subfolder()
    {
        Directory.CreateDirectory(Path.Combine(_root, "bucket-j", "2024", "Q1"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.MoveAsync("bucket-j", "2024", "2024/Q1"));
    }

    [Fact]
    public async Task RenameAsync_With_Empty_Path_Renames_Bucket_Folder()
    {
        await _service.CreateBucketAsync("bucket-main");
        await _service.UploadAsync("bucket-main", "test2/readme.txt", [1]);

        await _service.RenameAsync("bucket-main", string.Empty, "bucket-main-new");

        Assert.False(Directory.Exists(Path.Combine(_root, "bucket-main")));
        Assert.True(File.Exists(Path.Combine(_root, "bucket-main-new", "test2", "readme.txt")));
    }

    [Fact]
    public async Task MoveFilesAsync_Moves_Multiple_Items_To_Destination_Folder()
    {
        await _service.UploadAsync("bucket-k", "test3/readme.txt", [1]);
        await _service.UploadAsync("bucket-k", "test4/test5/a.txt", [2]);
        Directory.CreateDirectory(Path.Combine(_root, "bucket-k", "test2"));

        await _service.MoveFilesAsync("bucket-k", "test2", ["test3", "test4/test5/a.txt"]);

        Assert.True(File.Exists(Path.Combine(_root, "bucket-k", "test2", "test3", "readme.txt")));
        Assert.True(File.Exists(Path.Combine(_root, "bucket-k", "test2", "a.txt")));
        Assert.False(Directory.Exists(Path.Combine(_root, "bucket-k", "test3")));
    }

    [Fact]
    public async Task UploadStreamAsync_With_Empty_Bucket_Stores_At_Root()
    {
        var payload = Encoding.UTF8.GetBytes("root-upload-" + Guid.NewGuid().ToString("N"));
        await using var uploadStream = new MemoryStream(payload);

        await _service.UploadStreamAsync(string.Empty, "root-only.txt", uploadStream, payload.Length);

        var expectedPath = Path.Combine(_root, "root-only.txt");
        Assert.True(File.Exists(expectedPath));
        Assert.Equal(payload, await File.ReadAllBytesAsync(expectedPath));
    }

    [Fact]
    public async Task DeleteBucketAsync_Empty_Bucket_Throws_Before_Delete()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteBucketAsync(string.Empty));
    }

    [Fact]
    public async Task RenameAsync_Empty_Bucket_And_Path_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RenameAsync(string.Empty, string.Empty, "new-root"));
    }

    [Fact]
    public async Task RenameAsync_Throws_On_Path_Traversal()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RenameAsync("bucket-a", "../secret.txt", "safe.txt"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}

