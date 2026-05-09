# Jarvis OpenTelemetry — kiến trúc

Tài liệu mô tả **cấu trúc giải pháp**, **luồng đăng ký** và **điểm mở rộng**. Hướng dẫn cấu hình, env, checklist vận hành: [`opentelemetry-skill.md`](opentelemetry-skill.md).

## Mục tiêu thiết kế

- Gom **trace**, **metrics**, **logs** theo [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet) và OTLP.
- **Cấu hình** qua `IConfiguration` (section `OTEL`) + biến môi trường `OTEL_*` nơi SDK hỗ trợ.
- **Mở rộng** không sửa core: plug-in DI (`ITraceInstrumentation`, …) và package vệ tinh (Redis).

## Cấu trúc solution

| Thành phần | Vai trò |
|------------|---------|
| **`Jarvis.OpenTelemetry`** | Thư viện chính: hosting, options, instrumentation mặc định, middleware, semantic conventions. |
| **`Jarvis.OpenTelemetry.Instrumentation.StackExchangeRedis`** | Tùy chọn: đăng ký Redis trace qua `ITraceInstrumentation`; phụ thuộc `IConnectionMultiplexer`. |

Các ứng dụng (ví dụ **Sample**) tham chiếu project `Jarvis.OpenTelemetry` và gọi `AddJarvisOpenTelemetry` + `UseJarvisOpenTelemetry`.

## Bố cục package `Jarvis.OpenTelemetry`

```
Jarvis.OpenTelemetry/
├── Abstractions/          # Hợp đồng mở rộng (enrich, instrumentation, exporter)
├── Configuration/         # JarvisOpenTelemetryOptions + nhánh Tracing / Metric / Logging / Resource
├── Extensions/            # AddJarvisOpenTelemetry, UseJarvisOpenTelemetry
├── Hosting/               # JarvisOpenTelemetryHostBuilder
├── Instrumentations/    # Enrich HTTP header, path filter, sampler factory
├── Logging/             # LoggerExtensions (helper log exception)
├── Middleware/            # TraceEnrichmentMiddleware, LogEnrichmentMiddleware
└── SemanticConventions/   # Hằng attribute (HTTP, user, exception, …)
```

## Luồng bootstrap (startup)

1. **`AddJarvisOpenTelemetry(IServiceCollection, IConfiguration, configureServices?)`**
   - Bind `Configure<JarvisOpenTelemetryOptions>` từ section `OTEL`.
   - Snapshot `Get<JarvisOpenTelemetryOptions>()` để tạo `JarvisOpenTelemetryHostBuilder`.
   - Đăng ký enricher ASP.NET (`HttpRequestHeaderEnrichment`, `UserRequestEnrichment`, …).
   - Gọi `configureServices` để app đăng ký `IEnrichTraceService`, plug-in, v.v.

2. **`JarvisOpenTelemetryHostBuilder`**
   - `AddOpenTelemetry()` trên `IServiceCollection` → một `OpenTelemetryBuilder` nội bộ.
   - Trong constructor, đăng ký callback `ConfigureOpenTelemetryTracerProvider` / `MeterProvider` / `LoggerProvider` để khi host build sẽ gom mọi implementation `ITraceInstrumentation`, `ITraceExporter`, `IMetricInstrumentation`, `IMetricExporter`, `ILoggingExporter` từ DI.

3. Chuỗi fluent (do app gọi):
   - **`ConfigureResource`**: resource (service, env, attributes, host/container detector).
   - **`ConfigureTrace`**: sampler, ASP.NET Core + HttpClient instrumentation, filter path, enrich, OTLP/console.
   - **`ConfigureMetric`**: runtime/process/http/aspnet metrics, histogram view, OTLP/console.
   - **`ConfigureLogging`**: `AddLogging` → OpenTelemetry logger options + OTLP/console (**không** `ClearProviders` — để tích hợp Serilog/NLog sau).

4. **`UseJarvisOpenTelemetry`**: `TraceEnrichmentMiddleware`, `LogEnrichmentMiddleware` (tags từ `IEnrichTraceService` / scope log từ `IEnrichLogService`).

## Phân tách signal trong model cấu hình

- **`JarvisOpenTelemetryOptions`**: identity + `Attributes`, `Resource`, và ba nhánh **`Tracing`**, **`Metric`**, **`Logging`**.
- **`TraceSignalOptions`** (kế thừa `OtlpExporterOptions`): OTLP trace + sampler + `HttpTraceEnrichment`, `AspNetCoreInstrumentation`, `HttpClient`.
- **`MetricTelemetryOptions`**: OTLP metric + `HistogramAggregation`, `IncludeConsoleExporter`.
- **`OpenTelemetryLoggingOptions`**: OTLP log + cờ OpenTelemetry logger (`IncludeFormattedMessage`, …).

Snapshot options tại bước `AddJarvisOpenTelemetry`; exporter OTLP trong callback SDK map trực tiếp từ options đã bind vào `AddOtlpExporter(...)`.

## Điểm mở rộng (abstractions)

| Nhóm | Ví dụ |
|------|--------|
| Enrich trace (middleware + span) | `IEnrichTraceService`, `IAspNetCoreEnrichHttpRequest/Response`, `IUserInfoResolver` |
| Enrich log | `IEnrichLogService` |
| Plug-in provider | `ITraceInstrumentation`, `ITraceExporter`, `IMetricInstrumentation`, `IMetricExporter`, `ILoggingExporter` |

Plug-in nên đăng ký **singleton**; resolve khi OpenTelemetry build provider.

## Package Redis (vệ tinh)

- Implement `ITraceInstrumentation` với `AddRedisInstrumentation(connection)`.
- Extension `AddJarvisRedisTraceInstrumentation()` — không nằm trong core để tránh kéo StackExchange.Redis vào mọi host.

## Tham chiếu ngoài

- [OpenTelemetry Specification](https://opentelemetry.io/docs/specs/otel/)
- [OTLP Exporter (.NET)](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol)
