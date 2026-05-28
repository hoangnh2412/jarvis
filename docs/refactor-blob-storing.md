# Refactor Jarvis.BlobStoring — Code Review & Kế hoạch

Review branch **`refactor-blob-storing`** so với **`develop`** theo [code-review-dotnet skill](../.opencode/skills/code-review-dotnet/SKILL.md) và [refactoring-rules.md](./refactoring-rules.md).

**Phạm vi review:** toàn bộ source hiện tại (không chỉ diff PR).

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

## Kiến trúc hiện tại

### Options & DI

| Type | Section config | Đăng ký |
|------|----------------|---------|
| `JarvisBlobStoringOptions` | `BlobStoring` | `AddCoreBlobStoring(configure)` — chỉ `DefaultProvider` |
| `FileSystemBlobOptions` | `BlobStoring:FileSystem` | `UseFileSystem(configure?)` |
| `MinIOBlobStoringOption` | `BlobStoring:MinIO` | `UseMinIO(configure?)` — package MinIO |
| `AwsS3BlobOptions` | `BlobStoring:AwsS3` | `UseAwsS3(configure?)` — package AwsS3 |

Default `IBlobStoringService`:

```csharp
services.TryAddSingleton(sp =>
{
    var full = Path.GetFullPath(Path.Combine(root, bucket, fileName));
    var rootFull = Path.GetFullPath(root);
    if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Invalid blob path.");
    return full;
}
```

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

### Host mẫu

```csharp
// FileSystem only
builder.AddCoreBlobStoring();

// Override FileSystem path
builder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.FileSystem))
    .UseFileSystem(fs =>
    {
        fs.RootPath = @"D:\uploads";
        fs.SubPath = "tenant-1";
    });

// MinIO
builder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.MinIO))
    .UseMinIO(minio => { minio.Endpoint = "localhost:9000"; /* … */ });
```

### `IBlobStoringService` (contract)

```csharp
Task UploadAsync(string bucket, string fileName, byte[] bytes);
Task<byte[]> DownloadAsync(string bucket, string fileName);
Task DeleteAsync(string bucket, string fileName);
Task DeletesAsync(string bucket, IEnumerable<string> fileNames);
Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800); // seconds
Task<IReadOnlyList<string>> GetFileNamesAsync(string bucket, string? prefix = null);
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

- [ ] `CancellationToken` + stream-based upload/download
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

*Cập nhật: 2026-05-21 — review theo `code-review-dotnet` trên branch `refactor-blob-storing` vs `develop`.*
