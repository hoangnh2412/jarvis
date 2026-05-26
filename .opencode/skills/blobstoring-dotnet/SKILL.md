---
name: blobstoring-dotnet
description: Thiết lập Jarvis.BlobStoring — upload/download file qua IBlobStoringService với FileSystem hoặc MinIO keyed DI. Dùng khi tích hợp blob storage .NET, section FileSystem/MinIO, hoặc inject IBlobStoringService.
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis.BlobStoring — Orchestrator

Skill điều phối `Jarvis.BlobStoring`, `Jarvis.BlobStoring.FileSystem`, `Jarvis.BlobStoring.MinIO` trên ASP.NET Core.

Hướng dẫn người dùng: [README.md](README.md). API interface: [reference/iblob-api.md](reference/iblob-api.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Project chưa có blob storing Jarvis | [workflows/init.md](workflows/init.md) |
| Đã có một provider, thêm provider thứ hai | [workflows/add.md](workflows/add.md) |

## Quy tắc cốt lõi

- Đăng ký **keyed** `IBlobStoringService` — key `"FileSystem"` / `"MinIO"` (khớp `[FromKeyedServices("...")]`).
- `Configure<FileSystemOption>` / `Configure<MinIOOption>` từ section `FileSystem`, `MinIO` trong appsettings.
- **Singleton** cho service (stateless client; `MinioService` tạo client trong ctor).
- Layer thường gặp: **Infrastructure** hoặc **Host** (composition root).
- Không commit `SecretKey` — dùng User Secrets / env.
- `Jarvis.BlobStoring.AwsS3` — **stub**, chưa dùng trong skill này.

## Packages

| PackageId | Version* | Khi nào |
|---|---|---|
| `Jarvis.BlobStoring` | 1.0.0 | Abstractions |
| `Jarvis.BlobStoring.FileSystem` | 1.0.0 | Lưu file local |
| `Jarvis.BlobStoring.MinIO` | 1.0.0 | S3-compatible object storage |

\*Xem csproj repo Jarvis.

## Providers (atomic)

| Provider | Path |
|---|---|
| FileSystem | [providers/filesystem/SKILL.md](providers/filesystem/SKILL.md) |
| MinIO | [providers/minio/SKILL.md](providers/minio/SKILL.md) |

## Templates

- [templates/program-setup.cs](templates/program-setup.cs) — extension đăng ký DI
- [templates/appsettings-blob.json](templates/appsettings-blob.json)
- [templates/blob-service-usage.cs](templates/blob-service-usage.cs) — inject keyed service

## Output bắt buộc

- PackageReference trên Host/Infrastructure
- `*BlobStoringExtension.cs` hoặc block trong `HostLayerExtension`
- `appsettings` sections `FileSystem` / `MinIO`
- Service/host inject `[FromKeyedServices("MinIO")]` hoặc `"FileSystem"` resolve được
- `dotnet build` thành công
