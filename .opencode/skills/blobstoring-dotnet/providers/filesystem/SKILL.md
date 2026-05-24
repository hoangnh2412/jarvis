---
name: blobstoring-dotnet-filesystem
description: Đăng ký Jarvis BlobStoring FileSystem — keyed IBlobStoringService FileSystemService với FileSystemOption RootPath. Dùng khi lưu file trên disk local hoặc volume mount.
dependencies:
  - Jarvis.BlobStoring
  - Jarvis.BlobStoring.FileSystem
---

# FileSystem provider

Path vật lý: `{RootPath}/{SubPath}/{bucket}/{fileName}`.

## appsettings

```json
{
  "FileSystem": {
    "RootPath": "/var/app/storage",
    "SubPath": "uploads",
    "BucketName": "",
    "PartitionBy": 0
  }
}
```

## Registration

```csharp
using Jarvis.BlobStoring;
using Jarvis.BlobStoring.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

builder.Services.Configure<FileSystemOption>(builder.Configuration.GetSection("FileSystem"));
builder.Services.AddKeyedSingleton<IBlobStoringService, FileSystemService>("FileSystem");
```

## Inject

```csharp
public sealed class LocalFileService(
    [FromKeyedServices("FileSystem")] IBlobStoringService storage)
{
    public Task UploadAsync(string path, byte[] data, CancellationToken ct = default)
        => storage.UploadAsync(bucket: "documents", fileName: path, data);
}
```

## Lưu ý

- `ViewAsync` trả chuỗi rỗng — không presigned URL; dùng API riêng nếu cần link tải.
- `GetFileNames` hiện trả rỗng trên implementation mặc định — liệt kê file cần mở rộng hoặc dùng MinIO.
- Đảm bảo process có quyền ghi `RootPath` (container: mount volume).

## Validate

- Upload + download cùng `bucket` + `fileName`
- Path traversal: không cho `fileName` chứa `..` ở tầng application
