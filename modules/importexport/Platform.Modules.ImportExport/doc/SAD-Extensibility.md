# Software Architecture Document (SAD) — Extensibility (post-demo)

> Canonical **consumer architecture** after GĐ1 extensibility.  
> Full historical SAD: [`SAD-ExcelEngine.md`](SAD-ExcelEngine.md).  
> Copy-paste samples: inline below. Runnable demo: `Sample/ImportExport/`.

## Purpose

`Platform.Modules.ImportExport` is a **reusable library**. Other projects reference the NuGet package and **implement ports** for:

| # | Need (leader checklist) | Port |
|---|-------------------------|------|
| 1 | Customize template / load | `ITemplateStore` |
| 2 | Save DB (implement / delegate) | `IImportDataPersister` |
| 3 | Validate request (factory-style override) | `IImportRequestValidator` |
| 4 | Validate rows | `IRowValidator` |
| 5 | Processor (Excel I/O) | `IImportProcessor` / `IExportProcessor` |
| — | Unique vs existing DB (optional) | `IUniqueValueChecker` |
| — | Template schema discovery (entity fields) | `IImportExportEntityRegistry` / `IImportExportSchemaService` |

Engine owns Excel streaming (MiniExcel), mapping rules, and orchestration — **not** business schema at import runtime. Schema discovery for template UI uses a **code-first registry** (`key → ImportRow Type`) plus reflection on registered DTOs only.

## Ports (Contracts)

```text
Consumer app
    │
    ├─ ITemplateStore            load/save template metadata + file bytes/URI
    ├─ IImportDataPersister      save valid rows to consumer DB
    ├─ IUniqueValueChecker       optional uniqueness vs existing data
    ├─ IImportRequestValidator   request-level checks (overridable)
    ├─ IRowValidator             cell rules: required/unique/regex/maxLength/min/max
    ├─ IImportProcessor          Excel → rows (default MiniExcel)
    └─ IExportProcessor          rows → Excel
```

Default adapters live in Infrastructure (`DbTemplateStore`, `FolderTemplateStore`, `DefaultRowValidator`, `DefaultImportDataPersister`, `DefaultUniqueValueChecker`, …).  
`ITemplateService` là façade CRUD trên `ITemplateStore`.

## DI

Entry point: **`AddPlatformImportExport()`**. Đăng ký persister/store **trước** nếu cần ghi đè default:

```csharp
// 1) Production — lưu row + template (trước AddPlatformImportExport)
services.AddScoped<IImportDataPersister, MyOrderPersister>();
services.AddScoped<ITemplateStore, MyBlobTemplateStore>(); // Azure / S3 / custom

// 2) Engine
services.AddPlatformImportExport();

// 3) Hoặc dùng adapter có sẵn thay custom ITemplateStore:
// services.AddPlatformImportExportEntityFramework(configuration);   // Postgres + disk
// services.AddLlaExcelFolderTemplates();          // folder JSON + xlsx
```

Built-in template adapters:

| Method | Store |
|--------|--------|
| `AddPlatformImportExportEntityFramework(config)` | Postgres `MappingsJson` + files dưới `TemplateFilesRoot` |
| `AddLlaExcelFolderTemplates()` | JSON + xlsx trên disk |
| Custom `ITemplateStore` | Azure / AWS / MinIO / … |

---

## Sample: `IImportDataPersister` (Save DB) — copy this first

Import gọi `SaveAsync` **sau mỗi sheet** khi có row hợp lệ. Engine không biết bảng nghiệp vụ.

```csharp
using Platform.Modules.ImportExport.Contracts.Interfaces;
using Platform.Modules.ImportExport.Contracts.Models;
using Platform.Modules.ImportExport.Contracts.Responses;

public sealed class MyOrderPersister(MyDbContext db) : IImportDataPersister
{
    public async Task<ImportPersistResult> SaveAsync(
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        TemplateDefinition template,
        CancellationToken cancellationToken = default)
    {
        // template.Code / template.SheetName / template.Mappings — dùng để biết sheet nào
        var entities = new List<OrderEntity>(rows.Count);
        var errors = new List<ImportRowError>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                entities.Add(new OrderEntity
                {
                    Code = Convert.ToString(row.GetValueOrDefault("OrderCode")) ?? string.Empty,
                    Amount = Convert.ToDecimal(row.GetValueOrDefault("Amount") ?? 0m),
                    TemplateCode = template.Code,
                    SheetName = template.SheetName
                });
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError(
                    RowNumber: i + 1, // hoặc map lại excel row nếu bạn giữ metadata
                    Message: ex.Message,
                    ErrorCode: ImportErrorCodes.PersistFailed,
                    SheetName: template.SheetName));
            }
        }

        if (entities.Count > 0)
        {
            db.Orders.AddRange(entities);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new ImportPersistResult
        {
            SavedCount = entities.Count,
            Errors = errors
        };
    }
}
```

Đăng ký:

```csharp
services.AddScoped<IImportDataPersister, MyOrderPersister>();
services.AddPlatformImportExport();
```

---

## Sample: `ITemplateStore` (customize load/save template)

```csharp
public sealed class MyBlobTemplateStore(/* BlobClient, DbContext, ... */) : ITemplateStore
{
    public Task<TemplateDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => /* load metadata + download file bytes or set Location */;

    public Task SaveAsync(TemplateDefinition template, CancellationToken cancellationToken = default)
        => /* upsert metadata + upload Content to blob; set Location */;

    public Task DeleteAsync(string code, CancellationToken cancellationToken = default)
        => /* soft/hard delete */;

    public Task<IReadOnlyList<TemplateDefinition>> ListAsync(CancellationToken cancellationToken = default)
        => /* list */;
}
```

Không gọi `AddPlatformImportExportEntityFramework` nếu đã register store riêng.

---

## Sample: `IImportRequestValidator` (factory / request-level)

Gọi **một lần** trước khi đọc Excel. Override để thêm rule theo project (max size, whitelist template code, …).

```csharp
public sealed class MyImportRequestValidator(ImportRequestValidator inner) : IImportRequestValidator
{
    public void Validate(ImportWork work)
    {
        inner.Validate(work); // giữ rule mặc định của engine

        if (work.TemplateCode is "LEGACY" && work.Template.StartRow < 3)
            throw new InvalidTemplateException("LEGACY templates must start at row 3+.");
    }
}
```

> Nếu không inject `ImportRequestValidator` concrete, copy rule cần thiết hoặc gọi helper riêng.

---

## Sample: `IRowValidator` (per-row)

Có thể wrap `DefaultRowValidator` rồi thêm rule domain, hoặc thay toàn bộ.

```csharp
public sealed class MyRowValidator(DefaultRowValidator inner) : IRowValidator
{
    public async Task<IReadOnlyList<ImportRowError>> ValidateAsync(
        IReadOnlyDictionary<string, object?> row,
        RowValidationContext context,
        CancellationToken cancellationToken = default)
    {
        var errors = (await inner.ValidateAsync(row, context, cancellationToken)).ToList();

        if (row.TryGetValue("Country", out var country)
            && country is string c
            && c is not ("VN" or "US"))
        {
            errors.Add(new ImportRowError(
                context.ExcelRowNumber,
                "Country must be VN or US.",
                ColumnName: "Country",
                FieldName: "Country",
                ErrorCode: ImportErrorCodes.InvalidFormat,
                SheetName: context.SheetName));
        }

        return errors;
    }
}
```

---

## Sample: `IImportProcessor` (thay pipeline — hiếm)

Chỉ khi không dùng MiniExcel / cần format khác. Hầu hết team **không** cần override.

```csharp
public sealed class MyImportProcessor(/* deps */) : IImportProcessor
{
    public Task<ImportResult> ImportAsync(ImportWork work, CancellationToken cancellationToken)
        => /* custom read → validate → call IImportDataPersister → ImportResult.From(...) */;
}
```

---

## Sample: `IUniqueValueChecker` (trùng với DB sẵn có)

```csharp
public sealed class MyDbUniqueChecker(MyDbContext db) : IUniqueValueChecker
{
    public async Task<bool> ExistsAsync(
        string fieldName,
        object? value,
        CancellationToken cancellationToken = default)
    {
        if (fieldName.Equals("Email", StringComparison.OrdinalIgnoreCase) && value is string email)
            return await db.Customers.AnyAsync(x => x.Email == email, cancellationToken);

        return false;
    }
}
```

---

## Template schema (default Db adapter)

Table `excel_templates` only:

- `MappingsJson` (jsonb) — chứa mappings + sheet layout (`sheetName` / `sheets[]`)
- `Location` (path/URI to file)
- **No** `Kind`, **no** `Content` bytea, **no** `excel_template_mappings`, **no** `SheetName` column

## ColumnMapping rules

`required`, `unique`, `regex`, `maxLength`, `min`, `max`

Import: `InvalidRowBehavior.Continue` (default) keeps valid rows; errors under `sheets[].errorSummary` + logs.

## Entity schema registry (template UI)

Consumer registers **ImportRow DTOs** for template field discovery (reflection at design time only — import runtime still returns `Dictionary`).

```csharp
services.AddPlatformImportExport();
services.ConfigureImportExportEntities(o =>
{
    o.Add<StudentImportRow>("Student", "Học sinh");
});
```

HTTP (module API or host mirror):

```http
GET /api/v1/importexport/entities
GET /api/v1/importexport/entities/{key}/fields
```

Settings group **`ImportExportEntities`** (host) documents catalog keys (read-only). Runtime template codes remain in group **`ImportExport`**.

## Import response (multi-sheet)

```json
{
  "sheets": [
    {
      "sheetName": "Orders",
      "rows": [ { "OrderCode": "A1", "Amount": 10 } ],
      "errorSummary": [
        {
          "message": "...",
          "rowNumbers": [3],
          "columnName": "Amount",
          "fieldName": "Amount",
          "errorCode": "InvalidFormat",
          "count": 1
        }
      ],
      "processedRowCount": 5,
      "failedRowCount": 1
    }
  ],
  "processedRowCount": 5,
  "failedRowCount": 1,
  "savedCount": 4
}
```

## Package

```xml
<PackageReference Include="Platform.Modules.ImportExport" Version="1.1.0" />
<PackageReference Include="Platform.Modules.ImportExport.EntityFramework" Version="1.1.0" />
<PackageReference Include="Platform.Modules.ImportExport.API" Version="1.1.0" />
<PackageReference Include="Platform.ImportExport.MiniExcel" Version="1.1.0" />
```

---
