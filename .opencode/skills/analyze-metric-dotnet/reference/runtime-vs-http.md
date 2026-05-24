# Runtime metrics vs HTTP / custom app metrics

## Ba lớp metric

| Lớp | Ví dụ | Dashboard |
|-----|-------|-----------|
| CLR runtime | `dotnet_*`, `process_runtime_dotnet_*` | Dotnet Runtime Metrics |
| HTTP (OTel) | `http.server.request.duration`, `http.server.active_requests` | Service / APM |
| Custom Jarvis | `sample.http.api.calls`, meter từ `IMetricInstrumentation` | App-defined |

## Quy tắc

1. **Runtime dashboard** giải thích GC, JIT, thread pool — **không** thay metric RPS/latency HTTP.
2. Spike **JIT** hoặc **GC pause** có thể **giải thích** tail latency — cần correlate với HTTP panel cùng time range.
3. Metric custom (`sample.*`) đi qua OTLP pipeline Jarvis — có thể **không** xuất hiện trên dashboard runtime.

## Jarvis stack

| Thành phần | Skill |
|------------|-------|
| OTLP metric, custom meter | [telemetry-dotnet](../../telemetry-dotnet/README.md) |
| Health Prometheus endpoint | [healthcheck-dotnet](../../healthcheck-dotnet/README.md) |
| Scaffold backend | [jarvis-dotnet](../../jarvis-dotnet/README.md) |

## Khi user hỏi "metric API của tôi"

- Nếu tên `sample.*` hoặc meter custom → telemetry-dotnet + backend Grafana của OTLP.
- Nếu tên `dotnet_*` → skill này + providers.

## Agent

- Nêu rõ đang nói **lớp nào** trước khi diễn giải.
- Không dùng `dotnet_exceptions_total` để suy ra HTTP 5xx rate nếu app swallow exception.
