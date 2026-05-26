# Workflow: Thêm instrumentation / enricher

Áp dụng khi project **đã có** `AddJarvisOpenTelemetry` và cần bổ sung một capability mới.

## Checklist

```text
- [ ] 1. Xác định provider (enrich | redis | entityframework | trace-plugin | …)
- [ ] 2. Đọc providers/<provider>/SKILL.md
- [ ] 3. Cài package / project reference
- [ ] 4. Cập nhật appsettings OTEL (nếu cần)
- [ ] 5. Đăng ký trong configureServices hoặc ConfigureTrace/Metric
- [ ] 6. Validate signal tương ứng
```

## Bước 1 — Chọn provider

Đọc **chỉ** file atomic trong `providers/`:

```text
providers/enrich/SKILL.md
providers/redis/SKILL.md
providers/entityframework/SKILL.md
providers/trace-plugin/SKILL.md
providers/metric-plugin/SKILL.md
providers/logging-plugin/SKILL.md
```

## Bước 2 — NuGet / ProjectReference

Cài dependency từ frontmatter `dependencies` của provider SKILL.

## Bước 3 — Registration

| Loại | Vị trí đăng ký |
|---|---|
| Enricher, `ITraceInstrumentation` | Callback `configureServices` của `AddJarvisOpenTelemetry` |
| EF / Redis OTEL package | `ConfigureTrace(options => …)` |
| `IMetricInstrumentation` | Callback `configureServices` |
| `ILoggingExporter` | Callback `configureServices` |

**Redis:** `IConnectionMultiplexer` phải đăng ký **trước** Redis instrumentation.

## Bước 4 — Config (nếu cần)

- Trace: `OTEL:Tracing` — sampler, `ExcludedPathPrefixes`, `HttpTraceEnrichment`
- Không thêm secret collector vào repo

## Bước 5 — Validate

- Enricher: tag/scope xuất hiện trên request có `HttpContext`
- Redis/EF: span con khi gọi Redis/DB
- Custom plug-in: không lỗi DI lúc build provider

## Anti-patterns

- Đăng ký plug-in sau `Build()` — provider không thấy implementation
- Redis trace mà chưa có `IConnectionMultiplexer`
- Đổi `appsettings` runtime kỳ vọng tự reload OTEL provider
