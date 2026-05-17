# Kiến trúc — {Product}

## Layers

| Project | Trách nhiệm |
|---|---|
| Domain.Shared | Enum, constant shared |
| Domain | Entity, repository interface, domain events |
| Application | Command/query, handler, DTO |
| Infrastructure | EF Core, repository implementation, external adapters |
| Host | Composition root, controllers, middleware pipeline |

## Jarvis integration

- **Application:** `Jarvis.Application` (CQRS dispatchers)
- **Infrastructure:** `Jarvis.EntityFramework` (DbContext factory, UoW, multitenancy)
- **Host:** `Jarvis.Mvc`, `Jarvis.Swashbuckle`, `Jarvis.HealthChecks`, `Jarvis.OpenTelemetry`

## DI entry points

- `Host.AddHostLayer()` → `Application.AddApplicationLayer()` + `Infrastructure.AddInfrastructureLayer()`
- `Infrastructure.AddInfrastructureLayer()` → `Domain.AddDomainLayer()` + persistence

Chi tiết scaffold: skill `jarvis-dotnet` → `workflows/scaffold.md`.
