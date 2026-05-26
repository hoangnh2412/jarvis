# blobstoring-dotnet

Skill tích hợp **Jarvis.BlobStoring** — lưu file qua `IBlobStoringService` (FileSystem, MinIO). Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Chưa có blob storing | [workflows/init.md](./workflows/init.md) |
| Thêm MinIO khi đã có FileSystem (hoặc ngược lại) | [workflows/add.md](./workflows/add.md) + [providers/](./providers/) |

**Không dùng cho:** scaffold toàn solution → [jarvis-dotnet](../jarvis-dotnet/README.md) (có thể thêm package blob sau).

## Cách gọi

```text
@.opencode/skills/blobstoring-dotnet/workflows/init.md

Đăng ký FileSystem + MinIO keyed IBlobStoringService cho MyApp.Infrastructure.
RootPath /data, MinIO localhost:9000.
```

```text
@.opencode/skills/blobstoring-dotnet/providers/minio/SKILL.md

Chỉ thêm MinIO provider vào project đã có FileSystem.
```

## Quy tắc (tóm tắt)

- Key DI: `"FileSystem"`, `"MinIO"`
- Inject: `[FromKeyedServices("MinIO")] IBlobStoringService`
- API: `UploadAsync`, `DownloadAsync`, `DeleteAsync`, `ViewAsync` (presigned — MinIO), `GetFileNames`

## Providers

| Provider | SKILL |
|----------|-------|
| FileSystem | [providers/filesystem/SKILL.md](./providers/filesystem/SKILL.md) |
| MinIO | [providers/minio/SKILL.md](./providers/minio/SKILL.md) |

## Liên quan

- [reference/iblob-api.md](./reference/iblob-api.md) — contract đầy đủ
- [caching-dotnet](../caching-dotnet/README.md) — cache blob/metadata qua `GetOrSetAsync`
- [jarvis-dotnet](../jarvis-dotnet/README.md) — layer Infrastructure
