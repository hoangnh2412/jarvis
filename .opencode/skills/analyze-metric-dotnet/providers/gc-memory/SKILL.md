# Provider: GC & Memory

Nhóm panel **GC** và **Memory** — thường dùng khi nghi ngờ memory pressure hoặc GC pause.

## Metrics chính

| Khái niệm | `dotnet_*` | `process_runtime_dotnet_*` |
|-----------|------------|------------------------------|
| GC collections | `dotnet_gc_collections_total` | `process_runtime_dotnet_gc_collections_count_total` |
| GC heap size | `dotnet_gc_heap_size_bytes` | `process_runtime_dotnet_gc_heap_size_bytes` |
| GC committed | `dotnet_gc_committed_bytes` | `process_runtime_dotnet_gc_committed_bytes` |
| GC pause | `dotnet_gc_pause_time_seconds_total` | `process_runtime_dotnet_gc_pause_time_seconds_total` |
| Alloc rate | `dotnet_gc_alloc_rate_bytes` | `process_runtime_dotnet_gc_alloc_rate_bytes` |

## Ý nghĩa

- **Collections (Gen0/1/2)**: tần suất GC — Gen2 cao kéo dài → pressure / large object.
- **Heap / committed**: footprint bộ nhớ managed — committed tăng dài hạn đáng theo dõi.
- **Pause time**: ảnh hưởng latency tail — correlate với P99 HTTP nếu có.
- **Alloc rate**: throughput cấp phát — spike traffic → spike alloc có thể expected.

## Đọc đồ thị

| Pattern | Gợi ý |
|---------|--------|
| Gen2 tăng sau traffic | Có thể bình thường — so sánh với RPS |
| Committed memory tăng monotonic | Nghi leak / cache không evict — heap dump, dotnet-gcdump |
| Pause time spike | GC pressure — xem alloc rate, object lifetime |
| Alloc rate cao ngắn | Burst request — không đủ kết luận leak |

## Quy tắc agent

- Phân biệt **spike ngắn** vs **xu hướng dài hạn** (committed, Gen2).
- Gợi ý `dotnet-counters` / dump khi user cần xác nhận leak — không khẳng định leak chỉ từ một panel.

## PromQL mẫu

```promql
sum by (generation) (
  rate({__name__=~"dotnet_gc_collections_total|process_runtime_dotnet_gc_collections_count_total", job="$job"}[5m])
)
```
