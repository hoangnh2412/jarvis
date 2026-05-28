---
name: caching-dotnet-redis-distributed
description: Bật Redis distributed cache qua UseRedisDistributedCache và Cache:DistributedGroups. Dùng khi project Jarvis cần L2 cache Redis theo nhóm cluster.
dependencies:
  - Jarvis.Caching.Redis
  - StackExchange.Redis
---

# Redis distributed cache

## appsettings

```json
{
  "Cache": {
    "DefaultDistributedGroup": "Default",
    "DefaultDistributedType": "Redis",
    "DistributedGroups": {
      "Redis": {
        "Default": {
          "Configuration": "127.0.0.1:6379",
          "InstanceName": "app_"
        }
      }
    },
    "Items": {
      "ProductDemo": {
        "Key": "Product:{tenantId}:{id}",
        "MemSeconds": 300,
        "DistributedSeconds": 3600,
        "DistributedGroup": "Default"
      }
    }
  }
}
```

## Registration

```csharp
builder.AddJarvisCaching()
    .UseRedisDistributedCache();
```

Item cần `DistributedSeconds > 0` để dùng Redis.
