using Jarvis.BlobStoring;
using Microsoft.Extensions.DependencyInjection;

namespace {Product}.Application.Services;

public sealed class DocumentStorageService(IBlobStoringService blobs)
{
    public Task UploadAsync(string bucket, string fileName, byte[] content, CancellationToken cancellationToken = default)
        => blobs.UploadAsync(bucket, fileName, content, cancellationToken);
}

public sealed class MinioDocumentService(
    [FromKeyedServices("MinIO")] IBlobStoringService minio)
{
    public Task<string> GetPresignedUrlAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
        => minio.ViewAsync(bucket, fileName, expireTime: 1800, cancellationToken);
}
