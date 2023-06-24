using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Infrastructure.File.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;

namespace Infrastructure.File.Minio
{
    public class MinioService : IFileService
    {
        private readonly ObjectStorageOption _options;
        private readonly MinioClient _minio;
        private readonly ILogger<MinioService> _logger;

        public MinioService(
            IOptions<ObjectStorageOption> options,
            IMinioHttpClient minioHttpClient,
            ILogger<MinioService> logger)
        {
            _options = options.Value;

            _minio = new MinioClient()
                .WithEndpoint(_options.Endpoint)
                .WithCredentials(_options.AccessKey, _options.SecretKey);

            if (_options.UseSsl)
                _minio = _minio.WithSSL();

            _minio = _minio
                .WithHttpClient(minioHttpClient.GetClient())
                .Build();

            _logger = logger;
        }

        public async Task DeleteAsync(string bucket, string fileName)
        {
            var args = new RemoveObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileName);
            await _minio.RemoveObjectAsync(args);
        }

        public async Task DeletesAsync(string bucket, List<string> fileNames)
        {
            var args = new RemoveObjectsArgs()
                    .WithBucket(bucket)
                    .WithObjects(fileNames);
            IObservable<DeleteError> observable = await _minio.RemoveObjectsAsync(args);
            IDisposable subscription = observable.Subscribe(
                deleteError => _logger.LogDebug($"Đã xoá file: {deleteError.Key} trên bucket {bucket}"),
                ex => _logger.LogError(ex.Message, ex),
                () => _logger.LogDebug($"Thực hiện xong thao tác xoá trên bucket {bucket}")
            );
            observable.Wait();
            subscription.Dispose();
        }

        public async Task<byte[]> DownloadAsync(string bucket, string fileName)
        {
            var args1 = new StatObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName);
            await _minio.StatObjectAsync(args1);

            MemoryStream memoryStream = new MemoryStream();
            var args2 = new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName)
                .WithCallbackStream((stream) =>
                {
                    stream.CopyTo(memoryStream);
                });
            await _minio.GetObjectAsync(args2);
            var bytes = memoryStream.ToArray();
            return bytes;
        }

        public async Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800)
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName)
                .WithExpiry(expireTime * 60);
            return await _minio.PresignedGetObjectAsync(args);
        }

        public async Task UploadAsync(string bucket, string fileName, byte[] bytes)
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
                await _minio.PutObjectAsync(args);
            }
            // catch (InvalidBucketNameException)
            // {
            //     // _logger.LogError(ex, ex.Message);
            //     await _minio.MakeBucketAsync(name);

            //     // Upload a file to bucket.
            //     await UploadAsync(fileName, bytes, bucketName);
            // }
            catch (ConnectionException)
            {
                try
                {
                    await ReUpload(bucket, fileName, bytes);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            catch (UnexpectedMinioException ex)
            {
                if (ex.ServerMessage != "The specified bucket does not exist")
                    throw ex;

                try
                {
                    await ReUpload(bucket, fileName, bytes);
                }
                catch (System.Exception ex2)
                {
                    throw ex2;
                }
            }
            catch (BucketNotFoundException)
            {
                try
                {
                    await ReUpload(bucket, fileName, bytes);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task ReUpload(string bucket, string fileName, byte[] bytes)
        {
            var args = new MakeBucketArgs()
                .WithBucket(bucket);
            await _minio.MakeBucketAsync(args);

            // Upload a file to bucket.
            await UploadAsync(bucket, fileName, bytes);
        }

        public List<string> GetFileNames(string bucket, string prefix = null)
        {
            var fileNames = new List<string>();

            var args = new ListObjectsArgs()
                    .WithBucket(bucket)
                    .WithPrefix(prefix)
                    .WithRecursive(true);
            IObservable<Item> observable = _minio.ListObjectsAsync(args);
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
}