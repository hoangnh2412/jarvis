# OpenCode skills — Jarvis .NET

Bản đồ skill AI cho repo Jarvis framework. Mỗi skill có **README** (người dùng) và **SKILL.md** (agent).

## Skills

| Skill | README | Mô tả |
|-------|--------|--------|
| **jarvis-dotnet** | [skills/jarvis-dotnet/README.md](./skills/jarvis-dotnet/README.md) | Scaffold / init / add solution Jarvis |
| **caching-dotnet** | [skills/caching-dotnet/README.md](./skills/caching-dotnet/README.md) | Memory + Redis cache |
| **entityframework-dotnet** | [skills/entityframework-dotnet/README.md](./skills/entityframework-dotnet/README.md) | EF multitenancy |
| **swashbuckle-dotnet** | [skills/swashbuckle-dotnet/README.md](./skills/swashbuckle-dotnet/README.md) | Swagger / OpenAPI |
| **healthcheck-dotnet** | [skills/healthcheck-dotnet/README.md](./skills/healthcheck-dotnet/README.md) | Health endpoints |
| **telemetry-dotnet** | [skills/telemetry-dotnet/README.md](./skills/telemetry-dotnet/README.md) | OpenTelemetry |
| **analyze-metric-dotnet** | [skills/analyze-metric-dotnet/README.md](./skills/analyze-metric-dotnet/README.md) | Đọc Grafana Dotnet Runtime Metrics |
| **blobstoring-dotnet** | [skills/blobstoring-dotnet/README.md](./skills/blobstoring-dotnet/README.md) | FileSystem / MinIO blob |
| **code-review-dotnet** | [skills/code-review-dotnet/README.md](./skills/code-review-dotnet/README.md) | Review PR C#/.NET |

## Module còn trong `jarvis-dotnet/modules/`

Foundation, Application, Authentication, Notification, … — chưa tách skill riêng.

## Prompt nhanh

```text
@.opencode/skills/jarvis-dotnet/workflows/scaffold.md
Scaffold backend .NET 9: Product=Acme, product=acme
```

```text
@.opencode/skills/entityframework-dotnet/workflows/init.md
Init Jarvis EF + single DB cho MyApp
```

```text
@.opencode/skills/analyze-metric-dotnet/workflows/analyze.md
Giải thích panel GC và thread pool trên Dotnet Runtime Metrics, job=myapp
```

Framework overview: [README.md](../README.md) (repo gốc).
