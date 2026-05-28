# Workflow: Panel trống / không thấy metric

Áp dụng khi Grafana **không có series** hoặc PromQL trả rỗng.

## Checklist

```text
- [ ] 1. job / instance / time range
- [ ] 2. Prometheus có target scrape?
- [ ] 3. Đúng họ tên metric (dotnet_* vs process_runtime_dotnet_*)
- [ ] 4. .NET version vs panel CPU (.NET 9+)
- [ ] 5. OTLP remote write / exporter path
```

## Bước 1 — Grafana variables

| Kiểm tra | Gợi ý |
|----------|--------|
| `job` | Khớp label Prometheus của pod/process |
| `instance` | `host:port` hoặc pod name tùy scrape config |
| Time range | Mở rộng; metric mới sau deploy có delay |

## Bước 2 — Prometheus

```promql
{__name__=~"dotnet_.*|process_runtime_dotnet_.*", job="<job>"}
```

- Không có sample → process chưa export runtime metric hoặc scrape sai target.
- Chỉ có `process_runtime_dotnet_*` → app .NET 6/7/8 + OTel Runtime, chưa có built-in `dotnet_*`.

## Bước 3 — .NET 9+ CPU panels

Panel **CPU Seconds by Job** / **Available CPUs** dùng `dotnet_process_cpu_*` — **không có** trên runtime cũ nếu chỉ cài OTel Instrumentation.Runtime.

## Bước 4 — Jarvis app

Runtime metric ≠ metric từ `ConfigureMetric()`:

- App Jarvis export OTLP → backend khác (Mimir/Tempo stack) — dashboard runtime vẫn cần **runtime instrumentation** trên process.
- Custom counter (`sample.http.api.calls`) — xem stack metric nghiệp vụ, không kỳ vọng trên dashboard Dotnet Runtime.

## Bước 5 — Health Prometheus (khác)

`Jarvis.HealthChecks` có `/health/prometheus` — metric health check, **không** thay `dotnet_*` runtime. Xem [healthcheck-dotnet](../../healthcheck-dotnet/README.md).

## Output

Liệt kê đã kiểm tra gì, thiếu gì, bước fix cụ thể (bật runtime instrumentation, sửa scrape, đổi query regex).
