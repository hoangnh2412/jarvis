# Workflow: HealthChecks UI

Áp dụng khi project **đã có** `builder.AddHealthChecks()` + `app.UseHealthChecks()` và cần dashboard giám sát (Dev/Staging).

## Checklist

```text
- [ ] 1. Xác nhận core healthcheck đã chạy (init xong)
- [ ] 2. Thêm section HealthChecks:Ui vào appsettings.json
- [ ] 3. Khai báo Endpoints (URI tuyệt đối)
- [ ] 4. Production: Ui.Enabled = false; Dev: override Enabled = true
- [ ] 5. Validate /healthchecks-ui và worker poll
```

## Bước 1 — Điều kiện

`Jarvis.HealthChecks` tự đăng ký **AspNetCore.HealthChecks.UI** + InMemory storage khi `HealthChecks:Ui:Enabled` = `true`.

Host **không** gọi `AddHealthChecksUI` thủ công — chỉ cần:

```csharp
builder.AddHealthChecks();
// ... readiness ...
app.UseHealthChecks();
```

## Bước 2 — Endpoints UI

| Path | Mục đích | Config |
|---|---|---|
| `/healthchecks-ui` | Dashboard SPA | `Ui.UIPath` (mặc định) |
| `/healthchecks-api` | JSON API cho SPA | `Ui.ApiPath` (mặc định) |

Các probe mà UI poll (khai báo trong `Ui.Endpoints`):

| Name gợi ý | URI |
|---|---|
| Startup | `{baseUrl}/health/startup` |
| Liveness | `{baseUrl}/health/live` |
| Readiness | `{baseUrl}/health/ready` |

`{baseUrl}` = URL Kestrel thực tế (thường khớp `App:SelfUrl`), **không** trailing slash.

## Bước 3 — appsettings.json

Thêm vào section `HealthChecks` (cùng cấp `Readiness`):

```json
{
  "HealthChecks": {
    "Ui": {
      "Enabled": false,
      "EvaluationTimeSeconds": 10,
      "UIPath": "/healthchecks-ui",
      "ApiPath": "/healthchecks-api",
      "Endpoints": [
        {
          "Name": "Startup",
          "Uri": "http://localhost:8080/health/startup"
        },
        {
          "Name": "Liveness",
          "Uri": "http://localhost:8080/health/live"
        },
        {
          "Name": "Readiness",
          "Uri": "http://localhost:8080/health/ready"
        }
      ],
      "Webhooks": []
    }
  }
}
```

**Quy tắc:**

- `Endpoints[].Uri` — URI **tuyệt đối**; khai báo trong config, **không** hardcode fallback trong code.
- `EvaluationTimeSeconds` — clamp **5–300** (Jarvis).
- Đổi host/port → cập nhật từng `Uri` (hoặc override theo môi trường).

Tham chiếu: [jarvis/Sample/appsettings.json](../../../Sample/appsettings.json) (`HealthChecks:Ui`).

## Bước 4 — Bật theo môi trường

**Base** (`appsettings.json`): `Enabled: false`, giữ đủ `Endpoints`.

**Development** (`appsettings.Development.json`) — chỉ bật UI:

```json
{
  "HealthChecks": {
    "Ui": {
      "Enabled": true
    }
  }
}
```

Production/Staging: giữ `Enabled: false` trừ khi ops yêu cầu dashboard.

## Bước 5 — Validate

- [ ] `HealthChecks:Ui:Enabled` đúng môi trường
- [ ] `Endpoints` có URI tuyệt đối, khớp URL app đang listen
- [ ] `GET /healthchecks-ui` → dashboard mở được
- [ ] UI worker poll thành công — `/health/*` **anonymous** (không 401)

## Anti-patterns

- Hardcode `Endpoints` trong C# thay vì config
- `Enabled: true` trên Production không chủ đích
- URI relative hoặc sai port so với Kestrel
- Bỏ qua 401 trên probe khiến dashboard luôn Unhealthy
