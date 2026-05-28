---
name: caching-dotnet-otel-redis
description: Instrument Redis cache connections cho OpenTelemetry trace qua AddJarvisCachingDistributedRedisInstrumentation và AddJarvisCachingMemoryInvalidationRedisInstrumentation.
dependencies:
  - Jarvis.Caching.Redis
---

# OpenTelemetry — Redis cache

Instrument **từng** connection — không gộp invalidation với distributed cache.

## Registration

Trong `ConfigureTrace` (sau `AddJarvisOpenTelemetry`):

```csharp
using Jarvis.Caching.Redis.Extensions;

.ConfigureTrace(options =>
{
    options.AddJarvisCachingDistributedRedisInstrumentation(builder.Configuration);
    options.AddJarvisCachingMemoryInvalidationRedisInstrumentation();
})
```

`UseRedisMemoryCacheInvalidation()` phải chạy **trước** `Build()`.

Chi tiết OTEL host: [telemetry-dotnet/SKILL.md](../../../telemetry-dotnet/SKILL.md).
