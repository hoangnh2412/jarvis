---
name: healthcheck-dotnet
description: Thiết lập Jarvis.HealthChecks cho ASP.NET Core — init liveness/startup/readiness, HealthChecks UI, thêm provider readiness. Dùng khi tích hợp healthcheck .NET, /health/live, /health/ready, /healthchecks-ui, hoặc thêm dependency (PostgreSQL, MySQL, SQL Server, Redis, …).
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis.HealthChecks — Orchestrator

Skill điều phối tích hợp `Jarvis.HealthChecks` vào ASP.NET Core minimal hosting.

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Project chưa có healthcheck | [workflows/init.md](workflows/init.md) |
| Đã có core, cần thêm dependency | [workflows/add.md](workflows/add.md) |
| Đã có core, cần dashboard UI | [workflows/ui.md](workflows/ui.md) |

## Quy tắc cốt lõi

- Gọi `builder.AddHealthChecks()` (Jarvis) **trước** readiness registrations.
- Gọi `app.UseHealthChecks()` sau khi build pipeline.
- Liveness = process/runtime; readiness = infrastructure dependencies.
- Không đặt database vào liveness.
- Config path đầy đủ: `ConnectionStrings:MainDb` — không hardcode.
- Timeout: `HealthChecks:DefaultTimeoutSeconds`, clamp 1–120.

## Endpoints

| Endpoint | Mục đích |
|---|---|
| `/health/live` | Liveness |
| `/health/ready` | Readiness |
| `/health/startup` | Startup probe |
| `/health` | Aggregate |
| `/health/prometheus` | Prometheus metrics (khi bật) |
| `/healthchecks-ui` | Dashboard SPA — xem [workflows/ui.md](workflows/ui.md) |
| `/healthchecks-api` | JSON API cho SPA |

## Templates

- [templates/program-setup.cs](templates/program-setup.cs) — `Program.cs` tối thiểu
- [templates/base-healthcheck.cs](templates/base-healthcheck.cs) — custom `IHealthCheck` + extension readiness

## Providers (atomic)

Chỉ đọc provider cần dùng:

| Provider | Path |
|---|---|
| PostgreSQL | [providers/postgresql/SKILL.md](providers/postgresql/SKILL.md) |
| MySQL | [providers/mysql/SKILL.md](providers/mysql/SKILL.md) |
| SQL Server | [providers/mssql/SKILL.md](providers/mssql/SKILL.md) |
| Oracle | [providers/oracle/SKILL.md](providers/oracle/SKILL.md) |
| Redis | [providers/redis/SKILL.md](providers/redis/SKILL.md) |
| Kafka | [providers/kafka/SKILL.md](providers/kafka/SKILL.md) |
| RabbitMQ | [providers/rabbitmq/SKILL.md](providers/rabbitmq/SKILL.md) |
| MinIO | [providers/minio/SKILL.md](providers/minio/SKILL.md) |
| HTTP | [providers/http/SKILL.md](providers/http/SKILL.md) |
| SignalR | [providers/signalr/SKILL.md](providers/signalr/SKILL.md) |

## Cấu trúc code trong project

```text
HealthChecks/
├── *HealthCheck.cs              # custom IHealthCheck (nếu cần)
└── ReadinessHealthCheckExtensions.cs
```

- Mỗi dependency một file `*HealthCheck.cs` hoặc registration riêng trong extension.
- Gom registration tại `*ReadinessHealthCheckExtensions.cs`.
- Không nhét toàn bộ readiness logic vào `Program.cs`.

## Output bắt buộc

- Thay đổi `Program.cs`
- Extension/readiness registration
- `appsettings.json` (`HealthChecks:Readiness`; `HealthChecks:Ui` theo [workflows/ui.md](workflows/ui.md))

## Checklist validation

### Wiring & packages

- [ ] Package `Jarvis.HealthChecks` (và provider NuGet nếu có readiness)
- [ ] `builder.AddHealthChecks()` **trước** readiness registrations
- [ ] `app.UseHealthChecks()` sau pipeline (trước `Run`)
- [ ] `dotnet build` thành công

### Probe endpoints (mọi môi trường)

- [ ] `GET /health/live` → 200 (không phụ thuộc DB)
- [ ] `GET /health/startup` → 200 sau app started
- [ ] `GET /health/ready` → 200 khi deps healthy; 503 khi một dep down
- [ ] `GET /health` → JSON aggregate (API key nếu đã cấu hình `DetailedEndpointApiKey`)
- [ ] Probe **không cần JWT** (anonymous, không 401/403)
- [ ] Readiness checks gắn tag `readiness`; không có DB trên liveness

### Readiness (khi đã thêm provider)

- [ ] `HealthChecks:Readiness` dùng config key path, resolve được giá trị
- [ ] `DefaultTimeoutSeconds` trong khoảng 1–120
- [ ] Mỗi provider đã đăng ký trong `*ReadinessHealthCheckExtensions.cs`
- [ ] Tắt từng dependency → `/health/ready` phản ánh Unhealthy

### Prometheus (khi bật)

- [ ] `EnablePrometheusMetrics: true`
- [ ] `GET /health/prometheus` trả metrics

### HealthChecks UI (khi bật — [workflows/ui.md](workflows/ui.md))

- [ ] `HealthChecks:Ui:Enabled` đúng môi trường (Production thường `false`)
- [ ] `Endpoints[].Uri` tuyệt đối, khớp URL Kestrel
- [ ] `GET /healthchecks-ui` mở dashboard
- [ ] UI worker poll thành công (không 401 trên `/health/*`)
