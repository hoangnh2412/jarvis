---
name: healthcheck-dotnet-oracle
description: Đăng ký Oracle readiness qua AddOracle() và HealthChecks:Readiness. Dùng khi project .NET cần probe Oracle DB trên /health/ready.
dependencies:
  - AspNetCore.HealthChecks.Oracle
---

# Oracle

```json
"HealthChecks": {
  "Readiness": {
    "Oracle": "ConnectionStrings:OracleDb"
  }
}
```

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
