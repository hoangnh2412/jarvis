---
name: telemetry-dotnet
description: Thiết lập Jarvis.OpenTelemetry cho ASP.NET Core — trace/metric/log OTLP, enrich middleware, instrumentation plug-in. Dùng khi tích hợp telemetry .NET, section OTEL, hoặc thêm Redis/EF/enricher.
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis.OpenTelemetry — Orchestrator

Skill điều phối gắn `Jarvis.OpenTelemetry` vào ASP.NET Core minimal hosting.

Kiến trúc & hướng dẫn: [README.md](README.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Project chưa có Jarvis OTEL | [workflows/init.md](workflows/init.md) |
| Đã có core, cần thêm instrumentation/enricher | [workflows/add.md](workflows/add.md) |

## Quy tắc cốt lõi

- `AddJarvisOpenTelemetry(configuration, configureServices)` → `.ConfigureResource()` → `.ConfigureLogging()` / `.ConfigureTrace()` / `.ConfigureMetric()`.
- Plug-in (`ITraceInstrumentation`, enricher, …) đăng ký **trong** callback `configureServices` (trước `Build()`).
- `app.UseJarvisOpenTelemetry()` khi cần tag trace / log scope từ `IEnrich*`.
- Config theo signal: `OTEL:Tracing`, `OTEL:Metric`, `OTEL:Logging`.
- `HttpTraceEnrichment`: allowlist header — không capture toàn bộ header.
- Không hard-code OTLP secrets; dùng env / secret store.
- Jarvis logging **không** `ClearProviders` — Serilog/NLog thêm riêng.
- Options snapshot lúc startup — đổi config runtime cần restart.

## Templates

- [templates/program-setup.cs](templates/program-setup.cs) — `Program.cs` tối thiểu
- [templates/enrich-service.cs](templates/enrich-service.cs) — `IEnrichTraceService` / `IEnrichLogService`
- [templates/otel-appsettings.json](templates/otel-appsettings.json) — mẫu section `OTEL`

## Providers (atomic)

Chỉ đọc provider cần dùng:

| Provider | Path |
|---|---|
| Enrich trace/log | [providers/enrich/SKILL.md](providers/enrich/SKILL.md) |
| Redis trace | [providers/redis/SKILL.md](providers/redis/SKILL.md) |
| EF Core trace | [providers/entityframework/SKILL.md](providers/entityframework/SKILL.md) |
| Custom trace plug-in | [providers/trace-plugin/SKILL.md](providers/trace-plugin/SKILL.md) |
| Custom metric plug-in | [providers/metric-plugin/SKILL.md](providers/metric-plugin/SKILL.md) |
| Custom log exporter | [providers/logging-plugin/SKILL.md](providers/logging-plugin/SKILL.md) |

## Biến môi trường OTLP (tham chiếu)

| Mục đích | Ví dụ |
|---|---|
| OTLP chung | `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_HEADERS` |
| Trace | `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` |
| Metrics | `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT` |
| Logs | `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT` |
| Resource | `OTEL_RESOURCE_ATTRIBUTES`, `OTEL_SERVICE_NAME` |

## Output bắt buộc

- Thay đổi `Program.cs`
- `appsettings.json` section `OTEL` (khi đổi config)
- Checklist: OTLP endpoint/env, sampling, allowlist header, plug-in singleton

## Background worker

Kế thừa `Jarvis.OpenTelemetry.HostedServices.BaseWorker` — cron + trace/log scope mỗi tick. Chi tiết: [README.md](README.md#background-worker-cron).

## Checklist production

- Sampling: `Tracing:Sampler` + `TraceIdRatio` hợp lý
- PII: allowlist header chặt
- Cardinality: tag ổn định
- Noise: `ExcludedPathPrefixes` cho `/health`, `/swagger`
- Secrets: OTLP headers qua env, không commit
