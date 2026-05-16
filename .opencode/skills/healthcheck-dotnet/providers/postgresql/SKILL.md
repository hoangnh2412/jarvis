---
name: healthcheck-dotnet-postgresql
description: Đăng ký PostgreSQL readiness qua AddNpgSql() và HealthChecks:Readiness. Dùng khi project .NET cần probe Npgsql trên /health/ready.
dependencies:
  - AspNetCore.HealthChecks.NpgSql
---

# PostgreSQL

```json
"HealthChecks": {
  "Readiness": {
    "Database": "ConnectionStrings:MainDb"
  }
}
```

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
