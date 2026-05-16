---
name: healthcheck-dotnet-mssql
description: Đăng ký SQL Server readiness qua AddSqlServer() (SELECT 1) và HealthChecks:Readiness. Dùng khi project .NET cần probe MSSQL trên /health/ready.
dependencies:
  - AspNetCore.HealthChecks.SqlServer
---

# SQL Server

```json
"HealthChecks": {
  "Readiness": {
    "SqlServer": "ConnectionStrings:MainDb"
  }
}
```

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
