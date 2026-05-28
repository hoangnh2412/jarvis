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

    public virtual async Task DeleteAsync(string bucket, string fileName)
    {
        _logger.LogDebug("Deleting {FileName} from MinIO bucket {Bucket}", fileName, bucket);

        var args = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName);
        await Client.RemoveObjectAsync(args).ConfigureAwait(false);
    }

    public virtual async Task DeletesAsync(string bucket, IEnumerable<string> fileNames)
    {
        var names = fileNames.ToList();
        _logger.LogDebug(
            "Deleting {Count} object(s) from MinIO bucket {Bucket}",
            names.Count,
            bucket);

        var args = new RemoveObjectsArgs()
            .WithBucket(bucket)
            .WithObjects(names);
        await Client.RemoveObjectsAsync(args).ConfigureAwait(false);
    }

    public virtual async Task<byte[]> DownloadAsync(string bucket, string fileName)
    {
        _logger.LogDebug("Downloading {FileName} from MinIO bucket {Bucket}", fileName, bucket);

        var argsStat = new StatObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName);
        await Client.StatObjectAsync(argsStat).ConfigureAwait(false);

        using var memoryStream = new MemoryStream();
        var argsGet = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));
        await Client.GetObjectAsync(argsGet).ConfigureAwait(false);

        var bytes = memoryStream.ToArray();
        _logger.LogDebug(
            "Downloaded {FileName} from MinIO bucket {Bucket}, {ByteCount} bytes",
            fileName,
            bucket,
            bytes.Length);
        return bytes;
    }

    public virtual async Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800)
    {
        _logger.LogDebug(
            "Creating presigned URL for {FileName} in MinIO bucket {Bucket}, expireSeconds={ExpireSeconds}",
            fileName,
            bucket,
            expireTime);

        var args = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithExpiry(expireTime * 60);
        return await Client.PresignedGetObjectAsync(args).ConfigureAwait(false);
    }

    public virtual async Task UploadAsync(string bucket, string fileName, byte[] bytes)
    {
        try
        {
            await PutObjectAsync(bucket, fileName, bytes).ConfigureAwait(false);
        }
        catch (ConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "MinIO connection failed for upload {FileName} to bucket {Bucket}, retrying with bucket creation",
                fileName,
                bucket);
            await ReUploadAsync(bucket, fileName, bytes).ConfigureAwait(false);
        }
        catch (UnexpectedMinioException ex) when (ex.ServerMessage == "The specified bucket does not exist")
        {
            _logger.LogWarning(
                ex,
                "MinIO bucket {Bucket} does not exist for {FileName}, creating bucket and retrying upload",
                bucket,
                fileName);
            await ReUploadAsync(bucket, fileName, bytes).ConfigureAwait(false);
        }
        catch (BucketNotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "MinIO bucket {Bucket} not found for {FileName}, creating bucket and retrying upload",
                bucket,
                fileName);
            await ReUploadAsync(bucket, fileName, bytes).ConfigureAwait(false);
        }
    }

    private async Task PutObjectAsync(string bucket, string fileName, byte[] bytes)
    {
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
        await Client.PutObjectAsync(args).ConfigureAwait(false);
    }

    private async Task ReUploadAsync(string bucket, string fileName, byte[] bytes)
    {
        _logger.LogInformation("Creating MinIO bucket {Bucket}", bucket);

        var args = new MakeBucketArgs().WithBucket(bucket);
        await Client.MakeBucketAsync(args).ConfigureAwait(false);
        await PutObjectAsync(bucket, fileName, bytes).ConfigureAwait(false);
    }

    public virtual IEnumerable<string> GetFileNames(string bucket, string? prefix = null) =>
        GetFileNamesAsync(bucket, prefix).GetAwaiter().GetResult();

    public virtual async Task<IReadOnlyList<string>> GetFileNamesAsync(string bucket, string? prefix = null)
    {
        _logger.LogDebug(
            "Listing objects in MinIO bucket {Bucket}, prefix={Prefix}",
            bucket,
            prefix ?? string.Empty);

        var fileNames = new List<string>();
        var args = new ListObjectsArgs()
            .WithBucket(bucket)
            .WithPrefix(prefix)
            .WithRecursive(true);

        var observable = Client.ListObjectsEnumAsync(args);
        await foreach (var item in observable.ConfigureAwait(false))
            fileNames.Add(item.Key);

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
