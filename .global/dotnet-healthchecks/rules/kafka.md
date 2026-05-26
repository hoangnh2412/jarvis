---
name: dotnet-healthchecks-kafka
type: provider
dependencies:
  - AspNetCore.HealthChecks.Kafka
---

# Kafka Healthcheck Provider

## Purpose

Đăng ký Kafka readiness healthcheck.

---

## Rules

- Always dùng `AddKafka()`
- Always gắn:
  `HealthCheckTags.Readiness`
- Never hardcode bootstrap servers
- Prefer timeout từ core config
- Prefer validate broker connectivity thay vì topic business logic
- Never dùng liveness cho Kafka dependency

---

## Configuration

```json
{
  "HealthChecks": {
    "Readiness": {
      "Kafka": "Messaging:Kafka:BootstrapServers"
    }
  }
}
```

---

## Example

```csharp
var keyPath = readiness.GetValue<string>("Kafka");

var bootstrapServers = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(bootstrapServers))
{
    healthChecks.AddKafka(
        setup =>
        {
            setup.BootstrapServers = bootstrapServers;
        },
        name: "kafka",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```

---

## Validation

- Package installed
- AddKafka registered
- readiness tag applied
- broker connectivity validated
- endpoint `/health/ready` validated