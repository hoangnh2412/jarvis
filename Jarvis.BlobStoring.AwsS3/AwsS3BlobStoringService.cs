// Jarvis.BlobStoring.AwsS3 — Amazon S3 IBlobStoringService implementation.
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.BlobStoring.AwsS3;

public class AwsS3BlobStoringService : IBlobStoringService, IDisposable
{
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

    private string ResolveBucket(string bucket) =>
        string.IsNullOrWhiteSpace(bucket) ? _defaultBucket : bucket;

    public virtual async Task UploadAsync(string bucket, string fileName, byte[] bytes)
    {
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug(
            "Uploading {FileName} to S3 bucket {Bucket}, {ByteCount} bytes",
            fileName,
            resolvedBucket,
            bytes.Length);

        using var stream = new MemoryStream(bytes);
        var request = new PutObjectRequest
        {
            BucketName = resolvedBucket,
            Key = fileName,
            InputStream = stream
        };
        await Client.PutObjectAsync(request).ConfigureAwait(false);
    }

    public virtual async Task<byte[]> DownloadAsync(string bucket, string fileName)
    {
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug("Downloading {FileName} from S3 bucket {Bucket}", fileName, resolvedBucket);

        var request = new GetObjectRequest
        {
            BucketName = resolvedBucket,
            Key = fileName
        };
        using var response = await Client.GetObjectAsync(request).ConfigureAwait(false);
        using var memory = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memory).ConfigureAwait(false);

        var result = memory.ToArray();
        _logger.LogDebug(
            "Downloaded {FileName} from S3 bucket {Bucket}, {ByteCount} bytes",
            fileName,
            resolvedBucket,
            result.Length);
        return result;
    }

    public virtual async Task DeleteAsync(string bucket, string fileName)
    {
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug("Deleting {FileName} from S3 bucket {Bucket}", fileName, resolvedBucket);

        var request = new DeleteObjectRequest
        {
            BucketName = resolvedBucket,
            Key = fileName
        };
        await Client.DeleteObjectAsync(request).ConfigureAwait(false);
    }

    public virtual async Task DeletesAsync(string bucket, IEnumerable<string> fileNames)
    {
        var names = fileNames.ToList();
        var resolvedBucket = ResolveBucket(bucket);
        _logger.LogDebug(
            "Deleting {Count} object(s) from S3 bucket {Bucket}",
            names.Count,
            resolvedBucket);

        foreach (var fileName in names)
            await DeleteAsync(bucket, fileName).ConfigureAwait(false);
    }

    public virtual Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800)
    {
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

    public virtual IEnumerable<string> GetFileNames(string bucket, string? prefix = null) =>
        GetFileNamesAsync(bucket, prefix).GetAwaiter().GetResult();

    public virtual async Task<IReadOnlyList<string>> GetFileNamesAsync(string bucket, string? prefix = null)
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
            response = await Client.ListObjectsV2Async(request).ConfigureAwait(false);
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
