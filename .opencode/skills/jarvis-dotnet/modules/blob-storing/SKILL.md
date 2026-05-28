---
name: jarvis-dotnet-blob-storing
description: Cài Jarvis.BlobStoring — FileSystem (built-in), MinIO, AWS S3. Dùng khi app upload/lưu file qua IBlobStoringService.
dependencies:
  - Jarvis.BlobStoring
  - Jarvis.BlobStoring.MinIO
  - Jarvis.BlobStoring.AwsS3
---

# Blob storing

## Packages

| Project | PackageId | Ghi chú |
|---|---|---|
| Jarvis.BlobStoring | `Jarvis.BlobStoring` | Core + **FileSystem** (không cần package riêng) |
| Jarvis.BlobStoring.MinIO | `Jarvis.BlobStoring.MinIO` | Optional |
| Jarvis.BlobStoring.AwsS3 | `Jarvis.BlobStoring.AwsS3` | Optional |

## Implementation

| Provider | Service | Config section |
|---|---|---|
| FileSystem | `FileSystemBlobStoringService` | `BlobStoring:FileSystem` (core) |
| MinIO | `MinioBlobStoringService` | `BlobStoring:MinIO` (`MinIOBlobStoringOption` trong package MinIO) |
| AwsS3 | `AwsS3BlobStoringService` | `BlobStoring:AwsS3` (`AwsS3BlobOptions` trong package AwsS3) |

Core `JarvisBlobStoringOptions` chỉ có `DefaultProvider` + `FileSystem`. MinIO/S3 options và `Use*` nằm package vệ tinh; auto-default theo `BlobStoringProviderRegistry` và `AutoSelectPriority` trong từng section config (mặc định: MinIO 30, AwsS3 20, FileSystem 10).

## Đăng ký DI (host)

```csharp
using Jarvis.BlobStoring.Extensions;
// using Jarvis.BlobStoring.MinIO.Extensions;
// using Jarvis.BlobStoring.AwsS3.Extensions;

// FileSystem — AddBlobStoring đăng ký sẵn UseFileSystem
builder.AddBlobStoring();

// + MinIO
// builder.AddBlobStoring().UseMinIO();

// + AWS S3
// builder.AddBlobStoring().UseAwsS3();
```

`DefaultProvider` override auto-detect (MinIO → AwsS3 → FileSystem).

Inject:

```csharp
public class FileService(IBlobStoringService blobs) { }
```

Override: `services.AddSingleton<IBlobStoringService, Custom>()` sau `AddBlobStoring`.
