// Platform.Modules.Files — Application facade for file operations (orchestrates blob storage).
using Platform.BlobStoring;

namespace Platform.Modules.Files.Services;

/// <summary>
/// Facade for file management. UI and HTTP API call this interface — not <see cref="Platform.BlobStoring.IBlobStoringService"/> directly.
/// </summary>
public interface IFileManager
{
    Task CreateBucketAsync(
        string bucket,
        string? path = null,
        CancellationToken cancellationToken = default);

    Task UploadAsync(
        string bucket,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken = default);

    Task UploadStreamAsync(
        string bucket,
        string fileName,
        Stream content,
        long? contentLength = null,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<BlobReadResult> OpenReadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default);

    Task DeleteFilesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default);

    Task DeleteBucketAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    Task<string> ViewAsync(
        string bucket,
        string fileName,
        int expireTime = 1800,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    Task RenameAsync(
        string bucket,
        string path,
        string newName,
        CancellationToken cancellationToken = default);

    Task MoveAsync(
        string bucket,
        string sourcePath,
        string destinationFolder,
        CancellationToken cancellationToken = default);

    Task MoveFilesAsync(
        string bucket,
        string destinationFolder,
        IEnumerable<string> sourcePaths,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a folder/file tree when an <see cref="Storage.IFileTreeProvider"/> is registered for the active blob provider.
    /// </summary>
    Task<Models.FileTreeNode> GetTreeAsync(
        string? bucket = null,
        string? rootPath = null,
        CancellationToken cancellationToken = default);
}
