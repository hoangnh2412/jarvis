---
name: dotnet-healthchecks-http
type: provider
dependencies:
  - System.Net.Http
---

# HTTP Healthcheck Rule

## Purpose

Rule dùng chung cho healthcheck dạng HTTP endpoint.

Các provider như:
- external API
- internal service
- gateway
- webhook

phải reuse pattern này thay vì tự implement riêng.

---

## Rules

- Always dùng `IHttpClientFactory` để CreateClient
- Always dùng async HTTP request
- Always support cancellation token
- Always timeout request
- Always gắn:
  `HealthCheckTags.Readiness`
- Prefer `GET`
- Prefer endpoint configurable từ config
- Never hardcode domain/url
- Never validate business data
- Never parse business response

---

## Healthy Status

Mặc định:

- `200-299` => Healthy

Provider-specific rule có thể override thêm:
- `400`
- `403`

nếu service behavior yêu cầu.

---

## Configuration

```json
{
  "HealthChecks": {
    "Readiness": {
      "ServiceEndpoint": "Services:Sample:Endpoint"
    }
  }
}
```

---

## Example

```csharp
public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
{
    try
    {
        var endpoint = _options.Endpoint;

        using var response =
            await _httpClient.GetAsync(
                endpoint,
                cancellationToken);

        return response.IsSuccessStatusCode
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy(
                $"Endpoint returned {response.StatusCode}");
    }
    catch (Exception ex)
    {
        return HealthCheckResult.Unhealthy(
            "Endpoint unreachable",
            ex);
    }
}
```