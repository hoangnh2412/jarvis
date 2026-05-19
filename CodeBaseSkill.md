# Jarvis Codebase Skill — Refactoring Rules

Tài liệu này mô tả **mindset, behavior và style code** rút ra từ `Jarvis.HealthChecks`, `Jarvis.OpenTelemetry`, `Jarvis.EntityFramework`. Dùng làm checklist khi refactor các module khác (Caching, BlobStorage, Notification, Authentication, …).

---

## 1. Triết lý kiến trúc

### 1.1 Jarvis.* = thư viện hạ tầng, không phải app code

- Mỗi project là **NuGet package** độc lập (`PackageId`, `GeneratePackageOnBuild`, `net9.0`).
- Phụ thuộc `Jarvis.Domain` cho contract; implementation nằm trong project Jarvis tương ứng.
- Host (Sample / app thật) chỉ gọi vài dòng extension; logic domain nằm ở host.

### 1.2 Core vs Host-owned (ranh giới rõ)

| Jarvis cung cấp (core) | Host tự đăng ký (owned) |
|------------------------|-------------------------|
| Liveness, startup, process-resources | Readiness: SQL, Redis, HTTP deps, … |
| Map `/health/live`, `/ready`, `/startup`, `/health` | Custom `IHealthCheck` có tag `readiness` |
| OTEL resource, ASP.NET/HttpClient trace, OTLP mặc định | `ITraceInstrumentation`, enricher, exporter plugin |
| Repository base, multitenancy resolver, `AddCoreDbContext` | `TDbContext`, migration, connection string thật, UoW cụ thể |

**Rule refactor:** Khi tách module mới, xác định trước phần nào là “batteries included” và phần nào host phải plug-in — đừng nhét business logic vào core.

### 1.3 Convention over configuration, nhưng opt-out rõ ràng

- Mặc định **an toàn cho production** (filter health paths, allowlist header trace, tenant bắt buộc khi có `ITenantEntity`).
- Tắt tính năng bằng **sentinel có ý nghĩa**, không chỉ `null`:
  - `0` → bỏ qua check (memory, CPU, system probe).
  - `""` (empty string) → **tắt có chủ đích** (process name, Windows service).
  - `null` → hành vi mặc định (vd. process = current process).
- `Math.Clamp` cho giá trị config (poll interval UI, cache TTL).

---

## 2. Cấu trúc project & file

```
Jarvis.{Module}/
├── Abstractions/          # Interface plug-in (nếu cần mở rộng)
├── Configuration/         # Options POCO, thường kế thừa options của vendor
├── Extensions/            # DI + pipeline (ServiceCollection, ApplicationBuilder, Host)
├── Hosting/               # Fluent builder (nếu setup nhiều bước)
├── {Feature}/             # Implementation theo concern (Instrumentations, DataStorages, Repositories)
├── Helpers/               # static internal — logic thuần, không DI
└── SemanticConventions/    # Hằng số tên attribute/tag (OTEL)
```

**Quy ước file:**

- **Một file, một responsibility** — options class riêng, extension riêng, health check riêng.
- **Comment dòng đầu file** (EN) mô tả vai trò module/file trong pipeline.
- `internal` cho implementation; `public` chỉ API host cần gọi.
- `sealed` cho class triển khai; `static` cho extension/helper.
- Primary constructor (C# 12) khi dependency ≤ 3 và không cần field phức tạp.

---

## 3. Đăng ký DI & hosting API

### 3.1 Ba entry point chuẩn

| Giai đoạn | Kiểu extension | Ví dụ |
|-----------|----------------|-------|
| Composition | `IHostApplicationBuilder` | `AddHealthChecks`, `AddEntityFramework` |
| Services chi tiết | `IServiceCollection` | `AddCoreDbContext`, `AddJarvisOpenTelemetry` |
| Pipeline / startup | `WebApplication` / `IApplicationBuilder` | `UseHealthChecks`, `UseJarvisOpenTelemetry`, `EnsureMigrateDb` |

**Rule:** Tên method ngắn, có prefix `Add` / `Use` / `Ensure`; return `builder`/`app`/`services` để chain fluent.

### 3.2 TryAdd* — library không “chiếm” host

```csharp
// Jarvis đăng ký mặc định
builder.Services.TryAddScoped(typeof(IQueryRepository<>), typeof(BaseQueryRepository<>));
builder.Services.TryAddKeyedScoped<ITenantIdResolver, HeaderTenantIdResolver>(nameof(HeaderTenantIdResolver));

// Host ghi đè: AddScoped sau AddEntityFramework → registration cuối thắng
```

**Rule refactor:** Module Jarvis luôn dùng `TryAdd*` / `TryAddKeyed*` / `TryAddEnumerable`; host dùng `Add*` khi override.

### 3.3 Options binding

```csharp
builder.Services
    .AddOptions<JarvisXxxOptions>()
    .BindConfiguration(JarvisXxxOptions.SectionName);

var options = new JarvisXxxOptions();
builder.Configuration.GetSection(SectionName).Bind(options);
configure?.Invoke(options);  // snapshot cho đăng ký có điều kiện
```

- `public const string SectionName = "HealthChecks"` (hoặc `"OTEL"`).
- Nested options: `Ui`, `System`, `Tracing`, … — class con `sealed`, collection init `= []`.
- Callback `Action<TOptions>? configure` **sau** bind — cho host chỉnh không cần file config.

### 3.4 Fluent builder khi setup > 1 signal

`JarvisOpenTelemetryHostBuilder`: `ConfigureResource()` → `ConfigureTrace()` → `ConfigureMetric()` → `ConfigureLogging()`.

Mỗi method nhận optional `Action<Builder>?` cho host chèn thêm mà không fork core.

**Rule refactor:** Nếu module có ≥ 2 pipeline độc lập (register + export + middleware), cân nhắc builder thay vì một method 200 dòng.

### 3.5 Plugin qua interface + multi-registration

```csharp
// Đăng ký nhiều implementation cùng interface
services.AddSingleton<IAspNetCoreEnrichHttpRequest, HttpRequestHeaderEnrichment>();
services.AddSingleton<IAspNetCoreEnrichHttpRequest, UserRequestEnrichment>();

// Runtime: resolve tất cả
foreach (var item in sp.GetServices<IAspNetCoreEnrichHttpRequest>())
    await item.EnrichAsync(...);
```

Interfaces plug-in gợi ý:

| Signal | Interface |
|--------|-----------|
| Trace | `ITraceInstrumentation`, `ITraceExporter`, `IEnrichTraceService`, `IAspNetCoreEnrichHttpRequest` |
| Metric | `IMetricInstrumentation`, `IMetricExporter` |
| Log | `ILoggingExporter`, `IEnrichLogService` |
| Domain | `IUserInfoResolver` (host implement) |

Hook vào pipeline qua `ConfigureOpenTelemetry*` callback hoặc `configureServices` delegate — **không** hard-code type của host.

### 3.6 Keyed services cho strategy pattern

- Key = `nameof(ResolverType)` hoặc `typeof(TDbContext).Name`.
- Factory (`ITenantIdResolverFactory`, `ITenantConnectionStringResolverFactory`) orchestrate keyed resolver.

**Rule refactor:** Mỗi “cách resolve” (header, query, config, DB lookup) = một keyed implementation + factory chọn theo context.

---

## 4. Configuration & options design

### 4.1 POCO options

- `sealed class` cho options gốc và nested (HealthChecks); `class` khi kế thừa vendor (`TraceSignalOptions : OtlpExporterOptions`).
- Property có default hợp lý trong initializer (`= 80`, `= true`, `= []`).
- XML doc giải thích **đơn vị** (MB, ms, percent) và **hành vi khi 0/null/empty**.

### 4.2 Bilingual docs (HealthChecks style)

- Summary EN + câu VI ngắn trong cùng `<summary>`.
- Comment inline `// EN: … / VI: …` cho logic không hiển nhiên.
- OpenTelemetry/EF: chủ yếu EN, link semconv khi cần.

### 4.3 Điều kiện đăng ký từ snapshot

Chỉ add health check / instrumentation khi config “bật”:

```csharp
if (options.ProcessAllocatedMemoryMegabytesCeiling > 0)
    healthChecks.AddProcessAllocatedMemoryHealthCheck(...);

if (options.Ui.Enabled)
    ThirdPartyUi.Register(...);
```

OTLP: đăng ký exporter khi có Endpoint trong config **hoặc** biến môi trường `OTEL_*` — hỗ trợ Docker/K8s không cần appsettings.

---

## 5. Runtime behavior & production mindset

### 5.1 An toàn mặc định

- Health detailed endpoint: optional API key header; probe công khai không auth.
- OTEL HTTP headers: **allowlist**, tắt mặc định, truncate length.
- Trace path filter: exclude `/health`, swagger (configurable prefixes).
- EF tenant: `EnsureTenantIdForSave` throw rõ message; `BeginSuppressScope()` cho migration cross-tenant.

### 5.2 Hiệu năng

- Health liveness: `IMemoryCache` TTL ngắn; timeout ms thấp (500–800ms); `Task.FromResult` khi không I/O.
- EF read: `AsNoTracking()` trong `GetQuery()`; write dùng tracked `DbSet`.
- Async library: `.ConfigureAwait(false)` consistently.
- Sync path: `Task.CompletedTask`, `Task.FromResult` — không `async` giả.

### 5.3 Observability tích hợp sẵn

- Tags/constants tập trung (`HealthCheckTags`, `HttpAttributes`, `UserAttributes`).
- Publisher/logger cho trạng thái xấu (`IHealthCheckPublisher` → Warning/Error log).
- Activity ↔ log: `ActivityTrackingOptions.TraceId | SpanId`.
- Prometheus song song OTLP (health metrics path riêng).

### 5.4 Kubernetes / ops

- Tags: `liveness`, `readiness`, `startup` — map endpoint riêng.
- `DisableRateLimiting()` trên probe endpoints.
- Startup: `IStartupCompletionNotifier` + optional auto-complete on `ApplicationStarted`.
- Windows-only code: `OperatingSystem.IsWindows()` + `#pragma warning disable CA1416`.

### 5.5 Tách third-party tránh xung đột

`JarvisHealthChecksThirdPartyUi` namespace riêng — wrap `AddHealthChecksUI` để không đụng tên extension của package gốc.

**Rule refactor:** Wrapper namespace/class riêng khi API vendor trùng tên với Jarvis.

---

## 6. Code style C#

| Khía cạnh | Chuẩn Jarvis |
|-----------|--------------|
| Nullable | `enable` — `Guid?`, `string?`, throw `ArgumentNullException.ThrowIfNull` |
| Usings | Implicit usings; file-scoped namespace |
| LINQ | `static` lambda khi không capture: `.Where(static x => ...)` |
| Locking | `Lock` type (.NET 9) cho static state (CPU sampling) |
| Reflection | Chỉ khi cần convention scan (`TypeHelper`, `IConcurrencyCheck`); helper tách riêng |
| EF internal API | `#pragma warning disable EF1001` có comment giải thích |
| Errors | `InvalidOperationException` message actionable (tenant, connection string, repository not registered) |

**Không làm:**

- Helper 1–2 dòng chỉ gọi 1 lần — inline.
- Interface + 1 implementation duy nhất trong cùng assembly (trừ khi là plug-in point cho host).
- Over-abstract factory khi chỉ có 1 strategy.

---

## 7. Patterns theo layer (áp dụng khi refactor)

### 7.1 HealthChecks-like module

1. `JarvisXxxOptions` + `SectionName` + nested options.
2. `IHostApplicationBuilder.AddXxx(configure?)` — bind, snapshot, conditional register.
3. Constants cho tags/keys (`HealthCheckTags`).
4. `WebApplication.UseXxx()` — map endpoints, middleware, lifetime hooks.
5. Internal `IHealthCheck` / publisher nếu cần — không expose type.
6. Host extension riêng file `*Readiness*Extensions.cs` trong app.

### 7.2 OpenTelemetry-like module

1. `AddJarvisXxx(IConfiguration, configureServices?)` → return fluent builder.
2. `Abstractions/` cho mọi điểm mở rộng host.
3. Default instrumentations đăng ký trong core; host thêm qua callback.
4. `UseJarvisXxx()` middleware mỏng — orchestrate `GetServices<T>()`.
5. `SemanticConventions/` — không hard-code string literal rải rác.
6. Options kế thừa vendor options khi map 1:1 (`OtlpExporterOptions`).

### 7.3 EntityFramework-like module

1. **CQRS repository split:** `IQueryRepository` / `ICommandRepository` / `IBulkCommandRepository` → base class mỏng kế thừa `EfRepositoryCore`.
2. **Unit of Work** sở hữu `DbContext` lifecycle, tenant switch, transaction.
3. **Context base class** (`BaseStorageContext<T>`): global filter, concurrency, save validation.
4. **Hai mode multitenancy** document rõ trong XML:
   - Shared DB: query filter + `SetTenantId` — `AddCoreDbContext<T>(configure)`.
   - Dedicated DB: interceptor rewrite connection — `AddCoreDbContext<T, TResolver>(configure)`.
5. `EnsureMigrateDb<TUoW>()` trên pipeline + `TenantScopedContextValidation.BeginSuppressScope()`.
6. Helpers `internal static` — pagination, expression combine, column projection.

---

## 8. Checklist refactor module mới

Dùng cho Caching, BlobStorage, Notification, Authentication, …

### Phase A — Thiết kế

- [ ] Contract (`Jarvis.Domain` hoặc `Abstractions/`): interface nhỏ, host implement.
- [ ] Vẽ bảng Core vs Host-owned.
- [ ] Chọn section config (`Cache`, `BlobStorage`, …) + nested options.
- [ ] Liệt kê plug-in interfaces (exporter, resolver, enricher, …).

### Phase B — Implementation

- [ ] `Jarvis.{Module}.csproj`: net9, nullable, GenerateDocumentationFile.
- [ ] `AddXxx` trên `IHostApplicationBuilder` hoặc `IServiceCollection` + `UseXxx` nếu có HTTP pipeline.
- [ ] `TryAdd*` cho mọi default; keyed nếu ≥ 2 strategy.
- [ ] Options bind + snapshot + `configure?` callback.
- [ ] Sentinel disable (`0`, `""`, `null`) documented trong XML.
- [ ] Third-party wrap namespace riêng nếu extension name clash.
- [ ] Fail-fast validation với message hướng dẫn fix.

### Phase C — Host integration (Sample)

- [ ] 3–5 dòng trong `Program.cs`: Add → Configure → Use.
- [ ] Demo implementation trong `Sample/` (không trong Jarvis core).
- [ ] Skill/workflow riêng (`opencode/skills/{module}-dotnet/`) — **không** nhét vào file này.

### Phase D — Production

- [ ] Timeout / cache / no-tracking / allowlist mặc định an toàn.
- [ ] Tích hợp OTEL tags hoặc health readiness nếu module phụ thuộc hạ tầng ngoài.
- [ ] Log level mapping cho trạng thái degraded (Warning) vs failed (Error).

---

## 9. Anti-patterns (tránh khi refactor)

| Anti-pattern | Thay bằng |
|--------------|-----------|
| Business rule trong Jarvis.* | Host service / handler |
| `AddSingleton` cứng implementation host cần thay | `TryAdd` + host `Add` sau |
| Một method đăng ký 300 dòng | Builder hoặc private `RegisterXxx` |
| Config bool `EnableFoo` không có default rõ | Default + doc khi false |
| Readiness trong core Jarvis.HealthChecks | Host `AddCheck` + tag `readiness` |
| Connection string cố định trong core EF | `AddCoreDbContext` + resolver/interceptor |
| Magic string attribute/log | `SemanticConventions` static class |
| `async void` / thiếu `ConfigureAwait` trong library | `Task` + `ConfigureAwait(false)` |

---

## 10. Tham chiếu nhanh — file mẫu theo pattern

| Pattern | Tham chiếu |
|---------|------------|
| Host builder + conditional register | `HealthCheckServiceExtensions.cs` |
| Options + sentinel disable | `JarvisHealthCheckOptions.cs`, `JarvisHealthCheckSystemOptions.cs` |
| Pipeline map + security gate | `HealthCheckWebApplicationExtensions.cs` |
| Third-party isolation | `HealthChecksUiThirdPartyRegistration.cs` |
| Fluent OTEL builder | `JarvisOpenTelemetryHostBuilder.cs` |
| Plugin + multi-enricher | `OpenTelemetryServiceCollectionExtensions.cs`, middleware enrich |
| TryAdd + keyed multitenancy | `HostApplicationBuilderExtension.cs` (EF) |
| UoW + context lifecycle | `BaseUnitOfWork.cs` |
| Global filter + save guard | `BaseStorageContext.cs`, `TenantScopedContextValidation.cs` |
| Shared repository core | `EfRepositoryCore.cs` |

---

*Cập nhật khi thêm module Jarvis mới — giữ file này ở mức **rules & mindset**, chi tiết từng module nằm trong `opencode/skills/{module}-dotnet/`.*
