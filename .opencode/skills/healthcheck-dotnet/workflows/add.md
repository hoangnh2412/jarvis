# Workflow: Thêm readiness provider

Áp dụng khi project **đã có** `builder.AddHealthChecks()` và cần thêm một dependency mới.

## Checklist

```text
- [ ] 1. Xác định provider (postgresql | mysql | mssql | …)
- [ ] 2. Đọc providers/<provider>/SKILL.md
- [ ] 3. Cài NuGet package
- [ ] 4. Thêm config key vào HealthChecks:Readiness
- [ ] 5. Thêm registration vào ReadinessHealthCheckExtensions
- [ ] 6. Validate /health/ready
```

## Bước 1 — Chọn provider

Đọc **chỉ** file atomic tương ứng trong `providers/`:

```text
providers/postgresql/SKILL.md
providers/mysql/SKILL.md
providers/mssql/SKILL.md
```

Không load provider không dùng.

## Bước 2 — NuGet

Cài package từ frontmatter `dependencies` của provider SKILL.

## Bước 3 — appsettings.json

Thêm entry vào `HealthChecks:Readiness` — value là config key path:

```json
{
  "HealthChecks": {
    "Readiness": {
      "Database": "ConnectionStrings:MainDb"
    }
  }
}
```

## Bước 4 — Registration

Trong `{App}ReadinessHealthCheckExtensions`:

1. Lấy `probeTimeout` từ `DefaultTimeoutSeconds` (đã có sẵn nếu init xong).
2. Copy snippet từ `providers/<provider>/SKILL.md` vào method `TryAdd*Readiness`.
3. Gắn `tags: [HealthCheckTags.Readiness]`.

## Bước 5 — Custom IHealthCheck (nếu provider yêu cầu)

MinIO, HTTP external API → tạo file mới trong `HealthChecks/`:

- `{Name}HealthCheck.cs` — logic probe
- Đăng ký `.AddCheck<{Name}HealthCheck>(...)`

Tham khảo [templates/base-healthcheck.cs](../templates/base-healthcheck.cs).

## Bước 6 — Validate

- Package installed
- Registration có readiness tag
- Config key resolve được connection string
- `GET /health/ready` phản ánh trạng thái dependency

## Anti-patterns

- Hardcode connection string trong code
- Đặt DB check vào liveness
- Gộp nhiều provider trong một `IHealthCheck` class
- Heavy query trong readiness (SQL Server: dùng `SELECT 1`)
