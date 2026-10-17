# ADR: Naming ImportExport

**Date:** 2026-08-05  
**Status:** Accepted

## Context

Module GĐ1 tồn tại dưới tên `Sample.ExcelEngine.*` trong `modules/ImportExportModule/`. Cần chuẩn hóa theo convention Platform (cùng mô hình với `Platform.Modules.Setting`).

## Decision

| Vai trò | Package / Project |
|---------|-------------------|
| Core (ports, services, DI builder) | `Platform.Modules.ImportExport` |
| EF template store + default entity | `Platform.Modules.ImportExport.EntityFramework` |
| HTTP API (opt-in) | `Platform.Modules.ImportExport.API` |
| Unit tests | `Platform.Modules.ImportExport.Tests` |
| MiniExcel adapter | `Platform.ImportExport.MiniExcel` (`frameworks/`) |
| Demo host | `Sample` (không có project Host riêng trong `modules/`) |

**Không có:** meta-package `Complete`, project `Application`, standalone `Host` trong `modules/importexport/`.

### DI

- `AddCoreImportExport()` → fluent builder
- `UseMiniExcel()`, `UseEntityFramework<TEntity, TUnitOfWork, TTenant>()`, `UseHttpApi()`
- NuGet version **1.2.0** (align core + satellite)
- PackageId HTTP: `Platform.Modules.ImportExport.API` (folder `.Api`)

### Folder

`modules/importexport/` + `frameworks/Platform.ImportExport.MiniExcel/`

## Consequences

- Breaking rename từ `Sample.ExcelEngine.*`
- Consumer reference từng package riêng (giống Setting)
- Multi-tenant không nằm trong rename này
- Thư mục legacy `modules/ImportExportModule` **đã gỡ** — chỉ còn `modules/importexport/`
