---
name: jarvis-dotnet
description: Scaffold solution .NET 9 phân lớp + cài Jarvis framework từ folder trống — F5 chạy Swagger. Dùng khi tạo project backend mới, cài package Jarvis, hoặc thêm module vào solution có sẵn.
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis Framework — Orchestrator

Skill điều phối **scaffold solution chuẩn** và **cài đặt Jarvis** trên ASP.NET Core **.NET 9**.

## Luồng chính (khuyến nghị)

```text
Folder trống → skill jarvis-dotnet → solution phân lớp + Jarvis → F5
```

| Bước | Workflow |
|---|---|
| 1. Scaffold từ đầu | **[workflows/scaffold.md](workflows/scaffold.md)** |
| 2. Thêm module Jarvis | [workflows/add.md](workflows/add.md) |
| 3. Chỉ cài package (solution có sẵn) | [workflows/init.md](workflows/init.md) |

## Cấu trúc solution tiêu chuẩn

```text
{product}-backend/
├── src/{Product}.sln
│   ├── {Product}.Domain.Shared
│   ├── {Product}.Domain
│   ├── {Product}.Application      → Jarvis.Application
│   ├── {Product}.Infrastructure   → Jarvis.EntityFramework
│   └── {Product}.Host             → Jarvis.Mvc, OTEL, HealthChecks, Swagger
└── tests/
```

Chi tiết folder, DI convention, Jarvis mapping: [reference/solution-structure.md](reference/solution-structure.md).

**Composition root:** `Program.cs` chỉ gọi `AddHostLayer()` / `UseHostLayer()`.

## Templates scaffold

| Tài nguyên | Path |
|---|---|
| Cây thư mục | [templates/solution-tree.txt](templates/solution-tree.txt) |
| Layer extensions + Host | [templates/layers/](templates/layers/) |
| csproj Jarvis refs | [templates/layer-csproj/](templates/layer-csproj/) |
| README / Architecture | [templates/docs-README.md](templates/docs-README.md) |

## Hai cách cài Jarvis

| Cách | Khi nào |
|---|---|
| **ProjectReference** | Monorepo cạnh repo Jarvis (`{JarvisRoot}`) |
| **NuGet** | Repo độc lập, feed nội bộ |

**PackageId:** `Jarvis.Authentication.*` folder → NuGet `Jarvis.Authentications.*`.

## Catalog package (NuGet)

Phiên bản tham chiếu từ repo Jarvis (`develop`):

| Module | PackageId | Version | Layer |
|---|---|---|---|
| Domain shared | `Jarvis.Domain.Shared` | 1.0.0 | Domain.Shared |
| Domain | `Jarvis.Domain` | 1.1.1 | Host (enricher) |
| Application | `Jarvis.Application` | 1.2.1 | Application |
| Application contracts | `Jarvis.Application.Contracts` | 1.2.1 | Application |
| Entity Framework | `Jarvis.EntityFramework` | 1.0.0 | Infrastructure |
| Caching | `Jarvis.Caching` | 1.1.0 | Infrastructure (**bắt buộc trước EF**) |
| Caching Redis | `Jarvis.Caching.Redis` | 1.1.0 | Infrastructure (tùy chọn) |
| MVC | `Jarvis.Mvc` | 1.1.0 | Host |
| Swashbuckle | `Jarvis.Swashbuckle` | 1.0.1 | Host |
| Health checks | `Jarvis.HealthChecks` | 1.0.0 | Host |
| OpenTelemetry | `Jarvis.OpenTelemetry` | 1.0.1 | Host |
| Authentication | `Jarvis.Authentications.*` | 1.0.1 | Host |

Bảng đầy đủ / monorepo ProjectReference: [workflows/init.md](workflows/init.md), csproj repo Jarvis.

## Quy tắc DI (develop)

| Thứ tự | Lý do |
|---|---|
| `AddJarvisCaching()` → `AddEntityFramework()` | EF bọc `ITenantConnectionStringResolver` qua `ICacheService` |
| `AddCoreDbContext` sau `AddEntityFramework` | Multitenancy + interceptor |
| `AddJarvisOpenTelemetry` trước `Build()` | Plug-in trong callback `configureServices` |

Skill chuyên sâu: [entityframework-dotnet](../entityframework-dotnet/README.md) · [caching-dotnet](../caching-dotnet/README.md) · [telemetry-dotnet](../telemetry-dotnet/README.md)

## Modules (atomic)

| Module | Path | Doc con / skill chuyên sâu |
|---|---|---|
| Foundation | [modules/foundation/SKILL.md](modules/foundation/SKILL.md) | — |
| Application | [modules/application/SKILL.md](modules/application/SKILL.md) | — |
| Entity Framework | — | [entityframework-dotnet](../entityframework-dotnet/README.md) |
| Caching | — | [caching-dotnet](../caching-dotnet/README.md) |
| Authentication | [modules/authentication/SKILL.md](modules/authentication/SKILL.md) | — |
| Blob storing | — | [blobstoring-dotnet](../blobstoring-dotnet/README.md) |
| Notification | [modules/notification/SKILL.md](modules/notification/SKILL.md) | — |
| Swashbuckle | — | [swashbuckle-dotnet](../swashbuckle-dotnet/README.md) |
| OpenTelemetry | — | [telemetry-dotnet](../telemetry-dotnet/README.md) · [SKILL.md](../telemetry-dotnet/SKILL.md) |
| Health checks | — | [healthcheck-dotnet](../healthcheck-dotnet/README.md) · [SKILL.md](../healthcheck-dotnet/SKILL.md) |

## F5 sau scaffold

- Startup project: `{Product}.Host`
- `launchSettings.json` → Swagger
- `GET /api/ping` — không cần DB
- `/health/live` — OK; `/health/ready` cần PostgreSQL nếu bật DB check

## Output bắt buộc (scaffold)

- Solution 5 project + 2 test projects
- `*LayerExtension.cs` mỗi layer
- `Program.cs` mỏng
- `appsettings` + `launchSettings`
- `dotnet build` thành công
