# Refactoring Rules

Tài liệu **mindset, behavior và style code** khi phát triển hoặc refactor module `Jarvis.*`. Kết hợp:

- [README.md](../README.md) — triết lý **Clean Architecture** + **Module Atomic**
- [.opencode/README.md](../.opencode/README.md) — bản đồ skill AI (`*-dotnet`)
- [template-skill.md](./template-skill.md) — cây thư mục skill chuẩn

**Phạm vi:** rules trong file này. Chi tiết từng module (package, appsettings, workflow) nằm trong `.opencode/skills/<module>-dotnet/` — **không** duplicate dài trong đây.

---

## 0. Hai trục thiết kế

Jarvis tách **chiều dọc** (solution app) và **chiều ngang** (package framework). Refactor phải thỏa cả hai.

```text
  Clean Architecture (app)          Module Atomic (Jarvis.*)
  ─────────────────────────         ──────────────────────────
  Host                              Jarvis.Mvc, Jarvis.HealthChecks, …
    ↑                                   ↓ cắm vào Host / Infrastructure
  Infrastructure  ←────────────  Jarvis.EntityFramework, Jarvis.Caching, …
    ↑
  Application                     Jarvis.Application (CQRS)
    ↑
  Domain                          Jarvis.Domain (contract)
    ↑
  Domain.Shared
```

### 0.1 Clean Architecture — code thuộc layer nào

| Layer (app) | Jarvis / app code | Rule |
|-------------|-------------------|------|
| **Domain.Shared** | Enum, constant | Không reference Jarvis infrastructure |
| **Domain** | Entity, `IRepository`, events | Không EF, Redis, S3, SMTP |
| **Application** | Command/query/handler, DTO | Chỉ interface; inject abstraction |
| **Infrastructure** | `*LayerExtension`, DbContext, adapter | Reference package Jarvis adapter; implement domain port |
| **Host** | `Program.cs`, `AddHostLayer()` | Composition root — gọi extension, không business rule |

- Dependency **một chiều** lên trên; không reference ngược.
- `Program.cs` / `HostLayerExtension` mỏng — logic DI nằm extension từng module.
- Business rule (policy ACL, virus scan, naming bucket theo domain) → **host**, không `Jarvis.*` core.

### 0.2 Module Atomic — package và provider

| Nguyên tắc | Áp dụng khi refactor |
|------------|----------------------|
| Một concern, một `PackageId` | `Jarvis.Caching` ≠ `Jarvis.EntityFramework` |
| Core + satellite | Core: contract + default; satellite: `Jarvis.*.MinIO`, `Jarvis.Caching.Redis`, `Jarvis.Authentications.Jwt` |
| Chỉ reference cần dùng | Host csproj không kéo cả monorepo |
| Extension, không sửa core | `Use{Provider}(this XxxBuilder)` — host/third-party thêm satellite |
| Không vòng phụ thuộc module | Module A dùng contract của B được; B không reference A |
| Config có điều kiện | Section appsettings + snapshot; `Use*` throw rõ nếu thiếu field bắt buộc |

### 0.3 OpenCode skills — tài liệu song song code

Mỗi module Atomic có skill `*-dotnet` trong `.opencode/skills/` (không nhét workflow dài vào `jarvis-dotnet/modules/`).

```text
.opencode/skills/<skill-name>/
├── SKILL.md              # Orchestrator — agent đọc trước
├── README.md             # Developer — prompt @
├── workflows/init.md     # Chưa có module trên project
├── workflows/add.md      # Thêm provider / pattern
├── providers/<name>/SKILL.md   # Một biến thể = một file
├── patterns/<name>/SKILL.md      # EF multitenancy (thay providers)
├── templates/            # program-setup.cs, appsettings-*.json
└── reference/            # Doc dài, load khi cần
```

| Vai trò | File |
|---------|------|
| Scaffold / 5 layer | [jarvis-dotnet](../.opencode/skills/jarvis-dotnet/README.md) |
| Module Jarvis | Bảng đầy đủ trong [.opencode/README.md](../.opencode/README.md) |
| Review PR | [code-review-dotnet](../.opencode/skills/code-review-dotnet/README.md) |

**Rule:** Thay đổi hành vi package → cập nhật skill `*-dotnet` tương ứng (orchestrator + provider atomic), không chỉ code.

---

## 1. Triết lý kiến trúc (Jarvis.*)

### 1.1 Thư viện hạ tầng, không phải app code

- Mỗi project = **NuGet package** (`PackageId`, `GeneratePackageOnBuild`, `net9.0`).
- Contract dùng chung: `Jarvis.Domain`, `IBlobStoringService`, `ICacheService`, …
- **Sample** / app consumer: vài dòng extension; demo readiness, enricher, `TDbContext` nằm host.

### 1.2 Core vs Host-owned

| Jarvis cung cấp (core / batteries included) | Host tự đăng ký (owned) |
|---------------------------------------------|-------------------------|
| Liveness, startup, process-resources | Readiness: SQL, Redis, HTTP deps, … |
| Map `/health/live`, `/ready`, `/startup` | Custom `IHealthCheck` tag `readiness` |
| OTEL resource, ASP.NET trace, OTLP default | `ITraceInstrumentation`, enricher, exporter plugin |
| Repository base, multitenancy, `AddCoreDbContext` | `TDbContext`, migration, connection string, UoW cụ thể |
| `IBlobStoringService` + FileSystem default | Bucket policy, virus scan, metadata DB |
| Memory cache + `GetOrSetAsync` contract | Cache key naming theo domain |

**Rule:** Trước khi refactor, vẽ bảng Core vs Host-owned — không nhét business vào core.

### 1.3 Convention over configuration, opt-out rõ

- Mặc định an toàn production (filter health, allowlist trace header, tenant khi `ITenantEntity`).
- Sentinel có ý nghĩa: `0` bỏ check; `""` tắt chủ đích; `null` = default.
- `Math.Clamp` cho TTL, poll interval.

---

## 2. Cấu trúc project Jarvis.*

### 2.1 Core package

```text
Jarvis.{Module}/
├── Abstractions/          # Plug-in host (nếu cần)
├── Configuration/         # Options POCO, SectionName
├── Extensions/            # Add* / Use* DI + pipeline
├── Hosting/               # Fluent builder, registry (nếu multi-provider)
├── {Feature}/             # Implementation theo concern
├── Helpers/               # static internal
└── SemanticConventions/   # Hằng tag/attribute (OTEL)
```

### 2.2 Satellite package (Atomic provider)

```text
Jarvis.{Module}.{Provider}/
├── Extensions/            # Use{Provider}(this XxxBuilder)
├── {Provider}*Service.cs  # Implementation
├── *Options.cs            # Bind {Module}:{Provider}
└── *Defaults.cs           # IsEnabled, AutoSelectPriority
```

- Satellite **chỉ** reference core `Jarvis.{Module}` + vendor SDK.
- Không duplicate options gốc — section lồng dưới section module gốc.

### 2.3 Quy ước file

- Một file, một responsibility — options / extension / service tách file.
- Comment dòng đầu (EN): vai trò trong pipeline.
- `internal` implementation; `public` API host cần.
- `sealed` class triển khai; `static` extension/helper.
- Primary constructor khi dependency ≤ 3; inject `ILogger<T>` cho adapter có I/O.

---

## 3. Đăng ký DI & hosting API

### 3.1 Ba entry point chuẩn

| Giai đoạn | Kiểu | Ví dụ |
|-----------|------|-------|
| Composition | `IHostApplicationBuilder` | `AddXxx` trên host builder |
| Services | `IServiceCollection` | Đăng ký chi tiết khi tách khỏi host builder |
| Pipeline | `WebApplication` | `UseXxx` middleware / endpoints |

Tên: prefix `Add` / `Use` / `Ensure`; return builder để chain.

### 3.2 TryAdd* — library không chiếm host

```csharp
builder.Services.TryAddKeyedSingleton<IXxxService, DefaultXxxService>(key);

// Host override: AddSingleton sau AddCoreXxx → registration cuối thắng
```

Module Jarvis: `TryAdd*` / `TryAddKeyed*` / `TryAddEnumerable`. Host: `Add*` khi override.

### 3.3 Options binding + snapshot

```csharp
builder.Services
    .AddOptions<JarvisXxxOptions>()
    .BindConfiguration(JarvisXxxOptions.SectionName);

var snapshot = new JarvisXxxOptions();
builder.Configuration.GetSection(SectionName).Bind(snapshot);
configure?.Invoke(snapshot);
```

- `public const string SectionName`.
- Nested options: class con `sealed`, `= []` cho collection.
- `Action<TOptions>? configure` sau bind — chỉnh không cần file.

**Atomic:** Core options chỉ field dùng chung. Options theo provider nằm **satellite**, bind section lồng (`{Module}:{Provider}`).

### 3.4 Fluent builder + registry (multi-provider)

Khi module có nhiều implementation tùy chọn:

1. `AddCoreXxx()` → bind options, registry (nếu cần), factory service mặc định.
2. Return `XxxBuilder` (snapshot options cho đăng ký có điều kiện).
3. `Use{Provider}()` — `TryAddKeyedSingleton` + `registry.Register(key, priority)`.
4. `DefaultProvider` rỗng → chọn provider **đã đăng ký** có priority cao nhất.

**Rule:** Config priority trong JSON **không** có tác dụng nếu chưa gọi `Use*`. Fail-fast khi `Use{Provider}()` mà thiếu field bắt buộc.

### 3.5 Plugin qua interface + multi-registration

```csharp
services.AddSingleton<IAspNetCoreEnrichHttpRequest, HttpRequestHeaderEnrichment>();
services.AddSingleton<IAspNetCoreEnrichHttpRequest, UserRequestEnrichment>();

foreach (var item in sp.GetServices<IAspNetCoreEnrichHttpRequest>())
    await item.EnrichAsync(...);
```

Host implement `IUserInfoResolver`, `ITraceInstrumentation`, … — không hard-code type host trong core.

### 3.6 Keyed services

- Key = `nameof(...)` hoặc tên type ổn định — tránh magic string rải rác.
- Factory chọn strategy theo context (tenant header, query, config).

### 3.7 Lazy client + IDisposable (adapter nặng)

Adapter có client nặng (HTTP, SDK):

- `Lazy<TClient>` — không connect trong constructor.
- `IDisposable` — dispose chỉ khi `IsValueCreated`.
- `ILogger<T>` — `LogDebug` operation, không log secret.

---

## 4. Configuration & options design

### 4.1 POCO options

- `sealed` cho options Jarvis; kế thừa vendor khi map 1:1 (`OtlpExporterOptions`).
- XML doc: đơn vị (MB, ms) và hành vi `0` / `null` / `""`.
- `AutoSelectPriority`: `0` = built-in default trong `*Defaults.ResolveAutoSelectPriority`.

### 4.2 Bilingual docs (HealthChecks style)

- Summary EN + câu VI ngắn khi team cần.
- OTEL/EF: EN + link semconv.

### 4.3 Đăng ký có điều kiện

```csharp
if (options.ProcessAllocatedMemoryMegabytesCeiling > 0)
    healthChecks.AddProcessAllocatedMemoryHealthCheck(...);
```

OTLP: endpoint config **hoặc** `OTEL_*` env.

---

## 5. Runtime behavior & production mindset

### 5.1 An toàn mặc định

- Health detailed: optional API key; probe public không auth.
- OTEL headers: allowlist, truncate.
- Trace: exclude `/health`, swagger.
- EF tenant: `EnsureTenantIdForSave`; `BeginSuppressScope()` cho migration.

### 5.2 Hiệu năng

- Health liveness: cache TTL ngắn; timeout thấp; `Task.FromResult` khi không I/O.
- EF read: `AsNoTracking()`; async `.ConfigureAwait(false)`.
- Không `async` giả khi sync đủ.

### 5.3 Observability

- Tags/constants tập trung; publisher degraded → Warning / failed → Error.
- Prometheus song song OTLP khi cần.

### 5.4 Third-party isolation

Namespace/wrapper riêng khi extension name clash (`JarvisHealthChecksThirdPartyUi`).

---

## 6. Code style C#

| Khía cạnh | Chuẩn Jarvis |
|-----------|--------------|
| Nullable | `enable`; `ThrowIfNull` |
| Namespace | File-scoped |
| LINQ | `static` lambda khi không capture |
| Locking | `Lock` (.NET 9) cho static state |
| Errors | `InvalidOperationException` message actionable |

**Không làm:** helper 1–2 dòng một lần; interface + 1 impl cùng assembly (trừ plug-in); factory khi chỉ 1 strategy.

---

## 7. Anti-patterns

| Anti-pattern | Thay bằng |
|--------------|-----------|
| Business rule trong `Jarvis.*` | Host handler / domain service |
| Reference satellite không dùng | Chỉ package cần (Atomic) |
| Config đầy đủ nhưng không gọi `Use*` | Gọi extension → registry mới có provider |
| `AddSingleton` cứng trong core | `TryAdd` + host `Add` sau |
| Module skill dài trong `jarvis-dotnet/modules/` | Skill `*-dotnet` riêng |
| Application reference `Jarvis.EntityFramework` | Interface Domain; impl Infrastructure |
| Readiness trong core HealthChecks | Host `AddCheck` + tag `readiness` |
| Magic string log/trace | `SemanticConventions` |
| Connect SDK trong constructor | `Lazy<T>` + dispose có điều kiện |

---

## 8. Tham chiếu

| Tài liệu | Path |
|----------|------|
| Framework overview | [README.md](../README.md) |
| Skill hub (module + workflow) | [.opencode/README.md](../.opencode/README.md) |
| Template skill | [template-skill.md](./template-skill.md) |
| Scaffold solution | [jarvis-dotnet/workflows/scaffold.md](../.opencode/skills/jarvis-dotnet/workflows/scaffold.md) |

---

*Cập nhật khi thêm module Jarvis — giữ file ở mức **rules & mindset**; workflow và config chi tiết trong `.opencode/skills/{module}-dotnet/`.*
