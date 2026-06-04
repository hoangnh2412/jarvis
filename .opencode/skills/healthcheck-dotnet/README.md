# healthcheck-dotnet

Skill tích hợp **Jarvis.HealthChecks** — liveness, startup, readiness. Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Chưa có healthcheck Jarvis | [workflows/init.md](./workflows/init.md) |
| Thêm PostgreSQL, Redis, … | [workflows/add.md](./workflows/add.md) + [providers/](./providers/) |
| Bật HealthChecks UI dashboard | [workflows/ui.md](./workflows/ui.md) |

Scaffold `jarvis-dotnet` đã có `/health/live` tối thiểu — dùng skill này khi cần **readiness** dependency.

## Cách gọi

```text
@.opencode/skills/healthcheck-dotnet/workflows/add.md

Thêm readiness PostgreSQL cho MyApp.Host. ConnectionStrings:MainDb.
```

## Endpoints

| Path | Mục đích |
|------|----------|
| `/health/live` | Liveness — không đặt DB |
| `/health/ready` | Readiness — DB, Redis, … |
| `/health/startup` | Startup probe |
| `/health` | Aggregate |
| `/healthchecks-ui` | Dashboard (khi `Ui.Enabled`) |

## Quy tắc

- `builder.AddHealthChecks()` **trước** readiness registrations
- `app.UseHealthChecks()` cuối pipeline
- Config: `HealthChecks:DefaultTimeoutSeconds` (1–120)
- Không hardcode connection string

## Providers

| Provider | SKILL |
|----------|-------|
| PostgreSQL | [providers/postgresql/SKILL.md](./providers/postgresql/SKILL.md) |
| MySQL | [providers/mysql/SKILL.md](./providers/mysql/SKILL.md) |
| SQL Server | [providers/mssql/SKILL.md](./providers/mssql/SKILL.md) |
| Redis | [providers/redis/SKILL.md](./providers/redis/SKILL.md) |

Xem bảng đầy đủ trong [SKILL.md](./SKILL.md).

## Bootstrap trong solution Jarvis

Scaffold `jarvis-dotnet` đã gọi `AddHealthChecks()` / `UseHealthChecks()` trong `HostLayerExtension`. Package: `Jarvis.HealthChecks` 1.0.0 trên Host.

Chỉ cần skill này khi thêm **readiness** (PostgreSQL, Redis, …) hoặc project chưa dùng Jarvis scaffold.

## Liên quan

- [jarvis-dotnet/README.md](../jarvis-dotnet/README.md) — scaffold & layer Host
