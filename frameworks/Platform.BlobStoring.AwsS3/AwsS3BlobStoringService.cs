// Platform.BlobStoring.AwsS3 — Amazon S3 IBlobStoringService implementation.
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.BlobStoring.Helpers;

namespace Platform.BlobStoring.AwsS3;

public class AwsS3BlobStoringService : IBlobStoringService, IDisposable
{
    private const int MaxDeleteObjectsPerRequest = 1000;

    private readonly AwsS3BlobOptions _options;
    private readonly Lazy<IAmazonS3> _client;
    private readonly ILogger<AwsS3BlobStoringService> _logger;
    private readonly string _defaultBucket;
    private bool _disposed;

    public AwsS3BlobStoringService(
        IOptions<AwsS3BlobOptions> options,
        ILogger<AwsS3BlobStoringService> logger)
    {
        _options = options.Value;
        _defaultBucket = _options.BucketName;
        _logger = logger;
        _client = new Lazy<IAmazonS3>(CreateClient);
    }

    private IAmazonS3 Client => _client.Value;

    private IAmazonS3 CreateClient()
    {
        _logger.LogDebug(
            "Creating Amazon S3 client for region {Region}, explicitCredentials={HasCredentials}",
            _options.Region,
            !string.IsNullOrWhiteSpace(_options.AccessKey));

        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Region)
        };

        return string.IsNullOrWhiteSpace(_options.AccessKey)
            ? new AmazonS3Client(config)
            : new AmazonS3Client(_options.AccessKey, _options.SecretKey, config);
    }

    private string ResolveBucket(string bucket)
    {
        var resolved = string.IsNullOrWhiteSpace(bucket) ? _defaultBucket : bucket;
        if (string.IsNullOrWhiteSpace(resolved))
        {
            throw new InvalidOperationException(
                "S3 bucket is required. Set BlobStoring:AwsS3:BucketName or pass bucket in the route.");
        }

        return resolved;
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

        var resolvedBucket = ResolveBucket(bucket);
        var size = BlobStoringStreamHelper.ResolveUploadSize(content, contentLength);
        _logger.LogDebug(
            "Uploading stream for {FileName} to S3 bucket {Bucket}, contentLength={ContentLength}",
            fileName,
            resolvedBucket,
            size?.ToString() ?? "(unknown)");

        var request = new PutObjectRequest
        {
            BucketName = resolvedBucket,
            Key = fileName,
            InputStream = content,
            AutoCloseStream = false
        };

        if (size is > 0)
            request.Headers.ContentLength = size.Value;

        await Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
    }

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
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug("Opening read stream for {FileName} from S3 bucket {Bucket}", fileName, resolvedBucket);

        var request = new GetObjectRequest
        {
            BucketName = resolvedBucket,
            Key = fileName
        };

        var response = await Client.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);
        return BlobReadResult.FromStream(
            response.ResponseStream,
            contentType: response.Headers.ContentType,
            contentLength: response.ContentLength,
            disposeAsync: () =>
            {
                response.Dispose();
                return ValueTask.CompletedTask;
            });
    }

    public virtual async Task DeleteAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug("Deleting {FileName} from S3 bucket {Bucket}", fileName, resolvedBucket);

        var request = new DeleteObjectRequest
        {
            BucketName = resolvedBucket,
            Key = fileName
        };
        await Client.DeleteObjectAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task DeletesAsync(
        string bucket,
        IEnumerable<string> fileNames,
        CancellationToken cancellationToken = default)
    {
        var names = fileNames.Where(static n => !string.IsNullOrWhiteSpace(n)).ToList();
        if (names.Count == 0)
            return;

        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug(
            "Deleting {Count} object(s) from S3 bucket {Bucket}",
            names.Count,
            resolvedBucket);

        for (var offset = 0; offset < names.Count; offset += MaxDeleteObjectsPerRequest)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = names.Skip(offset).Take(MaxDeleteObjectsPerRequest).ToList();
            var request = new DeleteObjectsRequest
            {
                BucketName = resolvedBucket,
                Objects = chunk.ConvertAll(key => new KeyVersion { Key = key })
            };

            var response = await Client.DeleteObjectsAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.DeleteErrors is not { Count: > 0 } errors)
                continue;

            var failed = string.Join(", ", errors.Select(static e => $"{e.Key}: {e.Message}"));
            _logger.LogError(
                "S3 batch delete failed for {FailedCount} object(s) in bucket {Bucket}: {FailedObjects}",
                errors.Count,
                resolvedBucket,
                failed);

            throw new InvalidOperationException(
                $"Failed to delete {errors.Count} object(s) from S3 bucket '{resolvedBucket}': {failed}");
        }
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

    public virtual Task<string> ViewAsync(
        string bucket,
        string fileName,
        int expireTime = 1800,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug(
            "Creating presigned URL for {FileName} in S3 bucket {Bucket}, expireSeconds={ExpireSeconds}",
            fileName,
            resolvedBucket,
            expireTime);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = resolvedBucket,
            Key = fileName,
            Expires = DateTime.UtcNow.AddSeconds(expireTime)
        };
        return Task.FromResult(Client.GetPreSignedURL(request));
    }

    public virtual async Task<IReadOnlyList<string>> GetFileNamesAsync(
        string bucket,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug(
            "Listing objects in S3 bucket {Bucket}, prefix={Prefix}",
            resolvedBucket,
            prefix ?? string.Empty);

        var names = new List<string>();
        var request = new ListObjectsV2Request
        {
            BucketName = resolvedBucket,
            Prefix = prefix
        };

        ListObjectsV2Response response;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            response = await Client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);
            names.AddRange(response.S3Objects.Select(o => o.Key));
            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated);

        _logger.LogDebug(
            "Listed {Count} object(s) in S3 bucket {Bucket}, prefix={Prefix}",
            names.Count,
            resolvedBucket,
            prefix ?? string.Empty);

        return names;
    }

    public virtual async Task RenameAsync(
        string bucket,
        string path,
        string newName,
        CancellationToken cancellationToken = default)
    {
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

        await CopyPrefixAsync(bucket, normalized, destination, deleteSource: true, cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual Task MoveAsync(
        string bucket,
        string sourcePath,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        if (ObjectStoragePathHelper.IsDestinationInsideSource(sourcePath, destinationFolder))
        {
            throw new InvalidOperationException(
                "Cannot move an item into itself or one of its subfolders.");
        }

        var destinationPath = ObjectStoragePathHelper.BuildMoveDestinationPath(sourcePath, destinationFolder);
        return CopyPrefixAsync(bucket, sourcePath, destinationPath, deleteSource: true, cancellationToken);
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

    private async Task CopyPrefixAsync(
        string bucket,
        string sourcePath,
        string destinationPath,
        bool deleteSource,
        CancellationToken cancellationToken)
    {
        var source = ObjectStoragePathHelper.NormalizeKey(sourcePath);
        var destination = ObjectStoragePathHelper.NormalizeKey(destinationPath);

        if (string.Equals(source, destination, StringComparison.Ordinal))
            return;

        var resolvedBucket = ResolveBucket(bucket);
        var allKeys = await GetFileNamesAsync(bucket, cancellationToken: cancellationToken).ConfigureAwait(false);
        var affected = ObjectStoragePathHelper.CollectKeysUnderPrefix(allKeys, source).ToList();

        if (affected.Count == 0 && allKeys.Any(k => k.Equals(source, StringComparison.Ordinal)))
            affected.Add(source);

        if (affected.Count == 0)
            throw new InvalidOperationException(
                $"Object or prefix '{sourcePath}' was not found in bucket '{resolvedBucket}'.");

        foreach (var key in affected)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetKey = ObjectStoragePathHelper.ReplacePrefix(key, source, destination);

            var request = new CopyObjectRequest
            {
                SourceBucket = resolvedBucket,
                SourceKey = key,
                DestinationBucket = resolvedBucket,
                DestinationKey = targetKey
            };

            await Client.CopyObjectAsync(request, cancellationToken).ConfigureAwait(false);
        }

        if (deleteSource)
            await DeletesAsync(bucket, affected, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_client.IsValueCreated)
        {
            _logger.LogDebug("Disposing Amazon S3 client for region {Region}", _options.Region);
            _client.Value.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
