---
name: healthcheck-dotnet-minio
description: Đăng ký MinIO readiness bằng custom IHealthCheck (GET minio/health/live). Dùng khi project .NET cần probe object storage MinIO trên /health/ready.
dependencies:
  - System.Net.Http
---

# MinIO (custom IHealthCheck)

```json
"HealthChecks": {
  "Readiness": {
    "MinIO": "MinIO:Endpoint"
  }
}
```

```csharp
// Trong IHealthCheck.CheckHealthAsync:
var endpoint = _options.Endpoint;

if (!endpoint.StartsWith("http"))
    endpoint = $"http://{endpoint}";

if (!endpoint.EndsWith("/"))
    endpoint += "/";

endpoint += "minio/health/live";

using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

return response.StatusCode switch
{
    HttpStatusCode.OK
    or HttpStatusCode.BadRequest
    or HttpStatusCode.Forbidden
        => HealthCheckResult.Healthy(),
    _ => HealthCheckResult.Unhealthy()
};
```

```csharp
healthChecks.AddCheck<MinIOHealthCheck>(
    "minio",
    failureStatus: HealthStatus.Unhealthy,
    tags: [HealthCheckTags.Readiness],
    timeout: probeTimeout);
```
