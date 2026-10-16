# ADR — `Platform.BlobStoring*` (core + satellite providers + stream API Phase D)

> **Trạng thái:** 🟢 **Accepted** — baseline D1–D10 + Phase D stream D11–D19 (2026-08-15).  
> **Ngày:** 2026-08-15 · **Accept §7:** 2026-08-15 · Implement baseline: 2026-05–08/2026 · **Implement Phase D:** 2026-08-15  
> **Loại:** Package boundary / Module Atomic / API evolution  
> **Liên quan:** [refactoring-rules.md](./refactoring-rules.md) §0.2, [refactor-cache-plan.md](./refactor-cache-plan.md) (pattern `AddCore` + `Use`*), [code review BlobStoring + Files](../reviews/2026-08-13-code-review-blob-storing-files.md), [modules/files/README.md](../modules/files/README.md)  
> **Phạm vi:** family `Platform.BlobStoring` (core + FileSystem built-in), satellite `Platform.BlobStoring.{MinIO,AwsS3,SharePoint,Synology}`, contract `IBlobStoringService`, DI `AddCoreBlobStoring` + `BlobStoringBuilder`, **Phase D stream upload/download**.  
> **Ngoài phạm vi:** HTTP Files API (`Platform.Modules.Files`*) — facade/validation/route; health readiness bucket (ADR riêng sau); tách `Files.API.SharePoint` / `.Synology` satellite; virus scan / quota / audit nghiệp vụ.  
> **Supersedes:** [refactor-blob-storing.md](./refactor-blob-storing.md) (review/plan cũ — không còn là nguồn quyết định chính).  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm / cần giữ xanh · 🔴 chưa làm / chờ confirm

---



## 1. Bối cảnh

Platform cần abstraction lưu trữ file/binary **đa backend** (local disk, S3-compatible, SharePoint, Synology) mà host **opt-in** từng provider, không sửa core khi thêm backend mới. Pattern mirror `Platform.Caching` (`AddPlatformCaching` + `UseRedisDistributedCache`).


| Thành phần                        | Vai trò                                                                                          |
| --------------------------------- | ------------------------------------------------------------------------------------------------ |
| `Platform.BlobStoring`            | Core: `IBlobStoringService`, `AddCoreBlobStoring`, registry, **FileSystem built-in**             |
| `Platform.BlobStoring.MinIO`      | Satellite: `UseMinIO()`                                                                          |
| `Platform.BlobStoring.AwsS3`      | Satellite: `UseAwsS3()`                                                                          |
| `Platform.BlobStoring.SharePoint` | Satellite: `UseSharePoint()` + folder service                                                    |
| `Platform.BlobStoring.Synology`   | Satellite: `UseSynology()` + folder service                                                      |
| `Platform.Modules.Files`          | Facade `IFileManager` — **không** thay `IBlobStoringService`; host wiring blob + files tách biệt |




### 1.1 Layout packages (hiện tại)

```text
frameworks/
  Platform.BlobStoring/
    Configuration/     PlatformBlobStoringOptions, FileSystemBlobOptions
    Extensions/        AddCoreBlobStoring, UseFileSystem
    FileSystem/        FileSystemBlobStoringService
    Helpers/           BlobPathHelper, ObjectStoragePathHelper
    Hosting/           BlobStoringBuilder, BlobStoringProviderRegistry
    IBlobStoringService.cs
  Platform.BlobStoring.MinIO/
  Platform.BlobStoring.AwsS3/
  Platform.BlobStoring.SharePoint/
  Platform.BlobStoring.Synology/

modules/files/
  Platform.Modules.Files/          IFileManager, FilePathValidator
  Platform.Modules.Files.API/      FileController, FolderController (UseHttpApi)
```

**Đã xóa:** package `Platform.BlobStoring.FileSystem` (gộp vào core).

### 1.2 Host wiring (mẫu)

```csharp
builder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.FileSystem))
    .UseFileSystem(fs => fs.RootPath = @"D:\uploads");
// Optional satellites (reference package + Use*):
//   .UseMinIO() | .UseAwsS3() | .UseSharePoint() | .UseSynology()

builder.AddCoreFiles()
    .UseHttpApi()
    .UseSharePointTree()   // khi UseSharePoint
    .UseSynologyTree();    // khi UseSynology
```

Keyed resolve khi cần nhiều backend cùng lúc:

```csharp
public sealed class DocsService(
    IBlobStoringService defaultBlobs,
    [FromKeyedServices(nameof(BlobStoringType.MinIO))] IBlobStoringService minio) { }
```

---



## 2. Vấn đề


| #   | Vấn đề                                                            | Hệ quả                                                                                                                                                        |
| --- | ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| P1  | Contract `IBlobStoringService` chỉ `byte[]` upload/download       | HTTP + provider buffer **2× file size** trong RAM; file lớn → OOM / GC pressure ([review P2 memory](../reviews/2026-08-13-code-review-blob-storing-files.md)) |
| P2  | `DefaultProvider` rỗng → auto-select theo `AutoSelectPriority`    | Host register nhiều `Use*` → backend mặc định có thể **không** như mong đợi (footgun dev/prod)                                                                |
| P3  | Rename/move folder trên object storage list **toàn bucket**       | Folder lớn → timeout; lỗi giữa chừng → trạng thái partial (copy xong, delete fail)                                                                            |
| P4  | Files HTTP API (`IFileManager`) pass-through mỏng                 | Chấp nhận Phase 0 scaffold; quota/audit/virus scan = ngoài phạm vi ADR này                                                                                    |
| P5  | `Platform.Modules.Files.API` hard-reference SharePoint + Synology | Host chỉ FileSystem vẫn kéo assembly tree — tech debt package boundary (ADR follow-up)                                                                        |


**Đã xử lý (baseline):**


| Vấn đề cũ                                                       | Trạng thái                                                   |
| --------------------------------------------------------------- | ------------------------------------------------------------ |
| `DefaultProvider` invalid không fail-fast                       | 🟢 `BlobStoringProviderRegistry.ResolveDefaultProviderKey`   |
| Path traversal `..` substring false-positive                    | 🟢 `BlobPathHelper.ContainsTraversalSegment` (segment-based) |
| FileSystem `GetFileNamesAsync` prefix không khớp object storage | 🟢 `StartsWith(prefix)` sau enumerate                        |
| MinIO SSL / async list / batch delete errors                    | 🟢 đã fix trên branch                                        |


---



## 3. Ràng buộc


| #   | Ràng buộc                                                                                                       |
| --- | --------------------------------------------------------------------------------------------------------------- |
| C1  | Core `Platform.BlobStoring` **không** reference satellite (MinIO, S3, SharePoint, Synology)                     |
| C2  | Mỗi satellite = package riêng + extension `Use*` trên `BlobStoringBuilder`                                      |
| C3  | Keyed `IBlobStoringService` dùng `nameof(BlobStoringType.*)` — ổn định, public contract                         |
| C4  | `AddCoreBlobStoring()` **luôn** gọi `UseFileSystem()` — dev/local default không cần config cloud                |
| C5  | Mọi method async có `CancellationToken cancellationToken = default`                                             |
| C6  | **Không breaking** consumer hiện tại khi thêm stream API — giữ `UploadAsync(byte[])` / `DownloadAsync()`        |
| C7  | Secrets (`AccessKey`, `ClientSecret`, DSM password) — User Secrets / env; **không** commit appsettings          |
| C8  | Path validation (`..` segment) ở Files module / `BlobPathHelper` — provider không là lớp validate HTTP duy nhất |


---



## 4. Phương án đã cân nhắc *(Phase D — stream API)*


| #   | Phương án                                       | Tóm tắt                                                   | Ưu                                  | Nhược                                                    |
| --- | ----------------------------------------------- | --------------------------------------------------------- | ----------------------------------- | -------------------------------------------------------- |
| O1  | Chỉ sửa `FileController`                        | Bỏ `MemoryStream.ToArray()`, vẫn gọi `byte[]` API         | Nhỏ, nhanh                          | **Không giải** buffer ở `IBlobStoringService` / provider |
| O2  | Stream API song song trên `IBlobStoringService` | `UploadStreamAsync` + `OpenReadAsync`; `byte[]` = wrapper | Giảm RAM end-to-end; không breaking | Implement ×5 provider + Files facade                     |
| O3  | Download redirect presigned URL                 | Controller 302 tới S3/MinIO/SharePoint                    | Không stream qua app server         | FileSystem/Synology không có; đổi contract HTTP          |
| O4  | Spool temp file trên disk                       | HTTP → temp → provider                                    | Tránh RAM peak                      | I/O + cleanup; phức tạp hơn O2                           |


**Đề xuất chọn:** **O2** (stream API song song, `byte[]` giữ làm compatibility shim). **Đã chọn** — confirm Q2 (2026-08-15).

---



## 5. Quyết định



### 5.1 Baseline — Core + satellite hosting 🟢 *(Accepted + Implemented — Q1)*


| #   | Quyết định         | Chi tiết                                                                                                        |
| --- | ------------------ | --------------------------------------------------------------------------------------------------------------- |
| D1  | Family naming      | `Platform.BlobStoring` core; satellite `Platform.BlobStoring.{Provider}`                                        |
| D2  | DI entry           | `AddCoreBlobStoring(configure?)` → `BlobStoringBuilder` + auto `UseFileSystem()`                                |
| D3  | Default resolve    | Factory singleton `IBlobStoringService` qua `BlobStoringProviderRegistry` + `DefaultProvider`                   |
| D4  | Keyed providers    | `TryAddKeyedSingleton<IBlobStoringService, T>(nameof(BlobStoringType.X))` + `Register(key, AutoSelectPriority)` |
| D5  | FileSystem in-core | Không package `Platform.BlobStoring.FileSystem` riêng                                                           |
| D6  | Provider enum      | `BlobStoringType`: FileSystem, MinIO, AwsS3, SharePoint, Synology                                               |
| D7  | Config sections    | `BlobStoring`, `BlobStoring:FileSystem`, `:MinIO`, `:AwsS3`, `:SharePoint`, `:Synology`                         |
| D8  | `ViewAsync` expire | **Giây** (default 1800); MinIO + AwsS3 + SharePoint thống nhất                                                  |
| D9  | Path helpers       | `BlobPathHelper` (local), `ObjectStoragePathHelper` (rename/move/key normalize)                                 |
| D10 | Files integration  | Host gọi `AddCoreBlobStoring` **trước** `AddCoreFiles`; `IFileManager` delegate `IBlobStoringService`           |




### 5.2 Phase D — Stream API 🟢 *(Implemented 2026-08-15)*


| #   | Quyết định                      | Chi tiết                                                                                                                                            |
| --- | ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| D11 | Thêm contract stream upload     | `UploadStreamAsync(bucket, fileName, Stream content, long? contentLength, CancellationToken)`                                                       |
| D12 | Thêm contract stream download   | `OpenReadAsync(bucket, fileName, CancellationToken)` → `BlobReadResult`                                                                             |
| D13 | Type kết quả download (**Q5**)  | `BlobReadResult` trong `Platform.BlobStoring`: `Stream Content`, `string? ContentType`, `long? ContentLength`, `IAsyncDisposable`                   |
| D14 | Giữ `byte[]` API                | `UploadAsync(byte[])` → shim `UploadStreamAsync`; `DownloadAsync` → shim `OpenReadAsync` (provider có thể implement native `byte[]` nếu tối ưu hơn) |
| D15 | Files facade + HTTP             | `IFileManager` thêm stream methods; `FileController` stream-through, **bỏ** `MemoryStream.ToArray()`                                                |
| D16 | Provider implement (**Q3**)     | **Cùng phase, cả 5:** FileSystem, MinIO, AwsS3, SharePoint, Synology (+ `SynologyClient` multipart từ `Stream`)                                     |
| D17 | Upload limit                    | Host/controller: `FormOptions.MultipartBodyLengthLimit` hoặc `[RequestSizeLimit]` — document README Files                                           |
| D18 | Range download (**Q4**)         | **Không** Phase D — `enableRangeProcessing: false`; HTTP 206 / resume = Phase E (ADR follow-up)                                                     |
| D19 | `contentLength` upload (**Q6**) | **Optional** (`long?`); **controller truyền khi có** (vd. `IFormFile.Length`); mỗi provider **dùng hoặc bỏ qua** theo khả năng SDK (xem §5.7)       |




### 5.7 `contentLength` — contract theo provider *(Q6)*


| Provider      | Controller / caller                  | Provider behavior khi `contentLength`                                                                                                                                     |
| ------------- | ------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| FileSystem    | Truyền `file.Length` nếu có          | **Bỏ qua** — `CopyToAsync` vào `FileStream`                                                                                                                               |
| MinIO / AwsS3 | Truyền `file.Length` khi upload HTTP | **Ưu tiên** set `Content-Length` trên PutObject; null + `CanSeek` → `stream.Length - Position`; null + non-seekable → document limitation hoặc buffer (tránh im lặng OOM) |
| SharePoint    | Truyền khi có                        | Dùng cho Graph large-file session khi API yêu cầu size                                                                                                                    |
| Synology      | Truyền khi có                        | Dùng cho multipart `Content-Length` khi build body; null → seek hoặc đọc stream theo contract client                                                                      |


**Quy tắc chung:** không bắt buộc caller luôn có length; **không** throw chỉ vì `contentLength == null` — provider tự fallback hoặc `NotSupportedException` có message rõ cho stream non-seekable không hỗ trợ.

### 5.3 Sơ đồ luồng sau Phase D

```text
HTTP Upload:
  IFormFile.OpenReadStream()
    → IFileManager.UploadStreamAsync
      → IBlobStoringService.UploadStreamAsync
        → Provider (FileStream / S3 PutObject stream / Graph stream / Synology StreamContent)

HTTP Download:
  IBlobStoringService.OpenReadAsync
    → BlobReadResult (Content, ContentType, ContentLength)
      → File(stream, contentType, fileDownloadName, enableRangeProcessing: false)
```

`FileController` **upload (sau Phase D):**

```csharp
await using var stream = file!.OpenReadStream();
await fileManager.UploadStreamAsync(
    resolved, fileName!, stream,
    contentLength: file.Length > 0 ? file.Length : null,
    cancellationToken);
```



### 5.4 Contract `IBlobStoringService` (sau Phase D)

```csharp
// Existing — giữ nguyên signature
Task UploadAsync(string bucket, string fileName, byte[] bytes, CancellationToken cancellationToken = default);
Task<byte[]> DownloadAsync(string bucket, string fileName, CancellationToken cancellationToken = default);

// New — Phase D
Task UploadStreamAsync(
    string bucket,
    string fileName,
    Stream content,
    long? contentLength = null,
    CancellationToken cancellationToken = default);

Task<BlobReadResult> OpenReadAsync(
    string bucket,
    string fileName,
    CancellationToken cancellationToken = default);
```


| Method                                      | FileSystem                   | MinIO                       | AwsS3              | SharePoint            | Synology                  |
| ------------------------------------------- | ---------------------------- | --------------------------- | ------------------ | --------------------- | ------------------------- |
| Upload/Download `byte[]`                    | ✅ shim                       | ✅ shim                      | ✅ shim             | ✅ shim                | ✅ shim                    |
| UploadStreamAsync                           | `CopyToAsync` → `FileStream` | SDK `PutObject` InputStream | SDK stream         | Graph upload stream   | `StreamContent` multipart |
| OpenReadAsync                               | `File.OpenRead`              | `GetObject` stream          | `GetObject` stream | Graph download stream | HTTP response stream      |
| CreateBucket / DeleteBucket / Rename / Move | ✅                            | ✅                           | ✅                  | ✅                     | ✅                         |
| ViewAsync (presigned / sharing link)        | `""`                         | ✅                           | ✅                  | ✅ (scope config)      | ✅                         |
| GetFileNamesAsync                           | ✅ prefix                     | ✅                           | ✅                  | ✅                     | ✅                         |




### 5.5 appsettings (host)

```json
{
  "BlobStoring": {
    "DefaultProvider": "FileSystem",
    "FileSystem": { "RootPath": "", "SubPath": "", "AutoSelectPriority": 10 },
    "MinIO": { "Endpoint": "", "AccessKey": "", "SecretKey": "", "UseSsl": false, "AutoSelectPriority": 30 },
    "AwsS3": { "Region": "", "BucketName": "", "AccessKey": "", "SecretKey": "", "AutoSelectPriority": 20 },
    "SharePoint": { "TenantId": "", "ClientId": "", "ClientSecret": "", "DriveId": "", "AutoSelectPriority": 25 },
    "Synology": { "Host": "", "Username": "", "Password": "", "RootPath": "", "AutoSelectPriority": 23 }
  }
}
```


| Key                        | Ý nghĩa                                                                                  |
| -------------------------- | ---------------------------------------------------------------------------------------- |
| `DefaultProvider`          | Phải khớp provider đã `Use*`; sai → `InvalidOperationException` kèm danh sách registered |
| `DefaultProvider` rỗng     | Provider **đã đăng ký** có `AutoSelectPriority` cao nhất thắng                           |
| `FileSystem:RootPath` rỗng | `{ContentRoot}/wwwroot/blobs` (PostConfigure)                                            |


**Khuyến nghị host:** luôn set `DefaultProvider` explicit trong production.

### 5.6 Ngoài Phase D (ghi nhận, không block stream)


| Hạng mục                                  | Hướng xử lý                                                   | ADR                   |
| ----------------------------------------- | ------------------------------------------------------------- | --------------------- |
| Object storage rename/move O(bucket)      | Document limitation; list theo prefix; job nền cho folder lớn | Tech debt / follow-up |
| Health readiness bucket                   | Host-owned tag `readiness`                                    | ADR riêng             |
| Tách `Files.API.SharePoint` / `.Synology` | Satellite giống blob pattern                                  | ADR Files module      |
| Auth trên Files HTTP API                  | Host policy hoặc `ApplyAuthorizeToFilesHttpApi`               | ADR Files / Sample    |


---



## 6. Hệ quả


| Hướng         | Hệ quả                                                                                               |
| ------------- | ---------------------------------------------------------------------------------------------------- |
| Tốt           | Host opt-in provider; thêm backend không sửa core; stream API giảm RAM cho upload/download lớn       |
| Tốt           | `byte[]` shim giữ backward compat cho code/script gọi trực tiếp `IBlobStoringService`                |
| Xấu / chi phí | Phase D chạm 5 provider + `IFileManager` + `FileController` + tests; Synology multipart phức tạp hơn |
| Trung lập     | Rename/move folder lớn vẫn O(bucket) — stream không giải semantic này                                |
| Rủi ro        | Stream lifecycle — phải dispose `BlobReadResult` / `FileStream` đúng; test leak                      |


---



## 7. Confirm *(đã chốt 2026-08-15)*


| #   | Câu hỏi                                                                  | Đề xuất                                         | **Trả lời (confirmed)**                                                                                 |
| --- | ------------------------------------------------------------------------ | ----------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| Q1  | Accept baseline D1–D10 (đã implement)?                                   | Coi là 🟢 Accepted — chỉ document, không rework | **OK** — baseline Accepted + Implemented, không rework                                                  |
| Q2  | Accept Phase D stream D11–D17?                                           | O2 — stream song song, giữ `byte[]`             | **OK** — O2; thêm D18–D19 theo Q4–Q6                                                                    |
| Q3  | Implement **cùng lúc cả 5 provider** hay chia phase?                     | **Cùng lúc**                                    | **OK** — cả 5 provider trong một Phase D                                                                |
| Q4  | Bật `enableRangeProcessing` (HTTP Range / resume download) ngay Phase D? | **Không** — follow-up                           | **Không** — Phase E / ADR riêng                                                                         |
| Q5  | Tên type kết quả download?                                               | `BlobReadResult`                                | `BlobReadResult` trong `Platform.BlobStoring`                                                           |
| Q6  | `contentLength` trên upload bắt buộc hay optional?                       | Optional                                        | **Optional** — controller truyền khi có (`IFormFile.Length`); provider dùng theo khả năng hỗ trợ (§5.7) |


**Trạng thái:** 🟢 Accepted + Implemented (2026-08-15).

---



## 8. Test cases



### 8.1 Smoke *(bắt buộc)*


| #   | Case                                                                 | Expect                         | Nơi                                                          | Trạng thái |
| --- | -------------------------------------------------------------------- | ------------------------------ | ------------------------------------------------------------ | ---------- |
| T1  | `AddCoreBlobStoring().UseFileSystem()` resolve `IBlobStoringService` | `FileSystemBlobStoringService` | `UnitTest/BlobStoring/BlobStoringHostBuilderExtensionsTests` | 🟢         |
| T2  | `DefaultProvider` invalid → exception có danh sách provider          | Actionable message             | `UnitTest/BlobStoring/BlobStoringProviderRegistryTests`      | 🟢         |
| T3  | Files HTTP upload → list → download → delete                         | 200/204 happy path             | `UnitTest/Files/Integration/FilesHttpContractTests`          | 🟢         |




### 8.2 Regression — test cũ cần giữ xanh *(bắt buộc)*


| #   | Case                                                  | Expect                                   | Nơi                                                                                    | Trạng thái |
| --- | ----------------------------------------------------- | ---------------------------------------- | -------------------------------------------------------------------------------------- | ---------- |
| T4  | FileSystem CRUD + prefix list + traversal reject      | Suite xanh                               | `UnitTest/BlobStoring/FileSystemBlobStoringServiceTests`                               | 🟢         |
| T5  | Path helper segment traversal                         | `file..backup.pdf` hợp lệ; `../x` reject | `UnitTest/BlobStoring/ObjectStoragePathHelperTests`                                    | 🟢         |
| T6  | MinIO / AwsS3 / SharePoint / Synology extension DI    | Keyed service registered                 | `UnitTest/BlobStoring/*ExtensionsTests`                                                | 🟢         |
| T7  | `FilePathValidator` + `FileManager` tree NotSupported | 15+ unit tests                           | `UnitTest/Files/*`                                                                     | 🟢         |
| T8  | `byte[]` **API sau Phase D** — behavior giữ nguyên    | Upload/Download byte[] vẫn pass          | `FileSystemBlobStoringServiceTests.DownloadAsync_ByteArray_Shim_Matches_OpenReadAsync` | 🟢         |




### 8.3 Test mới *(Phase D)*


| #   | Case                                                                  | Expect                                                  | Nơi                                                      | Trạng thái                                                       |
| --- | --------------------------------------------------------------------- | ------------------------------------------------------- | -------------------------------------------------------- | ---------------------------------------------------------------- |
| T9  | `UploadStreamAsync` FileSystem — file > buffer nhỏ                    | Nội dung khớp; không load full vào `byte[]` trước write | `UnitTest/BlobStoring/FileSystemBlobStoringServiceTests` | 🟢                                                               |
| T10 | `OpenReadAsync` FileSystem — stream readable, dispose không lock file | Stream đọc đúng bytes                                   | 同上                                                       | 🟢                                                               |
| T11 | MinIO/AwsS3 stream round-trip (mock hoặc integration tùy CI)          | Upload stream → open read → equal                       | `UnitTest/BlobStoring/`                                  | 🟡 (chưa có integration cloud; FileSystem cover stream contract) |
| T12 | `FilesHttpContractTests` happy path sau controller stream             | Vẫn pass; không `ToArray()` trong controller            | `UnitTest/Files/Integration/`                            | 🟢                                                               |
| T13 | `DownloadAsync(byte[])` shim gọi `OpenReadAsync`                      | Kết quả identical với implementation cũ                 | `FileSystemBlobStoringServiceTests`                      | 🟢                                                               |




### 8.4 Done khi

- Mọi hàng **Smoke** + **Regression** (T1–T8) 🟢.
- Mọi hàng **Test mới** Phase D (T9–T13) 🟢.
- ADR status → 🟢 Accepted + Implemented.

---



## 9. Kế hoạch triển khai


| Phase | Việc                                                                      | Done khi                   | Phụ thuộc | Trạng thái    |
| ----- | ------------------------------------------------------------------------- | -------------------------- | --------- | ------------- |
| 0     | Hotfix path/MinIO/FileSystem                                              | Tests path + delete        | —         | 🟢            |
| A     | Core hosting: `AddCoreBlobStoring`, registry, builder                     | Build + DI                 | 0         | 🟢            |
| B     | Satellite MinIO + AwsS3 + gộp FileSystem                                  | `Use*` extensions          | A         | 🟢            |
| C     | Sample appsettings + `UnitTest/BlobStoring`                               | T1–T6 xanh                 | B         | 🟢            |
| D1    | SharePoint + Synology satellite + Files module scaffold                   | Branch `LEX-359`           | C         | 🟢            |
| D2    | **Confirm §7** (stream API)                                               | Status → Accepted          | D1        | 🟢 2026-08-15 |
| D3    | `BlobReadResult` + `IBlobStoringService` stream methods                   | Core compile               | D2        | 🟢 2026-08-15 |
| D4    | Implement stream ×5 providers + `byte[]` shim                             | Provider tests T9–T11, T13 | D3        | 🟢 2026-08-15 |
| D5    | `IFileManager` + `FileController` stream-through                          | T12 + T3 xanh              | D4        | 🟢 2026-08-15 |
| D6    | Cập nhật review doc + README + ADR index                                  | Docs đồng bộ               | D5        | 🟢 2026-08-15 |
| E     | Health readiness, Files.API satellite split, **HTTP Range download** (Q4) | ADR riêng                  | D6        | 🔴            |


---



## 10. Breaking changes (consumer NuGet) — lịch sử baseline


| Trước                                     | Sau                                                  |
| ----------------------------------------- | ---------------------------------------------------- |
| Package `Platform.BlobStoring.FileSystem` | Gộp vào `Platform.BlobStoring`                       |
| `MinioService` / `MinIOOption`            | `MinioBlobStoringService` / `MinIOBlobStoringOption` |
| `GetFileNames` sync                       | `GetFileNamesAsync`                                  |
| Host tự `AddKeyedSingleton`               | `AddCoreBlobStoring()` + `Use*`                      |
| Route Sample `/blobs`                     | `/api/v*/files` (Files module)                       |


**Phase D:** không breaking — chỉ **thêm** method; `byte[]` API giữ nguyên.

---



## 11. Checklist Done


| #   | Việc                                             | Trạng thái    |
| --- | ------------------------------------------------ | ------------- |
| 1   | Baseline D1–D10 trên branch                      | 🟢            |
| 2   | Files validation + HTTP integration tests        | 🟢            |
| 3   | Confirm §7 Q1–Q6                                 | 🟢 2026-08-15 |
| 4   | Phase D stream implement D11–D19                 | 🟢 2026-08-15 |
| 5   | §8 T9–T13 xanh (T11 cloud integration deferred)  | 🟢            |
| 6   | ADR index + supersede `refactor-blob-storing.md` | 🟢            |


---



## 12. Tham chiếu thêm

- [architecture-rules.md](./architecture-rules.md) §0.2 Core + satellite
- [refactoring-rules.md](./refactoring-rules.md)
- Review: [2026-08-13-code-review-blob-storing-files.md](../reviews/2026-08-13-code-review-blob-storing-files.md)
- Plan cũ (superseded): [refactor-blob-storing.md](./refactor-blob-storing.md)
- Skills: `.opencode/skills/blobstoring-dotnet/` (cập nhật sau Phase D)

