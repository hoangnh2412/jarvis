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

| Layer product | Jarvis packages |
|---|---|
| Application | `Jarvis.Application` |
| Infrastructure | `Jarvis.EntityFramework` |
| Host | `Jarvis.Mvc`, `Jarvis.Swashbuckle`, `Jarvis.HealthChecks`, `Jarvis.OpenTelemetry`, `Jarvis.Domain` |

Chi tiết csproj: [templates/layer-csproj/](../templates/layer-csproj/).

## Bootstrap tối thiểu (Host)

Nếu chưa có `HostLayerExtension`, copy từ [templates/layers/HostLayerExtension.cs](../templates/layers/HostLayerExtension.cs).

```csharp
builder.AddHostLayer();
var app = builder.Build();
app.UseHostLayer();
```

## Module tùy chọn

Xem [add.md](add.md) và `modules/*/SKILL.md`.

## Validate

- `dotnet build`
- Swagger + `/api/ping` (nếu đã scaffold controller mẫu)
