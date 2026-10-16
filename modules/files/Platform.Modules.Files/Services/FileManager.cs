using Microsoft.Extensions.Options;
using Platform.BlobStoring;
using Platform.BlobStoring.Configuration;
using Platform.BlobStoring.Hosting;
using Platform.Modules.Files.Models;
using Platform.Modules.Files.Storage;

namespace Platform.Modules.Files.Services;

/// <summary>Default <see cref="IFileManager"/> — delegates blob I/O to <see cref="IBlobStoringService"/>.</summary>
public sealed class FileManager(
    IFileStorageResolver storageResolver,
    IOptions<PlatformBlobStoringOptions> options,
    BlobStoringProviderRegistry registry,
    IEnumerable<IFileTreeProvider> treeProviders) : IFileManager
{
    private readonly IFileStorageResolver _storageResolver = storageResolver;
    private readonly PlatformBlobStoringOptions _options = options.Value;
    private readonly BlobStoringProviderRegistry _registry = registry;
    private readonly IReadOnlyList<IFileTreeProvider> _treeProviders = treeProviders.ToList();

    private IBlobStoringService Blobs => _storageResolver.Resolve();

    public Task CreateBucketAsync(string bucket, string? path = null, CancellationToken cancellationToken = default) =>
        Blobs.CreateBucketAsync(bucket, path, cancellationToken);

    public Task UploadAsync(string bucket, string fileName, byte[] bytes, CancellationToken cancellationToken = default) =>
        Blobs.UploadAsync(bucket, fileName, bytes, cancellationToken);

    public Task UploadStreamAsync(
        string bucket,
        string fileName,
        Stream content,
        long? contentLength = null,
        CancellationToken cancellationToken = default) =>
        Blobs.UploadStreamAsync(bucket, fileName, content, contentLength, cancellationToken);

    public Task<byte[]> DownloadAsync(string bucket, string fileName, CancellationToken cancellationToken = default) =>
        Blobs.DownloadAsync(bucket, fileName, cancellationToken);

    public Task<BlobReadResult> OpenReadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default) =>
        Blobs.OpenReadAsync(bucket, fileName, cancellationToken);

    public Task DeleteFileAsync(string bucket, string fileName, CancellationToken cancellationToken = default) =>
        Blobs.DeleteFileAsync(bucket, fileName, cancellationToken);

    public Task DeleteFilesAsync(string bucket, IEnumerable<string> fileNames, CancellationToken cancellationToken = default) =>
        Blobs.DeleteFilesAsync(bucket, fileNames, cancellationToken);

    public Task DeleteBucketAsync(string bucket, string? prefix = null, CancellationToken cancellationToken = default) =>
        Blobs.DeleteBucketAsync(bucket, prefix, cancellationToken);

    public Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800, CancellationToken cancellationToken = default) =>
        Blobs.ViewAsync(bucket, fileName, expireTime, cancellationToken);

    public Task<IReadOnlyList<string>> ListAsync(string bucket, string? prefix = null, CancellationToken cancellationToken = default) =>
        Blobs.GetFileNamesAsync(bucket, prefix, cancellationToken);

    public Task RenameAsync(string bucket, string path, string newName, CancellationToken cancellationToken = default) =>
        Blobs.RenameAsync(bucket, path, newName, cancellationToken);

    public Task MoveAsync(string bucket, string sourcePath, string destinationFolder, CancellationToken cancellationToken = default) =>
        Blobs.MoveAsync(bucket, sourcePath, destinationFolder, cancellationToken);

    public Task MoveFilesAsync(
        string bucket,
        string destinationFolder,
        IEnumerable<string> sourcePaths,
        CancellationToken cancellationToken = default) =>
        Blobs.MoveFilesAsync(bucket, destinationFolder, sourcePaths, cancellationToken);

    public Task<FileTreeNode> GetTreeAsync(
        string? bucket = null,
        string? rootPath = null,
        CancellationToken cancellationToken = default)
    {
        var providerKey = _registry.ResolveDefaultProviderKey(_options.DefaultProvider);
        var provider = _treeProviders.FirstOrDefault(p =>
            string.Equals(p.ProviderKey, providerKey, StringComparison.Ordinal));

        if (provider is null)
        {
            throw new NotSupportedException(
                $"{FileErrorCode.Message(FileErrorCode.TreeNotSupported)} (provider: '{providerKey}').");
        }

        return provider.GetTreeAsync(bucket, rootPath, cancellationToken);
    }
}
