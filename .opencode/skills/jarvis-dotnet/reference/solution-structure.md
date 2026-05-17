# Cấu trúc solution .NET phân lớp + Jarvis

Khung scaffold cho repo mới. Ranh giới Clean Architecture / DDD chi tiết: skill `dotnet-clean-architecture`, `dotnet-ddd` (nếu có trong workspace).

## Dependency flow

```text
{Product}.Domain.Shared
        ↑
{Product}.Domain
        ↑
{Product}.Application
        ↑
{Product}.Infrastructure
        ↑
{Product}.Host
```

- **Host** → Application, Infrastructure (composition root).
- **Application** → Domain (use case; không reference Infrastructure).
- **Infrastructure** → Domain (adapter, persistence).
- **Domain** → Domain.Shared.

## Gốc repository

```text
{product}-backend/
├── README.md
├── docs/
│   └── Architecture.md
├── src/
│   └── {Product}.sln
├── tests/
│   ├── {Product}.Domain.Tests/
│   └── {Product}.Application.Tests/
└── build/                    # khi có CI/CD
```

## Jarvis package → layer

| Layer product | Jarvis packages (NuGet / ProjectReference) |
|---|---|
| Domain.Shared | `Jarvis.Domain.Shared` (tùy chọn — error/response base) |
| Domain | *(không bắt buộc Jarvis — giữ domain thuần)* |
| Application | `Jarvis.Application`, `Jarvis.Application.Contracts` |
| Infrastructure | `Jarvis.EntityFramework`, `Jarvis.Caching.*`, `Jarvis.BlobStoring.*`, `Jarvis.Notification.*` |
| Host | `Jarvis.Mvc`, `Jarvis.Swashbuckle`, `Jarvis.HealthChecks`, `Jarvis.OpenTelemetry`, `Jarvis.Authentication.*` |

## DI convention

| Layer | File | Method |
|---|---|---|
| Domain | `DependencyInjection/DomainLayerExtension.cs` | `AddDomainLayer` |
| Application | `DependencyInjection/ApplicationLayerExtension.cs` | `AddApplicationLayer` |
| Infrastructure | `DependencyInjection/InfrastructureLayerExtension.cs` | `AddInfrastructureLayer` → gọi `AddDomainLayer` + persistence |
| Host | `DependencyInjection/HostLayerExtension.cs` | `AddHostLayer` → Application + Infrastructure + pipeline |

`Program.cs` chỉ gọi:

```csharp
builder.AddHostLayer();
// ...
app.UseHostLayer();
```

## Câu hỏi trước scaffold (5.9 rút gọn)

| # | Câu hỏi | Hành động |
|---|---|---|
| Q1 | Một hay nhiều bounded context? | Nhiều → `Domain/Features/<Context>/` |
| Q2 | BackgroundService? | `Host/Workers/` hoặc `{Product}.Worker` |
| Q3 | Bus/cache/blob? | Folder trong Infrastructure |
| Q4 | AutoMapper? | `Application/Mappings/` |
| Q5 | Specification? | `Domain/Specifications/` |
| Q6 | Integration test DB? | `Infrastructure.Tests` |
| Q7 | Exceptions shared? | `Domain.Shared/Exceptions/` |

Scaffold mặc định skill: **một context**, **Web API Controllers**, **không MediatR**, **PostgreSQL**, **Swagger + OTEL + HealthChecks** tối thiểu.
