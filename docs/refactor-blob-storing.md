# Refactor Jarvis.BlobStoring — Code Review & Kế hoạch

Tài liệu review toàn bộ `Jarvis.BlobStoring`, `Jarvis.BlobStoring.FileSystem`, `Jarvis.BlobStoring.MinIO`, `Jarvis.BlobStoring.AwsS3` theo [code-review skill](../.opencode/skills/code-review/SKILL.md) và mindset [CodeBaseSkill.md](../CodeBaseSkill.md).

**Phạm vi review:** toàn bộ source hiện tại (không chỉ diff PR).

---

## Mục tiêu nghiệp vụ (yêu cầu)

| # | Yêu cầu | Trạng thái hiện tại |
|---|---------|---------------------|
| 1 | MinIO, S3, FileSystem abstract qua `Jarvis.BlobStoring` | **Đạt** — `AddBlobStoring` + `UseFileSystem` / `UseMinIO` / `UseAwsS3`; `AwsS3BlobStoringService` implement |
| 2 | Mặc định không khai báo S3/MinIO → FileSystem | **Đạt** — `BlobStoringProviderSelection` auto MinIO → AwsS3 → FileSystem; host gọi `.UseFileSystem()` |
| 3 | Mở rộng provider mới **không sửa** codebase Jarvis khi dùng ở project khác | **Đạt** — extension trên `BlobStoringBuilder` + `TryAddKeyedSingleton`; override `IBlobStoringService` sau `AddBlobStoring` |

---

## Critical Issues

### `Jarvis.BlobStoring.MinIO/MinioService.cs` — constructor SSL

**Issue:** Khi `UseSsl == true`, code gọi `_minioClient = _minioClient.WithSSL()` trước khi `_minioClient` được gán → `NullReferenceException` lúc khởi tạo DI.

```csharp
if (_options.UseSsl)
    _minioClient = _minioClient.WithSSL();  // _minioClient chưa tồn tại

_minioClient = builder.Build();
```

**Impact:** Production crash ngay khi bật SSL trong config.

**Suggested fix:**

```csharp
var clientBuilder = new MinioClient()
    .WithEndpoint(_options.Endpoint)
    .WithCredentials(_options.AccessKey, _options.SecretKey);

if (_options.UseSsl)
    clientBuilder = clientBuilder.WithSSL();

_minioClient = clientBuilder.Build();
```

---

### `Jarvis.BlobStoring.FileSystem/FileSystemService.cs` — `DeleteAsync` logic đảo

**Issue:**

```csharp
if (!System.IO.File.Exists(path))
    System.IO.File.Delete(path);
```

Xóa file khi **không** tồn tại; file thật **không** bị xóa.

**Impact:** Data không được dọn; caller tưởng đã delete thành công.

**Suggested fix:** `if (File.Exists(path)) File.Delete(path);`

---

### `Jarvis.BlobStoring.FileSystem/FileSystemService.cs` — `DeletesAsync` sai path

**Issue:**

```csharp
if (!System.IO.File.Exists(name))
    System.IO.File.Delete(name);
```

Dùng `name` (relative) thay vì `path` đã `Combine(RootPath, SubPath, bucket, name)` — có thể xóa nhầm file ở cwd hoặc không xóa gì.

**Impact:** Mất dữ liệu ngoài ý muốn (path traversal nhẹ) hoặc leak file.

**Suggested fix:** Kiểm tra và xóa `path`; tạo thư mục cha trước upload nếu cần.

---

### `Jarvis.BlobStoring.FileSystem/FileSystemService.cs` — `UploadAsync` không tạo thư mục

**Issue:** `WriteAllBytesAsync` tới path có subfolder chưa tồn tại → `DirectoryNotFoundException`.

**Impact:** Upload fail trên bucket/path mới (common case).

**Suggested fix:** `Directory.CreateDirectory(Path.GetDirectoryName(path)!);` trước khi ghi.

---

### `Jarvis.BlobStoring.MinIO/MinioService.cs` — `GetFileNames` block thread (deadlock / starvation risk)

**Issue:** `ListObjectsEnumAsync` + Rx `Subscribe` + `observable.Wait()` chạy đồng bộ trên thread gọi (thường là ASP.NET request thread).

**Impact:** Under load: thread pool starvation, latency spike; trên context có `SynchronizationContext`, pattern `Wait()` + callback có thể gây deadlock (scenario: callback post về context đang bị block bởi `Wait()`).

**Suggested fix:** Implement async `GetFileNamesAsync` (hoặc `IAsyncEnumerable<string>`) dùng `await foreach` / async iterator MinIO SDK; bỏ `Wait()` và Rx trong library code.

---

## Suggestions

### `Jarvis.BlobStoring/IBlobStoringService.cs`

**Issue:** Toàn bộ API sync-allocation (`byte[]`), không có `CancellationToken`, không stream.

**Impact:** File lớn → LOH pressure, OOM; không hủy được khi client disconnect; khó tích hợp với `ICacheService.GetOrSetAsync` (skill caching đã ghi chú thiếu CT).

**Suggested fix (phase sau, breaking có kiểm soát):**

- `UploadAsync(..., Stream content, CancellationToken ct = default)`
- `DownloadAsync` → `Stream` hoặc `IAsyncEnumerable<ReadOnlyMemory<byte>>`
- Thêm `CancellationToken` cho mọi method async

---

### `Jarvis.BlobStoring.MinIO/MinioService.cs` — `DeletesAsync`

**Issue:** `RemoveObjectsAsync` trả về observable/errors nhưng không xử lý (code comment-out); caller không biết object nào fail.

**Impact:** Partial delete im lặng → inconsistent bucket state.

**Suggested fix:** Await completion, aggregate `DeleteError`, throw `InvalidOperationException` với danh sách key lỗi hoặc return `BulkDeleteResult`.

---

### `Jarvis.BlobStoring.MinIO/MinioService.cs` — `DownloadAsync` / `UploadAsync`

**Issue:** `MemoryStream` không `await using`; upload content-type typo `application/actet-stream`; `ReUpload` gọi đệ quy `UploadAsync` có thể double-create bucket trên race.

**Impact:** Handle leak nhỏ; client cache sai MIME; edge race khi nhiều upload song song vào bucket mới.

**Suggested fix:** `await using var stream = new MemoryStream(bytes)`; sửa content-type; tách `EnsureBucketExistsAsync` idempotent.

---

### `Jarvis.BlobStoring.FileSystem/FileSystemService.cs` — stub methods

**Issue:** `GetFileNames` → `Enumerable.Empty`; `ViewAsync` → `""`.

**Impact:** Feature parity sai giữa providers; host không phân biệt “chưa implement” vs “file không có”.

**Suggested fix:** Implement list qua `Directory.EnumerateFiles`; `ViewAsync` throw `NotSupportedException` hoặc map qua static file middleware URL (document trong XML).

---

### Path traversal / bucket sanitization (FileSystem + future S3)

**Issue:** `bucket` + `fileName` ghép trực tiếp vào path không normalize/canonicalize.

**Impact:** `fileName = "../../../secrets.json"` có thể thoát `RootPath` (path traversal).

**Suggested fix:** Helper `internal static` trong core:

```csharp
static string ResolveSafePath(string root, string bucket, string fileName)
{
    var full = Path.GetFullPath(Path.Combine(root, bucket, fileName));
    var rootFull = Path.GetFullPath(root);
    if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Invalid blob path.");
    return full;
}
```

---

### Kiến trúc & DI — toàn module

**Issue:**

| Thành phần | Vấn đề |
|------------|--------|
| `BlobStoringBuilder` | Class rỗng — không fluent chain |
| `ObjectStorageOption` | Chỉ `Type` string; không `SectionName`, không nested options |
| `BlobStoringType` | Thiếu `AwsS3`; không khớp `ObjectStorageOption` |
| Core `.csproj` | `FrameworkReference Microsoft.AspNetCore.App` — coupling không cần cho storage library |
| `Jarvis.BlobStoring.AwsS3` | Stub `Class1`, không reference core, không `PackageId` |
| Host | Skill ghi host tự `AddKeyedSingleton` — vi phạm convention Jarvis (`AddJarvisCaching` + `UseRedis`) |

**Impact:** Không đạt mục tiêu (2)(3); mỗi app copy-paste DI; dễ đăng ký sai provider.

**Suggested fix:** Xem mục **Thiết kế mục tiêu** bên dưới.

---

## Best Practices & Improvements

- **File header comment (EN)** cho mỗi file theo `CodeBaseSkill.md` §2.
- **`sealed`** cho implementation; **`virtual`** chỉ khi host cần override (MinIO/FileSystem đã có `virtual` — giữ nếu document là extension point host).
- **Bilingual XML** trên options (`MinIOBlobStoringOption`, `FileSystemOption`) — đơn vị, sentinel disable.
- **`ConfigureAwait(false)`** trong toàn bộ library async (MinIO chưa nhất quán).
- **Structured logging** — `LogError(ex, "…")` thay vì `LogError(ex.Message, ex)` trong `GetFileNames`.
- **Unit tests** cho FileSystem path helper, delete/upload, và MinIO SSL builder (mock `IMinioClient` nếu tách factory).

---

## Summary

| Project | Đánh giá |
|---------|----------|
| `Jarvis.BlobStoring` | Contract mỏng OK; thiếu configuration, DI extensions, builder, abstractions plug-in |
| `Jarvis.BlobStoring.FileSystem` | **Blocked** — bug delete/path nghiêm trọng; stub list/view |
| `Jarvis.BlobStoring.MinIO` | **Needs changes** — bug SSL; blocking `GetFileNames`; delete/upload hardening |
| `Jarvis.BlobStoring.AwsS3` | **Not started** |
| **Overall** | **blocked** cho production — sửa Critical trước; refactor kiến trúc để đạt 3 mục tiêu nghiệp vụ |

---

## Đối chiếu CodeBaseSkill.md

### Core vs Host-owned

| Jarvis (core) | Host (owned) |
|---------------|--------------|
| `IBlobStoringService`, options gốc, `AddBlobStoring`, chọn default FileSystem | Bucket naming theo domain, policy ACL, virus scan, metadata DB |
| `BlobStoringBuilder` + `TryAdd` default `FileSystemService` | `AddKeyed` thêm provider thứ hai nếu multi-backend |
| Validation path an toàn (helper) | URL public cho `ViewAsync` (CDN, signed URL policy) |
| Extension `UseMinIO` / `UseAwsS3` trong package con | Custom `IBlobStoringService` implement + `AddBlobStoring(configure)` override |

### Anti-patterns hiện tại

| Anti-pattern | Hiện trạng |
|--------------|------------|
| Host copy DI thủ công | Skill blob-storing yêu cầu `AddKeyedSingleton` tay |
| Core reference ASP.NET full | `Jarvis.BlobStoring.csproj` |
| Builder rỗng | `BlobStoringBuilder` |
| Không `TryAdd` | Host không thể override default an toàn |
| Provider không đồng nhất | FileSystem stub vs MinIO đầy đủ hơn |

### Tham chiếu pattern nên bắt chước

- `Jarvis.Caching.Extensions.JarvisCachingHostBuilderExtensions` — `AddJarvisCaching` + snapshot + `JarvisCachingBuilder`
- `Jarvis.Caching.Redis.CachingBuilderExtension.UseRedisDistributedCache` — package vệ tinh gắn vào builder
- `Jarvis.HealthChecks.HealthCheckServiceExtensions` — bind options, conditional register

---

## Thiết kế mục tiêu

### Cấu trúc thư mục đề xuất

```
Jarvis.BlobStoring/
├── Abstractions/
│   └── IBlobStoringConfigurator.cs    # plug-in: package/host đăng ký provider
├── Configuration/
│   ├── JarvisBlobStoringOptions.cs     # SectionName = "BlobStoring"
│   └── BlobStoringProviderOptions.cs   # Type, DefaultProvider
├── Extensions/
│   └── BlobStoringHostBuilderExtensions.cs  # AddBlobStoring
├── Hosting/
│   └── BlobStoringBuilder.cs           # fluent: UseFileSystem | host callback
├── Helpers/
│   └── BlobPathHelper.cs               # path traversal safe (internal)
└── IBlobStoringService.cs

Jarvis.BlobStoring/
├── FileSystem/
│   ├── FileSystemOption.cs
│   └── FileSystemBlobStoringService.cs
├── Extensions/
│   ├── BlobStoringHostBuilderExtensions.cs   # AddBlobStoring (+ UseFileSystem)
│   └── FileSystemBlobStoringExtensions.cs

Jarvis.BlobStoring.MinIO/
├── Configuration/MinIOBlobOptions.cs
├── Extensions/MinIOBlobStoringExtensions.cs       # builder.UseMinIO()
└── MinioBlobStoringService.cs

Jarvis.BlobStoring.AwsS3/
├── Configuration/AwsS3BlobOptions.cs
├── Extensions/AwsS3BlobStoringExtensions.cs       # builder.UseAwsS3()
└── AwsS3BlobStoringService.cs
```

### Config mẫu (`appsettings.json`)

```json
{
  "BlobStoring": {
    "DefaultProvider": "FileSystem",
    "FileSystem": {
      "RootPath": "D:/data/blobs",
      "SubPath": ""
    },
    "MinIO": {
      "Endpoint": "",
      "AccessKey": "",
      "SecretKey": "",
      "UseSsl": false
    },
    "AwsS3": {
      "Region": "",
      "BucketName": "",
      "AccessKey": "",
      "SecretKey": ""
    }
  }
}
```

**Quy tắc chọn provider (mục tiêu 1 + 2):**

1. Snapshot sau bind config.
2. Nếu `MinIO:Endpoint` **không rỗng** → đăng ký MinIO keyed + set default = MinIO (hoặc theo `DefaultProvider`).
3. Else nếu `AwsS3` có credential/region hợp lệ (rule document trong XML) → S3.
4. Else → **`TryAddSingleton<IBlobStoringService, FileSystemBlobStoringService>`** (default không keyed) **và** keyed `"FileSystem"` cho multi-provider.

Sentinel (theo CodeBaseSkill):

- `Endpoint == ""` → coi MinIO **tắt**, không đăng ký MinIO package logic.
- Tương tự S3 khi thiếu region/bucket.

### DI API mục tiêu (mục tiêu 3 — mở rộng không sửa Jarvis core)

```csharp
// Program.cs — tối thiểu
var blob = builder.AddBlobStoring(); // TryAdd FileSystem default nếu không có MinIO/S3

// Chỉ khi cần — reference package tương ứng
blob.UseMinIO();      // Jarvis.BlobStoring.MinIO
// blob.UseAwsS3();   // Jarvis.BlobStoring.AwsS3

// Host custom provider — KHÔNG fork Jarvis
builder.Services.AddSingleton<IBlobStoringConfigurator, AzureBlobConfigurator>();
// hoặc sau AddBlobStoring:
builder.Services.AddSingleton<IBlobStoringService, MyCompanyBlobService>();
```

**`IBlobStoringConfigurator` (gợi ý):**

```csharp
public interface IBlobStoringConfigurator
{
    void Configure(BlobStoringBuilder builder, IServiceCollection services);
}
```

Core trong `AddBlobStoring`:

```csharp
foreach (var c in services.BuildServiceProvider()... ) // ❌ anti-pattern
```

Đúng pattern: `services.TryAddEnumerable(ServiceDescriptor.Singleton<IBlobStoringConfigurator, ...>())` và gọi trong một `IConfigureOptions` hoặc trong `AddBlobStoring` sau khi collect descriptors — hoặc đơn giản hơn: **chỉ dùng extension methods** trên `BlobStoringBuilder` (giống Caching Redis), host/project thứ ba tạo package `Contoso.BlobStoring.Azure` với `UseAzureBlob(this BlobStoringBuilder builder)` — **không cần sửa repo Jarvis**.

Keyed services (khi cần nhiều backend):

```csharp
services.TryAddKeyedSingleton<IBlobStoringService, FileSystemBlobStoringService>(
    nameof(BlobStoringType.FileSystem));
```

Default resolve:

```csharp
services.TryAddSingleton<IBlobStoringService>(sp =>
    sp.GetRequiredKeyedService<IBlobStoringService>(options.DefaultProvider));
```

Host override: `services.AddSingleton<IBlobStoringService, Custom>()` **sau** `AddBlobStoring` → registration cuối thắng (document trong skill).

### `BlobStoringType` & enum

```csharp
public enum BlobStoringType
{
    FileSystem = 1,
    MinIO = 2,
    AwsS3 = 3
}
```

### Package references

| Package | Reference |
|---------|-----------|
| `Jarvis.BlobStoring` | `Microsoft.Extensions.Hosting.Abstractions`, `Options`, `DI.Abstractions` — **bỏ** `Microsoft.AspNetCore.App` nếu không cần |
| `Jarvis.BlobStoring.*` | ProjectReference → core only |
| `Jarvis.BlobStoring.AwsS3` | AWSSDK.S3 + align `PackageId` / `GeneratePackageOnBuild` |

---

## Kế hoạch refactor (phases)

### Phase 0 — Hotfix (trước merge bất kỳ) ✅ 2026-05-21

- [x] Sửa MinIO SSL constructor (`MinioBlobStoringService`)
- [x] Sửa FileSystem `DeleteAsync` / `DeletesAsync` / tạo thư mục upload (`FileSystemBlobStoringService`)
- [x] Thêm `BlobPathHelper` + dùng trong FileSystem

### Phase A — Core contract & hosting ✅ 2026-05-21

- [x] `JarvisBlobStoringOptions` + `SectionName`
- [x] `BlobStoringHostBuilderExtensions.AddBlobStoring(IHostApplicationBuilder, Action<>?)`
- [x] `BlobStoringBuilder` giống `JarvisCachingBuilder`
- [x] Logic default FileSystem khi MinIO/S3 “tắt” (`BlobStoringProviderSelection`)
- [x] `TryAdd*` cho default `IBlobStoringService`
- [x] Cập nhật `.opencode/skills/jarvis-dotnet/modules/blob-storing/SKILL.md`

### Phase B — Package vệ tinh ✅ 2026-05-21

- [x] `FileSystemBlobStoringExtensions.UseFileSystem`
- [x] Refactor `MinioService` → `MinioBlobStoringService` + `UseMinIO` + `GetFileNamesAsync` (nội bộ)
- [x] Implement `Jarvis.BlobStoring.AwsS3` (`AwsS3BlobStoringService`, `UseAwsS3`)
- [x] Align `PackageId`, file header comments (các file mới)

### Phase C — Sample & tests ✅ 2026-05-21

- [x] `Sample/appsettings.json` section `BlobStoring`
- [x] `Sample/Program.cs`: `AddBlobStoring().UseFileSystem()`
- [x] UnitTest: `UnitTest/BlobStoring/*` (6 tests pass)

### Phase D — API evolution (optional breaking) — chưa làm

- [ ] `CancellationToken`, stream-based upload/download
- [ ] `GetFileNamesAsync` trên `IBlobStoringService` (hiện chỉ có trên MinIO/S3 implementation)
- [ ] Health readiness check bucket (host-owned tag `readiness`)

---

## Mapping mục tiêu → hạng mục refactor

| Mục tiêu | Hạng mục cần làm |
|----------|------------------|
| (1) Abstract qua core | Giữ `IBlobStoringService` ở core; mọi provider implement; **không** expose MinIO/AWS type ra host |
| (2) Default FileSystem | `AddBlobStoring` conditional + `TryAddSingleton<IBlobStoringService, FileSystem*>` |
| (3) Mở rộng ngoài Jarvis | `BlobStoringBuilder` + extension method trong package/host; `TryAdd`; keyed optional; document override |

---

## Sample host sau refactor (target)

```csharp
// FileSystem (local dev) — built-in trong Jarvis.BlobStoring
builder.AddBlobStoring();

// Production MinIO — thêm package MinIO + điền BlobStoring:MinIO:Endpoint
builder.AddBlobStoring().UseMinIO();

// Inject
public class FileService(IBlobStoringService blobs) { ... }

// Multi backend
public class FileService(
    [FromKeyedServices(nameof(BlobStoringType.MinIO))] IBlobStoringService minio,
    [FromKeyedServices(nameof(BlobStoringType.FileSystem))] IBlobStoringService local) { ... }
```

---

## Commit message gợi ý (khi implement)

```text
refactor(blob-storing): add host DI, default FileSystem, fix provider bugs

* AddBlobStoring + BlobStoringBuilder aligned with Caching pattern
* Fix MinIO SSL init, FileSystem delete/path bugs
* Document refactor plan and review findings
```

---

---

## Tiến độ implement

| Phase | Ngày | Ghi chú |
|-------|------|---------|
| 0 | 2026-05-21 | Hotfix SSL, delete/path, `BlobPathHelper` |
| A | 2026-05-21 | `AddBlobStoring`, options, keyed default resolve |
| B | 2026-05-21 | `UseFileSystem` / `UseMinIO` / `UseAwsS3`, rename services |
| C | 2026-05-21 | Sample + 6 unit tests |
| D | — | Chưa bắt đầu |

**Host tối thiểu:** `AddBlobStoring()` bind options + đăng ký FileSystem + factory. Gọi `.UseMinIO()` / `.UseAwsS3()` khi cần package tương ứng.

*Cập nhật: 2026-05-21 — MinIO/AwsS3 options + provider registry tách khỏi core; gộp FileSystem vào `Jarvis.BlobStoring`.*
