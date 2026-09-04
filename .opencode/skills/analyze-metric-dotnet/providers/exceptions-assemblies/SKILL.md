# Provider: Exceptions & Assemblies

Nhóm panel **Exceptions** và **Assemblies** trên dashboard Dotnet Runtime Metrics.

## Metrics

| Panel | `dotnet_*` | `process_runtime_dotnet_*` |
|-------|------------|------------------------------|
| Exceptions | `dotnet_exceptions_total` | `process_runtime_dotnet_exceptions_count_total` |
| Assemblies | `dotnet_assembly_count` | `process_runtime_dotnet_assemblies_count` |

## Ý nghĩa

- **Exceptions**: tổng exception được ghi nhận bởi runtime (không phải mọi lỗi business đều throw).
- **Assemblies**: số assembly đã load — tăng sau deploy, plugin load, dynamic codegen.

## Đọc đồ thị

| Pattern | Gợi ý |
|---------|--------|
| Exception spike ngắn sau deploy | Có thể warm-up / config — đối chiếu logs |
| Exception tăng bền | Lỗi lặp — trace, log level Error |
| Assembly count nhảy một lần | Deploy / load assembly mới — **bình thường** |
| Assembly tăng liên tục | Hiếm — dynamic load, plugin leak — cần dump |

## Quy tắc agent

- **Không** kết luận bug nghiệp vụ chỉ từ `dotnet_assembly_count`.
- Exception metric **bổ sung** cho trace/log, không thay root-cause analysis.

## PromQL mẫu

```promql
rate({__name__=~"dotnet_exceptions_total|process_runtime_dotnet_exceptions_count_total", job="$job"}[5m])
```
