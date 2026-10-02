# Architecture review: Notifications (framework satellites + Module API)

| Mục | Giá trị |
|-----|---------|
| Ngày review | 2026-08-12 |
| Tiêu chuẩn | [ADRs/architecture-rules.md](../ADRs/architecture-rules.md) |
| ADR liên quan | [2026-08-11-adr-platform-notifications](../ADRs/2026-08-11-adr-platform-notifications.md) (D12 Atomic) |
| Phạm vi | `Platform.Notifications.Redis`, `Platform.Notifications.SignalR`, `Platform.Modules.Notifications.Api` (+ ngữ cảnh core `Platform.Notifications`) |
| Trọng tâm | Module Atomic, Clean Architecture ranh giới, DI/hosting, options, anti-pattern |

---

## Verdict

**Verdict (2026-08-12 review):** Atomic D12 đạt hướng tách package, nhưng **sai tầng** theo product intent sau đó.

> **Cập nhật ADR (cùng ngày):** boundary mới đã chốt tại [ADRs/2026-08-12-adr-platform-realtime-inbox-boundary.md](../ADRs/2026-08-12-adr-platform-realtime-inbox-boundary.md) — framework = **`Platform.Realtime*`** (hub/group/connection/backplane); inbox store + AppService → **`modules/notifications`**. Các finding P1 options-in-core / Redis-in-framework bên dưới được supersede bởi ADR đó (còn chờ implement code).

---

## 1. Cấu trúc package (thực tế)

```text
frameworks/
├── Platform.Notifications/                 # Core: contracts, AppService, options, AddCoreNotifications
├── Platform.Notifications.Redis/           # Satellite store
│   ├── Constants/RedisNotificationDefaults.cs
│   ├── Extensions/NotificationsRedisExtensions.cs
│   └── Store/…                             # RedisNotificationStore + scripts/keys
└── Platform.Notifications.SignalR/         # Satellite realtime
    ├── Constants/SignalRDefaults.cs
    ├── Extensions/…                        # UseSignalR, MapHub, backplane
    ├── Groups/, Hubs/, Services/

modules/notifications/
└── Platform.Modules.Notifications.Api/     # REST facade (không Domain/App/Infra riêng)
    ├── Controllers/, Extensions/, Models/
```

| Project | PackageId / TFM | Reference chính | Khớp §1.1 / §0.2 |
|---------|-----------------|-----------------|------------------|
| `Platform.Notifications.Redis` | `Platform.Notifications.Redis`, net9.0, `GeneratePackageOnBuild` | Core + StackExchange.Redis | ✅ Satellite chỉ core + vendor |
| `Platform.Notifications.SignalR` | `Platform.Notifications.SignalR`, net9.0 | Core + `Platform.DDD.Domain` + SignalR Redis backplane | ✅ Concern realtime tách; DDD cho identity hub |
| `Platform.Modules.Notifications.Api` | *(không có PackageId)* | Core + `Platform.DDD.Application.Contracts` | ✅ Không kéo Redis/SignalR; ⚠️ thiếu metadata NuGet so với Setting.API |

Host Sample compose đúng composition root (§0.1 Host):

```csharp
builder.AddNotificationModule();
builder.AddCoreNotifications()
    .UseSignalR()
    .UseRedisStore()
    .AddNotificationAppServiceWithRealtime<CurrentUserInfo, CurrentTenantInfo>();
// …
app.MapCoreNotificationHub<CurrentUserInfo, CurrentTenantInfo>();
```

---

## 2. Ma trận khớp `architecture-rules.md`

### 2.1 Hai trục thiết kế (§0)

| Rule | Đánh giá | Ghi chú |
|------|----------|---------|
| Module Atomic: một concern / PackageId | **Đạt** | Redis ≠ SignalR ≠ core |
| Core + satellite; `Use{Provider}` | **Đạt** | `UseRedisStore`, `UseSignalR`, `UseRedisBackplane` |
| Host không kéo monorepo thừa | **Đạt** | Module API → core only |
| Không vòng phụ thuộc module | **Đạt** | Satellites → core; Api → core |
| Clean Architecture 5-layer trong `modules/notifications` | **N/A theo ADR** | Không Domain/App/Infra riêng; AppService nằm framework core (O5/D2). Khác Setting (Library + EF + API). Chấp nhận nếu coi Notifications là **capability framework**, không bounded context nghiệp vụ |
| Dependency một chiều | **Đạt với caveat** | Core vẫn `FrameworkReference` AspNetCore + `ISignalRServerBuilder` trên `NotificationsBuilder` → leak concern SignalR vào core |

### 2.2 Cấu trúc project Platform.* (§2)

| Rule | Redis | SignalR | Module API |
|------|-------|---------|------------|
| Folder satellite: Extensions / Options / Defaults / impl | ⚠️ `Constants/` + `Store/`; **không** có `*Options.cs` riêng | ⚠️ tương tự; Hub/Services/Groups hợp lý | Controllers/Extensions/Models — ổn cho API thin |
| Options provider trong satellite, section lồng | ❌ Options Redis/Store/Backplane đang ở **core** `PlatformNotificationsOptions` | Cùng vấn đề (backplane fields trong core) | N/A |
| `internal` impl; `public` API host cần | ⚠️ `RedisNotificationStore` **public** | ⚠️ `NotificationHub` public, **không sealed** | Controller public — OK |
| Một file / một responsibility | **Đạt** | **Đạt** | **Đạt** |
| `sealed` + primary ctor ≤ 3 | **Đạt** (store) | Hub > 3 deps + không sealed | OK |

### 2.3 DI & hosting (§3)

| Rule | Đánh giá |
|------|----------|
| `Add*` composition / `Use*` pipeline | **Đạt** — `AddCoreNotifications`, `UseSignalR`, `MapCoreNotificationHub` |
| Snapshot options + `Action<T>? configure` | **Đạt** trên core |
| Fail-fast khi `Use*` thiếu config | **Đạt** — Redis store / backplane throw `InvalidOperationException` actionable |
| `TryAdd*` trong library | **Một phần** — AppService/Notifier dùng `TryAdd*`; Redis store dùng `RemoveAll` + `AddSingleton` (override cứng, không registry/priority §3.4) |
| Lazy client + dispose có điều kiện (§3.7) | ❌ `ConnectionMultiplexer.Connect` trong factory DI — connect sớm, không `Lazy<T>` / `IDisposable` có điều kiện |
| Keyed services ổn định | **Đạt** — key `Notifications.Redis.Store` |

### 2.4 Configuration (§4)

| Rule | Đánh giá |
|------|----------|
| `SectionName` const | **Đạt kỹ thuật**; ⚠️ giá trị vẫn `"SignalR"` (backward compat) — lệch tên module `Notifications` |
| Core options chỉ field dùng chung | ❌ `Redis` + `Store` nested options nằm core; satellite chỉ đọc snapshot |
| Sentinel / clamp | Một phần — retention/prune fallback trong store; validator core chỉ check `HubPath` |

### 2.5 Style & anti-pattern (§6–7)

| Anti-pattern / style | Hiện trạng |
|----------------------|------------|
| Business rule trong Platform.* | **Ổn** — không ACL/policy nguồn; scope từ `ICurrentUser` / `ICurrentTenant` |
| Reference satellite không dùng | **Ổn** — Api không reference Redis/SignalR |
| Config đủ nhưng không `Use*` | **Ổn** — Host phải gọi rõ (D12) |
| Magic string | ⚠️ Hub đọc `TenantClaimName` / `TenantHeaderKey` từ `IConfiguration` raw |
| Skill `*-dotnet` (§0.3) | ❌ ADR D13 còn mở — chưa có `.opencode/skills/notifications-dotnet/` |
| Nullable / file-scoped / ConfigureAwait | **Đạt** trên code đã đọc |

---

## 3. Findings (theo mức độ)

### [P1] Options Redis/Backplane/Store nằm trong core — lệch §2.2 / §3.3 Atomic

**File:** `frameworks/Platform.Notifications/Configuration/PlatformNotificationsOptions.cs`

`NotificationRedisBackplaneOptions` và `NotificationRedisStoreOptions` (Configuration, KeyPrefix, RetentionDays, …) gắn trên core. Satellite Redis/SignalR chỉ consume, không sở hữu options.

**Hệ quả:** Core “biết” chi tiết provider; khó thêm SQL store / backplane khác mà không phình section core. Section gốc vẫn tên `SignalR`.

**Đề xuất:** Giữ `HubPath` (và field dùng chung) trên core; chuyển nested Redis/Store sang satellite bind `Notifications:Redis` / `Notifications:Store` (hoặc giữ prefix `SignalR:*` tạm thời nhưng type options thuộc package satellite).

---

### [P1] Core couple ASP.NET SignalR — lệch §0.1 / “thư viện hạ tầng” mỏng

**File:** `NotificationsBuilder.SignalRServerBuilder` (`ISignalRServerBuilder`), `Platform.Notifications.csproj` → `FrameworkReference` AspNetCore.

Core đáng lẽ contract + AppService + options chung; builder property SignalR và AspNetCore reference kéo realtime vào package “core”.

**Đề xuất:** Chuyển `SignalRServerBuilder` / backplane wiring hoàn toàn sang `Platform.Notifications.SignalR`; core chỉ giữ `IHostApplicationBuilder` + snapshot options không phụ thuộc SignalR types (hoặc abstraction nhẹ không import Hub builder).

---

### [P2] Eager `ConnectionMultiplexer.Connect` — anti-pattern §3.7 / §7

**File:** `NotificationsRedisExtensions.ConnectStoreMultiplexer`

Connect sync trong factory đăng ký DI; không `Lazy<>`, không dispose có điều kiện khi `IsValueCreated`.

**Đề xuất:** Pattern giống Caching Redis (`RedisConnectionManager` / lazy): trì hoãn connect, timeout/AbortOnConnectFail giữ nguyên, dispose an toàn.

---

### [P2] REST + Hub thiếu `[Authorize]` — Host-owned auth chưa được enforce ở surface module

**File:** `NotificationsController`, `NotificationHub<,>`

Controller và Hub không có `[Authorize]`. Identity resolve từ ambient / claim / header; giả định Host đã `UseAuthentication`/`UseAuthorization` (Sample có), nhưng surface package không bắt buộc policy — cùng class rủi ro như review Setting (API lộ data theo tenant header).

**Đề xuất:** `[Authorize]` mặc định trên controller/hub; opt-out rõ nếu host demo cần anonymous (không khuyến nghị).

---

### [P2] Skill `notifications-dotnet` thiếu — §0.3 / ADR D13

Thay đổi hành vi package chưa có skill orchestrator/workflows/providers. Agent và host thiếu template `program-setup` / appsettings chuẩn.

**Đề xuất:** Scaffold skill theo [template-skill.md](../ADRs/template-skill.md); cập nhật khi đổi `Use*` / options.

---

### [P3] Visibility & sealing

- `RedisNotificationStore` public → nên `internal` (+ `InternalsVisibleTo` UnitTest) trừ khi host cần new trực tiếp (không nên).
- `NotificationHub<,>` không `sealed`.
- Module API: thiếu `PackageId` / `GeneratePackageOnBuild`; `RootNamespace` = `Module.Notifications` lệch tên project `Platform.Modules.Notifications.Api` (Setting.API đồng bộ hơn).

---

### [P3] Docs lệch cấu trúc

`modules/notifications/README.md` vẫn vẽ `NotificationAppService` / `Store` dưới cây SignalR — đã tách D12. Cập nhật cho khớp package layout thật.

---

### [P3] `UseRedisStore`: `RemoveAll` + `AddSingleton` thay vì registry §3.4

Chấp nhận được khi chỉ một store implementation tại một thời điểm; nếu sau này multi-store (Redis | SQL) nên chuyển `TryAddKeyed` + registry priority giống Caching/Blob.

---

## 4. Điểm mạnh (giữ nguyên)

1. **Atomic package boundary** rõ: Api → core; Host chọn satellite.
2. **Fluent builder + fail-fast** khi thiếu Redis config / gọi backplane trước `UseSignalR`.
3. **Persistence-first** trong `NotificationAppService` (ADR C4) — đúng mindset production.
4. **TryAdd** cho `INotificationAppService` / `ISignalRNotifier` — host override được.
5. **Module API mỏng** — chỉ ApplicationPart + controller; không đăng ký SignalR/Redis trong module (đúng Host composition root).
6. **Tenant/user scope** qua `ICurrentUser` / `ICurrentTenant` — khớp ADR current-user-tenant, không nhét business nguồn vào framework.
7. NuGet hygiene trên hai satellite: `PackageId`, net9.0, nullable, docs file.

---

## 5. Checklist hành động (ưu tiên)

| # | Việc | Rule |
|---|------|------|
| 1 | Tách Redis/Store/Backplane options khỏi core → satellite (+ cân nhắc rename section `Notifications`) | §2.2, §3.3, §4 |
| 2 | Gỡ coupling `ISignalRServerBuilder` / cân nhắc bỏ AspNetCore khỏi core nếu không còn cần | §0.1, §1.1 |
| 3 | Lazy Redis multiplexer + dispose có điều kiện | §3.7 |
| 4 | `[Authorize]` trên REST/Hub (hoặc document + enforce trên Sample) | Host-owned + an toàn mặc định §5.1 |
| 5 | Thêm skill `notifications-dotnet` (D13) | §0.3 |
| 6 | `internal` store; `sealed` hub; PackageId Module API; sửa README tree | §2.3 |
| 7 | (Sau) registry multi-store nếu có SQL provider | §3.4 |

---

## 6. Kết luận ngắn

Hai satellite **Redis** / **SignalR** và **Module API** đã đi đúng hướng Module Atomic sau D12 và khớp phần lớn mindset trong `architecture-rules.md`. Khoảng cách còn lại chủ yếu là **làm sạch ranh giới core vs provider** (options + AspNetCore leak), **lazy Redis**, **auth surface**, và **skill/docs** — không phải redesign luồng Notify → Store → Hub.
