---
name: jarvis-dotnet-entityframework
description: Cài Jarvis.EntityFramework — repository, multitenancy, AddCoreDbContext. Đọc doc con theo mô hình DB (single / separate / hybrid / custom DI).
dependencies:
  - Jarvis.EntityFramework
  - Npgsql.EntityFrameworkCore.PostgreSQL
---

# Entity Framework

## Packages

| Project | PackageId | Ghi chú |
|---|---|---|
| Jarvis.EntityFramework | `Jarvis.EntityFramework` | Core |
| Jarvis.EntityFramework.PostgreSql | — | ProjectReference; marker provider |
| Jarvis.EntityFramework.MySql | — | ProjectReference; placeholder |

**NuGet host (chọn một):** `Npgsql.EntityFrameworkCore.PostgreSQL`, `Pomelo.EntityFrameworkCore.MySql`, …

## Bootstrap

```csharp
builder.AddEntityFramework();
builder.AddAppDbContext(); // Infrastructure — xem doc con theo mô hình
```

## Chọn mô hình multitenancy

| Mô hình | Khi nào | Doc |
|---|---|---|
| **Single DB** | Một DB, phân tách `TenantId` + filter | [single-db.md](single-db.md) |
| **Separate tenant DB** | Mỗi tenant một DB; registry trên Master | [separate-tenant-db.md](separate-tenant-db.md) |
| **Hybrid** | Master + pool DB chung + DB riêng | [hybrid.md](hybrid.md) |
| **Tùy biến DI** | MST → TenantId, MinIO → connection, … (không sửa Jarvis) | [custom-di.md](custom-di.md) |

**Luôn đọc trước khi implement:** [setup.md](setup.md) (DbContext, UoW, entity, appsettings, API, luồng resolve).

## `AddCoreDbContext` (tóm tắt)

| Overload | Interceptor | Khi dùng |
|---|---|---|
| `AddCoreDbContext<TDb>(configure)` | Không | Single DB, Master DB |
| `AddCoreDbContext<TDb, TResolver>(configure)` | `TenantDbConnectionInterceptor` | Tenant app DB, hybrid |

## Sample & docs repo

| File | Nội dung |
|---|---|
| `Sample/HostApplicationBuilderExtension.cs` | Master + tenant DbContext |
| `Sample/Controllers/MultitenancyEfTestController.cs` | HTTP + job |
| `docs/entity-framework-multitenancy.md` | Batch job, filter |
