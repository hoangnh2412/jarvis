# telemetry-dotnet

Skill thiết lập **Jarvis.OpenTelemetry** — trace, metric, log OTLP. Agent đọc [SKILL.md](./SKILL.md) và workflow khi thực thi.

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Project chưa có Jarvis OTEL | [workflows/init.md](./workflows/init.md) |
| Thêm enrich / Redis / EF plug-in | [workflows/add.md](./workflows/add.md) |

**Không dùng cho:** scaffold solution mới → [jarvis-dotnet](../jarvis-dotnet/README.md).

## Cách gọi

```text
@.opencode/skills/telemetry-dotnet/workflows/init.md

Init Jarvis OpenTelemetry cho MyApp.Host. OTLP qua biến môi trường.
```

```text
@.opencode/skills/telemetry-dotnet/SKILL.md

Thêm Redis trace instrumentation + enrich header allowlist.
```

## Kiến trúc

| Thành phần | Vai trò |
|------------|---------|
| `Jarvis.OpenTelemetry` | Hosting, options, instrumentation mặc định, middleware |
| `Jarvis.OpenTelemetry.Instrumentation.StackExchangeRedis` | Redis trace qua `ITraceInstrumentation` (tùy chọn) |

```
Jarvis.OpenTelemetry/
├── Abstractions/       # IEnrichTraceService, ITraceInstrumentation, …
├── Configuration/      # OTEL:Tracing | Metric | Logging
├── Extensions/         # AddJarvisOpenTelemetry, UseJarvisOpenTelemetry
├── Hosting/            # JarvisOpenTelemetryHostBuilder
├── Instrumentations/
├── Middleware/         # TraceEnrichment, LogEnrichment
└── SemanticConventions/
```

## Luồng bootstrap

1. `AddJarvisOpenTelemetry(configuration, configureServices)` — bind `OTEL`, đăng ký enricher ASP.NET.
2. Fluent: `.ConfigureResource()` → `.ConfigureLogging()` / `.ConfigureTrace()` / `.ConfigureMetric()`.
3. Plug-in đăng ký **trong** `configureServices` (trước `Build()`).
4. `app.UseJarvisOpenTelemetry()` — tags từ `IEnrichTraceService` / log scope.

**Lưu ý:** không `ClearProviders` — Serilog/NLog thêm riêng. Options snapshot lúc startup.

## Bootstrap tối thiểu

```csharp
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services => { })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace()
    .ConfigureMetric();

app.UseJarvisOpenTelemetry();
```

Mẫu: [templates/program-setup.cs](./templates/program-setup.cs), [templates/otel-appsettings.json](./templates/otel-appsettings.json).

## Background worker (cron)

Kế thừa `Jarvis.OpenTelemetry.HostedServices.BaseWorker` — mỗi cron tick chạy trong DI scope riêng, có **Activity** (trace) và **log scope** (`WorkerName` tag).

```csharp
public sealed class MyJobWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<MyJobWorker> logger)
    : BaseWorker(scopeFactory, logger)
{
    protected override string CronExpression => "0 */5 * * *";

    protected override async Task ExecuteJobAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        // Resolve scoped services (UoW, DbContext) từ serviceProvider
    }
}
```

Đăng ký: `builder.Services.AddHostedService<MyJobWorker>()`.

Job EF multitenancy: [entityframework-dotnet/README.md](../entityframework-dotnet/README.md) — `CreateAsyncScope` → `SwitchDbContextAsync` → `GetRepositoryAsync` lại.

Scaffold Jarvis mặc định đã gọi `AddJarvisOpenTelemetry` trong `HostLayerExtension` — xem [jarvis-dotnet](../jarvis-dotnet/README.md).

## Checklist production

- Sampling: `Tracing:Sampler` + `TraceIdRatio`
- PII: allowlist header (`HttpTraceEnrichment`)
- `ExcludedPathPrefixes`: `/health`, `/swagger`
- OTLP secrets qua env — không commit

## Providers

| Provider | SKILL |
|----------|-------|
| Enrich | [providers/enrich/SKILL.md](./providers/enrich/SKILL.md) |
| Redis | [providers/redis/SKILL.md](./providers/redis/SKILL.md) |
| EF | [providers/entityframework/SKILL.md](./providers/entityframework/SKILL.md) |
| Trace plug-in | [providers/trace-plugin/SKILL.md](./providers/trace-plugin/SKILL.md) |

## Liên quan

- [SKILL.md](./SKILL.md) — quy tắc agent, env OTLP, output bắt buộc
- [jarvis-dotnet/README.md](../jarvis-dotnet/README.md) — scaffold Host đã wire OTEL
