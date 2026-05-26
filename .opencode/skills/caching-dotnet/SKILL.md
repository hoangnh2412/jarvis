---
name: caching-dotnet
description: Thiết lập Jarvis.Caching — memory + Redis distributed cache, invalidation pub/sub, cache-aside GetOrSetAsync. Dùng khi tích hợp cache .NET, section Cache, hoặc cache connection string cho EF.
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis.Caching — Orchestrator

Skill điều phối `Jarvis.Caching` + `Jarvis.Caching.Redis` trên ASP.NET Core.

Kiến trúc & vận hành: [README.md](README.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Project chưa có Jarvis cache | [workflows/init.md](workflows/init.md) |
| Đã có memory, cần Redis / invalidation / OTEL | [workflows/add.md](workflows/add.md) |

## Quy tắc cốt lõi

- `AddJarvisCaching()` bind section `Cache`, đăng ký `ICacheService` + memory.
- **Memory luôn tầng đầu**; Redis khi `DistributedSeconds > 0` trên item.
- Cache-aside: `GetOrSetAsync(param, query, ct)` — ưu tiên hơn `GetAsync` overload cũ.
- `null` từ loader → không ghi cache.
- Sau write DB/blob: `RemoveAsync(param)` — memory + Redis + pub/sub peers.
- **Trước EF:** `AddJarvisCaching()` → `AddEntityFramework()` (`CachingTenantConnectionStringResolver`).

## Packages

| PackageId | Version* | Khi nào |
|---|---|---|
| `Jarvis.Caching` | 1.1.0 | Bắt buộc |
| `Jarvis.Caching.Redis` | 1.1.0 | Redis distributed + invalidation |

\*Xem csproj repo Jarvis.

## Providers (atomic)

| Provider | Path |
|---|---|
| Redis distributed | [providers/redis-distributed/SKILL.md](providers/redis-distributed/SKILL.md) |
| Memory invalidation pub/sub | [providers/redis-invalidation/SKILL.md](providers/redis-invalidation/SKILL.md) |
| OpenTelemetry Redis trace | [providers/otel-redis/SKILL.md](providers/otel-redis/SKILL.md) |

## Templates

- [templates/program-setup.cs](templates/program-setup.cs)
- [templates/appsettings-cache.json](templates/appsettings-cache.json)

## Tham chiếu

- Ví dụ `GetOrSetAsync`: [reference/get-or-set.md](reference/get-or-set.md)

## Output bắt buộc

- `Program.cs` / `InfrastructureLayerExtension` — `AddJarvisCaching` (+ Redis extensions nếu dùng)
- `appsettings.json` section `Cache`
- Item `ConnectionString` nếu dùng cùng EF
- Validate: hit/miss, `RemoveAsync` sau update
