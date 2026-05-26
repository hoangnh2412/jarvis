---
name: telemetry-dotnet-enrich
description: Đăng ký IEnrichTraceService và IEnrichLogService trong AddJarvisOpenTelemetry. Dùng khi cần tag trace và log scope từ HttpContext (user, tenant) qua UseJarvisOpenTelemetry.
dependencies:
  - Jarvis.OpenTelemetry
---

# Enrich trace / log

```csharp
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        services.AddScoped<IEnrichLogService, EnrichLogService>();
        services.AddScoped<IEnrichTraceService, EnrichTraceService>();
    })
    // …
    ;

// Pipeline:
app.UseJarvisOpenTelemetry();
```

```csharp
// Implement (hoặc kế thừa EnrichDataService từ Jarvis.Domain):
public sealed class EnrichTraceService(IHttpContextAccessor accessor) : IEnrichTraceService
{
    public Task<Dictionary<string, string>> ExtractAsync() => Task.FromResult(new Dictionary<string, string>());
}
```

Mẫu đầy đủ: [templates/enrich-service.cs](../../templates/enrich-service.cs).
