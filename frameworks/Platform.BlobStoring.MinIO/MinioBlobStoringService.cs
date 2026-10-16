// Platform.BlobStoring.MinIO — MinIO/S3-compatible IBlobStoringService implementation.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Platform.BlobStoring.Helpers;

namespace Platform.BlobStoring.MinIO;

public class MinioBlobStoringService : IBlobStoringService, IDisposable
{
    private readonly MinIOBlobStoringOption _options;
    private readonly Lazy<IMinioClient> _minioClient;
    private readonly ILogger<MinioBlobStoringService> _logger;
    private bool _disposed;

    public MinioBlobStoringService(
        IOptions<MinIOBlobStoringOption> options,
        ILogger<MinioBlobStoringService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _minioClient = new Lazy<IMinioClient>(CreateClient);
    }

    private IMinioClient Client => _minioClient.Value;

    private IMinioClient CreateClient()
    {
        _logger.LogDebug(
            "Creating MinIO client for {Endpoint}, UseSsl={UseSsl}",
            _options.Endpoint,
            _options.UseSsl);

        var builder = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey);

        if (_options.UseSsl)
            builder = builder.WithSSL();

        return builder.Build();
    }

    private string ResolveBucket(string bucket)
    {
        var resolved = string.IsNullOrWhiteSpace(bucket) ? _options.BucketName : bucket;
        if (string.IsNullOrWhiteSpace(resolved))
        {
            throw new InvalidOperationException(
                "MinIO bucket is required. Set BlobStoring:MinIO:BucketName or pass bucket in the route.");
        }

        return resolved;
    }

    public virtual async Task DeleteAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug("Deleting {FileName} from MinIO bucket {Bucket}", fileName, resolvedBucket);

        var args = new RemoveObjectArgs()
            .WithBucket(resolvedBucket)
            .WithObject(fileName);
        await Client.RemoveObjectAsync(args, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task DeletesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resolvedBucket = ResolveBucket(bucket);
        var names = fileNames.Where(static n => !string.IsNullOrWhiteSpace(n)).ToList();
        if (names.Count == 0)
            return;

        _logger.LogDebug(
            "Deleting {Count} object(s) from MinIO bucket {Bucket}",
            names.Count,
            resolvedBucket);

        var args = new RemoveObjectsArgs()
            .WithBucket(resolvedBucket)
            .WithObjects(names);

        var deleteErrors = await Client.RemoveObjectsAsync(args, cancellationToken).ConfigureAwait(false);
        if (deleteErrors.Count == 0)
            return;

        var failed = string.Join(", ", deleteErrors.Select(static e => $"{e.Key}: {e.Message}"));
        _logger.LogError(
            "MinIO batch delete failed for {FailedCount} object(s) in bucket {Bucket}: {FailedObjects}",
            deleteErrors.Count,
            resolvedBucket,
            failed);

        throw new InvalidOperationException(
            $"Failed to delete {deleteErrors.Count} object(s) from MinIO bucket '{resolvedBucket}': {failed}");
    }

    Task IBlobStoringService.DeleteFileAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken)
        => DeleteAsync(bucket, fileName, cancellationToken);

    Task IBlobStoringService.DeleteFilesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken)
        => DeletesAsync(bucket, fileNames, cancellationToken);

    public virtual async Task<byte[]> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await using var result = await OpenReadAsync(bucket, fileName, cancellationToken).ConfigureAwait(false);
        return await BlobStoringStreamHelper.ReadAllBytesAsync(result.Content, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<BlobReadResult> OpenReadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug("Opening read stream for {FileName} from MinIO bucket {Bucket}", fileName, resolvedBucket);

        var argsStat = new StatObjectArgs()
            .WithBucket(resolvedBucket)
            .WithObject(fileName);
        var stat = await Client.StatObjectAsync(argsStat, cancellationToken).ConfigureAwait(false);

        var tempPath = Path.Combine(Path.GetTempPath(), "platform-minio-" + Guid.NewGuid().ToString("N"));
        var getArgs = new GetObjectArgs()
            .WithBucket(resolvedBucket)
            .WithObject(fileName)
            .WithFile(tempPath);
        await Client.GetObjectAsync(getArgs, cancellationToken).ConfigureAwait(false);

        var stream = new FileStream(
            tempPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose);

        return BlobReadResult.FromStream(
            stream,
            contentType: stat.ContentType,
            contentLength: stat.Size);
    }

    public virtual async Task<string> ViewAsync(
        string bucket,
        string fileName,
        int expireTime = 1800,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug(
            "Creating presigned URL for {FileName} in MinIO bucket {Bucket}, expireSeconds={ExpireSeconds}",
            fileName,
            resolvedBucket,
            expireTime);

        var args = new PresignedGetObjectArgs()
            .WithBucket(resolvedBucket)
            .WithObject(fileName)
            .WithExpiry(expireTime);
        return await Client.PresignedGetObjectAsync(args).ConfigureAwait(false);
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
        var resolvedBucket = ResolveBucket(bucket);
        try
        {
            await PutObjectAsync(resolvedBucket, fileName, content, contentLength, cancellationToken).ConfigureAwait(false);
        }
        catch (ConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "MinIO connection failed for upload {FileName} to bucket {Bucket}, retrying with bucket creation",
                fileName,
                resolvedBucket);
            await ReUploadStreamAsync(resolvedBucket, fileName, content, contentLength, cancellationToken).ConfigureAwait(false);
        }
        catch (UnexpectedMinioException ex) when (ex.ServerMessage == "The specified bucket does not exist")
        {
            _logger.LogWarning(
                ex,
                "MinIO bucket {Bucket} does not exist for {FileName}, creating bucket and retrying upload",
                resolvedBucket,
                fileName);
            await ReUploadStreamAsync(resolvedBucket, fileName, content, contentLength, cancellationToken).ConfigureAwait(false);
        }
        catch (BucketNotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "MinIO bucket {Bucket} not found for {FileName}, creating bucket and retrying upload",
                resolvedBucket,
                fileName);
            await ReUploadStreamAsync(resolvedBucket, fileName, content, contentLength, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task PutObjectAsync(
        string bucket,
        string fileName,
        Stream content,
        long? contentLength,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        cancellationToken.ThrowIfCancellationRequested();

        var size = BlobStoringStreamHelper.ResolveUploadSize(content, contentLength);
        _logger.LogDebug(
            "Uploading stream for {FileName} to MinIO bucket {Bucket}, contentLength={ContentLength}",
            fileName,
            bucket,
            size?.ToString() ?? "(unknown)");

        var args = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithStreamData(content)
            .WithContentType("application/octet-stream");

        if (size is > 0)
            args = args.WithObjectSize(size.Value);

        await Client.PutObjectAsync(args, cancellationToken).ConfigureAwait(false);
    }

    private async Task ReUploadStreamAsync(
        string bucket,
        string fileName,
        Stream content,
        long? contentLength,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Creating MinIO bucket {Bucket}", bucket);

        var args = new MakeBucketArgs().WithBucket(bucket);
        await Client.MakeBucketAsync(args, cancellationToken).ConfigureAwait(false);
        if (content.CanSeek)
            content.Position = 0;

        await PutObjectAsync(bucket, fileName, content, contentLength, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<IReadOnlyList<string>> GetFileNamesAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug(
            "Listing objects in MinIO bucket {Bucket}, prefix={Prefix}",
            resolvedBucket,
            prefix ?? string.Empty);

        var fileNames = new List<string>();
        var args = new ListObjectsArgs()
            .WithBucket(resolvedBucket)
            .WithPrefix(prefix)
            .WithRecursive(true);

        var observable = Client.ListObjectsEnumAsync(args, cancellationToken);
        await foreach (var item in observable.ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            fileNames.Add(item.Key);
        }

        _logger.LogDebug(
            "Listed {Count} object(s) in MinIO bucket {Bucket}, prefix={Prefix}",
            fileNames.Count,
            resolvedBucket,
            prefix ?? string.Empty);

        return fileNames;
    }

    public virtual async Task RenameAsync(
        string bucket,
        string path,
        string newName,
        CancellationToken cancellationToken = default)
    {
        var resolvedBucket = ResolveBucket(bucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        if (string.IsNullOrEmpty(ObjectStoragePathHelper.NormalizeKey(path)))
        {
            throw new NotSupportedException(
                "Renaming the object storage bucket/container itself is not supported by this provider.");
        }

        var normalized = ObjectStoragePathHelper.NormalizeKey(path);
        var parent = ObjectStoragePathHelper.GetParentPrefix(normalized);
        var destination = ObjectStoragePathHelper.CombineKey(
            string.IsNullOrEmpty(parent) ? null : parent,
            newName);

        await MovePrefixAsync(resolvedBucket, normalized, destination, cancellationToken).ConfigureAwait(false);
    }

    public virtual Task MoveAsync(
        string bucket,
        string sourcePath,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        var resolvedBucket = ResolveBucket(bucket);
        if (ObjectStoragePathHelper.IsDestinationInsideSource(sourcePath, destinationFolder))
        {
            throw new InvalidOperationException(
                "Cannot move an item into itself or one of its subfolders.");
        }

        var destinationPath = ObjectStoragePathHelper.BuildMoveDestinationPath(sourcePath, destinationFolder);
        return MovePrefixAsync(resolvedBucket, sourcePath, destinationPath, cancellationToken);
    }

    public virtual async Task MoveFilesAsync(
        string bucket,
        string destinationFolder,
        IEnumerable<string> sourcePaths,
        CancellationToken cancellationToken = default)
    {
        var resolvedBucket = ResolveBucket(bucket);
        foreach (var sourcePath in ObjectStoragePathHelper.OrderForBatchMove(sourcePaths))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await MoveAsync(resolvedBucket, sourcePath, destinationFolder, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task MovePrefixAsync(
        string bucket,
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        var source = ObjectStoragePathHelper.NormalizeKey(sourcePath);
        var destination = ObjectStoragePathHelper.NormalizeKey(destinationPath);

        if (string.Equals(source, destination, StringComparison.Ordinal))
            return;

        var allKeys = await GetFileNamesAsync(bucket, cancellationToken: cancellationToken).ConfigureAwait(false);
        var affected = ObjectStoragePathHelper.CollectKeysUnderPrefix(allKeys, source).ToList();

        if (affected.Count == 0 && allKeys.Any(k => k.Equals(source, StringComparison.Ordinal)))
            affected.Add(source);

        if (affected.Count == 0)
            throw new InvalidOperationException($"Object or prefix '{sourcePath}' was not found in bucket '{bucket}'.");

        foreach (var key in affected)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetKey = ObjectStoragePathHelper.ReplacePrefix(key, source, destination);

            var copyArgs = new CopyObjectArgs()
                .WithBucket(bucket)
                .WithObject(targetKey)
                .WithCopyObjectSource(new CopySourceObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(key));

            await Client.CopyObjectAsync(copyArgs, cancellationToken).ConfigureAwait(false);
        }

        await DeletesAsync(bucket, affected, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_minioClient.IsValueCreated && _minioClient.Value is IDisposable disposable)
        {
            _logger.LogDebug("Disposing MinIO client for {Endpoint}", _options.Endpoint);
            disposable.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
