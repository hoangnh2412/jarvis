# Student Default Import/Export Flow — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:subagent-driven-development` (recommended) or `superpowers:executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Module Student trong Sample có thể import/export ngay sau khi cài đặt (fresh tenant) mà không cần cấu hình thủ công template code trong Setting; vẫn cho phép admin override qua group `ImportExport`.

**Architecture:** Giữ pattern hiện tại (domain API `/students/import|export` không nhận `templateCode` từ client). Bổ sung **mã template mặc định cố định** + **bootstrap template** theo tenant khi gọi import/export lần đầu. Resolve template code theo thứ tự: Setting đã lưu (non-empty) → `DefaultValue` definition → constant mặc định. Engine ImportExport **không đổi contract**.

**Tech Stack:** .NET 9, Platform Setting, Platform.Modules.ImportExport (EF store + MiniExcel), Sample host, React (Sample web — chỉ cập nhật copy/hint nếu cần).

## Global Constraints

- Không đổi contract `ImportRequest` / `ExportRequest` / `IImportService` / `IExportService`.
- Không bắt client gửi `templateCode` trên Student API.
- Template mặc định **tenant-scoped** (cùng rule với `DbTemplateStore` — cần `X-Tenant-Id`).
- Import template phải có file `.xlsx` hợp lệ (header `Name`, `Age`); export template có thể **không có file bytes** (MiniExcel plain export khi `TemplateContent == null`).
- Mapping mặc định khớp `StudentImportRow`: `Name` (required), `Age`.
- Seeder/bootstrap **idempotent** — gọi nhiều lần không tạo duplicate.
- Không seed cross-tenant; chỉ tenant đang active trong request scope.

---

## Hiện trạng (vấn đề)

| Thành phần | Hành vi hiện tại | Hệ quả |
|---|---|---|
| `StudentController` | Chỉ nhận `File` (import) / `fileName` (export) | Đúng — client không gửi templateCode |
| `StudentService.RequireTemplateCodeAsync` | `GetAsync` Setting → empty → **400 BadRequest** | Fresh install fail ngay |
| `ImportExportSettingDefinition` | `DefaultValue = string.Empty` | UI form hiển thị rỗng, runtime không fallback |
| Template store | Không seed template Student | Dù có code cũng fail `TemplateNotFound` |
| `ImportExportSettingReader` | Chỉ đọc DB row, không fallback definition | Khác với `GetFormAsync` (có default) |

---

## Thiết kế mục tiêu

### Template codes mặc định (Sample-owned)

```csharp
// Sample/ImportExport/Constants/StudentImportExportDefaults.cs
public static class StudentImportExportDefaults
{
    public const string ImportTemplateCode = "student-import-default";
    public const string ExportTemplateCode = "student-export-default";
}
```

### Resolve template code

```
1. settingManager.GetAsync(key) → Value non-empty? → dùng
2. definition.DefaultValue non-empty? → dùng
3. StudentImportExportDefaults.* → dùng
```

Admin override: lưu template code khác vào Setting group `ImportExport`.

### Bootstrap templates (lazy, per-tenant)

Trước mỗi import/export Student:

```
EnsureDefaultTemplatesAsync(ct)
  ├─ if !exists(importCode) → create import template (xlsx + mappings)
  └─ if !exists(exportCode) → create export template (mappings only, no file bytes)
```

Gọi từ `StudentService` (scoped, đã có tenant context) — không cần `IHostedService` global.

---

## File map

| Action | Path | Responsibility |
|--------|------|----------------|
| Create | `Sample/ImportExport/Constants/StudentImportExportDefaults.cs` | Mã template mặc định |
| Create | `Sample/ImportExport/Services/StudentDefaultTemplateBootstrapper.cs` | Ensure import/export template tồn tại |
| Create | `Sample/ImportExport/Helpers/StudentTemplateFileFactory.cs` | Tạo bytes `.xlsx` header-only (MiniExcel) |
| Modify | `Sample/Settings/ImportExportSettingDefinition.cs` | `DefaultValue` → codes mặc định; mô tả "để trống = dùng mặc định" |
| Modify | `Sample/Services/ImportExportSettingReader.cs` | `ResolveTemplateCodeAsync` với fallback chain |
| Modify | `Sample/Services/StudentService.cs` | Bootstrap + resolve (bỏ throw khi empty) |
| Modify | `Sample/ImportExport/SampleImportExportExtensions.cs` | DI bootstrapper |
| Create | `UnitTest/Sample/StudentImportExportDefaultsTests.cs` | Test resolve + bootstrap idempotency |
| Modify | `Sample/clients/web/src/pages/StudentImportPage.tsx` | Cập nhật description (optional) |
| Modify | `Sample/clients/web/src/pages/StudentListPage.tsx` | Cập nhật description export (optional) |

---

## Task 1: Constants + Setting defaults

**Files:**
- Create: `Sample/ImportExport/Constants/StudentImportExportDefaults.cs`
- Modify: `Sample/Settings/ImportExportSettingDefinition.cs`

- [ ] **Step 1:** Tạo constants:

```csharp
namespace Sample.ImportExport.Constants;

public static class StudentImportExportDefaults
{
    public const string ImportTemplateCode = "student-import-default";
    public const string ExportTemplateCode = "student-export-default";
}
```

- [ ] **Step 2:** Cập nhật `ImportExportSettingDefinition`:

```csharp
using Sample.ImportExport.Constants;

// StudentImportTemplateCodeKey
setting.DefaultValue = StudentImportExportDefaults.ImportTemplateCode;
setting.Description = "Template code khi import học sinh. Để trống hoặc chưa lưu → dùng mặc định: student-import-default.";

// StudentExportTemplateCodeKey
setting.DefaultValue = StudentImportExportDefaults.ExportTemplateCode;
setting.Description = "Template code khi export học sinh. Để trống hoặc chưa lưu → dùng mặc định: student-export-default.";
```

- [ ] **Step 3:** Build Sample project.

```bash
dotnet build Sample/Sample.csproj
```

Expected: Build succeeded.

---

## Task 2: Template code resolver (fallback chain)

**Files:**
- Modify: `Sample/Services/ImportExportSettingReader.cs`

**Interfaces:**
- Consumes: `ISettingManager`, `ISettingDefinitionRegistry` (hoặc đọc default qua `GetFormAsync` item — chọn cách ít dependency nhất)
- Produces: `ResolveTemplateCodeAsync(string key, string fallbackCode, CancellationToken) → string`

**Khuyến nghị:** Inject thêm fallback từ caller (`StudentService` truyền `StudentImportExportDefaults.*`) để reader không hard-code Student.

- [ ] **Step 1:** Thêm method:

```csharp
public async Task<string> ResolveTemplateCodeAsync(
    string key,
    string fallbackCode,
    CancellationToken cancellationToken = default)
{
    var setting = await settingManager.GetAsync(key, cancellationToken).ConfigureAwait(false);
    var persisted = setting?.Value?.Trim();
    if (!string.IsNullOrWhiteSpace(persisted))
        return persisted;

    // Optional: nếu GetAsync null, thử definition default từ form metadata
    var form = await settingManager.GetFormAsync(ImportExportSettingDefinition.GroupName, cancellationToken)
        .ConfigureAwait(false);
    var item = form.FirstOrDefault(x => x.Key == key);
    var definitionDefault = item?.DefaultValue?.Trim();
    if (!string.IsNullOrWhiteSpace(definitionDefault))
        return definitionDefault;

    ArgumentException.ThrowIfNullOrWhiteSpace(fallbackCode);
    return fallbackCode;
}
```

- [ ] **Step 2:** Giữ `GetTemplateCodeAsync` cho chỗ khác (nếu cần); `StudentService` chuyển sang `ResolveTemplateCodeAsync`.

- [ ] **Step 3:** Build.

---

## Task 3: Xlsx factory cho import template

**Files:**
- Create: `Sample/ImportExport/Helpers/StudentTemplateFileFactory.cs`

- [ ] **Step 1:** Tạo helper dùng MiniExcel (Sample đã reference `Platform.ImportExport.MiniExcel`):

```csharp
using MiniExcelLibs;

namespace Sample.ImportExport.Helpers;

public static class StudentTemplateFileFactory
{
    public static async Task<byte[]> CreateImportHeaderFileAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream();
        var rows = new[] { new Dictionary<string, object> { ["Name"] = "", ["Age"] = "" } };
        await MiniExcel.SaveAsAsync(stream, rows, cancellationToken: cancellationToken);
        return stream.ToArray();
    }
}
```

- [ ] **Step 2:** Build — fix package/usings nếu cần.

---

## Task 4: Default template bootstrapper

**Files:**
- Create: `Sample/ImportExport/Services/StudentDefaultTemplateBootstrapper.cs`
- Modify: `Sample/ImportExport/SampleImportExportExtensions.cs`

**Interfaces:**
- Consumes: `ITemplateService` (hoặc `ITemplateStore` nếu cần export template không có content — xem Step 2 b)
- Produces: `Task EnsureAsync(CancellationToken)`

- [ ] **Step 1:** Implement bootstrapper:

```csharp
using Platform.Modules.ImportExport.Application.Templates;
using Platform.Modules.ImportExport.Application.Templates.Models;
using Platform.Modules.ImportExport.Domain.Models;
using Sample.ImportExport.Constants;
using Sample.ImportExport.Helpers;

namespace Sample.ImportExport.Services;

public sealed class StudentDefaultTemplateBootstrapper(ITemplateService templateService)
{
    private static readonly ColumnMapping[] DefaultMappings =
    [
        new("Name", "Name", Required: true, MaxLength: 200),
        new("Age", "Age", Required: true, Min: 1, Max: 150)
    ];

    public async Task EnsureAsync(CancellationToken cancellationToken = default)
    {
        await EnsureImportTemplateAsync(cancellationToken).ConfigureAwait(false);
        await EnsureExportTemplateAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureImportTemplateAsync(CancellationToken cancellationToken)
    {
        var code = StudentImportExportDefaults.ImportTemplateCode;
        if (await ExistsAsync(code, cancellationToken)) return;

        var content = await StudentTemplateFileFactory.CreateImportHeaderFileAsync(cancellationToken);
        await templateService.CreateAsync(new CreateTemplateRequest
        {
            Code = code,
            Name = "Student Import (default)",
            FileName = "student-import-default.xlsx",
            Content = content,
            Mappings = DefaultMappings,
            HeaderRow = 1,
            StartRow = 2,
            SheetName = "Sheet1"
        }, cancellationToken);
    }

    private async Task EnsureExportTemplateAsync(CancellationToken cancellationToken)
    {
        var code = StudentImportExportDefaults.ExportTemplateCode;
        if (await ExistsAsync(code, cancellationToken)) return;

        // Export runtime chấp nhận TemplateContent null → plain SaveAs.
        // ITemplateService.CreateAsync yêu cầu Content — dùng header-only xlsx tối thiểu.
        var content = await StudentTemplateFileFactory.CreateImportHeaderFileAsync(cancellationToken);
        await templateService.CreateAsync(new CreateTemplateRequest
        {
            Code = code,
            Name = "Student Export (default)",
            FileName = "student-export-default.xlsx",
            Content = content,
            Mappings = DefaultMappings,
            HeaderRow = 1,
            StartRow = 2,
            SheetName = "Sheet1"
        }, cancellationToken);
    }

    private async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            await templateService.GetForExecutionAsync(code, cancellationToken);
            return true;
        }
        catch (Platform.Modules.ImportExport.Domain.Exceptions.TemplateNotFoundException)
        {
            return false;
        }
    }
}
```

- [ ] **Step 2:** Đăng ký DI:

```csharp
builder.Services.AddScoped<StudentDefaultTemplateBootstrapper>();
```

- [ ] **Step 3:** Manual smoke (Postman/curl) sau khi Task 5 xong.

---

## Task 5: Wire StudentService

**Files:**
- Modify: `Sample/Services/StudentService.cs`

- [ ] **Step 1:** Inject `StudentDefaultTemplateBootstrapper`.

- [ ] **Step 2:** Thay `RequireTemplateCodeAsync` bằng:

```csharp
public async Task<ImportApiResponse> ImportAsync(...)
{
    await templateBootstrapper.EnsureAsync(cancellationToken);

    var templateCode = await importExportSettingReader.ResolveTemplateCodeAsync(
        ImportExportSettingDefinition.StudentImportTemplateCodeKey,
        StudentImportExportDefaults.ImportTemplateCode,
        cancellationToken);

    // ... existing importService.ImportAsync
}

public async Task<FileDownloadResponse> ExportAsync(...)
{
    await templateBootstrapper.EnsureAsync(cancellationToken);

    var templateCode = await importExportSettingReader.ResolveTemplateCodeAsync(
        ImportExportSettingDefinition.StudentExportTemplateCodeKey,
        StudentImportExportDefaults.ExportTemplateCode,
        cancellationToken);

    // ... existing exportService.ExportAsync
}
```

- [ ] **Step 3:** Xóa private `RequireTemplateCodeAsync` nếu không còn dùng.

- [ ] **Step 4:** Cập nhật XML doc `StudentController` — "template code: Setting override hoặc mặc định host".

---

## Task 6: Tests

**Files:**
- Create: `UnitTest/Sample/ImportExportSettingReaderTests.cs`
- Create: `UnitTest/Sample/StudentDefaultTemplateBootstrapperTests.cs` (integration nhẹ với in-memory store nếu có harness; nếu không → test resolver only + manual checklist)

- [ ] **Step 1:** Test resolver:

```csharp
[Fact]
public async Task ResolveTemplateCode_PersistedValue_WinsOverFallback()
{
    // mock ISettingManager: GetAsync returns "custom-code"
    // assert ResolveTemplateCodeAsync returns "custom-code"
}

[Fact]
public async Task ResolveTemplateCode_EmptyPersisted_UsesFallback()
{
    // GetAsync returns null or empty Value
    // assert returns StudentImportExportDefaults.ImportTemplateCode
}
```

- [ ] **Step 2:** Test bootstrap idempotency (optional integration):

```csharp
[Fact]
public async Task EnsureAsync_CalledTwice_DoesNotThrow()
{
    // arrange tenant + in-memory template store
    // act EnsureAsync x2
    // assert single template per code
}
```

- [ ] **Step 3:** Run tests:

```bash
dotnet test UnitTest/UnitTest.csproj --filter "FullyQualifiedName~ImportExport"
```

---

## Task 7: Frontend copy (optional, nhỏ)

**Files:**
- Modify: `Sample/clients/web/src/pages/StudentImportPage.tsx`
- Modify: `Sample/clients/web/src/pages/StudentListPage.tsx`

- [ ] **Step 1:** Cập nhật description:

Import page:
> "Upload file Excel (cột Name, Age). Hệ thống dùng template mặc định; admin có thể override trong Cài đặt → ImportExport."

List page export button tooltip/description tương tự.

- [ ] **Step 2:** (Optional) Trong `SettingPage` / group `ImportExport`, thêm hint placeholder default codes — không bắt buộc nếu `GetFormAsync` đã trả `DefaultValue`.

---

## Manual test plan

- [ ] Fresh tenant + header `X-Tenant-Id` hợp lệ.
- [ ] **Không** cấu hình group `ImportExport`.
- [ ] `POST /api/v1/students/import` với file `.xlsx` (Name, Age) → 200, rows saved.
- [ ] `POST /api/v1/students/export` → file `.xlsx` tải về có data.
- [ ] Vào Setting → đổi import template code sang template custom → import dùng template custom.
- [ ] Restart app → templates mặc định vẫn tồn tại (không duplicate).
- [ ] Tenant B (chưa bootstrap) → gọi import → bootstrap riêng cho tenant B.

---

## Out of scope (YAGNI)

- User module (chỉ áp dụng pattern tương tự khi cần).
- Thay đổi `ExcelController` generic API.
- Auto-migrate Setting row cũ đang empty (fallback đã xử lý).
- Seed template qua SQL migration.

---

## Rủi ro & mitigation

| Rủi ro | Mitigation |
|---|---|
| Thiếu `X-Tenant-Id` | Giữ lỗi hiện tại từ template store (`TenantRequired`) |
| Race tạo template đồng thời | `CreateAsync` unique index `(TenantId, Code)` → catch conflict, re-check exists |
| Admin xóa template mặc định | `EnsureAsync` tạo lại nếu not found |
| Setting lưu code trỏ template không tồn tại | Giữ lỗi `TemplateNotFound` — đúng nghiệp vụ |

---

## Execution order

1. Task 1 (constants + setting defaults)
2. Task 2 (resolver)
3. Task 3 (xlsx factory)
4. Task 4 (bootstrapper + DI)
5. Task 5 (StudentService wire-up)
6. Task 6 (tests)
7. Task 7 (frontend copy)
