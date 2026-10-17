# Platform ImportExport

Excel import/export module (chuẩn hóa từ `Sample.ExcelEngine`).

**Version NuGet:** `1.2.0` (core + satellite cùng version)

## Packages

| Package | Role |
|---------|------|
| `Platform.Modules.ImportExport` | Core: Domain + Application + DI |
| `Platform.Modules.ImportExport.API` | HTTP controllers opt-in (`UseHttpApi()`) — PackageId; folder project: `.Api` |
| `Platform.ImportExport.MiniExcel` | MiniExcel import/export processors |
| `Platform.Modules.ImportExport.EntityFramework` | `DbTemplateStore` + optional reference entity + `ConfigureExcelTemplate()` |

Demo/reference host: **`Sample`** (`SampleExcelTemplate` host-owned entity).

OpenCode skill: [`.opencode/skills/importexport-dotnet`](../../.opencode/skills/importexport-dotnet/README.md)

## Clean Architecture — `Platform.Modules.ImportExport`

```
Platform.Modules.ImportExport/
├── Domain/                         # Lõi nghiệp vụ, không phụ thuộc framework
│   ├── Entities/                   # IExcelTemplateEntity, extensions
│   ├── Enums/                      # ExecutionMode, InvalidRowBehavior
│   ├── Constants/                  # ImportExportErrorCode, EngineMessages, …
│   ├── Models/                     # ColumnMapping, TemplateConfiguration, SheetTemplate
│   └── Exceptions/                 # ExcelEngineException, TemplateNotFound, …
│
├── Application/                    # Use cases + ports (interfaces)
│   ├── Templates/                  # ITemplateService, ITemplateStore, TemplateLayoutParser
│   │   └── Models/                 # TemplateSummary, CreateTemplateRequest, …
│   ├── Import/                     # IImportService, ImportService, IImportProcessor, …
│   │   └── Models/                 # ImportRequest, ImportResult, ImportWork, …
│   ├── Export/                     # IExportService, ExportService, IExportProcessor
│   │   └── Models/                 # ExportRequest, ExportResult, ExportWork
│   ├── Schemas/                    # Entity registry + schema discovery
│   │   └── Models/                 # ImportExportEntitySummary, RegisteredImportExportEntity
│   └── Common/
│       ├── Execution/              # IExecutionExecutor, SyncExecutionExecutor
│       ├── Validation/             # MappingValidator, ColumnMappingJsonParser, IRowValidator, …
│       └── Options/                # ExcelEngineOptions (SectionName: ExcelEngine)
│
├── DependencyInjection/            # AddCoreImport() / AddCoreExport(), fluent builder
└── doc/                            # SAD, ADR
```

**Infrastructure** (implement ports) nằm ở package riêng — **không reference chéo**:
- `Platform.Modules.ImportExport.EntityFramework` → `ITemplateStore`, `ITemplateService`
- `Platform.ImportExport.MiniExcel` → `IImportProcessor`, `IExportProcessor`
- `Platform.Modules.ImportExport.API` → HTTP layer (chỉ reference core)

## Consumer DI

```csharp
builder.AddCoreImport()
    .ConfigureEntities(o => o.Add<StudentImportRow>("Student", "Học sinh"))
    .UseMiniExcel()
    .UseEntityFramework<SampleExcelTemplate, IAppUnitOfWork, CurrentTenantInfo>()
    .UseHttpApi();
builder.AddCoreExport();

// Hoặc cả hai: builder.AddCoreImportExport()...
```

`ExcelEngine` section bind tự động trong `AddCoreImport` / `AddCoreExport`. Override: `services.Configure<ExcelEngineOptions>(...)`.

## Run demo

```powershell
dotnet run --project Sample
```

## Docs

- [How-to: thêm module nghiệp vụ dùng ImportExport](./docs/how-to-add-module-importexport.md)
- [`Platform.Modules.ImportExport/doc/SAD.md`](./Platform.Modules.ImportExport/doc/SAD.md)
- [`Platform.Modules.ImportExport/doc/ADR-Naming-ImportExport.md`](./Platform.Modules.ImportExport/doc/ADR-Naming-ImportExport.md)
- `myDoc/Platform/ImportExport/SAD/Sad.md` — SAD canonical (v1.2.0)
- `myDoc/Platform/ImportExport/ADR/` — ADR index
