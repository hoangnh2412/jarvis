---
name: dotnet-healthchecks-redis
type: provider
dependencies:
  - AspNetCore.HealthChecks.Redis
---

# Redis Healthcheck Provider

## Purpose

Đăng ký Redis readiness healthcheck.

---

## Rules

- Always dùng `AddRedis()`
- Always gắn:
  `HealthCheckTags.Readiness`
- Never hardcode Redis connection string
- Prefer timeout từ core config
- Prefer dedicated healthcheck connection nếu production workload lớn
- Never query business data trong readiness

---

## Configuration

```json
{
  "HealthChecks": {
    "Readiness": {
      "Redis": "Cache:Redis:Configuration"
    }
  }
}
```

---

## Example

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

---

## Validation

- Package installed
- AddRedis registered
- readiness tag applied
- endpoint `/health/ready` validated