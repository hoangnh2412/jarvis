---
name: blobstoring-dotnet-minio
description: Đăng ký Jarvis BlobStoring MinIO — keyed MinioBlobStoringService với BlobStoring:MinIO Endpoint và credentials. Dùng khi lưu file S3-compatible object storage.
dependencies:
  - Jarvis.BlobStoring
  - Jarvis.BlobStoring.MinIO
---

# MinIO provider

S3-compatible; `bucket` = tên bucket MinIO, `fileName` = object key.  
Client tạo lazy (`Lazy<IMinioClient>`), `IDisposable`, có `ILogger`.

## appsettings

```json
{
  "BlobStoring": {
    "DefaultProvider": "MinIO",
    "MinIO": {
      "Endpoint": "localhost:9000",
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin",
      "UseSsl": false,
      "AutoSelectPriority": 30
    }
  }
}
```

Production: secret qua env / vault.

## Registration

```csharp
using Jarvis.BlobStoring.Extensions;
using Jarvis.BlobStoring.MinIO.Extensions;

builder.AddCoreBlobStoring()
    .UseMinIO();

// hoặc override trong code:
builder.AddCoreBlobStoring()
    .UseMinIO(m =>
    {
        m.Endpoint = "localhost:9000";
        m.AccessKey = "minioadmin";
        m.SecretKey = "minioadmin";
        m.UseSsl = false;
    });
```

## Inject

```csharp
public sealed class DocumentService(
    IBlobStoringService blobs,
    [FromKeyedServices("MinIO")] IBlobStoringService minio)
{
    public Task<string> GetPresignedUrlAsync(string key)
        => minio.ViewAsync("documents", key, expireTime: 1800);
}
```

## Lưu ý

- `UseMinIO()` yêu cầu `Endpoint` khác rỗng.
- Upload có retry tạo bucket khi bucket chưa tồn tại.
- `ViewAsync` — presigned URL.

## Validate

- `UploadAsync` → `DownloadAsync` bytes khớp
- Endpoint reachable từ container/pod
