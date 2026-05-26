---
name: dotnet-healthchecks-minio
type: provider
dependencies:
  - System.Net.Http
---

# MinIO Healthcheck Provider

## Purpose

Kiểm tra readiness của MinIO qua endpoint:

```text
/minio/health/live
```

Sử dụng custom `IHealthCheck`.

---

## Rules

- Always dùng custom `IHealthCheck`
- Always gắn:
  `HealthCheckTags.Readiness`
- Always normalize endpoint:
  - prepend `http://` nếu thiếu
  - append `/` nếu thiếu
  - append `minio/health/live`
- Healthy status:
  - `200`
  - `400`
  - `403`
- Never upload/download object trong readiness
- Prefer timeout từ core config
- HttpClient luôn đăng ký DI qua IHttpClientFactory

---

## Configuration

```json
{
  "HealthChecks": {
    "Readiness": {
      "MinIO": "MinIO:Endpoint"
    }
  }
}
```

---

## Example

```csharp
var endpoint = _options.Endpoint;

if (!endpoint.StartsWith("http"))
    endpoint = $"http://{endpoint}";

if (!endpoint.EndsWith("/"))
    endpoint += "/";

endpoint += "minio/health/live";

using var response =
    await _httpClient.GetAsync(endpoint, cancellationToken);

return response.StatusCode switch
{
    HttpStatusCode.OK
    or HttpStatusCode.BadRequest
    or HttpStatusCode.Forbidden
        => HealthCheckResult.Healthy(),

    _ => HealthCheckResult.Unhealthy()
};
```