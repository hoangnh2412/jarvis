// Platform.BlobStoring — Local disk IBlobStoringService implementation.
using Platform.BlobStoring.Configuration;
using Platform.BlobStoring.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Platform.BlobStoring.FileSystem;

public class FileSystemBlobStoringService(
    IOptions<FileSystemBlobOptions> options,
    ILogger<FileSystemBlobStoringService> logger)
    : IBlobStoringService
{
    private readonly FileSystemBlobOptions _options = options.Value;
    private readonly ILogger<FileSystemBlobStoringService> _logger = logger;

    public virtual Task CreateBucketAsync(
        string bucket,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var relativePath = string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.Trim().Trim('/');

        _logger.LogDebug(
            "Creating file system bucket {Bucket}, path={Path}",
            bucket,
            string.IsNullOrEmpty(relativePath) ? "(root)" : relativePath);

        var directory = string.IsNullOrEmpty(relativePath)
            ? BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, string.Empty)
            : BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, relativePath);

        Directory.CreateDirectory(directory);
        return Task.CompletedTask;
    }

    public virtual Task DeleteFileAsync(string bucket, string fileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Deleting {FileName} from file system bucket {Bucket}", fileName, bucket);

        var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, fileName);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public virtual Task DeleteFilesAsync(
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

    public virtual Task DeleteBucketAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(bucket) && string.IsNullOrEmpty(prefix))
            throw new InvalidOperationException("Cannot delete the configured file system storage root.");

        _logger.LogDebug(
            "Deleting file system bucket {Bucket}, prefix={Prefix}",
            bucket,
            prefix ?? string.Empty);

        var targetDirectory = string.IsNullOrEmpty(prefix)
            ? BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, string.Empty)
            : BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, prefix.TrimEnd('/'));

        if (Directory.Exists(targetDirectory))
            Directory.Delete(targetDirectory, recursive: true);

        return Task.CompletedTask;
    }

    public virtual async Task<byte[]> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await using var result = await OpenReadAsync(bucket, fileName, cancellationToken).ConfigureAwait(false);
        return await BlobStoringStreamHelper.ReadAllBytesAsync(result.Content, cancellationToken).ConfigureAwait(false);
    }

    public virtual Task<BlobReadResult> OpenReadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Opening read stream for {FileName} in file system bucket {Bucket}", fileName, bucket);

        var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Blob file '{fileName}' was not found in bucket '{bucket}'.", path);

        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        var fileInfo = new FileInfo(path);
        return Task.FromResult(BlobReadResult.FromStream(
              stream,
              contentType: "application/octet-stream",
              contentLength: fileInfo.Length));
    }

    public virtual async Task UploadAsync(
        string bucket,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        await using var stream = new MemoryStream(bytes);
        await UploadStreamAsync(bucket, fileName, stream, bytes.Length, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task UploadStreamAsync(
        string bucket,
        string fileName,
        Stream content,
        long? contentLength = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        _logger.LogDebug(
            "Uploading stream for {FileName} to file system bucket {Bucket}, contentLength={ContentLength}",
            fileName,
            bucket,
            contentLength?.ToString() ?? "(unknown)");

        var path = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, fileName);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);
        await content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
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

        var names = new List<string>();
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = Path.GetRelativePath(directory, file).Replace('\\', '/');
            if (string.IsNullOrEmpty(prefix) || relativePath.StartsWith(prefix, StringComparison.Ordinal))
                names.Add(relativePath);
        }

        _logger.LogDebug(
            "Listed {Count} file(s) in file system bucket {Bucket}, prefix={Prefix}",
            names.Count,
            bucket,
            prefix ?? string.Empty);

        return Task.FromResult<IReadOnlyList<string>>(names);
    }

    public virtual Task RenameAsync(
        string bucket,
        string path,
        string newName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        if (string.IsNullOrEmpty(path))
        {
            if (string.IsNullOrEmpty(bucket))
                throw new InvalidOperationException("Cannot rename the configured file system storage root.");

            _logger.LogDebug(
                "Renaming file system bucket {Bucket} to {NewName}",
                bucket,
                newName);

            var sourceDir = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, string.Empty);
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Bucket '{bucket}' was not found.");

            var baseDir = string.IsNullOrWhiteSpace(_options.SubPath)
                ? Path.GetFullPath(_options.RootPath)
                : Path.GetFullPath(Path.Combine(_options.RootPath, _options.SubPath));

            var destinationDir = Path.GetFullPath(Path.Combine(baseDir, newName));
            if (!destinationDir.StartsWith(baseDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(destinationDir, baseDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Resolved bucket path is outside the configured root.");
            }

            Directory.Move(sourceDir, destinationDir);
            return Task.CompletedTask;
        }

        _logger.LogDebug(
            "Renaming {Path} to {NewName} in file system bucket {Bucket}",
            path,
            newName,
            bucket);

        var source = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, path);
        var parent = Path.GetDirectoryName(source)
            ?? throw new InvalidOperationException($"Cannot resolve parent directory for '{path}'.");

        var destination = Path.Combine(parent, newName);
        MoveEntry(source, destination);
        return Task.CompletedTask;
    }

    public virtual Task MoveAsync(
        string bucket,
        string sourcePath,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ObjectStoragePathHelper.IsDestinationInsideSource(sourcePath, destinationFolder))
        {
            throw new InvalidOperationException(
                "Cannot move an item into itself or one of its subfolders.");
        }

        var destinationPath = ObjectStoragePathHelper.BuildMoveDestinationPath(sourcePath, destinationFolder);

        _logger.LogDebug(
            "Moving {SourcePath} to folder {DestinationFolder} ({DestinationPath}) in file system bucket {Bucket}",
            sourcePath,
            string.IsNullOrEmpty(destinationFolder) ? "(root)" : destinationFolder,
            destinationPath,
            bucket);

        var source = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, sourcePath);
        var destination = BlobPathHelper.Combine(_options.RootPath, _options.SubPath, bucket, destinationPath);
        MoveEntry(source, destination);
        return Task.CompletedTask;
    }

    public virtual async Task MoveFilesAsync(
        string bucket,
        string destinationFolder,
        IEnumerable<string> sourcePaths,
        CancellationToken cancellationToken = default)
    {
        foreach (var sourcePath in ObjectStoragePathHelper.OrderForBatchMove(sourcePaths))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await MoveAsync(bucket, sourcePath, destinationFolder, cancellationToken).ConfigureAwait(false);
        }
    }

    private static void MoveEntry(string source, string destination)
    {
        if (File.Exists(source))
        {
            var destinationDirectory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            File.Move(source, destination, overwrite: true);
            return;
        }

        if (Directory.Exists(source))
        {
            var destinationDirectory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            Directory.Move(source, destination);
            return;
        }

        throw new FileNotFoundException($"Path was not found.", source);
    }
}
