---
name: dotnet-healthchecks-mysql
type: provider
dependencies:
  - AspNetCore.HealthChecks.MySql
---

# MySQL Healthcheck Provider

## Purpose

Đăng ký MySQL readiness healthcheck.

---

## Rules

- Always dùng `AddMySql()`
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
      "MySql": "ConnectionStrings:MySqlDb"
    }
  }
}
```

---

## Example

```csharp
var keyPath = readiness.GetValue<string>("MySql");

var connectionString = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthChecks.AddMySql(
        connectionString,
        name: "mysql",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```

---

## Validation

- Package installed
- AddMySql registered
- readiness tag applied
- endpoint `/health/ready` validated