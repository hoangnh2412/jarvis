# Workflow: Thêm Redis / invalidation / OTEL

Áp dụng khi **đã có** `AddJarvisCaching()` và cần tầng distributed hoặc pub/sub.

## Checklist

```text
- [ ] 1. Chọn provider (redis-distributed | redis-invalidation | otel-redis)
- [ ] 2. Đọc providers/<name>/SKILL.md
- [ ] 3. Package Jarvis.Caching.Redis (+ StackExchange.Redis transitive)
- [ ] 4. Fluent extensions sau AddJarvisCaching
- [ ] 5. Cập nhật appsettings Cache
- [ ] 6. Validate multi-node invalidation (nếu bật)
```

## Bước 1 — Chọn provider

| Nhu cầu | Provider |
|---------|----------|
| Redis làm L2 cache | [redis-distributed](../providers/redis-distributed/SKILL.md) |
| Xóa memory giữa các node | [redis-invalidation](../providers/redis-invalidation/SKILL.md) |
| Trace Redis connections | [otel-redis](../providers/otel-redis/SKILL.md) |

Thường bật **cả ba** trên production multi-instance.

## Bước 2 — Wiring chuẩn

```csharp
using Jarvis.Caching.Extensions;
using Jarvis.Caching.Redis;

builder.AddJarvisCaching()
    .UseRedisDistributedCache()
    .UseRedisMemoryCacheInvalidation();
```

`UseRedisMemoryCacheInvalidation()` **trước** `Build()`.

## Bước 3 — appsettings

Bổ sung `DistributedGroups`, `MemoryInvalidation`, item `DistributedSeconds` / `DistributedGroup` — xem [templates/appsettings-cache.json](../templates/appsettings-cache.json).

## Bước 4 — OTEL (tùy chọn)

Trong `AddJarvisOpenTelemetry` → `ConfigureTrace` — xem [providers/otel-redis/SKILL.md](../providers/otel-redis/SKILL.md).

## Anti-patterns

- Dùng chung Redis connection cho invalidation và distributed mà không cấu hình `MemoryInvalidation:Redis` riêng khi cần tách
- Quên `RemoveAsync` sau command ghi DB
- Cache object quá lớn (full file blob) không cố ý
