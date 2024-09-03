using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Jarvis.BlobStoring.MinIO;

public class MinioService : IBlobStoringService
{
    private readonly MinIOOption _options;
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioService> _logger;

    public MinioService(
        IOptions<MinIOOption> options,
        // IMinioHttpClient minioHttpClient,
        ILogger<MinioService> logger)
    {
        _options = options.Value;

        var builder = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey);

        if (_options.UseSsl)
            _minioClient = _minioClient.WithSSL();

        _minioClient = builder.Build();

        // _minio = _minio
        //     .WithHttpClient(minioHttpClient.GetClient())
        //     .Build();

        _logger = logger;
    }

    public virtual async Task DeleteAsync(string bucket, string fileName)
    {
        var args = new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName);
        await _minioClient.RemoveObjectAsync(args);
    }

    public virtual async Task DeletesAsync(string bucket, IEnumerable<string> fileNames)
    {
        var args = new RemoveObjectsArgs()
                .WithBucket(bucket)
                .WithObjects(fileNames.ToList());

        var items = await _minioClient.RemoveObjectsAsync(args);
        // IObservable<DeleteError> observable = await _minioClient.RemoveObjectsAsync(args);
        // IDisposable subscription = observable.Subscribe(
        //     deleteError => _logger.LogDebug($"Deleted file: {deleteError.Key} on bucket {bucket}"),
        //     ex => _logger.LogError(ex.Message, ex),
        //     () => _logger.LogDebug($"Done on bucket {bucket}")
        // );
        // observable.Wait();
        // subscription.Dispose();
    }

    public virtual async Task<byte[]> DownloadAsync(string bucket, string fileName)
    {
        var args1 = new StatObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName);
        await _minioClient.StatObjectAsync(args1);

        MemoryStream memoryStream = new MemoryStream();
        var args2 = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithCallbackStream((stream) =>
            {
                stream.CopyTo(memoryStream);
            });
        await _minioClient.GetObjectAsync(args2);
        var bytes = memoryStream.ToArray();
        return bytes;
    }

    public virtual async Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithExpiry(expireTime * 60);
        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public virtual async Task UploadAsync(string bucket, string fileName, byte[] bytes)
    {
        try
        {
            MemoryStream stream = new MemoryStream(bytes);

            // Specify SSE-C encryption options
            // Aes aesEncryption = Aes.Create();
            // aesEncryption.KeySize = 256;
            // aesEncryption.GenerateKey();
            // var ssec = new SSEC(aesEncryption.Key);

            var args = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName)
                .WithObjectSize(stream.Length)
                .WithStreamData(stream)
                .WithContentType("application/actet-stream");
            await _minioClient.PutObjectAsync(args);
        }
        catch (ConnectionException)
        {
            try
            {
                await ReUpload(bucket, fileName, bytes);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        catch (UnexpectedMinioException ex)
        {
            if (ex.ServerMessage != "The specified bucket does not exist")
                throw;

            try
            {
                await ReUpload(bucket, fileName, bytes);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        catch (BucketNotFoundException)
        {
            try
            {
                await ReUpload(bucket, fileName, bytes);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task ReUpload(string bucket, string fileName, byte[] bytes)
    {
        var args = new MakeBucketArgs()
            .WithBucket(bucket);
        await _minioClient.MakeBucketAsync(args);

        // Upload a file to bucket.
        await UploadAsync(bucket, fileName, bytes);
    }

    public virtual IEnumerable<string> GetFileNames(string bucket, string? prefix = null)
    {
        var fileNames = new List<string>();

        var args = new ListObjectsArgs()
                .WithBucket(bucket)
                .WithPrefix(prefix)
                .WithRecursive(true);
        IObservable<Item> observable = (IObservable<Item>)_minioClient.ListObjectsEnumAsync(args);
        IDisposable subscription = observable.Subscribe(
            item => fileNames.Add(item.Key),
            ex => _logger.LogError(ex.Message, ex),
            () => _logger.LogDebug("Done")
        );
        observable.Wait();
        subscription.Dispose();

        return fileNames;
    }
}
