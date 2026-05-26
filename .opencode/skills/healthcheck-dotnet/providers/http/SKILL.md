---
name: healthcheck-dotnet-http
description: Đăng ký HTTP endpoint readiness bằng custom IHealthCheck (IHttpClientFactory, GET). Dùng khi project .NET cần probe API/dịch vụ HTTP ngoài trên /health/ready.
dependencies:
  - System.Net.Http
---

# HTTP endpoint (custom IHealthCheck)

```json
"HealthChecks": {
  "Readiness": {
    "ServiceEndpoint": "Services:Sample:Endpoint"
  }
}
```

```csharp
public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
{
    try
    {
        var endpoint = _options.Endpoint;

        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

        return response.IsSuccessStatusCode
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy($"Endpoint returned {response.StatusCode}");
    }
    catch (Exception ex)
    {
        return HealthCheckResult.Unhealthy("Endpoint unreachable", ex);
    }
}
```
