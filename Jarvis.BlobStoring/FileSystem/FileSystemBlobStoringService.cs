// Jarvis.BlobStoring — Local disk IBlobStoringService implementation.
using Jarvis.BlobStoring.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.BlobStoring.FileSystem;

public class FileSystemBlobStoringService(
    IOptions<FileSystemOption> options,
    ILogger<FileSystemBlobStoringService> logger)
    : IBlobStoringService
{
    private readonly FileSystemOption _options = options.Value;
    private readonly ILogger<FileSystemBlobStoringService> _logger = logger;

    public virtual Task DeleteAsync(string bucket, string fileName)
    {
        _logger.LogDebug("Deleting {FileName} from file system bucket {Bucket}", fileName, bucket);

        var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, fileName);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public virtual Task DeletesAsync(string bucket, IEnumerable<string> fileNames)
    {
        var names = fileNames.ToList();
        _logger.LogDebug(
            "Deleting {Count} file(s) from file system bucket {Bucket}",
            names.Count,
            bucket);

        foreach (var name in names)
        {
            var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, name);

            if (File.Exists(path))
                File.Delete(path);
        }

        return Task.CompletedTask;
    }

    public virtual async Task<byte[]> DownloadAsync(string bucket, string fileName)
    {
        _logger.LogDebug("Downloading {FileName} from file system bucket {Bucket}", fileName, bucket);

        var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, fileName);
        var bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);

        _logger.LogDebug(
            "Downloaded {FileName} from file system bucket {Bucket}, {ByteCount} bytes",
            fileName,
            bucket,
            bytes.Length);
        return bytes;
    }

    public virtual async Task UploadAsync(string bucket, string fileName, byte[] bytes)
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

        await File.WriteAllBytesAsync(path, bytes).ConfigureAwait(false);
    }

    public virtual Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800)
    {
        _logger.LogDebug(
            "ViewAsync not supported for file system; returning empty URL for {FileName} in bucket {Bucket}",
            fileName,
            bucket);
        return Task.FromResult(string.Empty);
    }

    public virtual IEnumerable<string> GetFileNames(string bucket, string? prefix = null)
    {
        _logger.LogDebug(
            "Listing files in file system bucket {Bucket}, prefix={Prefix}",
            bucket,
            prefix ?? string.Empty);

        var directory = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, string.Empty);
        if (!Directory.Exists(directory))
            return [];

        var searchPattern = string.IsNullOrEmpty(prefix) ? "*" : $"{prefix}*";
        var names = Directory
            .EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(directory, f).Replace('\\', '/'))
            .ToList();

        _logger.LogDebug(
            "Listed {Count} file(s) in file system bucket {Bucket}, prefix={Prefix}",
            names.Count,
            bucket,
            prefix ?? string.Empty);

        return names;
    }
}
