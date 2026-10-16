namespace Platform.BlobStoring;

public interface IBlobStoringService
{
    /// <summary>
    /// Upload file
    /// </summary>
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
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
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
    /// <param name="fileName">Path file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<byte[]> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload file from a readable stream without buffering the entire payload in the caller.
    /// </summary>
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
    /// <param name="fileName">Path to save file</param>
    /// <param name="content">Readable stream</param>
    /// <param name="contentLength">Optional size in bytes when known (e.g. <c>IFormFile.Length</c>)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UploadStreamAsync(
        string bucket,
        string fileName,
        Stream content,
        long? contentLength = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a readable stream for the blob. Caller must dispose the returned <see cref="BlobReadResult"/>.
    /// </summary>
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
    /// <param name="fileName">Path file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<BlobReadResult> OpenReadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an empty bucket (folder/container). Idempotent when the folder already exists.
    /// When <paramref name="path"/> is set, creates a nested folder under <paramref name="bucket"/>.
    /// </summary>
    /// <param name="bucket">Top-level folder (bucket).</param>
    /// <param name="path">Optional nested folder path relative to <paramref name="bucket"/> (e.g. <c>2024/Q1</c>).</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateBucketAsync(
        string bucket,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(CreateBucketAsync)} is not supported by this provider.");
    }

    /// <summary>
    /// Delete single file
    /// </summary>
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
    /// <param name="fileName">Path file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete multiple files
    /// </summary>
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
    /// <param name="fileNames">Paths to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFilesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bucket and all files under it. When <paramref name="prefix"/> is set, only that subtree is removed.
    /// Missing bucket is treated as a no-op.
    /// </summary>
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
    /// <param name="prefix">Optional subtree prefix within the bucket</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteBucketAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(DeleteBucketAsync)} is not supported by this provider.");
    }

    /// <summary>
    /// Return the link to view file without download
    /// </summary>
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
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
    /// <param name="bucket">
    /// File storage directory. Empty or whitespace = provider storage root
    /// (FileSystem/Synology/SharePoint root) or configured default bucket (MinIO/AwsS3).
    /// </param>
    /// <param name="prefix">Prefix to search file, include path of file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<string>> GetFileNamesAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a file or folder. <paramref name="path"/> is relative to <paramref name="bucket"/>.
    /// When <paramref name="path"/> is empty, renames the top-level <paramref name="bucket"/> folder itself.
    /// <paramref name="newName"/> is the new leaf name (not a full path).
    /// </summary>
    Task RenameAsync(
        string bucket,
        string path,
        string newName,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(RenameAsync)} is not supported by this provider.");
    }

    /// <summary>
    /// Moves a file or folder into <paramref name="destinationFolder"/> without renaming.
    /// <paramref name="destinationFolder"/> is relative to <paramref name="bucket"/>; empty = bucket root.
    /// </summary>
    Task MoveAsync(
        string bucket,
        string sourcePath,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(MoveAsync)} is not supported by this provider.");
    }

    /// <summary>
    /// Moves multiple files or folders into the same <paramref name="destinationFolder"/> without renaming.
    /// </summary>
    Task MoveFilesAsync(
        string bucket,
        string destinationFolder,
        IEnumerable<string> sourcePaths,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"{nameof(MoveFilesAsync)} is not supported by this provider.");
    }
}
