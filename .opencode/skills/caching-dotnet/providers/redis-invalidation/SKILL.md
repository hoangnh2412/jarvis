---
name: caching-dotnet-redis-invalidation
description: Bật pub/sub xóa memory cache giữa các node qua UseRedisMemoryCacheInvalidation. Dùng khi multi-instance ASP.NET Core cần đồng bộ memory sau write.
dependencies:
  - Jarvis.Caching.Redis
  - StackExchange.Redis
---

# Memory cache invalidation (pub/sub)

Redis **riêng** cho invalidation — không bắt buộc trùng multiplexer với `DistributedGroups`.

## appsettings

```json
{
  "Cache": {
    "MemoryInvalidation": {
      "Redis": {
        "Configuration": "127.0.0.1:6379"
      }
    }
  }
}
```

## Registration

```csharp
builder.AddJarvisCaching()
    .UseRedisDistributedCache()
    .UseRedisMemoryCacheInvalidation();
```

Sau write: `await cache.RemoveAsync(param, ct);` — xóa local + Redis key + notify peers.
