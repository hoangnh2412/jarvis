# Metric name families

Dashboard **Dotnet Runtime Metrics** (Grafana) dùng regex hỗ trợ **hai họ** tên do khác nguồn instrumentation.

## Bảng đối chiếu

| Khái niệm | Built-in .NET 9+ | OpenTelemetry.Instrumentation.Runtime |
|-----------|------------------|--------------------------------------|
| Exceptions | `dotnet_exceptions_total` | `process_runtime_dotnet_exceptions_count_total` |
| Assemblies | `dotnet_assembly_count` | `process_runtime_dotnet_assemblies_count` |
| GC collections | `dotnet_gc_collections_total` | `process_runtime_dotnet_gc_collections_count_total` |
| GC heap | `dotnet_gc_heap_size_bytes` | `process_runtime_dotnet_gc_heap_size_bytes` |
| GC committed | `dotnet_gc_committed_bytes` | `process_runtime_dotnet_gc_committed_bytes` |
| GC pause | `dotnet_gc_pause_time_seconds_total` | `process_runtime_dotnet_gc_pause_time_seconds_total` |
| Alloc rate | `dotnet_gc_alloc_rate_bytes` | `process_runtime_dotnet_gc_alloc_rate_bytes` |
| Thread pool threads | `dotnet_threadpool_num_threads` | `process_runtime_dotnet_thread_pool_threads_count` |
| Thread pool queue | `dotnet_threadpool_queue_length` | `process_runtime_dotnet_thread_pool_queue_length` |
| Process CPU | `dotnet_process_cpu_seconds_total` | — (built-in .NET 9+) |
| CPU count | `dotnet_process_cpu_count` | — |
| JIT methods | `dotnet_jit_methods_compiled_total` | `process_runtime_dotnet_jit_methods_compiled_count_total` |
| JIT IL | `dotnet_jit_il_bytes_total` | `process_runtime_dotnet_jit_il_compiled_size_bytes_total` |
| JIT time | `dotnet_jit_time_in_jit_seconds_total` | `process_runtime_dotnet_jit_compilation_time_seconds_total` |

## PromQL chung

```promql
{__name__=~"dotnet_.*|process_runtime_dotnet_.*", job="$job", instance="$instance"}
```

## Labels thường gặp

- `job`, `instance` — scrape target
- `generation` — GC gen (0/1/2) trên collection metrics
- `exception_type` — loại exception (nếu có)

## Nguồn metric

| Nguồn | Khi nào |
|-------|---------|
| .NET 9+ built-in | Runtime tự export `dotnet_*` |
| OTel Runtime package | `process_runtime_dotnet_*` trên .NET 6/7/8+ |
| Cả hai | Có thể trùng semantic — chọn một họ nhất quán trong alert |

Chi tiết panel: [providers/](../providers/).
