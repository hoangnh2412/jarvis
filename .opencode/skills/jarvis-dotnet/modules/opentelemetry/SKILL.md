---
name: jarvis-dotnet-opentelemetry
description: Cài Jarvis.OpenTelemetry — package và bootstrap tối thiểu. Chi tiết trace/metric/log dùng skill telemetry-dotnet.
dependencies:
  - Jarvis.OpenTelemetry
---

# OpenTelemetry

## Package

| PackageId | Version* |
|---|---|
| `Jarvis.OpenTelemetry` | 1.0.1 |

Tùy chọn: `Jarvis.OpenTelemetry.Instrumentation.StackExchangeRedis` (ProjectReference).

## Bootstrap tối thiểu

```csharp
using Jarvis.OpenTelemetry.Extensions;

builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services => { })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace()
    .ConfigureMetric();

app.UseJarvisOpenTelemetry();
```

## Config

Section `OTEL` trong appsettings — xem [telemetry-dotnet/templates/otel-appsettings.json](../../telemetry-dotnet/templates/otel-appsettings.json).

## Skill chuyên sâu

Toàn bộ workflow, providers (Redis, EF, enrich, plug-in):

→ **[telemetry-dotnet](../../telemetry-dotnet/SKILL.md)**

Kiến trúc: [docs/opentelemetry.md](../../../../docs/opentelemetry.md).
