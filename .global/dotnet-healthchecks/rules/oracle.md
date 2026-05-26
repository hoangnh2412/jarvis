---
name: dotnet-healthchecks-oracle
type: provider
dependencies:
  - AspNetCore.HealthChecks.Oracle
---

# Oracle Healthcheck Provider

## Purpose

Đăng ký Oracle readiness healthcheck.

---

## Rules

- Always dùng `AddOracle()`
- Always gắn:
  `HealthCheckTags.Readiness`
- Never hardcode connection string
- Prefer timeout từ core config

---

## Configuration

```json
{
  "HealthChecks": {
    "Readiness": {
      "Oracle": "ConnectionStrings:OracleDb"
    }
  }
}
```

---

## Example

```csharp
var keyPath = readiness.GetValue<string>("Oracle");

var connectionString = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthChecks.AddOracle(
        connectionString,
        name: "oracle",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```

---

## Validation

- Package installed
- AddOracle registered
- readiness tag applied
- endpoint `/health/ready` validated