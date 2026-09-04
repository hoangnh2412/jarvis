---
name: entityframework-dotnet
description: Thiết lập Jarvis.EntityFramework — repository, multitenancy, AddCoreDbContext, UoW. Dùng khi tích hợp EF Core .NET với single DB, separate tenant DB, hybrid, hoặc custom DI resolver.
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis.EntityFramework — Orchestrator

Skill điều phối `Jarvis.EntityFramework` trên ASP.NET Core. Hướng dẫn vận hành: [README.md](README.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Chưa có EF Jarvis | [workflows/init.md](workflows/init.md) |
| Đổi / thêm mô hình multitenancy | [workflows/add.md](workflows/add.md) |

## Quy tắc cốt lõi

- **`AddJarvisCaching()` trước `AddEntityFramework()`** — `CachingTenantConnectionStringResolver`.
- `AddEntityFramework()` → repository + keyed `ITenantIdResolver`.
- App đăng ký `ITenantConnectionStringResolver` qua `AddCoreDbContext`.
- Sau `SwitchDbContextAsync` → **`GetRepositoryAsync` lại**.
- UoW không đọc `ICurrentTenantAccessor` khi `SetTenantId` — tránh nhầm tenant.

## Packages

| PackageId | Layer |
|---|---|
| `Jarvis.EntityFramework` | Infrastructure |
| `Jarvis.Caching` | Infrastructure (trước EF) |
| `Npgsql.EntityFrameworkCore.PostgreSQL` (hoặc MySQL provider) | Infrastructure |

## Patterns (atomic)

| Mô hình | Path |
|---|---|
| Single DB + `TenantId` | [patterns/single-db/SKILL.md](patterns/single-db/SKILL.md) |
| Separate tenant DB | [patterns/separate-tenant-db/SKILL.md](patterns/separate-tenant-db/SKILL.md) |
| Hybrid | [patterns/hybrid/SKILL.md](patterns/hybrid/SKILL.md) |
| Custom DI resolver | [patterns/custom-di/SKILL.md](patterns/custom-di/SKILL.md) |

**Luôn đọc:** [reference/setup.md](reference/setup.md).

## `AddCoreDbContext`

| Overload | Interceptor | Khi dùng |
|---|---|---|
| `AddCoreDbContext<TDb>(configure)` | Không | Single DB, Master cố định |
| `AddCoreDbContext<TDb, TResolver>(configure)` | `TenantDbConnectionInterceptor` | Per-tenant connection |

## Templates

- [templates/infrastructure-extension.cs](templates/infrastructure-extension.cs)

## Output bắt buộc

- `InfrastructureLayerExtension` — caching + EF + DbContext
- `AppDbContext`, `AppUnitOfWork`, entities
- `appsettings` `ConnectionStrings`, `Cache:Items:ConnectionString`
- `dotnet build`; migrate nếu bật `AutoMigrate`
