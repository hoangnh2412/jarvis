# Workflow: Khởi tạo telemetry

Áp dụng khi project ASP.NET Core **chưa** có Jarvis OpenTelemetry.

## Checklist

```text
- [ ] 1. Phân tích Program.cs, section OTEL, collector endpoint
- [ ] 2. Thêm project/package Jarvis.OpenTelemetry
- [ ] 3. Cấu hình appsettings OTEL (resource + signals)
- [ ] 4. AddJarvisOpenTelemetry + Configure*
- [ ] 5. Đăng ký enricher / plug-in cần thiết (providers/*)
- [ ] 6. app.UseJarvisOpenTelemetry()
- [ ] 7. Validate trace/metric/log tới collector
```

## Bước 1 — Phân tích

1. Đọc [docs/opentelemetry.md](../../../docs/opentelemetry.md) và `Program.cs`.
2. Xác định signals cần bật: trace, metric, log.
3. Collector OTLP endpoint (config hoặc `OTEL_EXPORTER_OTLP_*`).
4. Sampling, `ExcludedPathPrefixes`, header allowlist.
5. Instrumentation bổ sung: Redis, EF, custom plug-in.

## Bước 2 — Packages

```xml
<ProjectReference Include="..\Jarvis.OpenTelemetry\Jarvis.OpenTelemetry.csproj" />
```

Tùy chọn (xem `providers/*/SKILL.md`):

- `Jarvis.OpenTelemetry.Instrumentation.StackExchangeRedis`
- `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- `OpenTelemetry.Instrumentation.StackExchangeRedis`

## Bước 3 — appsettings.json

Dùng [templates/otel-appsettings.json](../templates/otel-appsettings.json).

Ma trận section:

| Signal | Section | Nội dung chính |
|---|---|---|
| Resource | `OTEL` | `Name`, `Namespace`, `InstanceId`, `Attributes`, `Resource` |
| Trace | `OTEL:Tracing` | OTLP, `Sampler`, `TraceIdRatio`, `HttpTraceEnrichment`, `AspNetCoreInstrumentation` |
| Metrics | `OTEL:Metric` | OTLP, `HistogramAggregation` |
| Logs | `OTEL:Logging` | OTLP, `IncludeFormattedMessage`, `IncludeScopes` |

OTLP đăng ký khi có `Endpoint` trong config **hoặc** env `OTEL_EXPORTER_OTLP_*`.

## Bước 4 — Program.cs

Dùng [templates/program-setup.cs](../templates/program-setup.cs):

```csharp
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        // enrich + plug-in — xem providers/enrich, providers/redis, …
    })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace(/* instrumentation từ providers */)
    .ConfigureMetric();

var app = builder.Build();
// … middleware pipeline …
app.UseJarvisOpenTelemetry();
```

**Thứ tự:** `AddJarvisOpenTelemetry` trước `Build()`; `UseJarvisOpenTelemetry` sau routing nếu enrich phụ thuộc endpoint.

## Bước 5 — Enricher & plug-in

| Nhu cầu | Provider |
|---|---|
| Tag trace + log scope từ user/tenant | [providers/enrich](../providers/enrich/SKILL.md) |
| Trace Redis | [providers/redis](../providers/redis/SKILL.md) |
| Trace EF Core | [providers/entityframework](../providers/entityframework/SKILL.md) |
| Custom tracer | [providers/trace-plugin](../providers/trace-plugin/SKILL.md) |

## Bước 6 — Validate

- Span xuất hiện khi gọi API (trace bật + sampler không drop hết)
- Metric/log tới collector khi endpoint/env hợp lệ
- `/health`, `/swagger` bị loại khỏi trace nếu đã cấu hình `ExcludedPathPrefixes`
- Không có cookie/token trên span (kiểm tra allowlist)

## Anti-patterns

- `AlwaysOnSampler` + traffic lớn không kiểm soát
- Capture header rộng → PII trên span
- Plug-in đăng ký Scoped thay vì Singleton
- Quên `UseJarvisOpenTelemetry` nhưng vẫn cần `IEnrich*`
