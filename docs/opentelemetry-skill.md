# SKILL: Jarvis.OpenTelemetry — tích hợp và cấu hình

## 1. Purpose

Hướng dẫn agent/developer **gắn** `Jarvis.OpenTelemetry` vào ASP.NET Core: đăng ký DI (`AddJarvisOpenTelemetry`), pipeline (`ConfigureResource` / `ConfigureTrace` / `ConfigureMetric` / `ConfigureLogging`), middleware (`UseJarvisOpenTelemetry`), cấu hình section **`OTEL`**, biến môi trường OTLP, và **plug-in** (`ITraceInstrumentation`, …). Kiến trúc module, thư mục, luồng startup: [`opentelemetry.md`](opentelemetry.md).

## 2. Scope

Skill này áp dụng cho:

- Ứng dụng **.NET 9** / **ASP.NET Core** dùng `WebApplication` / `HostApplicationBuilder`.
- Repo có project reference **`Jarvis.OpenTelemetry`** (và tùy chọn **`Jarvis.OpenTelemetry.Instrumentation.StackExchangeRedis`** khi cần trace Redis).
- Cấu hình telemetry qua **`appsettings`** (`OTEL`) và/hoặc **`OTEL_*`** env.

Không áp dụng cho:

- Thay thế hoàn toàn collector/vendor SDK riêng mà không qua OpenTelemetry .NET (Jarvis không wrap vendor).
- Ứng dụng không dùng pipeline chuẩn `IServiceCollection` + `IApplicationBuilder` của ASP.NET Core.

## 3. Role

Bạn là **Senior Backend Engineer** hoặc **Reviewer** chịu trách nhiệm:

- Bật đúng signal (trace/metric/log), không lộ PII, sampling hợp lý.
- Đăng ký plug-in và enricher đúng vòng đời DI (ưu tiên singleton cho instrumentation/exporter).

## 4. Input

Dữ liệu đầu vào expected:

- **`Program.cs`**: chỗ gọi `AddJarvisOpenTelemetry`, chuỗi `Configure*`, `UseJarvisOpenTelemetry`.
- **`appsettings.json`**: section **`OTEL`** (và env tương ứng).
- **Requirement**: endpoint collector, sampling, route cần loại khỏi trace, header được phép lên span.
- (Tùy chọn) Danh sách dependency cần instrumentation thêm (Redis, …).

## 5. Output

Format đầu ra bắt buộc:

- **Code**: chỉnh `Program.cs`, extension đăng ký readiness-style cho plug-in, project reference nếu thêm package Redis.
- **Markdown**: cập nhật / giải thích keys `OTEL` khi đổi config.
- **Checklist** (cuối task): OTLP bật khi có `Endpoint` hoặc env; sampling; allowlist header; plug-in singleton.

## 6. Rules

Các nguyên tắc bắt buộc:

- **Always** gọi `AddJarvisOpenTelemetry(configuration, configureServices)` rồi mới chuỗi `.ConfigureResource()` → `.ConfigureLogging()` / `.ConfigureTrace()` / `.ConfigureMetric()` theo nhu cầu.
- **Always** gọi `app.UseJarvisOpenTelemetry()` khi dùng enrich middleware (trace tag + log scope từ `IEnrich*`).
- **Always** đăng ký plug-in (`ITraceInstrumentation`, …) **trong** callback `configureServices` của `AddJarvisOpenTelemetry` (hoặc trước khi `Build()`), để có mặt khi host build provider.
- **Prefer** tách cấu hình theo signal: trace-only dưới **`OTEL:Tracing`**, metric dưới **`OTEL:Metric`**, log dưới **`OTEL:Logging`** (xem bảng dưới).
- **Prefer** `HttpTraceEnrichment` **allowlist** header; không copy toàn bộ header mặc định.
- **Never** hard-code secrets collector vào repo; dùng env / secret store + `Headers` OTLP nếu cần.
- **Never** giả định `ClearProviders` trong Jarvis logging — Jarvis **không** xóa provider; Serilog/NLog thêm riêng ngoài package này.
- **When** dùng Redis package: đảm bảo **`IConnectionMultiplexer`** đã đăng ký trước `AddJarvisRedisTraceInstrumentation()`.

## 7. Process

Các bước xử lý:

1. **Analyze** — Đọc [`opentelemetry.md`](opentelemetry.md), `Program.cs`, section `OTEL`; xác định signal và collector.
2. **Design** — Chọn sampler, `ExcludedPathPrefixes`, `HttpTraceEnrichment`; có cần package Redis / custom `ITraceInstrumentation` không.
3. **Implement**
   - `AddJarvisOpenTelemetry` + `Configure*`.
   - `UseJarvisOpenTelemetry` sau middleware routing nếu enrich phụ thuộc endpoint (theo convention app).
   - Đăng ký `IEnrichTraceService` / `IEnrichLogService` / plug-in trong callback.
4. **Validate** — Trace có span; metric/log tới collector khi `Endpoint` hoặc `OTEL_EXPORTER_OTLP_*` hợp lệ; kiểm tra checklist production (mục 8).

## 8. Constraints

Giới hạn:

- **Performance**: sampling `ParentBasedRatio` + `TraceIdRatio` giảm tải; filter path giảm span nhiễu.
- **Cardinality**: chỉ allowlist header; tránh tag động (user id thô trên mọi span) nếu policy không cho phép.
- **Compatibility**: options snapshot lúc `AddJarvisOpenTelemetry`; đổi config lúc chạy cần restart hoặc cơ chế reload tùy host (Jarvis không tự reload provider).

**Checklist production (trước khi release):**

- Sampling: `Tracing:Sampler` + `TraceIdRatio` hợp lý với traffic.
- PII: allowlist header chặt; không đưa cookie/token lên span.
- Cardinality: tag ổn định; tránh high-cardinality tùy tiện.
- Noise: `ExcludedPathPrefixes` cho health/swagger/UI nếu cần.
- Secrets: OTLP headers qua env/secret store, không commit.
- Endpoint: collector đúng môi trường; không dựa localhost ngầm trên production.

## 9. Examples

### Input

- App cần OTLP tại `http://collector:4317`, bỏ trace cho `/health`, gửi `x-request-id` lên span.

### Output

**`appsettings.json` — ma trận section `OTEL` (signal × config):**

| Signal | Section | Nội dung chính |
|--------|---------|----------------|
| Resource | `OTEL` | `Name`, `Namespace`, `InstanceId`, `Attributes`, `Resource` (host/container detector) |
| Trace | `OTEL:Tracing` | OTLP (`Endpoint`, `TimeoutMilliseconds`, `BatchExportProcessorOptions`, …), `Sampler`, `TraceIdRatio`, `HttpTraceEnrichment`, `AspNetCoreInstrumentation`, `HttpClient` |
| Metrics | `OTEL:Metric` | OTLP, `HistogramAggregation`, `IncludeConsoleExporter` |
| Logs | `OTEL:Logging` | OTLP, `IncludeConsoleExporter`, `IncludeFormattedMessage`, `IncludeScopes`, `ParseStateValues` |

**Biến môi trường (OTLP / SDK)** — Jarvis vẫn đăng ký OTLP khi có `Endpoint` trong config **hoặc** env tương ứng (xem `JarvisOpenTelemetryHostBuilder`):

| Mục đích | Ví dụ |
|----------|--------|
| OTLP chung | `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_HEADERS`, `OTEL_EXPORTER_OTLP_PROTOCOL` |
| Trace | `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` |
| Metrics | `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT` |
| Logs | `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT` |
| Resource | `OTEL_RESOURCE_ATTRIBUTES`, `OTEL_SERVICE_NAME` |

Tham chiếu spec: [SDK environment variables](https://opentelemetry.io/docs/specs/otel/configuration/sdk-environment-variables/).

**Plug-in (đăng ký trước khi host build):**

| Interface | Mục đích |
|-----------|----------|
| `ITraceInstrumentation` | Thêm instrumentation lên `TracerProviderBuilder` |
| `ITraceExporter` | Thêm exporter trace |
| `IMetricInstrumentation` | Thêm instrumentation metric |
| `IMetricExporter` | Thêm exporter metric |
| `ILoggingExporter` | Cấu hình thêm `LoggerProviderBuilder` |

**Redis (tùy chọn):**

```csharp
// ProjectReference: Jarvis.OpenTelemetry.Instrumentation.StackExchangeRedis
// services phải có IConnectionMultiplexer
services.AddJarvisRedisTraceInstrumentation();
```

**`Program.cs` (rút gọn):**

```csharp
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, s =>
    {
        s.AddScoped<IEnrichLogService, EnrichLogService>();
        s.AddScoped<IEnrichTraceService, EnrichTraceService>();
        // s.AddJarvisRedisTraceInstrumentation();
    })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace()
    .ConfigureMetric();

// ...

app.UseJarvisOpenTelemetry();
```

## 10. Anti-patterns

Những điều cần tránh:

- Đăng ký `ITraceInstrumentation` **Scoped** rồi expect state an toàn trong provider singleton.
- Bật capture header rộng → token/cookie lên span (PII).
- `AlwaysOnSampler` + traffic cực lớn mà không có tail/ratio (chi phí và noise).
- Kỳ vọng đổi `appsettings` lúc chạy tự áp vào OpenTelemetry provider mà không restart (mặc định không).
- Quên `UseJarvisOpenTelemetry` nhưng vẫn cần tag từ `IEnrichTraceService` / scope từ `IEnrichLogService`.
