---
name: dotnet-healthchecks-signalr
type: provider
dependencies:
  - AspNetCore.HealthChecks.SignalR
---

# SignalR Healthcheck Provider

## Purpose

Đăng ký SignalR readiness healthcheck.

---

## Rules

- Always dùng `AddSignalRHub()`
  hoặc API tương đương version package
- Always gắn:
  `HealthCheckTags.Readiness`
- Prefer validate hub availability
- Never validate active user/session business logic
- Prefer timeout từ core config

---

## Configuration

```json
{
  "HealthChecks": {
    "Readiness": {
      "SignalRHub": "https://localhost:5001/hubs/notifications"
    }
  }
}
```

---

## Example

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

---

## Validation

- Package installed
- SignalR endpoint reachable
- readiness tag applied
- endpoint `/health/ready` validated