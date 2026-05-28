namespace Jarvis.BlobStoring;

public interface IBlobStoringService
{
    /// <summary>
    /// Upload file
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path to save file</param>
    /// <param name="bytes">Data of file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UploadAsync(
        string bucket,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Download file to bytes
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<byte[]> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete single file
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete multiple files
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileNames">Paths to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeletesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Return the link to view file without download
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="fileName">Path file</param>
    /// <param name="expireTime">Presigned URL lifetime in seconds (default 1800 = 30 minutes).</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<string> ViewAsync(
        string bucket,
        string fileName,
        int expireTime = 1800,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Return a list of paths that matching the prefix
    /// </summary>
    /// <param name="bucket">File storage directory</param>
    /// <param name="prefix">Prefix to search file, include path of file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<string>> GetFileNamesAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default);
}
