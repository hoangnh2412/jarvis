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

| Module | PackageId | Layer thường gặp |
|---|---|---|
| Domain shared | `Jarvis.Domain.Shared` | Domain.Shared |
| Domain | `Jarvis.Domain` | Host (enricher) |
| Application | `Jarvis.Application` | Application |
| Entity Framework | `Jarvis.EntityFramework` | Infrastructure |
| MVC | `Jarvis.Mvc` | Host |
| Swashbuckle | `Jarvis.Swashbuckle` | Host |
| Health checks | `Jarvis.HealthChecks` | Host |
| OpenTelemetry | `Jarvis.OpenTelemetry` | Host |
| Authentication | `Jarvis.Authentications.*` | Host |
| Caching | `Jarvis.Caching.*` | Infrastructure |

Bảng đầy đủ version: xem csproj repo Jarvis hoặc [workflows/init.md](workflows/init.md).

## Modules (atomic)

| Module | Path | Skill con |
|---|---|---|
| Foundation | [modules/foundation/SKILL.md](modules/foundation/SKILL.md) | — |
| Application | [modules/application/SKILL.md](modules/application/SKILL.md) | — |
| Entity Framework | [modules/entityframework/SKILL.md](modules/entityframework/SKILL.md) | — |
| Caching | [modules/caching/SKILL.md](modules/caching/SKILL.md) | — |
| Authentication | [modules/authentication/SKILL.md](modules/authentication/SKILL.md) | — |
| Blob storing | [modules/blob-storing/SKILL.md](modules/blob-storing/SKILL.md) | — |
| Notification | [modules/notification/SKILL.md](modules/notification/SKILL.md) | — |
| Swashbuckle | [modules/swashbuckle/SKILL.md](modules/swashbuckle/SKILL.md) | — |
| OpenTelemetry | [modules/opentelemetry/SKILL.md](modules/opentelemetry/SKILL.md) | [telemetry-dotnet](../telemetry-dotnet/SKILL.md) |
| Health checks | [modules/healthchecks/SKILL.md](modules/healthchecks/SKILL.md) | [healthcheck-dotnet](../healthcheck-dotnet/SKILL.md) |

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
