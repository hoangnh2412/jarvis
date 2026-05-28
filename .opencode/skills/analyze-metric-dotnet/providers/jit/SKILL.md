# Provider: JIT

Nhóm panel **JIT** — compilation sau deploy, tiered compilation.

## Metrics

| Panel | `dotnet_*` | `process_runtime_dotnet_*` |
|-------|------------|------------------------------|
| Methods jitted | `dotnet_jit_methods_compiled_total` | `process_runtime_dotnet_jit_methods_compiled_count_total` |
| IL bytes | `dotnet_jit_il_bytes_total` | `process_runtime_dotnet_jit_il_compiled_size_bytes_total` |
| JIT time | `dotnet_jit_time_in_jit_seconds_total` | `process_runtime_dotnet_jit_compilation_time_seconds_total` |

## Ý nghĩa

- Spike **ngay sau deploy** hoặc lần đầu hit code path: **bình thường** (tiered JIT, cold start).
- JIT time cao **liên tục** sau warm-up: hiếm — dynamic codegen, AssemblyLoadContext.

## Đọc đồ thị

| Pattern | Gợi ý |
|---------|--------|
| JIT spike 5–15 phút sau deploy | Expected — so với traffic ramp-up |
| JIT cao mọi lúc | Kiểm tra dynamic proxy, expression compile |
| IL bytes tăng mạnh một lần | Load assembly lớn — đồng bộ với assembly count |

## Quy tắc agent

- **Không** kết luận lỗi business từ JIT spike một lần.
- Gợi ý warm-up test / staged rollout nếu cold start ảnh hưởng SLA.

## PromQL mẫu

```promql
rate({__name__=~"dotnet_jit_time_in_jit_seconds_total|process_runtime_dotnet_jit_compilation_time_seconds_total", job="$job"}[5m])
```
