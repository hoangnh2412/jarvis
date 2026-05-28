# blobstoring-dotnet

Skill tích hợp **Jarvis.BlobStoring** — lưu file qua `IBlobStoringService` (FileSystem, MinIO, AWS S3). Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Chưa có blob storing | [workflows/init.md](./workflows/init.md) |
| Thêm MinIO / AwsS3 khi đã có FileSystem | [workflows/add.md](./workflows/add.md) + [providers/](./providers/) |

**Không dùng cho:** scaffold toàn solution → [jarvis-dotnet](../jarvis-dotnet/README.md).

## Cách gọi

```text
@.opencode/skills/blobstoring-dotnet/workflows/init.md

Đăng ký FileSystem + MinIO cho MyApp.Host. BlobStoring:FileSystem RootPath trống (wwwroot/blobs), MinIO localhost:9000.
```

```text
@.opencode/skills/blobstoring-dotnet/providers/awss3/SKILL.md

Thêm AwsS3 vào project đã có AddCoreBlobStoring + FileSystem.
```

## Quy tắc (tóm tắt)

- Entry: `AddCoreBlobStoring()` → `UseFileSystem()`; thêm `.UseMinIO()` / `.UseAwsS3()` khi cần
- Key DI: `"FileSystem"`, `"MinIO"`, `"AwsS3"`
- Inject mặc định: `IBlobStoringService`; keyed: `[FromKeyedServices("MinIO")]`
- API: `UploadAsync`, `DownloadAsync`, `DeleteAsync`, `ViewAsync`, `GetFileNames`

## Providers

| Provider | SKILL |
|----------|-------|
| FileSystem | [providers/filesystem/SKILL.md](./providers/filesystem/SKILL.md) |
| MinIO | [providers/minio/SKILL.md](./providers/minio/SKILL.md) |
| AwsS3 | [providers/awss3/SKILL.md](./providers/awss3/SKILL.md) |

## Liên quan

- [reference/iblob-api.md](./reference/iblob-api.md)
- [caching-dotnet](../caching-dotnet/README.md)
- [jarvis-dotnet](../jarvis-dotnet/README.md)
