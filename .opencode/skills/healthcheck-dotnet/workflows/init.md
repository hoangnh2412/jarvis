# Workflow: Khởi tạo healthcheck

Áp dụng khi project ASP.NET Core **chưa** có Jarvis healthcheck.

## Checklist

```text
- [ ] 1. Phân tích Program.cs và dependencies
- [ ] 2. Thêm package Jarvis.HealthChecks (+ provider packages nếu cần)
- [ ] 3. Đăng ký core: builder.AddHealthChecks()
- [ ] 4. Tạo ReadinessHealthCheckExtensions (nếu có readiness deps)
- [ ] 5. Đăng ký readiness từ providers/*/SKILL.md
- [ ] 6. app.UseHealthChecks()
- [ ] 7. Cấu hình appsettings.json
- [ ] 8. Validate endpoints
```

## Bước 1 — Phân tích

1. Đọc `Program.cs` (minimal hosting / `WebApplication`).
2. Liệt kê infrastructure dependencies cần readiness (DB, cache, messaging, HTTP deps).
3. Xác định startup workflow (migrate, warmup) → có cần `MarkStartupComplete()` không.

## Bước 2 — Packages

```xml
<PackageReference Include="Jarvis.HealthChecks" Version="*" />
```

Thêm package provider tương ứng (xem `dependencies` trong từng `providers/*/SKILL.md`).

## Bước 3 — Program.cs

Dùng [templates/program-setup.cs](../templates/program-setup.cs):

```csharp
builder.AddHealthChecks();
builder.Add{App}ReadinessHealthChecks();  // nếu có readiness

var app = builder.Build();
// ... middleware ...
app.UseHealthChecks();
```

**Thứ tự:** `AddHealthChecks()` → readiness extensions → `UseHealthChecks()` cuối pipeline (trước `Run`).

## Bước 4 — Readiness extension

Tạo `{App}ReadinessHealthCheckExtensions.cs` theo [templates/base-healthcheck.cs](../templates/base-healthcheck.cs):

- Section: `HealthChecks:Readiness`
- Timeout: `HealthChecks:DefaultTimeoutSeconds` (clamp 1–120)
- Mỗi provider: đọc snippet từ `providers/<name>/SKILL.md`

## Bước 5 — appsettings.json

```json
{
  "HealthChecks": {
    "DefaultTimeoutSeconds": 5,
    "MarkStartupCompleteOnApplicationStarted": true,
    "Readiness": {
      "Database": "ConnectionStrings:MainDb"
    }
  }
}
```

Giá trị trong `Readiness` là **configuration key path** (colon-separated), không phải connection string trực tiếp.

## Bước 6 — Startup probe

| `MarkStartupCompleteOnApplicationStarted` | Hành động |
|---|---|
| `true` (mặc định) | Không gọi `MarkStartupComplete()` |
| `false` | Gọi `MarkStartupComplete()` sau migrate/warmup |

## Bước 7 — Validate

- `GET /health/live` → 200
- `GET /health/startup` → 200 (sau app started)
- `GET /health/ready` → 200 khi deps healthy
- Package provider đã cài
- Readiness tagged `HealthCheckTags.Readiness`
