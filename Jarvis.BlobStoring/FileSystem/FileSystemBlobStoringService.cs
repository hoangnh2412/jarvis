// Jarvis.BlobStoring — Local disk IBlobStoringService implementation.
using Jarvis.BlobStoring.Configuration;
using Jarvis.BlobStoring.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.BlobStoring.FileSystem;

public class FileSystemBlobStoringService(
    IOptions<FileSystemBlobOptions> options,
    ILogger<FileSystemBlobStoringService> logger)
    : IBlobStoringService
{
    private readonly FileSystemBlobOptions _options = options.Value;
    private readonly ILogger<FileSystemBlobStoringService> _logger = logger;

    public virtual Task DeleteAsync(string bucket, string fileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Deleting {FileName} from file system bucket {Bucket}", fileName, bucket);

        var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, fileName);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public virtual Task DeletesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default)
    {
        var names = fileNames.ToList();
        _logger.LogDebug(
            "Deleting {Count} file(s) from file system bucket {Bucket}",
            names.Count,
            bucket);

        foreach (var name in names)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, name);

            if (File.Exists(path))
                File.Delete(path);
        }

        return Task.CompletedTask;
    }

    public virtual async Task<byte[]> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Downloading {FileName} from file system bucket {Bucket}", fileName, bucket);

        var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, fileName);
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(
            "Downloaded {FileName} from file system bucket {Bucket}, {ByteCount} bytes",
            fileName,
            bucket,
            bytes.Length);
        return bytes;
    }

    public virtual async Task UploadAsync(
        string bucket,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Uploading {FileName} to file system bucket {Bucket}, {ByteCount} bytes",
            fileName,
            bucket,
            bytes.Length);

        var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, fileName);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
    }

    public virtual Task<string> ViewAsync(
        string bucket,
        string fileName,
        int expireTime = 1800,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug(
            "ViewAsync not supported for file system; returning empty URL for {FileName} in bucket {Bucket}",
            fileName,
            bucket);
        return Task.FromResult(string.Empty);
    }

    public virtual Task<IReadOnlyList<string>> GetFileNamesAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug(
            "Listing files in file system bucket {Bucket}, prefix={Prefix}",
            bucket,
            prefix ?? string.Empty);

        var directory = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, string.Empty);
        if (!Directory.Exists(directory))
            return Task.FromResult<IReadOnlyList<string>>([]);

        var searchPattern = string.IsNullOrEmpty(prefix) ? "*" : $"{prefix}*";
        var names = new List<string>();
        foreach (var file in Directory.EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            names.Add(Path.GetRelativePath(directory, file).Replace('\\', '/'));
        }

        _logger.LogDebug(
            "Listed {Count} file(s) in file system bucket {Bucket}, prefix={Prefix}",
            names.Count,
            bucket,
            prefix ?? string.Empty);

        return Task.FromResult<IReadOnlyList<string>>(names);
    }
}
