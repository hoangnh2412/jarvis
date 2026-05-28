# analyze-metric-dotnet

Skill **đọc và giải thích** dashboard Grafana **Dotnet Runtime Metrics** (Prometheus) — không cài package Jarvis.

Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| User hỏi ý nghĩa panel GC, JIT, thread pool | [workflows/analyze.md](./workflows/analyze.md) + [providers/](./providers/) |
| Panel trống / không có data | [workflows/troubleshoot.md](./workflows/troubleshoot.md) |
| User dán PromQL có `dotnet_` / `process_runtime_dotnet_` | [workflows/analyze.md](./workflows/analyze.md) |

**Không dùng cho:**

- Cài OpenTelemetry / OTLP metric → [telemetry-dotnet](../telemetry-dotnet/README.md)
- Scaffold backend → [jarvis-dotnet](../jarvis-dotnet/README.md)

## Cách gọi

```text
@.opencode/skills/analyze-metric-dotnet/workflows/analyze.md

Giải thích panel GC Pause Time và Gen2 trên dashboard Dotnet Runtime Metrics.
job=myapp, instance=pod-abc, spike sau deploy 14:00.
```

```text
@.opencode/skills/analyze-metric-dotnet/providers/gc-memory/SKILL.md

GC committed memory tăng dài hạn — có phải leak không?
```

## Hai họ metric runtime

| Prefix | Nguồn |
|--------|--------|
| `dotnet_*` | Built-in .NET 9+ |
| `process_runtime_dotnet_*` | OpenTelemetry.Instrumentation.Runtime |

Chi tiết: [reference/metric-families.md](./reference/metric-families.md).

## Panel groups

| Nhóm | SKILL |
|------|-------|
| Exceptions & assemblies | [providers/exceptions-assemblies/SKILL.md](./providers/exceptions-assemblies/SKILL.md) |
| GC & memory | [providers/gc-memory/SKILL.md](./providers/gc-memory/SKILL.md) |
| CPU & thread pool | [providers/threadpool-cpu/SKILL.md](./providers/threadpool-cpu/SKILL.md) |
| JIT | [providers/jit/SKILL.md](./providers/jit/SKILL.md) |

## Liên quan

- [telemetry-dotnet](../telemetry-dotnet/README.md) — export metric OTLP / custom meter
- [reference/runtime-vs-http.md](./reference/runtime-vs-http.md) — không nhầm với `http.server.*` hay `sample.*`
