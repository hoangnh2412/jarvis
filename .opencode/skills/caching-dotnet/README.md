# caching-dotnet

Skill tích hợp **Jarvis.Caching** — memory + Redis, invalidation, cache-aside. Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Chưa có cache Jarvis | [workflows/init.md](./workflows/init.md) |
| Bật Redis / invalidation / OTEL | [workflows/add.md](./workflows/add.md) + [providers/](./providers/) |

Scaffold `jarvis-dotnet` đã gọi `AddJarvisCaching()` trong Infrastructure — dùng skill này khi bật **Redis** hoặc project chưa có Jarvis.

## Cách gọi

```text
@.opencode/skills/caching-dotnet/workflows/init.md

Init Jarvis Caching memory-only cho MyApp.Infrastructure.
```

```text
@.opencode/skills/caching-dotnet/workflows/add.md

Bật Redis distributed + memory invalidation cho MyApp.Host.
```

## Quy tắc

- `MemSeconds > 0` — memory; `DistributedSeconds > 0` — Redis
- `GetOrSetAsync` cho cache-aside; `RemoveAsync` sau write
- `AddJarvisCaching()` **trước** `AddEntityFramework()`

## Providers

| Provider | SKILL |
|----------|-------|
| Redis distributed | [providers/redis-distributed/SKILL.md](./providers/redis-distributed/SKILL.md) |
| Memory invalidation | [providers/redis-invalidation/SKILL.md](./providers/redis-invalidation/SKILL.md) |
| OTEL Redis | [providers/otel-redis/SKILL.md](./providers/otel-redis/SKILL.md) |

## Liên quan

- [entityframework-dotnet/README.md](../entityframework-dotnet/README.md) — cache connection string resolver
- [telemetry-dotnet/README.md](../telemetry-dotnet/README.md) — OTEL Redis instrumentation
- [jarvis-dotnet/README.md](../jarvis-dotnet/README.md) — scaffold
