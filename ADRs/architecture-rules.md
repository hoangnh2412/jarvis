# Architecture Rules

Tài liệu **mindset, behavior và style code** khi phát triển hoặc refactor `Jarvis.*` / module trong monorepo. Kết hợp:

- [README.md](../README.md) — triết lý **Clean Architecture** + **Module Atomic**

**Phạm vi:** rules & mindset trong file này. Chi tiết package / appsettings / workflow từng capability nằm tài liệu module hoặc ADR riêng — **không** duplicate dài trong đây.

---

## 0. Hai trục thiết kế

Jarvis tách **chiều dọc** (solution app) và **chiều ngang** (package Atomic). Refactor phải thỏa cả hai.

```text
  Clean Architecture (app)          Module Atomic (Jarvis.*)
  ─────────────────────────         ──────────────────────────
  Host                              Jarvis.Mvc, Jarvis.HealthChecks, …
    ↑                                   ↓ cắm vào Host / Infrastructure
  Infrastructure  ←────────────  Jarvis.EntityFramework, Jarvis.Caching, …
    ↑
  Application                     Jarvis.DDD.Application (CQRS)
    ↑
  Domain                          Jarvis.DDD.Domain (contract)
    ↑
  Domain.Shared
```

> Tên package trong sơ đồ là **ví dụ minh họa** triết lý (adapter ở Infrastructure, core DDD ở Domain/Application) — không phải inventory bắt buộc.

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

### 0.2 Ranh giới monorepo — `frameworks/` vs `modules/`

| Thư mục | Vai trò | Rule |
|---------|---------|------|
| **`frameworks/`** | Package hạ tầng Atomic (`Jarvis.*`) — publish NuGet, không chứa nghiệp vụ sản phẩm | Host/Infrastructure reference khi cần capability kỹ thuật |
| **`modules/`** | Bounded context nghiệp vụ / portal (`Jarvis.Modules.*`, optional frontend) | Opt-in theo sản phẩm; **không** nhét inbox/CRUD product vào framework chỉ vì “dùng chung kỹ thuật” |

- Framework = transport, persistence foundation, ambient identity, cache, blob, … — **reusable không gắn một product feature**.
- Module = feature/product surface (API, AppService, store policy, FE) — reference framework + (nếu cần) SPI Abstractions của module khác.
- Không vòng: module A ↛ module B runtime nặng chỉ để “khai báo”; dùng Abstractions khi đủ (xem §0.3).

### 0.3 Module Atomic — package và provider

| Nguyên tắc | Áp dụng khi refactor |
|------------|----------------------|
| Một concern, một `PackageId` | `Jarvis.Caching` ≠ `Jarvis.EntityFramework` |
| Core + satellite | Core: contract + default; satellite: provider / ORM / transport riêng |
| Chỉ reference cần dùng | Host csproj không kéo cả monorepo |
| Extension, không sửa core | `Use{Provider}(this XxxBuilder)` — host/third-party thêm satellite |
| Không vòng phụ thuộc module | Module A dùng contract/SPI của B được; B không reference A |
| Config có điều kiện | Section appsettings + snapshot; `Use*` throw rõ nếu thiếu field bắt buộc |
| SPI Abstractions (tuỳ nghiệp vụ) | Khi module khác chỉ cần **khai báo / contract mỏng** (vd. definition provider) mà không kéo runtime core → tách `*.Abstractions`; không bắt buộc với mọi module |

**Abstractions — khi nào:** nhiều consumer đóng góp metadata/SPI; muốn Domain sạch; tránh kéo manager/DI/AspNetCore. **Khi không:** SPI chỉ dùng nội bộ một package, hoặc tách sớm hơn nhu cầu.

---

## 1. Triết lý kiến trúc (Jarvis.*)

### 1.1 Thư viện hạ tầng, không phải app code

- Mỗi project = **NuGet package** (`PackageId`, `GeneratePackageOnBuild`, `net9.0`).
- Contract dùng chung: `Jarvis.DDD.Domain`, `IBlobStoringService`, `ICacheService`, …
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
├── Abstractions/          # Plug-in host / SPI nội bộ (nếu cần; hoặc package *.Abstractions tách — §0.3)
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

- Một file, một type top-level — bắt buộc (`class` / `record` / `interface` / `enum`); options / extension / service / type khác nhau không chung file.
- **Ngoại lệ:** nested type gắn chặt options cha (POCO bind section lồng) được phép trong cùng file options — vẫn `sealed`, không nhét service/extension vào đó.
- Comment dòng đầu (EN): vai trò trong pipeline.
- `internal` implementation; `public` API host cần.
- `sealed` class triển khai; `static` extension/helper.
- Primary constructor khi dependency ≤ 3; inject `ILogger<T>` cho adapter có I/O.

---

## 3. Đăng ký DI & hosting API

### 3.1 Ba entry point

| Giai đoạn | Kiểu | Vai trò |
|-----------|------|---------|
| Composition | `IHostApplicationBuilder` | `AddXxx` trên host builder |
| Services | `IServiceCollection` | Đăng ký chi tiết khi tách khỏi host builder |
| Pipeline | `WebApplication` | `UseXxx` middleware / endpoints |

Prefix `Add` / `Use` / `Ensure`; return builder để chain.

### 3.2 TryAdd* — library không chiếm host

Jarvis/module: `TryAdd*` / `TryAddKeyed*` / `TryAddEnumerable`. Host override: `Add*` sau `AddCoreXxx` (registration cuối thắng).

### 3.3 Options binding + snapshot

Bind `SectionName` vào DI options; snapshot từ configuration cho đăng ký có điều kiện; `Action<TOptions>?` sau bind để chỉnh không cần file. Nested options / section lồng: §2.2–§2.3. Core chỉ field dùng chung; options provider ở satellite.

### 3.4 Fluent builder + registry (multi-provider)

`AddCoreXxx()` → bind options + registry (nếu cần) → trả `XxxBuilder`. `Use{Provider}()` đăng ký implementation + priority. `DefaultProvider` rỗng → chọn provider **đã đăng ký** priority cao nhất. Config JSON không có tác dụng nếu chưa gọi `Use*`. Thiếu field bắt buộc khi `Use*` → fail-fast (xem §0.3).

### 3.5 Plugin & keyed services

Plugin: đăng ký nhiều impl cùng interface; core resolve `GetServices` / factory — không hard-code type host. Keyed: key = `nameof(...)` hoặc type ổn định; factory chọn theo context (tenant, query, config).

### 3.6 Lazy client (adapter nặng)

Client HTTP/SDK: `Lazy<T>` — không connect trong constructor; dispose chỉ khi đã tạo; log operation, không log secret.

---

## 4. Configuration & options

- Options Jarvis: `sealed`; map 1:1 vendor thì kế thừa options vendor.
- XML doc: đơn vị và hành vi sentinel (`0` / `null` / `""`) — chi tiết §1.3.
- `AutoSelectPriority`: `0` = default built-in trong `*Defaults`.
- Doc bilingual (EN + VI ngắn) khi team cần; OTEL/EF: EN + link semconv.
- Đăng ký có điều kiện theo options/env — không đăng ký check/exporter khi sentinel tắt.

---

## 5. Runtime & production mindset

- Mặc định an toàn production; chi tiết probe/auth/header/tenant filter → ADR / doc từng capability (§1.3).
- I/O: async thật + `ConfigureAwait(false)`; tránh `async` giả; read-path nhẹ khi không cần track.
- Observability: tag/constant tập trung (`SemanticConventions`); degraded → Warning, failed → Error.
- Extension third-party name clash → namespace/wrapper riêng.

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

## 7. Nguyên tắc làm việc khi sửa code

Giảm lỗi thường gặp khi AI/agent (và người) refactor. **Tradeoff:** thận trọng hơn tốc độ — tác vụ đơn giản thì dùng phán đoán.

### 7.1 Think Before Coding

Mỗi dòng thay đổi phải trace về request. Trước khi implement:

- Nêu giả định; không chắc → hỏi.
- Nhiều cách hiểu → trình bày hết, không tự chọn một.
- Có cách đơn giản hơn → nói ra; push back khi cần.
- Không rõ → dừng, gọi tên sự nhầm lẫn, hỏi.

### 7.2 Simplicity First

Code tối thiểu giải quyết vấn đề — không suy đoán:

- Không feature ngoài yêu cầu; không abstraction cho code dùng một lần.
- Không thêm flexibility/config nếu không được yêu cầu.
- Không xử lý error cho scenario bất khả thi.
- 200 dòng mà làm được 50 → viết lại.

### 7.3 Surgical Changes

Chỉ chạm những gì phải chạm; dọn chỉ những gì mình làm bẩn:

- Không “cải thiện” code/comment/format kế bên; không refactor thứ không hỏng.
- Giữ style hiện tại.
- Dead code không liên quan → mention, đừng xoá (trừ khi được yêu cầu).
- Orphan do thay đổi của mình (import/biến/function) → xoá.

### 7.4 Goal-Driven Execution

Định nghĩa success criteria; lặp đến khi verify được:

- Validation / bug → test tái hiện rồi làm pass.
- Refactor → test xanh trước và sau.
- Multi-step: plan ngắn `step → verify`.

**Đang hoạt động nếu:** diff ít thay đổi thừa, ít rewrite do overcomplication, câu hỏi làm rõ đến **trước** implement.

---

## 8. Anti-patterns

| Anti-pattern | Thay bằng |
|--------------|-----------|
| Business rule trong `Jarvis.*` / framework | Host handler / domain service / module nghiệp vụ |
| Nhét product feature vào `frameworks/` chỉ vì tái sử dụng kỹ thuật | Module trong `modules/` + reference framework |
| Module kéo full core chỉ để khai báo SPI | `*.Abstractions` khi nghiệp vụ cần (§0.3) |
| Reference satellite không dùng | Chỉ package cần (Atomic) |
| Config đầy đủ nhưng không gọi `Use*` | Gọi extension → registry mới có provider |
| `AddSingleton` cứng trong core | `TryAdd` + host `Add` sau |
| Application reference ORM / infra package | Interface Domain; impl Infrastructure |
| Readiness trong core HealthChecks | Host `AddCheck` + tag `readiness` |
| Magic string log/trace | `SemanticConventions` |
| Connect SDK trong constructor | `Lazy<T>` + dispose có điều kiện (§3.6) |
| Diff lan rộng / abstraction sớm / đoán yêu cầu | §7 Think / Simplicity / Surgical |
