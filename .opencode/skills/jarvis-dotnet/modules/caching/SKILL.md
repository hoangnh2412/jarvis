---
name: jarvis-dotnet-caching
description: Cài Jarvis.Caching — memory + Redis distributed cache qua CachingBuilder. Dùng khi project cần cache phân tầng mem/Redis theo section Cache.
dependencies:
  - Jarvis.Caching
  - Jarvis.Caching.Memory
  - Jarvis.Caching.Redis
  - StackExchange.Redis
---

# Caching

## Packages

| Project | PackageId |
|---|---|
| Jarvis.Caching | `Jarvis.Caching` |
| Jarvis.Caching.Memory | `Jarvis.Caching.Memory` |
| Jarvis.Caching.Redis | `Jarvis.Caching.Redis` |

## appsettings.json

```json
{
  "Cache": {
    "DefaultDistgroup": "Default",
    "DefaultDistType": "Redis",
    "DistGroups": {
      "Redis": {
        "Default": {
          "Configuration": "127.0.0.1:6379",
          "InstanceName": "app_"
        }
      }
    },
    "Items": {
      "MyCache": {
        "Key": "item_{id}",
        "MemSeconds": 180,
        "DistSeconds": 3600,
        "DistGroup": "Default"
      }
    }
  }
}
```

## Wiring (host)

`CachingBuilder` dùng pattern fluent từ extensions:

```csharp
// Bind CacheOption từ configuration, tạo builder và đăng ký ICacheService
// Xem Jarvis.Caching.Memory / Jarvis.Caching.Redis extensions:

// .UseMemoryCache()
// .UseDistributedRedisCache()
// .UseRedisMemCacheInvalidation(configuration)
```

Redis yêu cầu section `Cache:DistGroups:Redis` và `RedisConnectionManager` nội bộ package.

## Extension (Redis package)

| Extension | Mục đích |
|---|---|
| `UseMemoryCache` | In-memory layer |
| `UseDistributedRedisCache` | Redis dist cache theo DistGroups |
| `UseRedisMemCacheInvalidation` | Pub/sub invalidate memory |

Inject `ICacheService` sau khi `Build()` qua factory/host registration.
