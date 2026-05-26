---
name: dotnet-healthchecks-postgresql
type: provider
dependencies:
  - AspNetCore.HealthChecks.NpgSql
---

# PostgreSQL Healthcheck Provider

## Purpose

Đăng ký PostgreSQL readiness healthcheck.

---

## Rules

- Always dùng `AddNpgSql()`
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
      "Database": "ConnectionStrings:MainDb"
    }
  }
}
```

---

## Example

```csharp
var keyPath = readiness.GetValue<string>("Database");

var connectionString = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthChecks.AddNpgSql(
        connectionString,
        name: "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```

---

## Validation

- Package installed
- AddNpgSql registered
- readiness tag applied
- endpoint `/health/ready` validated