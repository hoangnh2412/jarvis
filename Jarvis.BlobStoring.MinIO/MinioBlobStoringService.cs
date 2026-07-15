// Jarvis.BlobStoring.MinIO — MinIO/S3-compatible IBlobStoringService implementation.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Jarvis.BlobStoring.MinIO;

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

    public virtual async Task DeleteAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Deleting {FileName} from MinIO bucket {Bucket}", fileName, bucket);

        var args = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName);
        await Client.RemoveObjectAsync(args, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task DeletesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var names = fileNames.Where(static n => !string.IsNullOrWhiteSpace(n)).ToList();
        if (names.Count == 0)
            return;

        _logger.LogDebug(
            "Deleting {Count} object(s) from MinIO bucket {Bucket}",
            names.Count,
            bucket);

        var args = new RemoveObjectsArgs()
            .WithBucket(bucket)
            .WithObjects(names);

        var deleteErrors = await Client.RemoveObjectsAsync(args, cancellationToken).ConfigureAwait(false);
        if (deleteErrors.Count == 0)
            return;

        var failed = string.Join(", ", deleteErrors.Select(static e => $"{e.Key}: {e.Message}"));
        _logger.LogError(
            "MinIO batch delete failed for {FailedCount} object(s) in bucket {Bucket}: {FailedObjects}",
            deleteErrors.Count,
            bucket,
            failed);

        throw new InvalidOperationException(
            $"Failed to delete {deleteErrors.Count} object(s) from MinIO bucket '{bucket}': {failed}");
    }

    public virtual async Task<byte[]> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug("Downloading {FileName} from MinIO bucket {Bucket}", fileName, bucket);

        var argsStat = new StatObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName);
        await Client.StatObjectAsync(argsStat, cancellationToken).ConfigureAwait(false);

        using var memoryStream = new MemoryStream();
        var argsGet = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithCallbackStream(stream =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                stream.CopyTo(memoryStream);
            });
        await Client.GetObjectAsync(argsGet, cancellationToken).ConfigureAwait(false);

        var bytes = memoryStream.ToArray();
        _logger.LogDebug(
            "Downloaded {FileName} from MinIO bucket {Bucket}, {ByteCount} bytes",
            fileName,
            bucket,
            bytes.Length);
        return bytes;
    }

    public virtual async Task<string> ViewAsync(
        string bucket,
        string fileName,
        int expireTime = 1800,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug(
            "Creating presigned URL for {FileName} in MinIO bucket {Bucket}, expireSeconds={ExpireSeconds}",
            fileName,
            bucket,
            expireTime);

        var args = new PresignedGetObjectArgs()
            .WithBucket(bucket)
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
        try
        {
            await PutObjectAsync(bucket, fileName, bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (ConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "MinIO connection failed for upload {FileName} to bucket {Bucket}, retrying with bucket creation",
                fileName,
                bucket);
            await ReUploadAsync(bucket, fileName, bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (UnexpectedMinioException ex) when (ex.ServerMessage == "The specified bucket does not exist")
        {
            _logger.LogWarning(
                ex,
                "MinIO bucket {Bucket} does not exist for {FileName}, creating bucket and retrying upload",
                bucket,
                fileName);
            await ReUploadAsync(bucket, fileName, bytes, cancellationToken).ConfigureAwait(false);
        }
        catch (BucketNotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "MinIO bucket {Bucket} not found for {FileName}, creating bucket and retrying upload",
                bucket,
                fileName);
            await ReUploadAsync(bucket, fileName, bytes, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task PutObjectAsync(
        string bucket,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug(
            "Uploading {FileName} to MinIO bucket {Bucket}, {ByteCount} bytes",
            fileName,
            bucket,
            bytes.Length);

        using var stream = new MemoryStream(bytes);
        var args = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithObjectSize(stream.Length)
            .WithStreamData(stream)
            .WithContentType("application/octet-stream");
        await Client.PutObjectAsync(args, cancellationToken).ConfigureAwait(false);
    }

    private async Task ReUploadAsync(
        string bucket,
        string fileName,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Creating MinIO bucket {Bucket}", bucket);

        var args = new MakeBucketArgs().WithBucket(bucket);
        await Client.MakeBucketAsync(args, cancellationToken).ConfigureAwait(false);
        await PutObjectAsync(bucket, fileName, bytes, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<IReadOnlyList<string>> GetFileNamesAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug(
            "Listing objects in MinIO bucket {Bucket}, prefix={Prefix}",
            bucket,
            prefix ?? string.Empty);

        var fileNames = new List<string>();
        var args = new ListObjectsArgs()
            .WithBucket(bucket)
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
            bucket,
            prefix ?? string.Empty);

        return fileNames;
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
