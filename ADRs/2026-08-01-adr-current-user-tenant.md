# ADR — Tách `ICurrentUser` / `ICurrentTenant` và giữ `IWorkContext` làm facade

> **Trạng thái:** 🟢 Accepted + **Implemented** (API + **D5** trong `Jarvis.DDD.Domain`) — 2026-08-01; D5 code 2026-08-06.  
> **Ngày:** 2026-08-01 · Bổ sung D5: 2026-08-06  
> **Phạm vi:** `Jarvis.DDD.Domain` (`Services/*` — current user/tenant contracts + accessors; `DataStorages` — tenant resolvers), DI `AddCoreDomain` / `AddCoreCurrentContext{TUser}`; semantic **home tenant (user)** vs **current tenant (working)**.  
> **Ngoài phạm vi:** Authorization/RBAC, module Identity/Tenants CRUD, thay đổi pipeline auth scheme (JWT/ApiKey/Basic), redesign UoW / connection-string resolve, `ClaimsPrincipal` ambient (để Auth story sau); **không** thêm `WorkContext.AmbientTenant` / hai `ICurrentTenant` trên facade; **package split** / Skill Host → ADR tiếp theo (không thuộc plan triển khai ADR này).  
> **Tech debt tiếp theo:** [Multitenancy package](./2026-08-06-adr-jarvis-multitenancy-package.md) 🟢 → [split packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) Phase 3–4 (Auth). OTEL enrich: [đã xong](./2026-08-01-adr-techdebt-otel-enrichment-out-of-ddd.md).  
> **Tham chiếu:** ABP `ICurrentUser` / `ICurrentTenant` / `ICurrentTenantAccessor`; [architecture-rules.md](./architecture-rules.md); multitenancy hiện có trong `Jarvis.DDD.Domain/DataStorages` (`UserTenantIdResolver`).  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

Hiện tại `IWorkContext` gộp user + tenant + token trong một abstraction, implement stub qua `IHttpContextAccessor`.

Song song, multitenancy đã có lớp riêng:

| Abstraction | Vai trò hiện tại |
|-------------|------------------|
| `ITenantIdResolverFactory` + keyed resolvers | Resolve tenant từ header / claim / query / host |
| `ICurrentTenantAccessor` (`AsyncLocal`) | Ambient tenant cho connection / interceptor |
| `IUnitOfWork.SwitchDbContextAsync` | Đổi tenant có chủ đích trên UoW |

Vấn đề: ISP kém, trùng nguồn tenant, chỉ HTTP, auth dính multitenant.

---

## 2. Quyết định (Accepted)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Tách User và Tenant | Hai contract độc lập: identity vs multitenancy |
| D2 | Đặt tên theo ABP | `ICurrentUser`, `ICurrentTenant` |
| D3 | Hai chế độ nguồn dữ liệu | (A) HTTP/API; (B) Non-HTTP qua ambient scope |
| D4 | Giữ `IWorkContext` | Facade: `User` + `Tenant` properties only |
| D5 | Hai nghĩa tenant, hai chỗ gắn | **Home / belonging** trên user identity; **current / working** trên `ICurrentTenant` — xem §4.5 |

### Confirm §8 (2026-08-01)

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 1 | Accept D1–D4? | **OK** |
| 2 | Nguồn HTTP tenant | **R2** — `GetIdAsync`: ambient ?? `ITenantIdResolverFactory` (cache scoped) |
| 3 | Ambient / profile user | Generic `TUser : ICurrentUserIdentity`; default `CurrentUserInfo` + store — chưa `ClaimsPrincipal` |
| 4 | `IWorkContext` | **Chỉ** property `User` / `Tenant` |
| 5 | Placement (phase này) | `Jarvis.DDD.Domain.Services` — split package = ADR tech debt riêng |

### Confirm D5 (2026-08-06)

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 6 | User thuộc tenant A, switch sang B — tách thế nào? | **OK** — `(await User.GetAsync())?.TenantId` = A; `Tenant.GetIdAsync()` = B |
| 7 | Thêm `WorkContext.AmbientTenant` / `ActualTenant`? | **Không** — ambient trong project = current working (`ICurrentTenantAccessor`) |

---

## 3. Mục tiêu

| # | Yêu cầu |
|---|---------|
| 1 | Inject `ICurrentUser` khi chỉ cần identity |
| 2 | Inject `ICurrentTenant` khi chỉ cần tenant |
| 3 | API: user từ claims; tenant R2 |
| 4 | Job: set ambient qua `Change` / `BeginScope` |
| 5 | Facade `IWorkContext` |
| 6 | Không phá UoW / `ICurrentTenantAccessor` |

---

## 4. Thiết kế

### 4.1 Phân tầng

```text
                    ┌─────────────────┐
                    │  IWorkContext   │  User + Tenant
                    └────────┬────────┘
               ┌─────────────┴─────────────┐
               ▼                           ▼
      ┌────────────────┐         ┌──────────────────┐
      │  ICurrentUser  │         │  ICurrentTenant  │
      └────────┬───────┘         └────────┬─────────┘
               │                          │
               ▼                          ▼
                    ┌─────────────────────┐    ┌───────────────────────────┐
                    │ ICurrentUserAccessor│    │ ICurrentTenantAccessor    │
                    │ CurrentUserInfo     │    │ + ITenantIdResolverFactory│
                    │ ICurrentUserStore   │    │ (R2: ambient ?? resolve)  │
                    │ (AsyncLocal)        │    │                           │
                    └─────────────────────┘    └───────────────────────────┘
```

### 4.2 Contract (generic user)

```csharp
public interface ICurrentUserIdentity
{
    Guid? UserId { get; }
    Guid? TokenId { get; }
    /// <summary>Home / belonging — tenant user thuộc về (không đổi theo switch).</summary>
    Guid? TenantId { get; }
}

public interface ICurrentUser<TUser> where TUser : class, ICurrentUserIdentity
{
    Task<TUser?> GetAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentUserStore<TUser> where TUser : class, ICurrentUserIdentity
{
    Task<TUser?> FindAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ICurrentTenant
{
    Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default);
    IDisposable Change(Guid tenantId);
}

public interface IWorkContext<TUser> where TUser : class, ICurrentUserIdentity
{
    ICurrentUser<TUser> User { get; }
    ICurrentTenant Tenant { get; }
}
```

Host customize: `class AppUser : ICurrentUserIdentity { FirstName; LastName; … }` + `ICurrentUserStore{TUser}` (DB/cache).

### 4.3 Nguồn dữ liệu

**HTTP — User:** `GetAsync` = ambient → user id từ token → `ICurrentUserStore{TUser}` (không có profile → `null`, không factory fallback).

**HTTP — Tenant (R2):** `GetIdAsync` (ambient ?? resolver).

**Job:** `ICurrentTenant.Change` / `ICurrentUserAccessor{TUser}.BeginScope`.

### 4.4 DI

- `AddCoreDomain()` — **không** đăng ký User/Tenant
- `AddCoreCurrentContext{TUser}()` — accessors, `ICurrentUser{TUser}`, `ICurrentTenant`, `IWorkContext{TUser}`
- Host: **bắt buộc** `ICurrentUserStore{TUser}` (DB/cache/…). Cần `ITenantIdResolverFactory` (EF) cho tenant R2.

### 4.5 Home tenant vs current tenant (D5)

Scenario: user **thuộc** tenant A nhưng **đang làm việc** trên tenant B (sau `ICurrentTenant.Change` / header switch / UoW `SwitchDbContextAsync`).

| API | Ý nghĩa | Ví dụ |
|-----|---------|--------|
| `(await WorkContext.User.GetAsync())?.TenantId` | Tenant user **thuộc về** (home / belonging) | A |
| `await WorkContext.Tenant.GetIdAsync()` | Tenant **đang làm việc** (current / ambient sau switch) | B |

```text
  User thuộc A, switch sang B
  ────────────────────────────────
  WorkContext.User  →  identity.TenantId  = A   (ổn định theo user)
  WorkContext.Tenant → GetIdAsync()       = B   (ambient / R2 / Change)
```

**Vì sao không gắn cả hai lên `IWorkContext` kiểu `Tenant` + `AmbientTenant`?**

1. Trong codebase, **ambient đã = current working** (`ICurrentTenantAccessor` / `Change` → `BeginScope`). Tên `AmbientTenant` vs `Tenant` dễ hiểu ngược.
2. Home không cùng shape với `ICurrentTenant` — không có `Change()` hợp lệ; thuộc identity user (hướng ABP: `ICurrentUser` mang tenant của user).
3. Facade chỉ gom contract có sẵn; nhân đôi hai `ICurrentTenant` trên WorkContext dễ gọi nhầm `Change` và phải đồng bộ với accessor.

**Nguồn dữ liệu khi implement D5**

| Tenant | Nguồn đề xuất |
|--------|----------------|
| Home (`identity.TenantId`) | Profile từ `ICurrentUserStore` (DB/cache; claim chỉ nếu host store tự map) |
| Current (`ICurrentTenant`) | Giữ R2: ambient ?? resolver chain (không đổi) |

**Implemented (2026-08-06):** `ICurrentUserIdentity.TenantId` từ store; sau `Change(B)`, `User.TenantId` vẫn A. Không còn `ICurrentUserFactory` / `NullCurrentUserStore`.

---

## 5. Phương án không chọn

| Phương án | Lý do |
|-----------|--------|
| Một `IWorkContext` duy nhất | ISP kém |
| `IUserContext` / `ITenantContext` | Lệch ABP |
| R1 middleware | Team chọn R2 — ít moving parts |
| `ClaimsPrincipal` ambient ngay | Để Auth story |
| `WorkContext.Tenant` + `WorkContext.AmbientTenant` (hai cổng tenant) | Trùng nghĩa ambient=current; home không phải `ICurrentTenant` — chọn D5 |
| `WorkContext.ActualTenant` song song `Tenant` | Đổi tên mơ hồ; home gắn User rõ hơn |

---

## 6. Hệ quả

- Ranh giới auth / multitenant rõ; job và API cùng contract.
- R2: một cổng đọc `GetIdAsync` (async); không property `Id` / sync-over-async.
- Mở rộng `ClaimsPrincipal` sau không phá `ICurrentUser` surface.
- **D5:** phân biệt rõ home (user) vs current (tenant working); sau switch vẫn đọc được tenant thuộc về user.

---

## 7. Kế hoạch triển khai

| Phase | Việc | Trạng thái |
|-------|------|------------|
| 0 | Accept + chốt checklist | 🟢 **Done** |
| 1 | `ICurrentUser{T}` / `ICurrentTenant` / accessors + impl (`GetAsync` / `GetIdAsync`) | 🟢 **Done** |
| 2 | `IWorkContext{T}` facade; `AddCoreDomain` trống; `AddCoreCurrentContext{T}` | 🟢 **Done** |
| 3 | `EnrichDataService{T}` dùng `GetAsync` / `GetIdAsync` | 🟢 **Moved** — `UserContextEnrichmentSource` trong `Jarvis.OpenTelemetry.DDD` |
| 4 | Sample job dùng `Change` | 🟢 **Skipped** (optional; UoW `SwitchDbContextAsync` đủ) |
| **5b** | **D5:** `ICurrentUserIdentity.TenantId` (+ store); tests A vs B; bỏ factory/null-store fallback | 🟢 **Done** |
| — | Tách Enrich khỏi Domain | 🟢 **Done** — [OTEL enrich](./2026-08-01-adr-techdebt-otel-enrichment-out-of-ddd.md) |

**Không còn trong plan ADR này** (theo dõi ADR riêng):

| Việc | ADR |
|------|-----|
| Tạo `Jarvis.Multitenancy` + move Tenant impl | [jarvis-multitenancy-package](./2026-08-06-adr-jarvis-multitenancy-package.md) |
| Move User → Authentication; xóa `AddCoreCurrentContext`; Skill/README Host | [split packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) |

---

## 8. Checklist confirm

- [x] **Accept** ADR (D1–D4 giữ nguyên)?
- [x] Nguồn HTTP tenant: **R2** qua `GetIdAsync`
- [x] User: generic `TUser` / default `CurrentUserInfo` — chưa ClaimsPrincipal
- [x] `IWorkContext{T}`: chỉ property `User` / `Tenant`
- [x] Package placement (phase này): `Jarvis.DDD.Domain.Services` — split package = ADR tech debt riêng
- [x] **D5:** home = `User` identity `TenantId`; current = `WorkContext.Tenant`; **không** `AmbientTenant` trên facade

---

## 9. Trạng thái quyết định

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** (2026-08-01) + **D5** Accepted (2026-08-06) |
| Implementation | 🟢 **Done** (D1–D5 trong Domain) |
| Còn lại | Không — follow-up: [Multitenancy](./2026-08-06-adr-jarvis-multitenancy-package.md) → [split packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) |
| Owner | Jarvis / DDD Domain |
