---
name: analyze-metric-dotnet
description: Giải thích dashboard Grafana Dotnet Runtime Metrics (Prometheus) — GC, memory, thread pool, JIT, exceptions. Dùng khi đọc panel dotnet_* hoặc process_runtime_dotnet_*, PromQL runtime, hoặc correlate CLR với latency/CPU.
metadata:
  audience: hoangnh
  workflow: github
---

# Analyze .NET Runtime Metrics — Orchestrator

Skill giúp agent/developer **đọc đúng** metric runtime CLR trên Grafana (Prometheus) — **không nhầm** với HTTP/RPS hay metric nghiệp vụ custom.

Hướng dẫn người dùng: [README.md](README.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Giải thích panel / xu hướng dashboard | [workflows/analyze.md](workflows/analyze.md) |
| Panel trống, không thấy series | [workflows/troubleshoot.md](workflows/troubleshoot.md) |

## Quy tắc cốt lõi

- Dashboard **Dotnet Runtime Metrics** = tầng **CLR** (GC, JIT, thread pool), không thay dashboard API.
- Hai họ tên song song: **`dotnet_*`** (.NET 9+ built-in) và **`process_runtime_dotnet_*`** (OpenTelemetry.Instrumentation.Runtime).
- Panel **CPU** (`dotnet_process_cpu_*`) — **yêu cầu .NET 9+** cho họ `dotnet_*`; runtime cũ có thể chỉ có OTel Runtime prefix.
- Phân biệt **spike sau deploy** (JIT, warm-up) vs **xu hướng xấu dài hạn** (Gen2, committed memory).
- **Không** kết luận lỗi business chỉ từ assembly count hoặc JIT spike một lần.
- Panel trống → kiểm tra `job` / `instance`, time range, Prometheus có scrape metric — không chỉ đổi tên query.

## Providers (panel groups)

Chỉ đọc nhóm panel liên quan câu hỏi:

| Nhóm | Path |
|---|---|
| Exceptions & assemblies | [providers/exceptions-assemblies/SKILL.md](providers/exceptions-assemblies/SKILL.md) |
| GC & memory | [providers/gc-memory/SKILL.md](providers/gc-memory/SKILL.md) |
| CPU & thread pool | [providers/threadpool-cpu/SKILL.md](providers/threadpool-cpu/SKILL.md) |
| JIT | [providers/jit/SKILL.md](providers/jit/SKILL.md) |

## Reference

| Tài liệu | Path |
|---|---|
| Họ tên metric | [reference/metric-families.md](reference/metric-families.md) |
| Runtime vs HTTP / custom OTLP | [reference/runtime-vs-http.md](reference/runtime-vs-http.md) |
| Checklist Grafana | [templates/grafana-analysis-checklist.md](templates/grafana-analysis-checklist.md) |

## Liên quan (cài đặt telemetry)

- [telemetry-dotnet](../telemetry-dotnet/README.md) — bật OTLP metric, custom `IMetricInstrumentation`
- [healthcheck-dotnet](../healthcheck-dotnet/README.md) — metric Prometheus `/health/prometheus` (khác pipeline)

## Output bắt buộc (khi user hỏi phân tích)

- Nêu panel đang nói tới (nhóm GC / JIT / …)
- Ý nghĩa ngắn + tín hiệu bất thường có thể
- Gợi ý bước tiếp (trace, deploy window, so sánh instance) — không bịa root cause nếu thiếu data
