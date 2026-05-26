---
name: healthcheck-dotnet-redis
description: Đăng ký Redis readiness qua AddRedis() và HealthChecks:Readiness. Dùng khi project .NET cần probe cache Redis trên /health/ready.
dependencies:
  - AspNetCore.HealthChecks.Redis
---

# Redis

```json
"HealthChecks": {
  "Readiness": {
    "Redis": "Cache:Redis:Configuration"
  }
}
```

```csharp
var keyPath = readiness.GetValue<string>("Redis");
var redisConnection = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(redisConnection))
{
    healthChecks.AddRedis(
        redisConnection,
        name: "redis",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```
