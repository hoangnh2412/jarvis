---
name: healthcheck-dotnet-kafka
description: Đăng ký Kafka readiness qua AddKafka() (broker connectivity) và HealthChecks:Readiness. Dùng khi project .NET cần probe Kafka trên /health/ready.
dependencies:
  - AspNetCore.HealthChecks.Kafka
---

# Kafka

```json
"HealthChecks": {
  "Readiness": {
    "Kafka": "Messaging:Kafka:BootstrapServers"
  }
}
```

```csharp
var keyPath = readiness.GetValue<string>("Kafka");
var bootstrapServers = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(bootstrapServers))
{
    healthChecks.AddKafka(
        setup => setup.BootstrapServers = bootstrapServers,
        name: "kafka",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```
