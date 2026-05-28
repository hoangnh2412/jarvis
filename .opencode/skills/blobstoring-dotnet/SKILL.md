---
name: blobstoring-dotnet
description: Thiết lập Jarvis.BlobStoring — upload/download file qua IBlobStoringService với FileSystem (core), MinIO hoặc AWS S3. Dùng khi tích hợp blob storage .NET, section BlobStoring, AddCoreBlobStoring, UseMinIO, UseAwsS3.
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis.BlobStoring — Orchestrator

Skill điều phối `Jarvis.BlobStoring` (core + FileSystem), `Jarvis.BlobStoring.MinIO`, `Jarvis.BlobStoring.AwsS3` trên ASP.NET Core.

Hướng dẫn người dùng: [README.md](README.md). API: [reference/iblob-api.md](reference/iblob-api.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Project chưa có blob storing Jarvis | [workflows/init.md](workflows/init.md) |
| Đã có FileSystem, thêm MinIO / AwsS3 (hoặc ngược lại) | [workflows/add.md](workflows/add.md) |

## Quy tắc cốt lõi

- Gọi `builder.AddCoreBlobStoring()` — bind `BlobStoring`, đăng ký registry + factory `IBlobStoringService` mặc định, tự `UseFileSystem()`.
- Provider bổ sung: `.UseMinIO()` / `.UseAwsS3()` trên `BlobStoringBuilder` (cần package + config hợp lệ).
- Keyed DI: `"FileSystem"`, `"MinIO"`, `"AwsS3"` (`nameof(BlobStoringType.*)`).
- `DefaultProvider` rỗng → provider **đã đăng ký** có `AutoSelectPriority` cao nhất (mặc định: MinIO 30, AwsS3 20, FileSystem 10).
- Config trong appsettings chỉ có hiệu lực auto-select khi đã gọi `Use*` tương ứng.
- Không hardcode `SecretKey` / `AccessKey` — User Secrets / env.
- Override default: `services.AddSingleton<IBlobStoringService, Custom>()` **sau** `AddCoreBlobStoring`.

## Packages

| PackageId | Khi nào |
|---|---|
| `Jarvis.BlobStoring` | Core + FileSystem (built-in) |
| `Jarvis.BlobStoring.MinIO` | S3-compatible (MinIO) |
| `Jarvis.BlobStoring.AwsS3` | Amazon S3 |

## Implementation

| Provider | Service | Config |
|---|---|---|
| FileSystem | `FileSystemBlobStoringService` | `BlobStoring:FileSystem` |
| MinIO | `MinioBlobStoringService` | `BlobStoring:MinIO` (`MinIOBlobStoringOption`) |
| AwsS3 | `AwsS3BlobStoringService` | `BlobStoring:AwsS3` (`AwsS3BlobOptions`) |

Core `JarvisBlobStoringOptions`: chỉ `DefaultProvider`. FileSystem/MinIO/S3 options và `Use*` tương ứng (`FileSystemBlobOptions`, …).

## Providers (atomic)

Chỉ đọc provider cần dùng:

| Provider | Path |
|---|---|
| FileSystem | [providers/filesystem/SKILL.md](providers/filesystem/SKILL.md) |
| MinIO | [providers/minio/SKILL.md](providers/minio/SKILL.md) |
| AwsS3 | [providers/awss3/SKILL.md](providers/awss3/SKILL.md) |

## Templates

- [templates/program-setup.cs](templates/program-setup.cs)
- [templates/appsettings-blob.json](templates/appsettings-blob.json)
- [templates/blob-service-usage.cs](templates/blob-service-usage.cs)

## Output bắt buộc

- `PackageReference` trên Host/Infrastructure
- `Program.cs` hoặc `*LayerExtension.cs`: `AddCoreBlobStoring()` (+ `UseMinIO` / `UseAwsS3` nếu cần)
- `appsettings.json` section `BlobStoring`
- Inject `IBlobStoringService` hoặc `[FromKeyedServices("MinIO")]` resolve được
- `dotnet build` thành công
