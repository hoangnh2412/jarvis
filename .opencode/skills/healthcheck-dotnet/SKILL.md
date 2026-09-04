---
name: healthcheck-dotnet
description: Thiết lập Jarvis.HealthChecks cho ASP.NET Core — init liveness/startup/readiness, thêm provider readiness. Dùng khi tích hợp healthcheck .NET, /health/live, /health/ready, hoặc thêm dependency (PostgreSQL, MySQL, SQL Server, Redis, …).
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
- `appsettings.json` (`HealthChecks:Readiness`)
- Checklist validation
