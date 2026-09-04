---
name: healthcheck-dotnet-rabbitmq
description: Đăng ký RabbitMQ readiness qua AddRabbitMQ() và HealthChecks:Readiness. Dùng khi project .NET cần probe message broker trên /health/ready.
dependencies:
  - AspNetCore.HealthChecks.RabbitMQ
---

# RabbitMQ

```json
"HealthChecks": {
  "Readiness": {
    "RabbitMQ": "Messaging:RabbitMQ:Uri"
  }
}
```

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
