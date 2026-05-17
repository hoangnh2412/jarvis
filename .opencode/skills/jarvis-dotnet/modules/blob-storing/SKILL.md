---
name: jarvis-dotnet-blob-storing
description: Cài Jarvis.BlobStoring — FileSystem và MinIO object storage. Dùng khi app upload/lưu file qua IBlobStoringService.
dependencies:
  - Jarvis.BlobStoring
  - Jarvis.BlobStoring.FileSystem
  - Jarvis.BlobStoring.MinIO
---

# Blob storing

## Packages

| Project | PackageId |
|---|---|
| Jarvis.BlobStoring | `Jarvis.BlobStoring` |
| Jarvis.BlobStoring.FileSystem | `Jarvis.BlobStoring.FileSystem` |
| Jarvis.BlobStoring.MinIO | `Jarvis.BlobStoring.MinIO` |

`Jarvis.BlobStoring.AwsS3` — stub, chưa implement.

## Implementation

| Provider | Service | Config |
|---|---|---|
| FileSystem | `FileSystemService` | `FileSystemOption` — base path |
| MinIO | `MinioService` | `MinIO:Endpoint`, credentials |

## Đăng ký DI (host)

```csharp
// Đăng ký keyed IBlobStoringService theo BlobStoringType
builder.Services.AddKeyedSingleton<IBlobStoringService, FileSystemService>("FileSystem");
builder.Services.AddKeyedSingleton<IBlobStoringService, MinioService>("MinIO");
```

Bind options từ `IConfiguration` (`IOptions<FileSystemOption>`, `IOptions<MinIOOption>`).

## Interface

```csharp
public interface IBlobStoringService
{
    // upload, download, delete — xem Jarvis.BlobStoring
}
```
