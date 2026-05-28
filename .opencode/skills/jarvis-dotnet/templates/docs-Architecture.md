# Kiến trúc — {Product}

## Layers

| Project | Trách nhiệm |
|---|---|
| Domain.Shared | Enum, constant shared |
| Domain | Entity, repository interface, domain events |
| Application | Command/query, handler, DTO |
| Infrastructure | EF Core, repository implementation, external adapters |
| Host | Composition root, controllers, middleware pipeline |

## Jarvis integration (scaffold mặc định)

| Layer | Package / API | Skill mở rộng (repo Jarvis `.opencode/skills/`) |
|-------|----------------|--------------------------------------------------|
| Application | `Jarvis.Application` — CQRS | `application-dotnet` |
| Infrastructure | `Jarvis.Caching` → `Jarvis.EntityFramework` | `caching-dotnet`, `entityframework-dotnet` |
| Host | Mvc, Swashbuckle, HealthChecks, OpenTelemetry | `foundation-dotnet`, `swashbuckle-dotnet`, `healthcheck-dotnet`, `telemetry-dotnet` |

Chưa có trong scaffold — thêm khi cần: `authentication-dotnet`, `notification-dotnet`, `blobstoring-dotnet`.

Bản đồ đầy đủ: skill `jarvis-dotnet` → `templates/SKILLS.md`.

## DI entry points

- `Host.AddHostLayer()` → `Application.AddApplicationLayer()` + `Infrastructure.AddInfrastructureLayer()`
- `Infrastructure.AddInfrastructureLayer()` → `Domain.AddDomainLayer()` + persistence

Chi tiết scaffold: skill `jarvis-dotnet` → `workflows/scaffold.md`.
