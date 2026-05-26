# Workflow: Cài Jarvis vào solution có sẵn

Áp dụng khi **đã có** solution phân lớp (hoặc single project) và chỉ cần **thêm Jarvis packages** + wiring — **không** scaffold từ đầu.

> Tạo repo mới từ folder trống: dùng **[scaffold.md](scaffold.md)** thay workflow này.

## Checklist

```text
- [ ] 1. Xác định layer từng project (Host / Infrastructure / Application)
- [ ] 2. Thêm PackageReference hoặc ProjectReference Jarvis
- [ ] 3. Tạo *LayerExtension nếu chưa có
- [ ] 4. Gọi AddHostLayer / AddInfrastructureLayer / AddApplicationLayer
- [ ] 5. appsettings (Json, Swagger, OTEL, HealthChecks)
- [ ] 6. dotnet build
```

## Map Jarvis → layer

| Layer product | Jarvis packages (version develop) |
|---|---|
| Application | `Jarvis.Application` 1.2.1, `Jarvis.Application.Contracts` 1.2.1 |
| Infrastructure | `Jarvis.EntityFramework` 1.0.0, **`Jarvis.Caching` 1.1.0** (trước EF) |
| Host | `Jarvis.Mvc` 1.1.0, `Jarvis.Swashbuckle` 1.0.1, `Jarvis.HealthChecks` 1.0.0, `Jarvis.OpenTelemetry` 1.0.1, `Jarvis.Domain` 1.1.1 |

**Thứ tự Infrastructure:** `AddJarvisCaching()` → `AddEntityFramework()` → `AddCoreDbContext(...)`.

Chi tiết csproj: [templates/layer-csproj/](../templates/layer-csproj/).

## Bootstrap tối thiểu (Host)

Nếu chưa có `HostLayerExtension`, copy từ [templates/layers/HostLayerExtension.cs](../templates/layers/HostLayerExtension.cs).

```csharp
builder.AddHostLayer();
var app = builder.Build();
app.UseHostLayer();
```

## Module tùy chọn

Xem [add.md](add.md) và [templates/SKILLS.md](../templates/SKILLS.md) — skill `*-dotnet` trong `.opencode/skills/`.

## Validate

- `dotnet build`
- Swagger + `/api/ping` (nếu đã scaffold controller mẫu)
