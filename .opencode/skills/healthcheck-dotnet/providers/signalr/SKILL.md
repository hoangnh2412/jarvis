---
name: healthcheck-dotnet-signalr
description: Đăng ký SignalR hub readiness qua AddSignalRHub() và HealthChecks:Readiness. Dùng khi project .NET cần probe hub SignalR trên /health/ready.
dependencies:
  - AspNetCore.HealthChecks.SignalR
---

# SignalR

```json
"HealthChecks": {
  "Readiness": {
    "SignalRHub": "https://localhost:5001/hubs/notifications"
  }
}
```

```csharp
var keyPath = readiness.GetValue<string>("SignalRHub");
var hubUrl = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(hubUrl))
{
    healthChecks.AddSignalRHub(
        hubUrl,
        name: "signalr",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```
