# Workflow: Phân tích dashboard runtime .NET

Áp dụng khi user cần **giải thích** panel Grafana/PromQL hoặc correlate runtime với latency/CPU.

## Checklist

```text
- [ ] 1. Xác nhận dashboard: Dotnet Runtime Metrics (CLR), không phải chỉ HTTP
- [ ] 2. Thu thập job, instance, time range, sự kiện deploy
- [ ] 3. Xác định nhóm panel (exceptions | gc | threadpool | jit)
- [ ] 4. Đọc providers/<nhóm>/SKILL.md
- [ ] 5. Phân biệt spike ngắn vs xu hướng dài hạn
- [ ] 6. Trả lời: ý nghĩa + giả thuyết + bước verify
```

## Bước 1 — Phạm vi

| Câu hỏi | Hành động |
|---------|-----------|
| Panel GC / memory? | [providers/gc-memory/SKILL.md](../providers/gc-memory/SKILL.md) |
| Thread pool / CPU? | [providers/threadpool-cpu/SKILL.md](../providers/threadpool-cpu/SKILL.md) |
| JIT sau deploy? | [providers/jit/SKILL.md](../providers/jit/SKILL.md) |
| Exception spike? | [providers/exceptions-assemblies/SKILL.md](../providers/exceptions-assemblies/SKILL.md) |

## Bước 2 — Biến Grafana

- **`job`**, **`instance`**: đúng process đang xem.
- Time range bao phủ deploy / incident.
- Query dùng `{__name__=~"dotnet_.*|process_runtime_dotnet_.*"}` → panel hỗ trợ cả hai họ tên.

## Bước 3 — Quy tắc diễn giải

1. Mô tả **metric đo gì** (counter/gauge, đơn vị).
2. **Bình thường** vs **đáng lo** cho loại workload đó.
3. Nếu có deploy: JIT/exception spike ngắn có thể expected.
4. Gen2 / committed memory tăng **dài hạn** → gợi ý leak, cache, retention — cần heap dump / compare instance.

## Bước 4 — Không nhầm pipeline

Đọc [reference/runtime-vs-http.md](../reference/runtime-vs-http.md):

- Runtime dashboard **không** thay thế metric `http.server.*` hoặc custom `sample.*` từ app Jarvis.

## Bước 5 — Output cho user

Cấu trúc gợi ý:

1. **Panel / metric** đang xét
2. **Quan sát** (số liệu / hình dạng đồ thị nếu user mô tả)
3. **Ý nghĩa kỹ thuật** (1–2 đoạn)
4. **Giả thuyết** (có điều kiện: "nếu đúng là Gen2 sau traffic tăng…")
5. **Bước tiếp** (trace, logs, so sánh instance, dotnet-counters)

## Sau analyze

Panel không có data → [troubleshoot.md](troubleshoot.md).

Cần bật metric app → [telemetry-dotnet](../../telemetry-dotnet/README.md).
