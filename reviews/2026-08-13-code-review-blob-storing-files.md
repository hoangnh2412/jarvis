# Code review: `Platform.BlobStoring*` + `modules/files`


| Mục         | Giá trị                                                                                            |
| ----------- | -------------------------------------------------------------------------------------------------- |
| Branch      | `LEX-359-Synology-SharePoint`                                                                      |
| Commit      | `8b2ae64` — appsettings: sharepoint, synology                                                      |
| Phạm vi     | `frameworks/Platform.BlobStoring*`, `modules/files` (Platform.Modules.Files + .API), Sample wiring |
| Ngày review | 2026-08-13                                                                                         |
| Cập nhật    | 2026-08-13 — fix validation P1, FileErrorCode, unit + integration test                             |
| Trọng tâm   | Bug, regression hành vi, bảo mật, thiếu test                                                       |


---

## Tóm tắt

Kiến trúc **Module Atomic + IFileManager** đi đúng hướng (tách blob DI khỏi HTTP API). **Đã xử lý** các gap validation P1 (traversal trên `fileName`/`prefix`, response 400 có `code`/`message` qua `FileErrorCode`) và bổ sung unit test cho `FilePathValidator`, `FileManager.GetTreeAsync`, DI smoke.

**Còn mở:** Files HTTP API không auth; `AddCoreFiles()` chưa fail-fast khi thiếu `AddCoreBlobStoring()`; `ListAsync` chưa validate `prefix`.

**Đánh giá tổng thể:** merge-ready có điều kiện — xử lý P1 auth trước khi expose production; cân nhắc fail-fast DI và integration test route.

---



## Thay đổi sau review (2026-08-13)


| Hạng mục                             | Trạng thái | Chi tiết                                                                            |
| ------------------------------------ | ---------- | ----------------------------------------------------------------------------------- |
| DELETE `prefix` validation           | ✅ Fixed    | `FilePathValidator.ValidateDelete` → whitespace + traversal                         |
| Upload/download `fileName` traversal | ✅ Fixed    | `ValidateUpload`, `ValidateDownload`, `ValidateView` dùng `ValidatePathSegment`     |
| `FileErrorCode` wire vào API         | ✅ Fixed    | `98201`–`98208` + `Message()`; controller trả `{ code, message }` BadRequest        |
| Unit test Files module               | ✅ Partial  | `UnitTest/Files/FilePathValidatorTests.cs`, `FileManagerTests.cs` — 15 test pass    |
| Integration test HTTP contract       | ✅ Added    | `UnitTest/Files/Integration/FilesHttpContractTests.cs` + `FilesTestServer` — 7 test |
| FolderController 501                 | ✅ Fixed    | Response gồm `FileErrorCode.TreeNotSupported`                                       |


**File chính:**

- `modules/files/Platform.Modules.Files/FileErrorCode.cs`
- `modules/files/Platform.Modules.Files/Validation/FilePathValidator.cs`
- `modules/files/Platform.Modules.Files/Validation/FileValidationResult.cs`
- `modules/files/Platform.Modules.Files.API/FileApiResponses.cs`
- `modules/files/Platform.Modules.Files.API/Controllers/FileController.cs`
- `UnitTest/Files/*`

**Integration test coverage (**`FilesHttpContractTests`**):**

- Route prefix `/api/v2/files` (không còn `/blobs`)
- Upload/download/delete `prefix` traversal → 400 `{ code: 98201, message }`
- `GET /files/tree` → 501 `{ code: 98202 }` (FileSystem, không tree provider)
- Happy path: create bucket → upload → list → download → delete file
- Delete batch traversal → 400

---



## Findings (theo mức độ nghiêm trọng)



### [P1] Files HTTP API không auth — upload/download/xóa bucket công khai

**File:** `modules/files/Platform.Modules.Files.API/Controllers/FileController.cs`, `FolderController.cs`

`UseHttpApi()` expose toàn bộ CRUD blob qua `api/v*/files/*` **không** `[Authorize]`, giống pattern Setting đã được flag trước đó. Sample gọi `AddSampleFiles().UseHttpApi()` mà không gắn policy.

Hậu quả: bất kỳ client nào reach được Sample/production host đều có thể:

- `POST .../files/{bucket}/upload` — ghi file tùy ý
- `GET .../files/{bucket}/download` — đọc file
- `DELETE .../files/{bucket}` (không `fileName`) — **xóa cả bucket/subtree**
- `POST .../files/{bucket}/delete-batch` — xóa hàng loạt
- `GET .../files/tree` — duyệt cây SharePoint/Synology nếu provider bật

**Đề xuất:** Host bắt buộc auth/policy (document mẫu trong README); hoặc module cung cấp `ApplyAuthorizeToFilesHttpApi` tương tự Setting sample.

---



### ~~[P1]~~ `DELETE /files/{bucket}` ~~—~~ `prefix` ~~không validate~~ ✅ Fixed

**File:** `FileController.cs`, `FilePathValidator.ValidateDelete`

`ValidateDelete` giờ validate:

- `bucket` required + traversal
- `fileName` optional — whitespace reject + traversal khi có giá trị
- `prefix` optional — whitespace reject + traversal khi có giá trị

Lỗi → `400 BadRequest` với `{ code: "98201"|"98208", message: "..." }`.

**Còn lại (P2):** Không có bước confirm / soft-delete cho bucket delete — vẫn dễ mất dữ liệu do nhầm lẫn hoặc CSRF trên host không auth.

---



### ~~[P1] Upload/download —~~ `fileName` ~~không validate traversal~~ ✅ Fixed

**File:** `FilePathValidator.ValidateUpload`, `ValidateDownload`, `ValidateView`, `FileController`

Traversal trên `fileName` (và `bucket`) được kiểm tra **trước** khi gọi `IFileManager` / storage. Response 400 thay vì 500 trên FileSystem.

`report..final.pdf` vẫn hợp lệ (segment-based, không false-positive).

---



### [P1] `AddCoreFiles()` không đủ — runtime fail nếu host quên `AddCoreBlobStoring()`

**File:** `Platform.Modules.Files/Services/FileManager.cs`, `PlatformFilesExtensions.cs`

`FileManager` inject:

- `IFileStorageResolver` → cần `IBlobStoringService`
- `BlobStoringProviderRegistry`, `IOptions<PlatformBlobStoringOptions>`

Chỉ gọi `AddCoreFiles()` mà không `AddCoreBlobStoring()` → DI resolve `FileManager` **fail lúc request đầu** (registry/options chưa đăng ký). XML doc có ghi nhưng không fail-fast lúc startup.

**Đề xuất:** Trong `AddCoreFiles`, gọi `services.GetOrAddProviderRegistry()` + validate `IBlobStoringService` registered (hoặc `IHostApplicationBuilder` extension order guard throw rõ ở startup).

**Test hiện có:** `PlatformFilesExtensionsTests.AddCoreFiles_With_AddCoreBlobStoring_Registers_FileManager` — smoke DI khi wiring đúng.

---



### ~~[P2] Breaking change route API —~~ `/blobs` ~~→~~ `/files` ~~không có test regression~~ ✅ Partial

**File:** refactor `Platform.Files.Api` → `Platform.Modules.Files.API`

Route mới: `api/v{version}/files/{bucket}/*`, tree: `api/v{version}/files/tree`.

- FE/integrator đang dùng route cũ sẽ **gãy**
- `UnitTest/Files/Integration/FilesHttpContractTests` xác nhận route `/api/v2/files` và happy path CRUD
- `modules/files/docs/file-management-plan.md` vẫn mô tả route `/blobs` cũ

**Còn lại:** Document breaking change; cập nhật plan doc; mở rộng integration test (rename/move, Sample host stack).

---



### [P2] `GET /files/{bucket}` list — `prefix` chưa validate

**File:** `FileController.ListAsync`

Không gọi `FilePathValidator` trước `ListAsync`. Traversal trên `prefix` có thể throw 500 từ FileSystem thay vì 400.

**Đề xuất:** Thêm `ValidateList(bucket, prefix)` tái sử dụng `ValidateOptionalPrefix`.

---



### [P2] Multi-provider + `DefaultProvider` rỗng — resolve sai backend

**File:** `Sample/Extensions/SampleFilesExtensions.cs`, `BlobStoringProviderRegistry.cs`, `FileManager.GetTreeAsync`

Sample có thể register **FileSystem + SharePoint + Synology** cùng lúc khi config đủ. `DefaultProvider` rỗng → auto-select priority cao nhất (Synology 23 > SharePoint 25 > … — cần verify priority map).

`IFileManager` luôn dùng **default** `IBlobStoringService` — không còn keyed route `/sharepoint/blobs`. Host cấu hình nhiều provider nhưng API unified chỉ hit **một** backend.

`GetTreeAsync` chọn tree provider theo `DefaultProvider`, không theo query — nếu tree provider đăng ký (SharePoint) nhưng default là FileSystem → **501** (response có `FileErrorCode.TreeNotSupported`).

**Đề xuất:** Sample set `DefaultProvider` explicit trong appsettings; document footgun; cân nhắc query `?provider=` hoặc workspace mapping sau.

---



### ~~[P2] Memory pressure — upload/download buffer full~~ `byte[]` ✅ Fixed (Phase D)

**File:** `FileController.cs`, `IBlobStoringService`, all providers

Đã thêm `UploadStreamAsync` / `OpenReadAsync` / `BlobReadResult`. HTTP upload/download stream-through; `byte[]` API giữ làm shim backward compat.

**ADR:** [2026-08-15-adr-platform-blob-storing.md](../ADRs/2026-08-15-adr-platform-blob-storing.md) §5.2 Phase D.

---



### [P2] Object storage rename/move — O(toàn bucket), không atomic

**File:** `MinioBlobStoringService.MovePrefixAsync`, `AwsS3BlobStoringService.CopyPrefixAsync`

Move/rename folder trên MinIO/S3:

1. `GetFileNamesAsync` **list toàn bucket** (không prefix filter hiệu quả)
2. Copy từng key + batch delete

Folder lớn → timeout, memory; lỗi giữa chừng → **duplicate/partial state** (copy xong, delete fail).

**Đề xuất:** Document limitation; list theo prefix; compensating transaction hoặc background job cho folder lớn.

---



### [P2] `SynologyFileTreeProvider` bỏ qua `rootPath`

**File:** `Platform.Modules.Files.API/Storage/SynologyFileTreeProvider.cs`

```csharp
var tree = await folderService.GetTreeAsync(bucket, cancellationToken);
// rootPath không được dùng
```

SharePoint provider có logic `rootPath ?? bucket`; Synology **ignore** `rootPath` → API `GET /files/tree?rootPath=2024` không hoạt động trên Synology.

**Đề xuất:** Map `rootPath` vào bucket hoặc combine path trước khi gọi `ISynologyFolderService`.

---



### [P2] SharePoint `ViewLinkScope: anonymous` — link công khai

**File:** `SharePointBlobStoringService` ctor

Log warning khi `ViewLinkScope == anonymous` — tốt. Nhưng host vẫn có thể cấu hình link public; `ViewAsync` API trả URL cho client không auth.

**Đề xuất:** Document rủi ro trong README host; default `organization` scope.

---



### [P2] `Platform.Modules.Files.API` hard-reference SharePoint + Synology

**File:** `Platform.Modules.Files.API.csproj`

Host chỉ cần FileSystem vẫn kéo SharePoint/Synology assemblies qua API package (tree adapters).

**Đề xuất:** Tách `Platform.Modules.Files.API.SharePoint` / `.Synology` satellite (giống blob pattern) — phase sau.

---



### [P3] `FileManager` pass-through — chưa có giá trị nghiệp vụ

**File:** `Platform.Modules.Files/Services/FileManager.cs`

Mọi method delegate 1:1 sang `IBlobStoringService`. Chưa có: quota, audit, virus scan hook, metadata, workspace routing.

Chấp nhận được cho **Phase 0 scaffold**, nhưng module chưa đáp ứng “quản lý file nghiệp vụ đầy đủ” — chỉ đổi tên layer.

---



### ~~[P3]~~ `FileErrorCode` ~~khai báo nhưng không dùng~~ ✅ Fixed

**File:** `Platform.Modules.Files/FileErrorCode.cs`

Mã lỗi `98201`–`98208` được dùng trong:

- `FilePathValidator` → `FileValidationResult.Fail(code, message?)`
- `FileApiResponses.ValidationError` → HTTP 400 `{ code, message }`
- `FileManager.GetTreeAsync` → `NotSupportedException` message
- `FolderController` → HTTP 501 `{ code: TreeNotSupported, message }`

---



### ~~[P3] Thiếu test cho Files module~~ ✅ Partial

**Coverage hiện có:**


| File test                                              | Nội dung                                                                      |
| ------------------------------------------------------ | ----------------------------------------------------------------------------- |
| `UnitTest/Files/FilePathValidatorTests.cs`             | upload/download/delete prefix traversal, move batch, FileErrorCode.Message    |
| `UnitTest/Files/FileManagerTests.cs`                   | `GetTreeAsync` → NotSupported; DI smoke `AddCoreFiles` + `AddCoreBlobStoring` |
| `UnitTest/Files/Integration/FilesHttpContractTests.cs` | route `/api/v2/files`, 400 validation body, 501 tree, CRUD happy path         |
| `UnitTest/Files/Helpers/FilesTestServer.cs`            | in-memory TestServer (FileSystem + `UseHttpApi`)                              |


**Còn thiếu:**


| Khu vực            | Test cần có                                                |
| ------------------ | ---------------------------------------------------------- |
| `ListAsync` prefix | Khi thêm validator                                         |
| Sample host E2E    | `WebApplicationFactory<Program>` với auth/middleware stack |


---



### [P3] Convention & docs

- Options/service class chưa `sealed` (refactoring-rules §2.3)
- Skill `blobstoring-dotnet` / `files-dotnet` chưa có trong repo

---



## Điểm tốt (giữ nguyên)


| Khu vực                                         | Ghi chú                                                          |
| ----------------------------------------------- | ---------------------------------------------------------------- |
| `AddCoreBlobStoring` + registry + keyed factory | Fail-fast `DefaultProvider` invalid — actionable message         |
| `BlobPathHelper.ContainsTraversalSegment`       | Segment-based, không false-positive `file..backup.pdf`           |
| `FilePathValidator` + `FileErrorCode`           | Validation tập trung, response API nhất quán                     |
| FileSystem `GetFileNamesAsync`                  | `StartsWith(prefix)` đồng bộ object storage                      |
| MinIO batch delete                              | Aggregate `DeleteError`, log + throw                             |
| AwsS3 / MinIO                                   | `Lazy<T>` client, `IDisposable` có điều kiện                     |
| Tách module Files                               | `AddCoreFiles` / `UseHttpApi` mirror Setting; blob wiring ở host |
| Synology session retry                          | `SynologyClient` xử lý session expired codes                     |


---



## Ma trận test hiện tại


| Package                         | Unit test                         | Ghi chú                         |
| ------------------------------- | --------------------------------- | ------------------------------- |
| Platform.BlobStoring            | ✅ Path, registry, FileSystem, DI  | Ổn                              |
| Platform.BlobStoring.MinIO      | ✅ SSL builder                     | Thiếu rename/move               |
| Platform.BlobStoring.AwsS3      | ✅ cơ bản                          | Thiếu rename/move               |
| Platform.BlobStoring.SharePoint | ✅ mapping, extensions             | Không integration Graph         |
| Platform.BlobStoring.Synology   | ✅ extensions, defaults            | Không integration DSM           |
| Platform.Modules.Files          | ✅ Validator, FileManager tree, DI | Thiếu list prefix               |
| Platform.Modules.Files.API      | ✅ HTTP contract (TestServer)      | Chưa test qua Sample host stack |


---



## Checklist đề xuất trước merge production

- [ ] Auth/policy trên Files HTTP API (host hoặc module helper)
- [x] Validate `fileName` / `prefix` / batch paths → 400 với `FileErrorCode`
- [ ] Validate `prefix` trên `GET /files/{bucket}` list
- [ ] Sample `DefaultProvider` explicit trong appsettings
- [ ] Fail-fast startup khi thiếu `AddCoreBlobStoring`
- [x] Unit test `FilePathValidator` + DI smoke `AddCoreFiles`
- [x] Integration test HTTP contract `/files` (`FilesHttpContractTests`)
- [ ] Document breaking route `/blobs` → `/files`
- [ ] Fix Synology tree `rootPath` hoặc document limitation

---



## Tham chiếu


| Tài liệu           | Path                                               |
| ------------------ | -------------------------------------------------- |
| Refactoring rules  | `ADRs/refactoring-rules.md`                        |
| Blob ADR           | `ADRs/refactor-blob-storing.md`                    |
| Files README       | `modules/files/README.md`                          |
| Review mẫu Setting | `reviews/2026-08-07-code-review-module-setting.md` |


---

*Reviewer: code-review (Cursor). Cập nhật sau fix validation + unit test — 2026-08-13.*