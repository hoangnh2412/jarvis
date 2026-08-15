# ADR — Generic `ICurrentTenant<TTenant>` (mirror `ICurrentUser<TUser>`)

> **Trạng thái:** 🟢 **Accepted + Implemented** (2026-08-07).  
> **Ngày:** 2026-08-06 · Accept §8: 2026-08-07 · Implement: 2026-08-07  
> **Loại:** API / Domain contract (follow-up sau package split)  
> **Liên quan:** [current-user-tenant](./2026-08-01-adr-current-user-tenant.md) (🟢 — **không** generic tenant lúc đó), [split packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) (🟢 Phase 2–4), [Multitenancy package](./2026-08-06-adr-jarvis-multitenancy-package.md) (🟢), [Multitenancy.EF](./2026-08-06-adr-jarvis-multitenancy-entityframework.md) (🟢), [architecture-rules.md](./architecture-rules.md).  
> **Phạm vi:** thêm **generic tenant profile** `ICurrentTenant<TTenant>` (+ identity + store + default `CurrentTenantInfo`); DI Multitenancy; **giữ** R2 + D5 (home vs current). **Không** facade `IWorkContext` (bỏ 2026-08-07 — inject `ICurrentUser` / `ICurrentTenant` trực tiếp).  
> **Ngoài phạm vi:** Tenants CRUD / Identity → [ADR `Jarvis.Tenants`](./2026-08-07-adr-jarvis-tenants-module.md); đổi semantic R2; generic hóa `ICurrentTenantAccessor` (Guid ambient cho EF/UoW); Phase 5 resolvers; Multitenancy.EF; ClaimsPrincipal ambient; `Change(TTenant)`.  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

Sau D1–D5 + package split:

| Abstraction | Shape hiện tại |
|-------------|----------------|
| `ICurrentUser<TUser>` | Generic profile — `GetAsync()` → ambient → token id → `ICurrentUserStore` |
| `ICurrentTenant` | **Non-generic** — chỉ `GetIdAsync()` + `Change(Guid)` |
| `IWorkContext<TUser>` | `User` generic; `Tenant` non-generic |
| `ICurrentTenantAccessor` | Ambient **Guid** (AsyncLocal) — UoW / connection / interceptor |

User đã có chỗ gắn field riêng (`AppUser : ICurrentUserIdentity`). Tenant **đang làm việc** chỉ có `Guid?` — host muốn `Name`, `Code`, connection metadata, feature flags… phải tự query ngoài `ICurrentTenant`.

ADR gốc **chủ động không** làm `ICurrentTenant<TTenant>` (checklist “Không generic”). Giờ cần ADR riêng để mirror User mà không phá D5 / R2 / Guid accessor.

Điểm neo code: `WorkContext.Tenant` vẫn `ICurrentTenant` non-generic:

```csharp
public ICurrentTenant Tenant { get; } = currentTenant;
```

---

## 2. Vấn đề

| # | Debt | Hệ quả |
|---|------|--------|
| D1 | `ICurrentTenant` chỉ id | Không load / cache profile tenant theo cùng pattern User |
| D2 | Host tự fetch tenant entity | Lặp R2 + store; lệch ISP với `ICurrentUser<TUser>` |
| D3 | `IWorkContext<TUser>.Tenant` non-generic | Facade không đối xứng; khó expose `TTenant` qua WorkContext |
| D4 | (Rủi ro nếu làm ẩu) Generic hóa accessor Guid | Phá EF/UoW — **không** thuộc giải pháp |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | **D5 giữ nguyên:** home = `User.TenantId`; current working = `ICurrentTenant*` (sau `Change` / R2) |
| C2 | **R2 giữ nguyên:** resolve id = ambient Guid ?? `ITenantIdResolverFactory` (cache scoped) |
| C3 | `ICurrentTenantAccessor` **vẫn Guid-only** — infra EF/UoW không phụ thuộc `TTenant` |
| C4 | Contract identity/store ở **Domain**; impl + `CurrentTenantInfo` + `AddCurrentTenant<T>()` ở **`Jarvis.Multitenancy`** |
| C5 | Authentication **không** reference Multitenancy; Domain **không** reference Multitenancy |
| C6 | Không đổi semantic `Change(Guid.Empty)` throw; không thêm Tenants CRUD |
| C7 | OTEL.DDD chỉ cần tenant **id** — tiếp tục `GetIdAsync` (hoặc `GetAsync()?.TenantId`); không cast `CurrentTenantInfo` |

---

## 4. Quyết định (Accepted 2026-08-07)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| G1 | Thêm `ICurrentTenantIdentity` | Tối thiểu `Guid? TenantId` — host mở rộng field |
| G2 | Thay non-generic → `ICurrentTenant<TTenant>` | Mirror `ICurrentUser<TUser>` |
| G3 | Thêm `ICurrentTenantStore<TTenant>` | `FindAsync(Guid tenantId)` — Host bắt buộc đăng ký |
| G4 | API đọc | **Hai cổng:** `GetIdAsync` (chỉ Guid, R2) + `GetAsync` (full `TTenant` qua store) — xem §4.0 |
| G5 | `Change` | **Chỉ** `Change(Guid)` — **không** overload `Change(TTenant)` (confirm 2026-08-07) |
| G6 | Default type | `CurrentTenantInfo` trong **Multitenancy** (như `CurrentUserInfo` trong Authentication) |
| G7 | Accessor | **Giữ** `ICurrentTenantAccessor` Guid-only; **không** thêm `ICurrentTenantAccessor<TTenant>` — xem §4.0 |
| G8 | `IWorkContext` | ~~`IWorkContext<TUser, TTenant>`~~ → **Bỏ facade** (2026-08-07) — inject `ICurrentUser` / `ICurrentTenant` trực tiếp |
| G9 | DI compose | `AddCurrentUser` + `AddCurrentTenant` (+ stores); **không** `AddWorkContext` |

### 4.0 Giải thích G4 & G7 (file + code hiện tại → dự kiến)

#### G4 — Các hàm / file nào?

G4 nói về **API đọc trên contract tenant công khai** — không phải accessor, không phải UoW.

| File hiện tại | Thành phần | Vai trò G4 |
|---------------|------------|------------|
| `frameworks/Jarvis.DDD.Domain/Services/ICurrentTenant.cs` | `GetIdAsync()`, `Change(Guid)` | Contract Domain — **đổi** thành `ICurrentTenant<TTenant>` |
| `frameworks/Jarvis.Multitenancy/CurrentTenant.cs` | impl `GetIdAsync`, `Change` | Impl — **đổi** thành `CurrentTenant<TTenant>` + thêm `GetAsync` |
| `frameworks/Jarvis.OpenTelemetry.DDD/UserContextEnrichmentSource.cs` | `workContext.Tenant.GetIdAsync()` | Consumer **chỉ cần id** → giữ gọi `GetIdAsync` |
| `UnitTest/Services/CurrentUserTenantTests.cs` | `work.Tenant.GetIdAsync()` | Consumer id — giữ |
| *(mới)* Domain `ICurrentTenantStore<TTenant>` | `FindAsync(Guid)` | Store profile — phục vụ `GetAsync` |
| *(mới)* Multitenancy `CurrentTenantInfo` | default `TTenant` | Giống `CurrentUserInfo` |

**Hiện tại** (`ICurrentTenant.cs` — chỉ id):

```csharp
public interface ICurrentTenant
{
    Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default);
    IDisposable Change(Guid tenantId);
}
```

**Dự kiến G4 = giữ cả hai cổng** (`ICurrentTenant<TTenant>`):

```csharp
public interface ICurrentTenant<TTenant> where TTenant : class, ICurrentTenantIdentity
{
    Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default); // giữ — R2, không store
    Task<TTenant?> GetAsync(CancellationToken cancellationToken = default); // mới — id → store
    IDisposable Change(Guid tenantId); // G5: chỉ Guid
}
```

**Impl hiện tại** (`CurrentTenant.cs`) — chỉ R2 id:

```csharp
public sealed class CurrentTenant(
    ICurrentTenantAccessor currentTenantAccessor,
    ITenantIdResolverFactory tenantIdResolverFactory) : ICurrentTenant
{
    public async Task<Guid?> GetIdAsync(...) { /* ambient ?? resolver */ }
    public IDisposable Change(Guid tenantId) => currentTenantAccessor.BeginScope(tenantId);
}
```

**Impl dự kiến** (cùng file, generic + store):

```csharp
public sealed class CurrentTenant<TTenant>(
    ICurrentTenantAccessor currentTenantAccessor,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ICurrentTenantStore<TTenant> tenantStore) : ICurrentTenant<TTenant>
    where TTenant : class, ICurrentTenantIdentity
{
    // GetIdAsync — GIỮ logic R2 như hiện tại (không đụng store)
    public async Task<Guid?> GetIdAsync(...) { /* ambient ?? resolver */ }

    // GetAsync — MỚI
    public async Task<TTenant?> GetAsync(...)
    {
        // cache scoped → GetIdAsync → tenantStore.FindAsync(id)
    }

    public IDisposable Change(Guid tenantId) => currentTenantAccessor.BeginScope(tenantId);
}
```

Consumer OTEL **không đổi pattern** (vẫn id):

```csharp
// UserContextEnrichmentSource.cs — hiện tại & sau G4
var tenantId = await workContext.Tenant.GetIdAsync() ?? Guid.Empty;
```

App cần profile (mới):

```csharp
var tenant = await workContext.Tenant.GetAsync(); // CurrentTenantInfo? / AppTenant?
var name = tenant?.Name;
```

---

#### G7 — Accessor: hiện tại vs dự kiến (không generic)

G7 nói về **ambient Guid** — file khác hẳn G4.

| File | Hiện tại | Sau ADR (G7 = giữ nguyên shape) |
|------|----------|----------------------------------|
| `Domain/Services/ICurrentTenantAccessor.cs` | `Guid? TenantId` + `BeginScope(Guid)` | **Không đổi** |
| `Multitenancy/CurrentTenantAccessor.cs` | `AsyncLocal<Guid?>` | **Không đổi** |
| `EntityFramework/.../BaseUnitOfWork.cs` | inject `ICurrentTenantAccessor`, `BeginScope(tenantId)` | **Không đổi** |
| `Domain/.../TenantConnectionStringResolverFactory.cs` | đọc `_currentTenantAccessor.TenantId` | **Không đổi** |
| Sample `*UnitOfWork.cs` | ctor nhận `ICurrentTenantAccessor` | **Không đổi** |

**Hiện tại** — contract + impl (giữ nguyên khi Accept G7):

```csharp
// ICurrentTenantAccessor.cs
public interface ICurrentTenantAccessor
{
    Guid? TenantId { get; }
    IDisposable BeginScope(Guid tenantId);
}

// CurrentTenantAccessor.cs
public sealed class CurrentTenantAccessor : ICurrentTenantAccessor
{
    private static readonly AsyncLocal<Guid?> Current = new();
    public Guid? TenantId => Current.Value;
    public IDisposable BeginScope(Guid tenantId) { /* set AsyncLocal */ }
}
```

**Infra dùng accessor (không đụng `ICurrentTenant` / profile)** — `BaseUnitOfWork.cs`:

```csharp
public abstract class BaseUnitOfWork<T>(
    ...
    ICurrentTenantAccessor currentTenantAccessor) // Guid ambient
{
    public async Task SwitchDbContextAsync(Guid tenantId, ...)
    {
        _tenantScope = _currentTenantAccessor.BeginScope(tenantId); // chỉ Guid
        ...
    }
}
```

**Hai lớp cạnh nhau (sau ADR):**

```text
App / OTEL / business
  └─ ICurrentTenant<TTenant>     GetIdAsync / GetAsync / Change     ← G4 đổi file này
        │
        ▼ đọc / ghi Guid
ICurrentTenantAccessor           TenantId / BeginScope(Guid)       ← G7 KHÔNG đổi
        │
        ▼
BaseUnitOfWork / connection resolver / interceptor
```

**Phương án G7 từ chối** (để hình dung “nếu làm sai”):

```csharp
// KHÔNG làm — phá EF
public interface ICurrentTenantAccessor<TTenant> // generic
{
    TTenant? Current { get; }
    IDisposable BeginScope(TTenant tenant);
}

// BaseUnitOfWork sẽ phải biết AppTenant → EF phụ thuộc host type
```

**Tóm lại G7:** file accessor + UoW **giữ code như hiện tại**; chỉ `ICurrentTenant` / `CurrentTenant` (G2–G4) thành generic + store.

```csharp
public interface ICurrentTenantIdentity
{
    Guid? TenantId { get; }
}

public interface ICurrentTenantStore<TTenant>
    where TTenant : class, ICurrentTenantIdentity
{
    Task<TTenant?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface ICurrentTenant<TTenant>
    where TTenant : class, ICurrentTenantIdentity
{
    Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default);
    Task<TTenant?> GetAsync(CancellationToken cancellationToken = default);
    IDisposable Change(Guid tenantId);
}

// Không còn IWorkContext / WorkContext / AddWorkContext (bỏ 2026-08-07).
```

### 4.2 Impl (Multitenancy)

```text
Jarvis.Multitenancy
  ├─ CurrentTenantInfo : ICurrentTenantIdentity   // TenantId, Name?, …
  ├─ CurrentTenant<TTenant> : ICurrentTenant<TTenant>
  ├─ CurrentTenantAccessor                       // Guid — không đổi
  └─ AddCurrentTenant<TTenant>()                 // accessor + ICurrentTenant<TTenant>
```

`GetAsync` (sketch):

```text
1. Nếu scoped đã cache TTenant (sau GetAsync trước đó) → trả cache
2. id = await GetIdAsync()   // R2
3. null id → null
4. store.FindAsync(id) → cache → return
```

### 4.3 DI Host (breaking)

| Extension | Đăng ký |
|-----------|---------|
| `AddCurrentUser<TUser>()` | accessor + `ICurrentUser<TUser>` |
| `AddCurrentTenant<TTenant>()` | Guid accessor + `ICurrentTenant<TTenant>` |

Host Sample:

```csharp
builder.AddCurrentUser<CurrentUserInfo>();
builder.AddCurrentTenant<CurrentTenantInfo>();
builder.Services.TryAddSingleton<ICurrentUserStore<CurrentUserInfo>, SampleCurrentUserStore>();
builder.Services.TryAddSingleton<ICurrentTenantStore<CurrentTenantInfo>, SampleCurrentTenantStore>();
```

OTEL bridge inject `ICurrentUser<TUser>` + `ICurrentTenant<TTenant>` (không qua facade).

### 4.4 D5 + generic (không đổi nghĩa)

| API | Nghĩa |
|-----|--------|
| `(await user.GetAsync())?.TenantId` | Home / belonging |
| `await tenant.GetIdAsync()` | Current working id (R2 / Change) |
| `(await tenant.GetAsync())?.TenantId` | Cùng current id (khi có profile) |
| `(await tenant.GetAsync())?.Name` (host field) | Metadata tenant **đang làm việc** |

### 4.5 Breaking surface

| Breaking | Hướng migrate |
|----------|----------------|
| Xóa `ICurrentTenant` non-generic | → `ICurrentTenant<TTenant>` / default `CurrentTenantInfo` |
| Xóa `IWorkContext` / `AddWorkContext` | Inject `ICurrentUser` + `ICurrentTenant` (OTEL tương tự) |
| `AddCurrentTenant()` → `AddCurrentTenant<T>()` | Sample + tests |
| Host thêm `ICurrentTenantStore<T>` | Bắt buộc như user store |

---

## 5. Phương án không chọn

| Phương án | Lý do |
|-----------|--------|
| Chỉ thêm `GetAsync` trên non-generic trả `object` / `IDictionary` | Không type-safe; lệch User |
| Generic `ICurrentTenantAccessor<TTenant>` thay Guid accessor | Phá EF/UoW / connection resolve (C3) — G7 |
| Bỏ `GetIdAsync`, chỉ `GetAsync()?.TenantId` | Ép load store mỗi lần cần id (OTEL, filter) — đắt / null khi thiếu store — đối lập G4 |
| `Change(TTenant)` overload | Không cần (G5) — đủ `Change(Guid)` + store trên `GetAsync` |
| `IWorkContext` facade | Không cần — ISP: inject đúng `ICurrentUser` / `ICurrentTenant` |
| Gộp tenant profile vào `ICurrentUserIdentity` | Nhầm home vs current (phá D5) |

---

## 6. Kế hoạch triển khai

| Phase | Việc | Done khi |
|-------|------|----------|
| 0 | Confirm §8 (G1–G9) | 🟢 **Done** (2026-08-07) |
| 1 | Domain: identity + store + `ICurrentTenant<T>` | 🟢 **Done** |
| 2 | Multitenancy: `CurrentTenantInfo`, `CurrentTenant<T>`, `AddCurrentTenant<T>` | 🟢 **Done** |
| 3 | Auth: `AddCurrentUser` (không WorkContext) | 🟢 **Done** |
| 4 | OTEL.DDD: enrich từ `ICurrentUser` + `ICurrentTenant` | 🟢 **Done** |
| 5 | Sample store + DI; UnitTest | 🟢 **Done** |
| 6 | Cập nhật README ADR index | 🟢 **Done** |
| 7 | Bỏ `IWorkContext` / `AddWorkContext` | 🟢 **Done** (2026-08-07) |

---

## 7. Hệ quả

### Tích cực

- Đối xứng User / Tenant: profile generic + store host-owned; **không** facade.
- Vẫn tách home (User) vs current (Tenant) — D5.
- Infra Guid accessor nguyên vẹn cho EF.

### Chi phí

- Breaking: xóa `IWorkContext`; OTEL/Host inject hai service.
- Mọi host dùng tenant phải có `ICurrentTenantStore`.

### Rủi ro

- Store chậm / null trong hot path dùng nhầm `GetAsync` thay `GetIdAsync`. Mitigation: XML-doc; OTEL giữ `GetIdAsync`.

---

## 8. Checklist confirm

### Confirm §8 (2026-08-07)

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 1 | G1 `ICurrentTenantIdentity`? | **OK** |
| 2 | G2 chỉ `ICurrentTenant<TTenant>` (xóa non-generic)? | **OK** |
| 3 | G3 `ICurrentTenantStore<T>`? | **OK** |
| 4 | G4 giữ cả `GetIdAsync` + `GetAsync`? | **OK** |
| 5 | G5 `Change(TTenant)`? | **Không** — chỉ `Change(Guid)` |
| 6 | G6 `CurrentTenantInfo` ∈ Multitenancy? | **OK** |
| 7 | G7 accessor Guid-only (không `Accessor<TTenant>`)? | **OK** |
| 8 | G8 `IWorkContext`? | **Bỏ** (2026-08-07) — không facade |
| 9 | G9 DI? | **OK** — `AddCurrentUser` + `AddCurrentTenant` (+ stores) |
| 10 | Sample `CurrentTenantInfo` + store demo? | **OK** |

- [x] G1 identity
- [x] G2 generic only
- [x] G3 store
- [x] G4 `GetIdAsync` + `GetAsync`
- [x] G5 chỉ `Change(Guid)`
- [x] G6 `CurrentTenantInfo` ∈ Multitenancy
- [x] G7 accessor Guid-only
- [x] G8 **bỏ** `IWorkContext` (2026-08-07)
- [x] G9 `AddCurrentUser` + `AddCurrentTenant`
- [x] Sample store demo

---

## 9. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** (2026-08-07) |
| Implementation | 🟢 **Done** (2026-08-07) |
| Evidence | `ICurrentTenant<T>` + store; Multitenancy `CurrentTenantInfo` / `AddCurrentTenant<T>`; OTEL inject User+Tenant; **không** `IWorkContext`; Sample + MT_10/MT_11 |
| Blocked by | — |
| Owner | Jarvis |
