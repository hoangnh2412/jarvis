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

Đọc **chỉ** file atomic tương ứng trong `modules/`.

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
| EntityFramework | `builder.AddEntityFramework()` trước DbContext |
| Authentication | `builder.Services.AddAuthentication()` chain |
| OpenTelemetry | đầu `Program.cs`, trước `Build()` |
| HealthChecks | trước `Build()`, `UseHealthChecks()` cuối pipeline |
| Swashbuckle | sau `AddCoreWebApi` |

## Bước 4 — Config

Thêm section appsettings theo module (xem từng SKILL).

## Bước 5 — Skill con (nếu có)

| Module | Skill chuyên sâu |
|---|---|
| OpenTelemetry | [telemetry-dotnet](../../telemetry-dotnet/SKILL.md) |
| HealthChecks | [healthcheck-dotnet](../../healthcheck-dotnet/SKILL.md) |

## Anti-patterns

- Reference `Jarvis.Mvc` mà không có `Jarvis.Domain.Shared` (Mvc phụ thuộc shared)
- `AddCoreDbContext` mà chưa `AddEntityFramework()` / chưa đăng ký `ITenantConnectionStringResolver`
- Copy toàn bộ Sample.csproj cho microservice chỉ cần API + DB
