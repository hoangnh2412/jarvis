using Jarvis.BlobStoring;
using Microsoft.Extensions.DependencyInjection;

namespace {Product}.Application.Services;

public sealed class DocumentStorageService(
    [FromKeyedServices("MinIO")] IBlobStoringService minio,
    [FromKeyedServices("FileSystem")] IBlobStoringService local)
{
    public Task UploadToMinioAsync(string tenantId, string fileName, byte[] content)
        => minio.UploadAsync(bucket: "documents", fileName: $"{tenantId}/{fileName}", content);

    public Task<byte[]> DownloadFromLocalAsync(string fileName)
        => local.DownloadAsync(bucket: "archive", fileName);
}
