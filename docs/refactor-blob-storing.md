# Refactor Jarvis.BlobStoring — Code Review & Kế hoạch

Review branch **`refactor-blob-storing`** so với **`develop`** theo [code-review-dotnet skill](../.opencode/skills/code-review-dotnet/SKILL.md) và [refactoring-rules.md](./refactoring-rules.md).

**Phạm vi review:** toàn bộ source hiện tại (không chỉ diff PR).

**Branch hiện tại:** `refactor-blob-storing` (base `develop`)

| Commit | Ghi chú |
|--------|---------|
| `3f65b2a7` | Refactor register BlobStoring |
| `346ac266` | Merge `develop` |
| `28c406c4` | rename refactoring rules |
| `96bea985` | CT trên API, async rename, xóa `FileSystemOption`, registry |

**Thay đổi chính trên branch:** `AddCoreBlobStoring` + `BlobStoringBuilder` + `BlobStoringProviderRegistry`; gộp FileSystem vào `Jarvis.BlobStoring`; thêm `Jarvis.BlobStoring.AwsS3`; refactor `MinioBlobStoringService`; skills `.opencode/skills/blobstoring-dotnet/`.

**Skill tham chiếu (cùng pattern [caching redis-distributed](../.opencode/skills/caching-dotnet/providers/redis-distributed/SKILL.md)):**

| Provider | Skill |
|----------|-------|
| Orchestrator | [blobstoring-dotnet/SKILL.md](../.opencode/skills/blobstoring-dotnet/SKILL.md) |
| FileSystem | [providers/filesystem/SKILL.md](../.opencode/skills/blobstoring-dotnet/providers/filesystem/SKILL.md) |
| MinIO | [providers/minio/SKILL.md](../.opencode/skills/blobstoring-dotnet/providers/minio/SKILL.md) |
| AwsS3 | [providers/awss3/SKILL.md](../.opencode/skills/blobstoring-dotnet/providers/awss3/SKILL.md) |

---

## Mục tiêu nghiệp vụ

| # | Yêu cầu | Trạng thái |
|---|---------|------------|
| 1 | MinIO, S3, FileSystem abstract qua `Jarvis.BlobStoring` | **Đạt** — `IBlobStoringService`, `AddCoreBlobStoring`, keyed providers |
| 2 | Mặc định không khai báo S3/MinIO → FileSystem | **Đạt** — chỉ `UseFileSystem()` đăng ký → auto-select FileSystem; config MinIO/S3 không có hiệu lực nếu không gọi `Use*` |
| 3 | Mở rộng không sửa core Jarvis | **Đạt** — `BlobStoringBuilder` + `Use*` trong package vệ tinh; `TryAddKeyedSingleton` |

---

## Critical Issues

None — `BlobStoringProviderRegistry.ResolveDefaultProviderKey` đã validate `DefaultProvider` explicit; ném `InvalidOperationException` kèm danh sách provider đã đăng ký và gợi ý `Use*`.

---

## Suggestions

### `Jarvis.BlobStoring/IBlobStoringService.cs`

**Issue:** API vẫn `byte[]` toàn bộ; chưa stream-based upload/download.

**Impact:** File lớn → memory pressure.

**Suggested fix (Phase D):** `Stream` upload/download (`CancellationToken` đã có trên mọi method).

---

### `Jarvis.BlobStoring/Helpers/BlobPathHelper.cs`

**Issue:** `ContainsTraversal` match substring `".."` → từ chối tên hợp lệ kiểu `file..backup.pdf`.

**Impact:** Upload fail edge-case khó debug.

**Suggested fix:** Chỉ reject segment path `..` sau split/normalize.

---

### `Jarvis.BlobStoring/FileSystem/FileSystemBlobStoringService.cs` — `GetFileNamesAsync` prefix

**Issue:** `searchPattern = $"{prefix}*"` không tương đương S3/MinIO prefix path (`2024/invoices`).

**Impact:** Hành vi list khác nhau giữa FileSystem và object storage.

**Suggested fix:** Filter `StartsWith(prefix)` trên relative path sau enumerate; hoặc document giới hạn FileSystem.

---

### Auto-select priority — footgun khi chain `Use*`

**Issue:** `DefaultProvider` rỗng → provider **đã đăng ký** có `AutoSelectPriority` cao nhất thắng (MinIO 30 > AwsS3 20 > FileSystem 10). Host gọi `AddCoreBlobStoring().UseMinIO()` trong dev có thể vô tình dùng MinIO thay vì FileSystem.

**Impact:** Ghi/read sai backend nếu không set `DefaultProvider` hoặc priority.

**Suggested fix:** Document rõ trong skill; Sample đã set `DefaultProvider: FileSystem` trong appsettings.

---

## Best Practices & Improvements

- Alias public `AddBlobStoring` → `AddCoreBlobStoring` (XML doc vẫn lẫn `AddBlobStoring`).
- Integration test DI: `DefaultProvider` invalid key → fail message (sau khi sửa Critical).
- Integration test: `AddCoreBlobStoring().UseMinIO()` + empty `DefaultProvider` → resolve MinIO.
- Health readiness cho bucket (host-owned, tag `readiness`) — Phase D.
- Breaking change doc cho consumer: gói `Jarvis.BlobStoring.FileSystem` removed; `GetFileNames` → `GetFileNamesAsync`; `MinioService` → `MinioBlobStoringService`.

---

## Đã sửa (so với review ban đầu)

| Hạng mục | Trạng thái |
|----------|------------|
| MinIO SSL constructor (`WithSSL` trên builder) | ✅ `MinioBlobStoringService` |
| FileSystem delete logic đảo / sai path | ✅ `FileSystemBlobStoringService` |
| Upload không tạo thư mục | ✅ |
| MinIO `GetFileNames` block thread (Rx + `Wait`) | ✅ `GetFileNamesAsync` + `await foreach` |
| `ViewAsync` expireTime MinIO vs S3 không thống nhất | ✅ cả hai dùng **giây** |
| `configure` không áp dụng FileSystem | ✅ `JarvisBlobStoringOptions` chỉ `DefaultProvider`; `UseFileSystem(fs => …)` + `FileSystemBlobOptions` |
| AwsS3 `DeletesAsync` tuần tự | ✅ `DeleteObjectsRequest` batch (1000/request) |
| Path traversal FileSystem | ✅ `BlobPathHelper` |
| AwsS3 stub | ✅ `AwsS3BlobStoringService` |
| Core DI / builder / registry | ✅ `AddCoreBlobStoring`, `BlobStoringBuilder`, `BlobStoringProviderRegistry` |
| `DefaultProvider` validate | ✅ `ResolveDefaultProviderKey` + `IsRegistered` |
| `CancellationToken` trên `IBlobStoringService` | ✅ |
| MinIO `DeletesAsync` aggregate `DeleteError` | ✅ `RemoveObjectsAsync` → `IList<DeleteError>`, log + throw |

---

## Summary

| Khu vực | Đánh giá |
|---------|----------|
| `Jarvis.BlobStoring` (core) | DI + registry + `BlobPathHelper` + FileSystem built-in — ổn |
| `Jarvis.BlobStoring.MinIO` | SSL, async list, presigned seconds, batch delete errors — ổn |
| `Jarvis.BlobStoring.AwsS3` | Batch delete, lazy client, dispose — ổn |
| `Sample` | `AddCoreBlobStoring()` + `appsettings` `DefaultProvider: FileSystem` |
| `UnitTest/BlobStoring` | 12 tests (path, registry, FileSystem, SSL builder, DI configure) |
| Skills | `blobstoring-dotnet` cập nhật `UseFileSystem(configure)` |

**Overall: merge-ready** — Critical đã xử lý; còn Suggestions (stream API, path prefix) có thể làm Phase D.

---

## Hướng dẫn host (branch `refactor-blob-storing`)

Pattern giống `AddJarvisCaching().UseRedisDistributedCache()`: core bind config + `Use*` đăng ký provider keyed.

### appsettings

`Sample/appsettings.json` — FileSystem là default rõ ràng (tránh auto-select MinIO khi có config MinIO nhưng chưa gọi `UseMinIO`):

```json
{
  "BlobStoring": {
    "DefaultProvider": "FileSystem",
    "FileSystem": {
      "RootPath": "",
      "SubPath": "",
      "AutoSelectPriority": 10
    },
    "MinIO": {
      "Endpoint": "",
      "AccessKey": "",
      "SecretKey": "",
      "UseSsl": false,
      "AutoSelectPriority": 30
    },
    "AwsS3": {
      "Region": "",
      "BucketName": "",
      "AccessKey": "",
      "SecretKey": "",
      "AutoSelectPriority": 20
    }
  }
}
```

| Key | Ý nghĩa |
|-----|---------|
| `DefaultProvider` | `"FileSystem"` / `"MinIO"` / `"AwsS3"` — phải khớp provider đã `Use*`; sai → `InvalidOperationException` |
| `DefaultProvider` rỗng | Provider đã đăng ký có `AutoSelectPriority` cao nhất thắng |
| `FileSystem:RootPath` rỗng | `{ContentRoot}/wwwroot/blobs` (PostConfigure) |
| `MinIO:Endpoint` rỗng | Không auto-select MinIO; `UseMinIO()` vẫn yêu cầu endpoint khi gọi |

Secrets: User Secrets / env — không commit `AccessKey` / `SecretKey`.

### Registration

**FileSystem only** (Sample `Program.cs`):

```csharp
using Jarvis.BlobStoring.Extensions;

builder.AddCoreBlobStoring();
```

`AddCoreBlobStoring()` → bind `BlobStoring`, registry, factory `IBlobStoringService` mặc định, tự `UseFileSystem()`.

**Override FileSystem path:**

```csharp
using Jarvis.BlobStoring;
using Jarvis.BlobStoring.Extensions;

builder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.FileSystem))
    .UseFileSystem(fs =>
    {
        fs.RootPath = @"D:\uploads";
        fs.SubPath = "tenant-1";
    });
```

**MinIO** — cần `Jarvis.BlobStoring.MinIO`:

```csharp
using Jarvis.BlobStoring.Extensions;
using Jarvis.BlobStoring.MinIO.Extensions;

builder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.MinIO))
    .UseMinIO();
```

**AwsS3** — cần `Jarvis.BlobStoring.AwsS3`:

```csharp
using Jarvis.BlobStoring.Extensions;
using Jarvis.BlobStoring.AwsS3.Extensions;

builder.AddCoreBlobStoring()
    .UseAwsS3();
```

**Keyed resolve** (cùng lúc nhiều backend):

```csharp
public sealed class DocumentService(
    IBlobStoringService blobs,
    [FromKeyedServices("MinIO")] IBlobStoringService minio)
{
    public Task<string> PresignAsync(string key)
        => minio.ViewAsync("documents", key, expireTime: 1800);
}
```

### Packages (host)

| PackageId | Khi nào |
|-----------|---------|
| `Jarvis.BlobStoring` | Luôn — core + FileSystem |
| `Jarvis.BlobStoring.MinIO` | Gọi `UseMinIO()` |
| `Jarvis.BlobStoring.AwsS3` | Gọi `UseAwsS3()` |

Sample hiện reference `Jarvis.BlobStoring` + `Jarvis.BlobStoring.MinIO`, chỉ gọi `AddCoreBlobStoring()` (FileSystem).

---

## Kiến trúc hiện tại

### Options & DI

| Type | Section config | Đăng ký |
|------|----------------|---------|
| `JarvisBlobStoringOptions` | `BlobStoring` | `AddCoreBlobStoring(configure)` — chỉ `DefaultProvider` |
| `FileSystemBlobOptions` | `BlobStoring:FileSystem` | `UseFileSystem(configure?)` |
| `MinIOBlobStoringOption` | `BlobStoring:MinIO` | `UseMinIO(configure?)` — package MinIO |
| `AwsS3BlobOptions` | `BlobStoring:AwsS3` | `UseAwsS3(configure?)` — package AwsS3 |

Default `IBlobStoringService` (factory sau `AddCoreBlobStoring`):

```csharp
builder.Services.TryAddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<JarvisBlobStoringOptions>>().Value;
    var registry = sp.GetRequiredService<BlobStoringProviderRegistry>();
    var key = registry.ResolveDefaultProviderKey(options.DefaultProvider);
    return sp.GetRequiredKeyedService<IBlobStoringService>(key);
});
```

Keyed keys: `"FileSystem"`, `"MinIO"`, `"AwsS3"` (`nameof(BlobStoringType.*)`).

### Cấu trúc project

```
Jarvis.BlobStoring/
├── Configuration/          JarvisBlobStoringOptions, FileSystemBlobOptions
├── Extensions/             AddCoreBlobStoring, UseFileSystem
├── FileSystem/             FileSystemBlobStoringService
├── Helpers/                BlobPathHelper
├── Hosting/                BlobStoringBuilder, BlobStoringProviderRegistry
└── IBlobStoringService.cs

Jarvis.BlobStoring.MinIO/   MinioBlobStoringService, UseMinIO
Jarvis.BlobStoring.AwsS3/   AwsS3BlobStoringService, UseAwsS3
```

**Đã xóa:** project `Jarvis.BlobStoring.FileSystem` (gộp vào core).

### `IBlobStoringService` (contract)

Mọi method có `CancellationToken cancellationToken = default`. Chi tiết: [iblob-api.md](../.opencode/skills/blobstoring-dotnet/reference/iblob-api.md).

```csharp
Task UploadAsync(string bucket, string fileName, byte[] bytes, CancellationToken cancellationToken = default);
Task<byte[]> DownloadAsync(string bucket, string fileName, CancellationToken cancellationToken = default);
Task DeleteAsync(string bucket, string fileName, CancellationToken cancellationToken = default);
Task DeletesAsync(string bucket, IEnumerable<string> fileNames, CancellationToken cancellationToken = default);
Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800, CancellationToken cancellationToken = default);
Task<IReadOnlyList<string>> GetFileNamesAsync(string bucket, string? prefix = null, CancellationToken cancellationToken = default);
```

| Method | FileSystem | MinIO | AwsS3 |
|--------|------------|-------|-------|
| Upload / Download / Delete | ✅ | ✅ | ✅ |
| ViewAsync (presigned) | `""` | ✅ | ✅ |
| GetFileNamesAsync | ✅ | ✅ | ✅ |

---

## Đối chiếu refactoring-rules.md

| Jarvis cung cấp | Host owned |
|-----------------|------------|
| `IBlobStoringService`, `AddCoreBlobStoring`, FileSystem default | Bucket naming, ACL, virus scan |
| `BlobStoringBuilder` + `Use*` extensions | Custom `IBlobStoringService` override sau Add |
| `BlobPathHelper` (FileSystem) | CDN / signed URL policy cho public file |
| `TryAdd*` keyed providers | `DefaultProvider` / priority trong appsettings |

Pattern tham chiếu: `Jarvis.Caching` (`AddJarvisCaching` + `UseRedisDistributedCache`).

| Caching (Redis) | BlobStoring (branch này) |
|-----------------|----------------------------|
| `Cache:DistributedGroups:Redis:Default` | `BlobStoring:FileSystem` / `MinIO` / `AwsS3` |
| `AddJarvisCaching().UseRedisDistributedCache()` | `AddCoreBlobStoring().UseFileSystem()` (auto) / `.UseMinIO()` / `.UseAwsS3()` |
| `DefaultDistributedGroup` + priority | `DefaultProvider` + `AutoSelectPriority` |
| Keyed `IConnectionMultiplexer` (host) | Keyed `IBlobStoringService` (`"MinIO"`, …) |

---

## Kế hoạch phases

### Phase 0 — Hotfix ✅

- [x] MinIO SSL, FileSystem delete/path/upload, `BlobPathHelper`

### Phase A — Core hosting ✅

- [x] `JarvisBlobStoringOptions`, `AddCoreBlobStoring`, `BlobStoringBuilder`, registry, `TryAdd` factory

### Phase B — Satellite packages ✅

- [x] `UseFileSystem` / `UseMinIO` / `UseAwsS3`
- [x] `MinioBlobStoringService`, `AwsS3BlobStoringService`
- [x] Gộp FileSystem vào core; xóa package `Jarvis.BlobStoring.FileSystem`

### Phase C — Sample & tests ✅

- [x] `Sample/appsettings.json` + `Program.cs`
- [x] `UnitTest/BlobStoring/*` (12 tests)

### Phase D — API evolution (chưa làm)

- [x] `CancellationToken` trên `IBlobStoringService`
- [ ] Stream-based upload/download
- [x] Validate `DefaultProvider` / keyed resolve (`BlobStoringProviderRegistry`)
- [x] MinIO `DeletesAsync` error aggregation
- [ ] Health readiness bucket (host)

---

## Breaking changes (consumer NuGet)

| Trước | Sau |
|-------|-----|
| Package `Jarvis.BlobStoring.FileSystem` | Gộp vào `Jarvis.BlobStoring` |
| `MinioService` / `MinIOOption` | `MinioBlobStoringService` / `MinIOBlobStoringOption` |
| `GetFileNames` sync | `GetFileNamesAsync` |
| Host tự `AddKeyedSingleton` | `AddCoreBlobStoring()` + `Use*` |

---

## Tiến độ

| Phase | Ngày | Ghi chú |
|-------|------|---------|
| 0–C | 2026-05-21 | Implement + tests 12/12 |
| Review #2 | 2026-05-21 | code-review-dotnet; validate `DefaultProvider` |
| D | — | stream API, health |

---

*Cập nhật: 2026-05-20 — branch `refactor-blob-storing`; appsettings/registration theo `blobstoring-dotnet` + pattern `caching-dotnet/providers/redis-distributed`.*
