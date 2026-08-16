# ADR (Tech Debt) — Tách Enrich Log/Trace khỏi `Jarvis.DDD.Domain`

> **Trạng thái:** 🟢 **Accepted** + **Implemented** (2026-08-06) — Phase 1–4 xong.  
> **Ngày:** 2026-08-01 · Confirm: 2026-08-06 (revise: bridge package) · Implement: 2026-08-06  
> **Loại:** Technical debt / package boundary  
> **Liên quan:** [2026-08-01-adr-current-user-tenant.md](./2026-08-01-adr-current-user-tenant.md) (**Implemented** trong Domain), [2026-08-01-adr-techdebt-split-current-user-tenant-packages.md](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) (D4), [architecture-software.md](./architecture-software.md).  
> **Phạm vi:** chuyển Enrich khỏi Domain → pipeline mặc định trong `Jarvis.OpenTelemetry` + adapter WorkContext trong package bridge **`Jarvis.OpenTelemetry.DDD`**; bỏ `ProjectReference` OpenTelemetry khỏi Domain.  
> **Ngoài phạm vi:** đổi semantic attribute names; redesign middleware OTEL / `IEnrich*` signature; tách project `Jarvis.OpenTelemetry.Abstractions`; đặt adapter ở Host/Sample hoặc `Jarvis.Mvc`.  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

OTEL đã có contract + consumer đúng chỗ:

| Thành phần | Package / vị trí hiện tại |
|------------|---------------------------|
| `IEnrichLogService`, `IEnrichTraceService` | **`Jarvis.OpenTelemetry`** (folder/namespace `Abstractions` — **không** có project riêng) |
| `LogEnrichmentMiddleware`, `TraceEnrichmentMiddleware`, hosted telemetry | `Jarvis.OpenTelemetry` |
| `EnrichDataService<TUser>`, `EnrichLogService<TUser>`, `EnrichTraceService<TUser>` | **`Jarvis.DDD.Domain.Services`** |

Impl Enrich đọc `IWorkContext<TUser>` và map sang `UserAttributes.*`. Middleware resolve `GetServices<IEnrich*>` — đã hỗ trợ nhiều enricher / thay thế qua DI.

Hệ quả boundary:

- `Jarvis.DDD.Domain` reference `Jarvis.OpenTelemetry` **chỉ** vì Enrich + conventions
- Domain mang telemetry cross-cut — trái Clean Architecture / Module Atomic
- Không thể đặt map WorkContext→OTEL trong `Jarvis.OpenTelemetry` (sẽ đảo chiều phụ thuộc OTEL → Domain)
- Không đặt ở Host/Sample — mỗi dự án mới phải copy lại adapter (không chấp nhận)

---

## 2. Vấn đề (Debt)

| # | Debt | Hệ quả |
|---|------|--------|
| D1 | Impl Enrich nằm trong Domain | Domain phụ thuộc OTEL; khó ship domain-only |
| D2 | Enrich gắn cứng `IWorkContext<TUser>` trong cùng package Domain | Telemetry và identity dính package; khó reuse OTEL không DDD |
| D3 | Chưa có “base default + customize” rõ ở OTEL | Consumer phải copy/register class Domain; khó thêm source khác (job, header, …) |
| D4 | Mã kết nối WorkContext↔OTEL nếu để Host | Setup dự án mới phải viết lại cùng một class + DI |

---

## 3. Ràng buộc (đã chốt)

| # | Ràng buộc |
|---|-----------|
| C1 | **Tạo** package bridge `Jarvis.OpenTelemetry.DDD` — chứa adapter WorkContext + DI helper dùng lại across Host |
| C2 | `Jarvis.OpenTelemetry` **không** reference `Jarvis.DDD.Domain` / `IWorkContext` |
| C3 | Domain **không** giữ impl Enrich — bỏ `ProjectReference` OpenTelemetry khỏi Domain khi hết consumer khác |
| C4 | OTEL cung cấp **base default** + cho phép customize (thêm source / thêm hoặc replace `IEnrich*`) |
| C5 | **Không** đặt adapter vào Host/Sample hay `Jarvis.Mvc` — chỉ register từ bridge |
| C6 | Bridge **được** reference cả `Jarvis.OpenTelemetry` và `Jarvis.DDD.Domain` (đây là chỗ kết nối duy nhất) |

---

## 4. Quyết định (Accepted)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| P1 | Tách 2 lớp: **pipeline Enrich** vs **nguồn dữ liệu** | OTEL sở hữu pipeline; nguồn WorkContext nằm ở bridge |
| P2 | Thêm `IEnrichmentSource` trong OTEL | `Task<Dictionary<string, string>> ExtractAsync()` — cùng shape với `IEnrich*` |
| P3 | Default enricher trong OTEL: **hai class riêng** | `EnrichLogService` / `EnrichTraceService` gộp mọi `IEnrichmentSource` |
| P4 | Customize | Xem bảng dưới |
| P5 | Adapter WorkContext trong **`Jarvis.OpenTelemetry.DDD`** | `UserContextEnrichmentSource<TUser>` map `IWorkContext` → `UserAttributes.*` |
| P6 | Không đưa `IWorkContext` vào OTEL core | OTEL giữ độc lập DDD; app không dùng DDD vẫn dùng được OTEL + `AddTelemetryEnrichment` |
| P7 | Extension DI | OTEL: `AddTelemetryEnrichment()`; Bridge: `AddUserContextTelemetryEnrichment<TUser>()` đăng ký source (+ gọi `AddTelemetryEnrichment` nếu chưa) |

**Cách chọn điểm mở rộng (P4):**

| Muốn | Làm |
|------|-----|
| Attribute **chung** log + trace | Thêm `IEnrichmentSource` (vd WorkContext ở bridge) |
| **Chỉ log** | Thêm / replace `IEnrichLogService` |
| **Chỉ trace** | Thêm / replace `IEnrichTraceService` |
| Đổi hành vi composite | Subclass `EnrichLog`/`EnrichTrace` + DI replace |

> `EnrichLogService` / `EnrichTraceService` **Domain hiện tại** không phải “nhánh riêng log/trace” — chúng cùng data WorkContext, chỉ khác interface. Sau cut-over: xóa Domain; composite cùng tên nằm ở **OTEL** (gộp sources); WorkContext nằm ở **bridge source**.

### Confirm (2026-08-06)

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 1 | Accept P1–P7 + C1–C6? | **OK** |
| 2 | Tên interface | **`IEnrichmentSource`** |
| 3 | Default enricher | **Hai class riêng** trong OTEL |
| 4 | Adapter WorkContext | **`Jarvis.OpenTelemetry.DDD`** (bridge) — không Host/Sample, không Mvc |
| 5 | Extension DI | OTEL `AddTelemetryEnrichment()` · Bridge `AddUserContextTelemetryEnrichment<TUser>()` |

### Target layout

```text
Jarvis.OpenTelemetry                         (core — không biết DDD)
  ├─ Abstractions/
  │    ├─ IEnrichLogService, IEnrichTraceService
  │    └─ IEnrichmentSource
  ├─ Enrichment/
  │    ├─ EnrichLogService / EnrichTraceService   (gộp sources)
  ├─ Extensions: AddTelemetryEnrichment()
  ├─ SemanticConventions.UserAttributes
  └─ Middleware / HostedServices

Jarvis.OpenTelemetry.DDD                     (bridge — mới)
  ├─ UserContextEnrichmentSource<TUser>
  └─ Extensions: AddUserContextTelemetryEnrichment<TUser>()
       → AddTelemetryEnrichment()
       → AddScoped<IEnrichmentSource, UserContext…<TUser>>()

Jarvis.DDD.Domain
  └─ (xóa EnrichData/Log/Trace; bỏ ProjectReference OpenTelemetry)

Host / Sample
  └─ services.AddUserContextTelemetryEnrichment<CurrentUserInfo>();
     (không chứa class adapter)
```

### Luồng phụ thuộc (một chiều)

```text
Host                    → OpenTelemetry.DDD
Host                    → DDD.Domain / OpenTelemetry (tuỳ nhu cầu)
OpenTelemetry.DDD       → OpenTelemetry
OpenTelemetry.DDD       → DDD.Domain
OpenTelemetry           ↛ DDD.Domain
DDD.Domain              ↛ OpenTelemetry
```

### Mapping từ code hiện tại

| Hiện tại (Domain) | Sau cut-over |
|-------------------|--------------|
| `EnrichDataService<TUser>.ExtractAsync` | `UserContextEnrichmentSource<TUser>` trong **OpenTelemetry.DDD** |
| `EnrichLogService<TUser>` / `EnrichTraceService<TUser>` | Xóa; thay bằng default OTEL |
| Sample `AddScoped<IEnrich*, Enrich*Service<…>>` | `AddUserContextTelemetryEnrichment<CurrentUserInfo>()` |

### Code mẫu (target)

> Sketch implement — namespace/folder có thể tinh chỉnh khi code; hành vi và biên package giữ nguyên.

#### 1) OTEL — contract nguồn

```csharp
// Jarvis.OpenTelemetry/Abstractions/IEnrichmentSource.cs
namespace Jarvis.OpenTelemetry.Abstractions;

public interface IEnrichmentSource
{
    Task<Dictionary<string, string>> ExtractAsync();
}
```

`IEnrichLogService` / `IEnrichTraceService` **giữ nguyên** (đã có):

```csharp
Task<Dictionary<string, string>> ExtractAsync();
```

#### 2) OTEL — default enricher (gộp mọi source)

```csharp
// Jarvis.OpenTelemetry/Enrichment/EnrichLogService.cs
namespace Jarvis.OpenTelemetry.Enrichment;

public sealed class EnrichLogService(
    IEnumerable<IEnrichmentSource> sources) : IEnrichLogService
{
    public async Task<Dictionary<string, string>> ExtractAsync()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            var data = await source.ExtractAsync().ConfigureAwait(false);
            foreach (var (key, value) in data)
                result[key] = value; // source sau ghi đè key trùng
        }
        return result;
    }
}

// EnrichTraceService — cùng pattern, implement IEnrichTraceService
public sealed class EnrichTraceService(
    IEnumerable<IEnrichmentSource> sources) : IEnrichTraceService
{
    public async Task<Dictionary<string, string>> ExtractAsync()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            var data = await source.ExtractAsync().ConfigureAwait(false);
            foreach (var (key, value) in data)
                result[key] = value;
        }
        return result;
    }
}
```

#### 3) OTEL — DI extension (core, không biết WorkContext)

```csharp
public static IServiceCollection AddTelemetryEnrichment(this IServiceCollection services)
{
    services.TryAddEnumerable(
        ServiceDescriptor.Scoped<IEnrichLogService, EnrichLogService>());
    services.TryAddEnumerable(
        ServiceDescriptor.Scoped<IEnrichTraceService, EnrichTraceService>());
    return services;
}
```

Middleware / hosted telemetry **không đổi** — vẫn `GetServices<IEnrich*>()`.

#### 4) Bridge — adapter WorkContext

```csharp
// Jarvis.OpenTelemetry.DDD/UserContextEnrichmentSource.cs
using Jarvis.DDD.Domain.Services;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.SemanticConventions;

namespace Jarvis.OpenTelemetry.DDD;

public sealed class UserContextEnrichmentSource<TUser>(
    IWorkContext<TUser> workContext) : IEnrichmentSource
    where TUser : class, ICurrentUserIdentity
{
    public async Task<Dictionary<string, string>> ExtractAsync()
    {
        var user = await workContext.User.GetAsync().ConfigureAwait(false);
        var userId = user?.UserId ?? Guid.Empty;
        var userName = user is CurrentUserInfo info && !string.IsNullOrEmpty(info.UserName)
            ? info.UserName
            : "anonymous";
        var tenantId = await workContext.Tenant.GetIdAsync().ConfigureAwait(false) ?? Guid.Empty;

        return new Dictionary<string, string>
        {
            { UserAttributes.Id, userId.ToString() },
            { UserAttributes.UserName, userName },
            { UserAttributes.TenantId, tenantId.ToString() },
        };
    }
}
```

#### 5) Bridge — DI extension

```csharp
// Jarvis.OpenTelemetry.DDD/Extensions/…
public static IServiceCollection AddUserContextTelemetryEnrichment<TUser>(
    this IServiceCollection services)
    where TUser : class, ICurrentUserIdentity
{
    services.AddTelemetryEnrichment();
    services.TryAddEnumerable(
        ServiceDescriptor.Scoped<IEnrichmentSource,
            UserContextEnrichmentSource<TUser>>());
    return services;
}
```

#### 6) Host/Sample — chỉ register (không chứa adapter)

**Trước:**

```csharp
.AddJarvisOpenTelemetry(builder.Configuration, services =>
{
    services.AddScoped<IEnrichLogService, EnrichLogService<CurrentUserInfo>>();
    services.AddScoped<IEnrichTraceService, EnrichTraceService<CurrentUserInfo>>();
})
```

**Sau:**

```csharp
.AddJarvisOpenTelemetry(builder.Configuration, services =>
{
    services.AddUserContextTelemetryEnrichment<CurrentUserInfo>();
})
```

Customize thêm nguồn:

```csharp
services.AddScoped<IEnrichmentSource, JobIdEnrichmentSource>();
```

App **không** dùng DDD: chỉ `AddTelemetryEnrichment()` + source tự viết — không cần package bridge.

---

## 5. Nguyên tắc migrate

1. **Contract OTEL trước** — giữ `IEnrich*`; thêm `IEnrichmentSource`; không đổi middleware.
2. **OTEL core độc lập DDD** — pipeline + default enricher không biết WorkContext.
3. **Mã kết nối chỉ ở bridge** — một chỗ share cho mọi Host; Host không copy adapter.
4. **Một nguồn user/tenant** — cut-over cùng PR: Sample đổi DI → xóa Domain `Enrich*`.
5. **Semantic keys** — adapter dùng `UserAttributes`; không hard-code string lệch convention.
6. **Sau ADR split user/tenant packages** — nếu `IWorkContext` rời Domain, cập nhật ProjectReference của bridge (retarget), không đưa lại Enrich vào Domain.

---

## 6. Kế hoạch implement

| Phase | Việc | Done khi | Tiến độ |
|-------|------|----------|---------|
| 1 | OTEL: `IEnrichmentSource` + `EnrichLog`/`EnrichTrace` + `AddTelemetryEnrichment()` | OTEL build độc lập; test composite | 🟢 |
| 2 | Tạo project `Jarvis.OpenTelemetry.DDD` + adapter + `AddUserContextTelemetryEnrichment<TUser>()` | Bridge build; ref OTEL + Domain | 🟢 |
| 3 | Sample đổi sang bridge extension; xóa Domain `Enrich*`; bỏ Domain→OTEL ref | Enrich chạy như hiện tại; Domain không ref OTEL | 🟢 |
| 4 | Skill templates + đánh dấu D4 ADR split user-tenant | Build + test xanh | 🟢 |

**Không block** ADR move `CurrentUser` / `CurrentTenant` package — Enrich có thể làm trước; bridge retarget sau nếu contract đổi package.

---

## 7. Inventory

### `Jarvis.OpenTelemetry` (core)

- `IEnrichLogService`, `IEnrichTraceService` (giữ)
- `IEnrichmentSource` (mới)
- `EnrichLogService`, `EnrichTraceService` (mới)
- `AddTelemetryEnrichment()` (mới)
- Middleware / hosted loops / `UserAttributes` (giữ)

### `Jarvis.OpenTelemetry.DDD` (mới)

- `UserContextEnrichmentSource<TUser>`
- `AddUserContextTelemetryEnrichment<TUser>()`
- ProjectReference → `Jarvis.OpenTelemetry`, `Jarvis.DDD.Domain`

### Xóa khỏi `Jarvis.DDD.Domain`

| Thành phần | Ghi chú |
|------------|---------|
| `EnrichDataService<TUser>` | → bridge adapter |
| `EnrichLogService<TUser>` | → OTEL default |
| `EnrichTraceService<TUser>` | → OTEL default |
| `ProjectReference` → OpenTelemetry | Bỏ |

### Host / Sample

- Gọi `AddUserContextTelemetryEnrichment<TUser>()` — **không** giữ class adapter

---

## 8. Hệ quả

### Tích cực

- Domain hết phụ thuộc OTEL.
- OTEL core vẫn dùng được không DDD.
- Adapter WorkContext **reuse** mọi dự án qua NuGet/project bridge — Host chỉ 1 dòng DI.
- Không kéo Domain vào Mvc.

### Chi phí

- Thêm 1 project/package (`Jarvis.OpenTelemetry.DDD`).
- Host dùng WorkContext enrich phải reference bridge (thay vì chỉ Domain + OTEL).

### Rủi ro

- Quên gọi extension bridge → thiếu user/tenant trên log/trace. Mitigation: Sample + skill template.
- Bridge phải retarget khi move `IWorkContext` khỏi Domain. Mitigation: ghi trong ADR split packages.
- Hai đường Enrich song song lúc migrate. Mitigation: Phase 2–3 cùng PR.

---

## 9. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** (confirm 2026-08-06; revise bridge) |
| Implementation | 🟢 **Done** (2026-08-06) — Phase 1–4 |
| Evidence | `Jarvis.OpenTelemetry.Enrichment.*`; `Jarvis.OpenTelemetry.DDD`; Domain hết `Enrich*` / hết ref OTEL; Sample `AddUserContextTelemetryEnrichment<CurrentUserInfo>()`; `UnitTest/OpenTelemetry/TelemetryEnrichmentTests` |
| Owner | Jarvis |
