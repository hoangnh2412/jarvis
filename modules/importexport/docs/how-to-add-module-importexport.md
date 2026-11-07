# How-to — Thêm module nghiệp vụ mới dùng Platform ImportExport

Hướng dẫn khi **host đã gắn ImportExport** và cần thêm một đối tượng/module nghiệp vụ mới (ví dụ `Teacher`, `User`) giống cách Sample đang dùng `Student`.

**Không nằm trong tài liệu này:** greenfield wire DI lần đầu (MiniExcel / EF / HttpApi, host-owned `IExcelTemplateEntity`, tenant ambient). Xem README module và Sample `AddSampleImportExport` nếu host chưa sẵn.

**Tham chiếu Sample (đã chạy):** `Student` — ImportRow, registry, persister, Setting template code, `StudentService` + API/FE.

---

## 1. Điều kiện tiên quyết

Host đã có:

| Hạng mục | Kỳ vọng |
|----------|---------|
| DI ImportExport | `AddCoreImport()` / `AddCoreExport()` + `.UseMiniExcel()` + `.UseEntityFramework<…>()` (+ `.UseHttpApi()` nếu cần) |
| Template store | Host-owned entity implement `IExcelTemplateEntity` + migration |
| Tenant | Ambient tenant được set trước khi CRUD template / import (nếu multi-tenant) |
| Ít nhất một entity mẫu | Ví dụ `Student` — dùng làm pattern copy |

---

## 2. Khái niệm cần khớp một `EntityKey`

Mọi chỗ dưới đây phải dùng **cùng một chuỗi** (ví dụ `"Teacher"`):

| Thành phần | Vai trò |
|------------|---------|
| Const `EntityKey` | Nguồn sự thật duy nhất trên host |
| `ConfigureEntities(...).Add<TRow>(key, displayName)` | Whitelist + schema cho UI tạo template |
| `IImportModulePersister.EntityKey` | Persist routing khi import |
| `AddImportModulePersister<T>(key)` | Đăng ký DI keyed (nên truyền **cùng** `EntityKey`) |
| `ImportRequest.EntityKey` | Host service **bắt buộc** set khi gọi `IImportService` |
| Template UI | User chọn entity khi tạo/sửa mapping (schema API) |

```text
[UI Template] chọn EntityKey → mapping fieldName theo ImportRow
        ↓
[Module service] ImportRequest { TemplateCode, EntityKey, File… }
        ↓
[ImportExport] parse Excel → Dictionary rows
        ↓
[ImportModulePersisterComposite] route theo EntityKey
        ↓
[Your IImportModulePersister] map row → entity nghiệp vụ → DB
```

**Lưu ý:** Registry / schema **không** map sang EF entity lúc import. Runtime vẫn là `Dictionary<string, object?>`; chỉ persister của bạn quyết định persist.

---

## 3. Checklist triển khai (thay `{Module}` = Teacher / …)

### Bước 1 — Const `EntityKey`

Thêm hằng số (Sample: `Sample/ImportExport/Constants/ImportExportEntityKeys.cs`):

```csharp
public static class ImportExportEntityKeys
{
    public const string Student = "Student";
    public const string Teacher = "Teacher"; // mới
}
```

Dùng **chỉ** const này mọi nơi — không hard-code chuỗi rải rác.

---

### Bước 2 — Import DTO (`*ImportRow`)

Tạo DTO **chỉ cho schema UI + DataAnnotations** (không phải EF entity).

Ví dụ Sample `StudentImportRow`:

```csharp
public sealed class TeacherImportRow
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }
}
```

Quy ước:

- Property public readable → hiện trên `GET …/entities/{key}/fields`
- Bỏ qua `Id` / `TenantId` (engine đã filter)
- Tên property = `fieldName` trong column mapping template

---

### Bước 3 — Đăng ký registry (`ConfigureEntities`)

Trong extension DI host (Sample: `SampleImportExportExtensions`):

```csharp
builder.AddCoreImport()
    .ConfigureEntities(options =>
    {
        options.Add<StudentImportRow>(ImportExportEntityKeys.Student, "Student");
        options.Add<TeacherImportRow>(ImportExportEntityKeys.Teacher, "Giáo viên"); // mới
    })
    .UseMiniExcel()
    .UseEntityFramework</* host template entity */, /* UoW */, /* tenant */>()
    .UseHttpApi();
```

Sau khi redeploy host: entity mới xuất hiện trong dropdown UI template.

---

### Bước 4 — `IImportModulePersister`

Implement persist theo `EntityKey`:

```csharp
public sealed class TeacherImportModulePersister(
    ITenantUnitOfWork unitOfWork,
    ICurrentTenant<CurrentTenantInfo> currentTenant) : IImportModulePersister
{
    public string EntityKey => ImportExportEntityKeys.Teacher;

    public async Task<ImportPersistResult> SaveAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        TemplateDefinition template,
        CancellationToken cancellationToken = default)
    {
        // 1. Đọc field theo fieldName mapping (Name, Department, …)
        // 2. Validate nghiệp vụ; gom ImportRowError nếu cần
        // 3. Insert qua repository / UoW của module
        // 4. return SavedCount + Errors
    }
}
```

Đăng ký DI (**truyền cùng `EntityKey`**):

```csharp
builder.Services
    .AddImportModulePersister<StudentImportModulePersister>(ImportExportEntityKeys.Student)
    .AddImportModulePersister<TeacherImportModulePersister>(ImportExportEntityKeys.Teacher);
```

> **Pitfall Sample:** code demo từng gọi `AddImportModulePersister<…>(nameof(StudentImportModulePersister))`. Composite route theo `IImportModulePersister.EntityKey` (không theo tên class). **Nên** luôn truyền `ImportExportEntityKeys.*` cho khớp và tránh nhầm khi dùng keyed resolve.

---

### Bước 5 — Setting: template code theo module

Import/export của module nghiệp vụ **không** hard-code `TemplateCode` trong API client. Sample lưu code trong Setting:

| Setting key (ví dụ) | Mục đích |
|---------------------|----------|
| `ImportExport.Teacher.ImportTemplateCode` | Code template dùng khi import |
| `ImportExport.Teacher.ExportTemplateCode` | Code template dùng khi export |

Pattern: thêm setting trong `ISettingDefinitionProvider` của host (Sample: `ImportExportSettingDefinition`), rồi module service đọc setting → truyền vào `ImportRequest.TemplateCode` / `ExportRequest.TemplateCode`.

Ops/UI: tạo template Excel trong ImportExport → copy `Code` → gán vào Setting của module.

---

### Bước 6 — Module service gọi ImportExport

**Import** — bắt buộc set `EntityKey`:

```csharp
var templateCode = await RequireTemplateCodeAsync(
    "ImportExport.Teacher.ImportTemplateCode", // hoặc const Setting key
    cancellationToken);

var result = await importService.ImportAsync(new ImportRequest
{
    TemplateCode = templateCode,
    FileStream = fileStream,
    FileName = fileName,
    Phase = ImportExecutionPhase.Import, // hoặc ValidateOnly
    EntityKey = ImportExportEntityKeys.Teacher
}, cancellationToken);
```

**Export** — không cần `EntityKey` (không qua persister). Load data module → `IExportService.ExportAsync` với `TemplateCode` + `Data`.

Tham chiếu: `Sample/Services/StudentService.cs` (`ImportCoreAsync` / `ExportAsync`).

---

### Bước 7 — API / FE của module (tuỳ product)

ImportExport HTTP generic (`ExcelController`) phục vụ template / import theo request body. Module nghiệp vụ thường thêm endpoint riêng (Sample: `StudentController` `POST …/import`, `…/import/validate`, `…/export`) để:

- Resolve `TemplateCode` từ Setting phía server
- Gắn `EntityKey` cố định của module
- Không bắt client biết chi tiết ImportExport

FE: trang upload file gọi API module (Sample: `StudentImportPage` + `students.ts`).

---

### Bước 8 — Tạo / cấu hình Excel template

1. Mở UI ImportExport (template list / form).
2. Chọn **Đối tượng** = key mới (`Teacher`) → auto-fill `fieldName` từ schema.
3. Map cột Excel (`columnName`) ↔ field.
4. Lưu template → ghi nhận `Code`.
5. Gán `Code` vào Setting import/export của module (bước 5).
6. (Import) Upload file thử qua API module; kiểm tra row lỗi / `SavedCount`.

---

## 4. Trách nhiệm: Host module vs ImportExport

| Việc | Ai làm |
|------|--------|
| Wire packages DI / tenant / template store | Host (một lần) |
| ImportRow DTO + registry | Host / module nghiệp vụ |
| Persist rows vào DB domain | Host `IImportModulePersister` |
| Parse Excel, validate mapping/layout engine | ImportExport |
| Schema discovery cho UI template | ImportExport (từ registry) |
| Chọn template code nghiệp vụ | Host Setting + module service |
| CRUD template Excel | ImportExport (+ UI) |

---

## 5. Pitfall thường gặp

| Triệu chứng | Nguyên nhân thường gặp |
|-------------|------------------------|
| `No IImportModulePersister registered for entity 'X'` | Persister chưa register, hoặc `EntityKey` property ≠ key request |
| Entity không hiện trên UI template | Thiếu `ConfigureEntities().Add<…>` hoặc chưa redeploy |
| Import “thành công” nhưng không lưu DB | `Phase = ValidateOnly`, hoặc persister no-op / lỗi bị nuốt vào `Errors` |
| Typo field sau mapping | `fieldName` trong template ≠ key đọc trong persister (case-insensitive helper nên có) |
| Cross-tenant | Ambient tenant chưa set trước import / persist |

---

## 6. Checklist Done

| # | Việc | Done |
|---|------|------|
| 1 | Const `EntityKey` | ☐ |
| 2 | `*ImportRow` + DataAnnotations | ☐ |
| 3 | `ConfigureEntities().Add<TRow>(key, displayName)` | ☐ |
| 4 | `IImportModulePersister` + `AddImportModulePersister(key)` cùng key | ☐ |
| 5 | Setting import/export template code | ☐ |
| 6 | Module service: `ImportRequest.EntityKey` + `TemplateCode` | ☐ |
| 7 | (Tuỳ chọn) Export qua `IExportService` | ☐ |
| 8 | (Tuỳ chọn) API/FE module | ☐ |
| 9 | Tạo template UI + gán Setting + smoke import/export | ☐ |

---

## 7. File Sample nên mở khi copy

| File | Việc |
|------|------|
| `Sample/ImportExport/Constants/ImportExportEntityKeys.cs` | Const key |
| `Sample/ImportExport/Models/StudentImportRow.cs` | DTO schema |
| `Sample/ImportExport/SampleImportExportExtensions.cs` | Registry + DI wire |
| `Sample/ImportExport/Services/StudentImportModulePersister.cs` | Persist |
| `Sample/Settings/ImportExportSettingDefinition.cs` | Template code Setting |
| `Sample/Services/StudentService.cs` | Gọi `IImportService` / `IExportService` |
| `Sample/Controllers/StudentController.cs` | HTTP module |
| `Sample/clients/web/src/pages/StudentImportPage.tsx` | FE upload |
