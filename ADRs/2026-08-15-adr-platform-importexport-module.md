# ADR — Module ImportExport — Module Atomic & ranh giới đã chốt (GĐ1)

> **Trạng thái:** 🟢 Accepted + Implemented  
> **Ngày:** 2026-08-15 · Accept: 2026-08-05 → 2026-08-11 · Implement: 2026-08-07 → 2026-08-11  
> **Loại:** Module Atomic / package boundary / persistence / multitenancy / API / testing  
> **Liên quan:** *(self-contained — không phụ thuộc ADR/SAD bên ngoài)*  
> **Phạm vi:** Naming & packages; host-owned template persistence; multi-tenant isolation; entity registry & schema API; unit test trong `UnitTest` (processor + store/schema); core validation/layout boundary.  
> **Ngoài phạm vi:** FE product đầy đủ; WebApplicationFactory / E2E product; shared cross-tenant templates; đổi import pipeline runtime (map sang entity EF); dual-package legacy `Sample.ExcelEngine`; project test riêng dưới `modules/importexport`.  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm

---

## 1. Bối cảnh

Module import/export Excel (GĐ1) từng tồn tại dưới tên `Sample.ExcelEngine.*` trong `modules/ImportExportModule/`, với meta-package 4 layer và ConsumerDemo tách khỏi monorepo. Cần chuẩn hóa theo Module Atomic của Platform, rồi khóa các ranh giới persistence, tenant, schema UI, validation, và chiến lược test.

| Thành phần | Trước chuẩn hóa |
|------------|-----------------|
| Naming | `Sample.ExcelEngine.*` — lệch Platform |
| Persistence | Module sở hữu `ExcelEngineDbContext` + migrations + concrete entity |
| Tenant | Rủi ro đọc HTTP header trong package; template/file dễ lẫn tenant |
| Template UI | User gõ `fieldName` tay hoặc `Type.GetType` từ Settings |
| Validation/layout | `MappingValidator` / parser nằm MiniExcel hoặc EF → satellite coupling |
| Test | Phụ thuộc HTTP/EF hoặc fixture Excel trên disk |

Tài liệu này **tổng hợp** các quyết định đã Accept + Implement thành một ADR chính, đủ để đọc độc lập.

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Prefix `Sample.ExcelEngine` + layer packages cứng | Consumer khó align Platform; khó tách adapter MiniExcel / EF |
| P2 | Không fluent DI builder | Host wiring dài, dễ thiếu adapter |
| P3 | Module sở hữu schema DB template | Consumer không control migration/catalog; khó cùng DbContext domain |
| P4 | Rủi ro đưa `IExcelTemplateEntity` vào `Platform.DDD.Domain` | Ô nhiễm DDD lõi |
| P5 | Module resolve tenant từ HTTP header | Không reuse worker/test; coupling ASP.NET |
| P6 | Template/`Code` global; file chung root | Cross-tenant leak / ghi đè |
| P7 | `fieldName` tay hoặc `Type.GetType` / reflect EF entity | Typo; không whitelist; lộ navigation nội bộ |
| P8 | Test qua HTTP/EF hoặc golden Excel disk | Chậm, flake, brittle |
| P9 | Validation/layout trong MiniExcel hoặc EF | Api/EF kéo phụ thuộc sai; vi phạm satellite isolation |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Tên module = `ImportExport` (không dùng `ExcelEngine` làm tên module) |
| C2 | Adapter Excel → `frameworks/Platform.ImportExport.MiniExcel`; target `net9.0`; giữ hành vi GĐ1 |
| C3 | Breaking rename — không dual-package legacy |
| C4 | `IExcelTemplateEntity` sống trong module ImportExport — **không** thêm vào `Platform.DDD.Domain` |
| C5 | Data access qua `IUnitOfWork` + `IRepository<TEntity>`; gỡ DbContext/migrations khỏi package EF module |
| C6 | ImportExport **không** reference `HeaderTenantIdResolver` / factory HTTP tenant |
| C7 | Unique `(TenantId, Code)` trên `excel_templates`; **không** template shared (`TenantId = Empty`) |
| C8 | File path: `{TemplateFilesRoot}/{tenantId}/{code}/{file}` |
| C9 | Module **không** reference type nghiệp vụ consumer; reflect trên Import DTO (`*ImportRow`), không EF entity |
| C10 | Unknown entity key → 404; **không** đổi `ColumnMapping` / `IImportProcessor` / `IImportDataPersister` vì schema API |
| C11 | Unit test ImportExport sống trong project `UnitTest` (`UnitTest/ImportExport/`) — **không** project test riêng dưới `modules/importexport` |
| C12 | Processor cases: SUT = processor + `ImportWork`/`ExportWork`; workbook in-memory; default persister no-op; được bổ sung store/schema/template tests cùng folder |
| C13 | Satellite isolation: MiniExcel ⊥ EntityFramework ⊥ Api — cả ba chỉ phụ thuộc Core |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | Giữ `Sample.ExcelEngine` + module-owned DbContext + resolve header trong package | Ít churn ngắn hạn | Coupling; lệch Platform forever |
| O2 | Module Atomic + host-owned entity + ambient tenant + code-first registry + unit test trong `UnitTest` + validation trong Core | Rõ boundary; giống Setting; reusable | Breaking; Host boilerplate hơn |
| O3 | Contract template trong DDD.Domain / Settings-only catalog / OpenAPI schema tĩnh / integration-only test | “Chuẩn” bề mặt hoặc đơn giản | Ô nhiễm DDD; drift; chậm/flake |

**Chọn O2** cho toàn bộ phạm vi GĐ1 dưới đây.

---

## 5. Quyết định

Chúng ta chấp nhận ImportExport là **Module Atomic** trên Platform, với host-owned persistence, ambient multi-tenant, entity registry cho authoring UI, validation/layout trong Core, và unit test tập trung trong `UnitTest`.

### 5.1 Naming & package layout

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Core | `Platform.Modules.ImportExport` — Domain + Application + DI |
| D2 | MiniExcel adapter | `Platform.ImportExport.MiniExcel` (`frameworks/`) |
| D3 | EF adapter | `Platform.Modules.ImportExport.EntityFramework` |
| D4 | HTTP opt-in | PackageId `Platform.Modules.ImportExport.API` + `UseHttpApi()` — thư mục project: `.Api` |
| D5 | Tests | Project `UnitTest` — folder `UnitTest/ImportExport/` (namespace `UnitTest.ImportExport.*`); **không** `Platform.Modules.ImportExport.Tests` |
| D6 | DI entry | `AddCoreImportExport()` (hoặc `AddCoreImport` / `AddCoreExport`) → fluent `.UseMiniExcel()`, `.UseEntityFramework<>()`, `.UseHttpApi()`, `.ConfigureEntities(...)` |
| D7 | Demo host | `Sample` trong monorepo — không Host/ConsumerDemo riêng trong `modules/` |
| D8 | Folder | `modules/importexport/`; gỡ `modules/ImportExportModule` |

```text
platform/
├── frameworks/Platform.ImportExport.MiniExcel/
├── modules/importexport/
│   ├── Platform.Modules.ImportExport/
│   ├── Platform.Modules.ImportExport.EntityFramework/
│   └── Platform.Modules.ImportExport.Api/
├── UnitTest/
│   └── ImportExport/          ← tests module (xUnit)
└── Sample/                    ← reference host
```

**Không có:** meta-package kéo tất cả; project `Application` tách riêng; standalone Host trong `modules/`; project test riêng dưới `modules/importexport`.

### 5.2 Host-owned Excel template persistence

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D9 | Contract | `IExcelTemplateEntity` trong `Platform.Modules.ImportExport.Domain` |
| D10 | Generic store | `DbTemplateStore<TEntity, TUnitOfWork>` trong EntityFramework package |
| D11 | Host entity | Product implement `IExcelTemplateEntity` (Sample: `SampleExcelTemplate`) |
| D12 | Migration | Thuộc host `DbContext` (Sample: `TenantDbContext`) |
| D13 | REMOVED | `ExcelEngineDbContext` và migrations trong package EF module |
| D14 | Reference entity (optional) | Package EF có thể ship `Entities.ExcelTemplate` + `ConfigureExcelTemplate()` cho quick start — production dùng type host riêng |
| D15 | File bytes | Lưu disk qua `Location` + `TemplateFilesRoot` — không cột `Content` bytea |

```text
Host (Sample)
  → entity : IExcelTemplateEntity
  → DbContext + UoW + Migrations
EntityFramework package
  → DbTemplateStore<TEntity, TUnitOfWork>
  → ImportExport core (ports)
```

### 5.3 Multi-tenant isolation

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D16 | Ambient tenant | `DbTemplateStore` lấy tenant từ `IStorageContext.TenantId` |
| D17 | Create | `entity.TenantId = ambient` khi insert |
| D18 | Query | Filter theo tenant hiện tại (EF global filter trên host entity khi dùng Platform multitenancy) |
| D19 | File I/O | Path có `tenantId` theo C8 |
| D20 | Host demo | Sample/Api **có thể** dùng header resolver — thuộc host, không phải contract package |
| D21 | Missing tenant | Throw khi ambient thiếu (fail-fast) |

```text
[Host] resolve tenant → SetTenantId / BeginScope
         ↓
[Module] DbTemplateStore.RequireTenantIdAsync()
         → filter + path + assign TenantId on create
```

### 5.4 Entity registry & schema discovery (template UI)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D22 | Registry | `ImportExportEntityRegistryOptions` + `IImportExportEntityRegistry` singleton |
| D23 | Schema service | `IImportExportSchemaService` — reflection + DataAnnotations trên Import DTO |
| D24 | DI | `ConfigureEntities(o => o.Add<TRow>(key, displayName))` trên builder |
| D25 | HTTP | `GET /api/v1/importexport/entities`, `GET …/entities/{key}/fields` |
| D26 | Filter properties | Bỏ `Id`, `TenantId`; public instance readable only |
| D27 | Import pipeline | **Không đổi** — schema chỉ phục vụ authoring template; runtime vẫn `Dictionary<string, object?>` |

```text
Host register StudentImportRow
  → UI GET /entities → ["Student"]
  → UI GET /entities/Student/fields → field list
  → User map columnName ↔ fieldName
  → Import dùng TemplateCode + mappings (runtime dictionary)
```

Contracts chính: `ImportExportEntitySummary`, `ImportExportFieldSchema`, `RegisteredImportExportEntity`.

### 5.5 Core validation & layout boundary

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D28 | Mapping validation | `MappingValidator`, `ColumnMappingJsonParser` → Core `Application.Common.Validation` |
| D29 | Layout parser | `TemplateLayoutParser` (+ `TemplateMappingDocument`) → Core `Application.Templates` |
| D30 | Gỡ coupling | Không project reference EF → MiniExcel |
| D31 | Api | Chỉ reference Core — không EF / MiniExcel |
| D32 | Options | `ExcelEngineOptions.SectionName = "ExcelEngine"` + bind trong `AddCoreImport` / `AddCoreExport` / `AddCoreImportExport` |

```text
Core ← MiniExcel
Core ← EntityFramework
Core ← Api
(MiniExcel ⊥ EntityFramework ⊥ Api)
```

MiniExcel vẫn giữ validator processor-specific (`ImportRequestValidator`, `DefaultRowValidator`, …).

### 5.6 Unit test strategy

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D33 | Project | `UnitTest` (xUnit, net9) — folder `UnitTest/ImportExport/` |
| D34 | References | `UnitTest.csproj` reference MiniExcel + EntityFramework + Api (+ Sample khi cần host types); processor cases dùng MiniExcel; store/tenant dùng EF InMemory helpers |
| D35 | Helpers | `ProcessorTestFactory`, `ExcelWorkbookFactory`, `CustomerTestData`, `ImportWorkFactory`, `ExportWorkFactory` (+ `FakeExcelTemplate`, `InMemoryTemplateStore`, `StubCurrentTenant`, …) |
| D36 | Layout test | `Import/`, `Export/`, `Validation/`, `Infrastructure/`, `Schemas/`, `Templates/`, `Helpers/` |
| D37 | Naming case | `CaseN_<Behavior>` trong `ImportCaseTests` / `ExportCaseTests`; assert hành vi engine thật (note legacy chỉ gợi ý) |
| D38 | Export RowCount | `ExportCaseTests.Case7_Export_RowCountLimitsToFirstNRecords` — giới hạn N bản ghi đầu khi template có `RowCount` |

Default ports trong processor test: `ImportRequestValidator`, `DefaultRowValidator`, `DefaultUniqueValueChecker`, `DefaultImportDataPersister` (no-op). Timeout soft = 0 trong test options.

Catalogue (khoảng): Import cases ~12; Export cases ~11; Validation / Infrastructure / Schemas / Templates / persister → **~80+** tests dưới `UnitTest/ImportExport`.

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Align Module Atomic / Setting; adapter thay thế được; cùng DB/UoW host; module chạy worker/test nếu host set ambient; UI mapping type-safe; CI nhanh; Api/folder-only không kéo MiniExcel qua EF |
| Xấu / chi phí | Breaking rename; Host boilerplate entity + migrate + register ImportRow + pipeline tenant; E2E/WebApplicationFactory product vẫn ngoài phạm vi unit suite |
| Trung lập | Hành vi import/export GĐ1 giữ nguyên; `MappingsJson` jsonb + soft-delete giữ nguyên; Settings group catalog entity (nếu có) song song UI, tách runtime template; test module nằm chung `UnitTest` với các package khác |

---

## 7. Confirm

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D38 (toàn bộ O2)? | OK |
| Q2 | Demo host = Sample? | OK |
| Q3 | Không đưa `IExcelTemplateEntity` vào DDD.Domain? | OK — C4 |
| Q4 | Không shared template / không resolve header trong package? | OK — C6, C7 |
| Q5 | Schema API không đổi import pipeline runtime? | OK — C10, D27 |
| Q6 | Unit test trong `UnitTest/ImportExport` (processor + store/schema)? | OK — C11, C12 |

Sau khi chốt từng phần (2026-08-05 → 2026-08-11): trạng thái tổng hợp → 🟢 Accepted + Implemented.

---

## 8. Test cases

### 8.1 Smoke *(bắt buộc)*

| # | Case | Expect | Nơi (file / filter) | Trạng thái |
|---|------|--------|---------------------|------------|
| T1 | Sample compose DI ImportExport + gọi luồng template/import tối thiểu | Không throw; resolve được service chính | Sample host | 🟢 |
| T2 | `dotnet test UnitTest` filter ImportExport | Suite xanh | `UnitTest/ImportExport/**` | 🟢 |

### 8.2 Regression — test cũ cần giữ xanh

| # | Case | Expect | Nơi (test hiện có) | Trạng thái |
|---|------|--------|--------------------|------------|
| T3 | Import processor cases (~12) | Hành vi GĐ1 giữ nguyên | `UnitTest/ImportExport/Import/*` | 🟢 |
| T4 | Export processor cases (~11, gồm RowCount Case 7) | Hành vi GĐ1 giữ nguyên | `UnitTest/ImportExport/Export/ExportCaseTests.cs` | 🟢 |
| T5 | Validation + Infrastructure + Schemas + Templates | Mapping/layout/registry/store không regression | `UnitTest/ImportExport/{Validation,Infrastructure,Schemas,Templates}/*` | 🟢 |
| T6 | Tenant isolation trên store (ambient + path) | Tenant A không thấy/ghi đè template/file tenant B | `UnitTest/ImportExport/Infrastructure/TemplateTenantIsolationTests.cs` | 🟢 |

### 8.3 Test mới *(đã cover)*

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T7 | Schema service / registry whitelist | Unknown key → lỗi; fields filter Id/TenantId | `UnitTest/ImportExport/Schemas/ImportExportSchemaServiceTests.cs` | 🟢 |
| T8 | Export RowCount Case 7 | Chỉ N row đầu theo template | `ExportCaseTests.Case7_Export_RowCountLimitsToFirstNRecords` | 🟢 |

### 8.4 Done khi

- Mọi hàng Smoke + Regression 🟢.
- Hàng Test mới (nếu có) 🟢.
- Không bắt buộc integration WebApplicationFactory cho GĐ1 nếu T1–T6 đã chứng minh quyết định.

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc | Trạng thái |
|-------|------|----------|-----------|------------|
| 0 | Confirm §7 theo từng slice | Accepted từng phần | — | 🟢 |
| 1 | Rename/layout packages + fluent DI | Build Sample xanh | 0 | 🟢 |
| 2 | Host-owned entity + generic store; gỡ DbContext module | Migrate host OK | 1 | 🟢 |
| 3 | Ambient tenant + path + unique `(TenantId, Code)` | Isolation verify | 2 | 🟢 |
| 4 | Move validation/layout vào Core; gỡ EF→MiniExcel | Satellite graph đúng | 1 | 🟢 |
| 5 | Entity registry + schema HTTP + Sample register | Swagger/UI auto-fill | 1 | 🟢 |
| 6 | Unit tests + helpers + Case7 (ban đầu dưới module) | Suite xanh | 1, 4, 5 | 🟢 |
| 7 | Chuyển tests → `UnitTest/ImportExport/` | Filter ImportExport xanh trên `UnitTest` | 6 | 🟢 |
| 8 | ADR tổng hợp trong `platform/ADRs` + index README | File + dòng index | 0–7 | 🟢 |

---

## 10. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | Package layout + DI trên Sample | 🟢 |
| 2 | Host-owned template + multi-tenant store/path | 🟢 |
| 3 | Core validation/layout + Api chỉ reference Core | 🟢 |
| 4 | Entity registry + schema API + Sample/FE picker | 🟢 |
| 5 | §8 Smoke + Regression xanh (`UnitTest/ImportExport`) | 🟢 |
| 6 | ADR tổng hợp + dòng trong `ADRs/README.md` | 🟢 |

---

## 11. Tham chiếu thêm

- Branch lịch sử: `LEX-259-1008`
- Verify:

```powershell
dotnet test UnitTest --filter FullyQualifiedName~ImportExport
```

- Consumer DI (tham khảo — Sample):

```csharp
builder.AddCoreImport()
    .ConfigureEntities(o => o.Add<StudentImportRow>("Student", "Student"))
    .UseMiniExcel()
    .UseEntityFramework<SampleExcelTemplate, ITenantUnitOfWork, CurrentTenantInfo>()
    .UseHttpApi();
builder.AddCoreExport();
```
