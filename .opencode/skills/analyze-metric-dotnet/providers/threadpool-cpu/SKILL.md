# Provider: CPU & Thread Pool

Nhóm panel **Thread Pool** và **CPU** — starvation thread pool, CPU process.

## Metrics

| Panel | `dotnet_*` | `process_runtime_dotnet_*` |
|-------|------------|------------------------------|
| Thread pool threads | `dotnet_threadpool_num_threads` | `process_runtime_dotnet_thread_pool_threads_count` |
| Queue length | `dotnet_threadpool_queue_length` | `process_runtime_dotnet_thread_pool_queue_length` |
| CPU time | `dotnet_process_cpu_seconds_total` | (thường qua process metrics khác) |
| CPU count | `dotnet_process_cpu_count` | — |

## Lưu ý .NET 9+

Panel **CPU Seconds by Job** và **Available CPUs** dùng `dotnet_process_cpu_*` — **chỉ có** khi runtime built-in metric (.NET 9+). App .NET 8 + chỉ OTel Runtime có thể **thiếu** panel CPU họ `dotnet_*`.

## Ý nghĩa

- **Queue length** cao kéo dài: thread pool starvation — sync-over-async, blocking I/O trên thread pool.
- **Num threads** tăng: pool mở rộng — đối chiếu queue.
- **CPU process**: CPU time của process CLR — không thay host CPU toàn node.

## Đọc đồ thị

| Pattern | Gợi ý |
|---------|--------|
| Queue length > 0 bền | Blocking — profile, trace slow path |
| Threads max + queue cao | Cần tăng MinThreads hoặc fix blocking (tạm thời) |
| CPU process cao + queue thấp | CPU-bound work trên pool — optimize hot path |

## Quy tắc agent

- Correlate với latency P99 nếu user có HTTP dashboard.
- Không khuyên tăng `ThreadPool.SetMinThreads` là fix đầu tiên — tìm blocking root cause.

## PromQL mẫu

```promql
{__name__=~"dotnet_threadpool_queue_length|process_runtime_dotnet_thread_pool_queue_length", job="$job"}
```
