---
name: blobstoring-dotnet-minio
description: Đăng ký Jarvis BlobStoring MinIO — keyed IBlobStoringService MinioService với MinIOOption Endpoint và credentials. Dùng khi lưu file S3-compatible object storage.
dependencies:
  - Jarvis.BlobStoring
  - Jarvis.BlobStoring.MinIO
---

# MinIO provider

S3-compatible; bucket = tên bucket MinIO, `fileName` = object key.

## appsettings

```json
{
  "MinIO": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "Region": "",
    "UseSsl": false,
    "BucketName": ""
  }
}
```

Production: secret qua env / vault — không commit file.

## Registration

```csharp
using Jarvis.BlobStoring;
using Jarvis.BlobStoring.MinIO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

builder.Services.Configure<MinIOOption>(builder.Configuration.GetSection("MinIO"));
builder.Services.AddKeyedSingleton<IBlobStoringService, MinioService>("MinIO");
```

## Inject

```csharp
public sealed class DocumentService(
    [FromKeyedServices("MinIO")] IBlobStoringService blobStorage)
{
    public async Task<string?> GetPresignedViewUrlAsync(string objectKey, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await blobStorage.ViewAsync(bucket: "documents", fileName: objectKey, expireTime: 1800);
    }
}
```

## Lưu ý

- Bucket phải tồn tại (hoặc tạo qua MinIO console / policy auto-create).
- `ViewAsync` — presigned URL (expire seconds).
- Kết hợp [caching-dotnet](../../../caching-dotnet/README.md): cache byte[] hoặc URL ngắn TTL, không cache file lớn bừa bãi.

## Validate

- `UploadAsync` → `DownloadAsync` bytes khớp
- `DeleteAsync` → download lỗi / 404 tương đương
- Endpoint reachable từ pod/container network
