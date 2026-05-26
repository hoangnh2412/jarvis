---
name: dotnet-healthchecks-sqlserver
type: provider
dependencies:
  - AspNetCore.HealthChecks.SqlServer
---

# SqlServer Healthcheck Provider

## Purpose

Đăng ký SQL Server readiness healthcheck.

---

## Rules

- Always dùng `AddSqlServer()`
- Always gắn:
  `HealthCheckTags.Readiness`
- Prefer lightweight query:
  `SELECT 1`
- Never dùng heavy query trong readiness
- Never hardcode connection string
- Prefer timeout từ core config

---

## Configuration

```json
{
  "HealthChecks": {
    "Readiness": {
      "SqlServer": "ConnectionStrings:MainDb"
    }
  }
}
```

---

## Example

```csharp
var keyPath = readiness.GetValue<string>("SqlServer");

var connectionString = configuration[keyPath!];

if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthChecks.AddSqlServer(
        connectionString: connectionString,
        healthQuery: "SELECT 1;",
        name: "sqlserver",
        failureStatus: HealthStatus.Unhealthy,
        tags: [HealthCheckTags.Readiness],
        timeout: probeTimeout);
}
```

---

## Validation

- Package installed
- AddSqlServer registered
- readiness tag applied
- endpoint `/health/ready` validated