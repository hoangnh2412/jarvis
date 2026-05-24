# Workflow: Thêm module Jarvis

Áp dụng khi project **đã có** foundation Jarvis và cần bổ sung một module.

## Checklist

```text
- [ ] 1. Xác định module (entityframework | authentication | caching | …)
- [ ] 2. Đọc modules/<module>/SKILL.md
- [ ] 3. Thêm ProjectReference / PackageReference
- [ ] 4. Đăng ký extension trong Program.cs
- [ ] 5. Thêm section appsettings
- [ ] 6. Validate
```

## Bước 1 — Chọn module

| Loại | Đọc |
|------|-----|
| Module trong `jarvis-dotnet/modules/` | `modules/<tên>/SKILL.md` |
| Caching | [caching-dotnet/SKILL.md](../../caching-dotnet/SKILL.md) |
| Entity Framework | [entityframework-dotnet/SKILL.md](../../entityframework-dotnet/SKILL.md) |
| Swashbuckle | [swashbuckle-dotnet/SKILL.md](../../swashbuckle-dotnet/SKILL.md) |
| Blob storing | [blobstoring-dotnet/SKILL.md](../../blobstoring-dotnet/SKILL.md) |
| OpenTelemetry | [telemetry-dotnet/SKILL.md](../../telemetry-dotnet/SKILL.md) |
| Health checks | [healthcheck-dotnet/SKILL.md](../../healthcheck-dotnet/SKILL.md) |

Không thêm package không dùng (giảm dependency surface).

## Bước 2 — Thêm package

Ví dụ thêm JWT auth:

```xml
<PackageReference Include="Jarvis.Authentications.Jwt" Version="1.0.1" />
```

Hoặc ProjectReference tới `Jarvis.Authentication.Jwt`.

## Bước 3 — Registration

Copy snippet từ `modules/<module>/SKILL.md` vào `Program.cs` đúng vị trí:

| Module | Thường đăng ký khi |
|---|---|
| Caching | **`AddJarvisCaching()` trước EntityFramework** (Infrastructure) |
| EntityFramework | sau `AddJarvisCaching()` |
| Authentication | `builder.Services.AddAuthentication()` chain |
| OpenTelemetry | đầu `Program.cs`, trước `Build()` |
| HealthChecks | trước `Build()`, `UseHealthChecks()` cuối pipeline |
| Swashbuckle | sau `AddCoreWebApi` |

## Bước 4 — Config

Thêm section appsettings theo module (xem từng SKILL).

## Bước 5 — Doc con / skill chuyên sâu (nếu có)

| Module | Đọc thêm |
|---|---|
| Entity Framework | [entityframework-dotnet](../../entityframework-dotnet/README.md) |
| Caching | [caching-dotnet](../../caching-dotnet/README.md) |
| Swashbuckle | [swashbuckle-dotnet](../../swashbuckle-dotnet/README.md) |
| Blob storing | [blobstoring-dotnet](../../blobstoring-dotnet/README.md) |
| OpenTelemetry | [telemetry-dotnet/README.md](../../telemetry-dotnet/README.md) · [workflows/add.md](../../telemetry-dotnet/workflows/add.md) |
| HealthChecks | [healthcheck-dotnet/README.md](../../healthcheck-dotnet/README.md) · [workflows/add.md](../../healthcheck-dotnet/workflows/add.md) |

## Anti-patterns

- Reference `Jarvis.Mvc` mà không có `Jarvis.Domain.Shared` (Mvc phụ thuộc shared)
- `AddCoreDbContext` mà chưa `AddEntityFramework()` / chưa đăng ký `ITenantConnectionStringResolver`
- Copy toàn bộ Sample.csproj cho microservice chỉ cần API + DB
