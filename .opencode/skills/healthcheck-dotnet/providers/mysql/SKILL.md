---
name: healthcheck-dotnet-mysql
description: Đăng ký MySQL readiness qua AddMySql() và HealthChecks:Readiness. Dùng khi project .NET cần probe MariaDB/MySQL trên /health/ready.
dependencies:
  - AspNetCore.HealthChecks.MySql
---

# MySQL

```json
"HealthChecks": {
  "Readiness": {
    "MySql": "ConnectionStrings:MySqlDb"
  }
}
```

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
