---
name: dotnet-healthchecks-rabbitmq
type: provider
dependencies:
  - AspNetCore.HealthChecks.RabbitMQ
---

# RabbitMQ Healthcheck Provider

## Purpose

Đăng ký RabbitMQ readiness healthcheck.

---

## Rules

- Always dùng `AddRabbitMQ()`
- Always gắn:
  `HealthCheckTags.Readiness`
- Always dùng connection string/configuration từ config
- Never hardcode credentials
- Prefer timeout từ core config
- Prefer dùng dedicated readiness connection thay vì reuse application channel

---

## Configuration

```json
{
  "HealthChecks": {
    "Readiness": {
      "RabbitMQ": "Messaging:RabbitMQ:Uri"
    }
  }
}
```

---

## Example

```csharp
var keyPath = readiness.GetValue<string>("RabbitMQ");

var rabbitUri = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(rabbitUri))
{
    healthChecks.AddRabbitMQ(
        rabbitConnectionString: rabbitUri,
        name: "rabbitmq",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```

---

## Validation

- Package installed
- AddRabbitMQ registered
- readiness tag applied
- endpoint `/health/ready` validated