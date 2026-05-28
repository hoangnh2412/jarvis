using Jarvis.BlobStoring;
using Microsoft.Extensions.DependencyInjection;

namespace {Product}.Application.Services;

public sealed class DocumentStorageService(IBlobStoringService blobs)
{
    public Task UploadAsync(string bucket, string fileName, byte[] content)
        => blobs.UploadAsync(bucket, fileName, content);
}

public sealed class MinioDocumentService(
    [FromKeyedServices("MinIO")] IBlobStoringService minio)
{
    public Task<string> GetPresignedUrlAsync(string bucket, string fileName)
        => minio.ViewAsync(bucket, fileName, expireTime: 1800);
}
