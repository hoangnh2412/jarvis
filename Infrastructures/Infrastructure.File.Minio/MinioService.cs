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
        private readonly MinioOptions _minioOptions;
        private readonly MinioClient _minio;
        private readonly ILogger<MinioService> _logger;

        public MinioService(
            IOptions<MinioOptions> minioOptions,
            MinioClient minio,
            ILogger<MinioService> logger)
        {
            _minioOptions = minioOptions.Value;
            _minio = minio;
            _logger = logger;
        }

        public async Task DeleteAsync(string fileName)
        {
            await _minio.RemoveObjectAsync(_minioOptions.BucketName, fileName);
        }

        public async Task DeletesAsync(List<string> fileNames)
        {
            IObservable<DeleteError> observable = await _minio.RemoveObjectAsync(_minioOptions.BucketName, fileNames);
            IDisposable subscription = observable.Subscribe(
                deleteError => _logger.LogDebug($"Đã xoá file: {deleteError.Key} trên bucket {_minioOptions.BucketName}"),
                ex => _logger.LogError(ex.Message, ex),
                () => _logger.LogDebug($"Thực hiện xong thao tác xoá trên bucket {_minioOptions.BucketName}")
            );
            observable.Wait();
            subscription.Dispose();
        }

        public async Task<byte[]> DownloadAsync(string fileName)
        {
            await _minio.StatObjectAsync(_minioOptions.BucketName, fileName);

            MemoryStream memoryStream = new MemoryStream();
            await _minio.GetObjectAsync(_minioOptions.BucketName, fileName, (stream) =>
            {
                stream.CopyTo(memoryStream);
            });
            var bytes = memoryStream.ToArray();
            return bytes;
        }

        public async Task<string> ViewAsync(string fileName, int expireTime)
        {
            return await _minio.PresignedGetObjectAsync(_minioOptions.BucketName, fileName, expireTime);
        }

        public async Task UploadAsync(string fileName, byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);

            // Specify SSE-C encryption options
            // Aes aesEncryption = Aes.Create();
            // aesEncryption.KeySize = 256;
            // aesEncryption.GenerateKey();
            // var ssec = new SSEC(aesEncryption.Key);

            await _minio.PutObjectAsync(
                bucketName: _minioOptions.BucketName,
                objectName: fileName,
                data: stream,
                size: stream.Length,
                contentType: "application/octet-stream",
                metaData: null,
                sse: null);
        }

        public List<string> GetFileNames(string prefix = null)
        {
            var fileNames = new List<string>();
            IObservable<Item> observable = _minio.ListObjectsAsync(_minioOptions.BucketName, prefix, true);
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