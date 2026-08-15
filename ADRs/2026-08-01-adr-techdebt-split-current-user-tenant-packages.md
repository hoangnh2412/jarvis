# ADR (Tech Debt) — Tách `ICurrentUser` / `ICurrentTenant` impl khỏi `Jarvis.DDD.Domain`

> **Trạng thái:** 🟢 **Accepted + Implemented** (Phase 2–4, 2026-08-06). Phase 5 resolvers = 🟢 **Done** (2026-08-07).  
> **Ngày:** 2026-08-01 · Accept §9: 2026-08-06 · Phase 2 Multitenancy: 2026-08-06 · Phase 3–4 Auth: 2026-08-06  
> **Loại:** Technical debt / package boundary  
> **Liên quan:** [2026-08-01-adr-current-user-tenant.md](./2026-08-01-adr-current-user-tenant.md) (API + **D5** 🟢), [2026-08-06-adr-jarvis-multitenancy-package.md](./2026-08-06-adr-jarvis-multitenancy-package.md) (🟢 Done), [2026-08-01-adr-techdebt-otel-enrichment-out-of-ddd.md](./2026-08-01-adr-techdebt-otel-enrichment-out-of-ddd.md) (🟢 Done), [architecture-rules.md](./architecture-rules.md), [refactor-authentication.md](./refactor-authentication.md).  
> **Phạm vi:** di chuyển **implementation + DI** Current User / Current Tenant ra khỏi `Jarvis.DDD.Domain` → `Jarvis.Authentication` / `Jarvis.Multitenancy`; Domain chỉ giữ **contract** (+ `WorkContext` zero-infra).  
> **Ngoài phạm vi:** Authorization; ClaimsPrincipal ambient; đổi semantic R2; **generic `ICurrentTenant<TTenant>`** → [ADR riêng](./2026-08-06-adr-generic-current-tenant.md); **D5** (🟢 done); Phase 5 move resolvers `DataStorages` (Later); leftover `Enrich*` Domain (ADR OTEL).  
> **Thứ tự:** (1) [Multitenancy package](./2026-08-06-adr-jarvis-multitenancy-package.md) 🟢 → (2) Auth User move (Phase 3–4) 🟢.  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

API current-user-tenant đã **Accepted + Implemented** trong Domain (`b6900c9` và follow-up + D5). Sau Phase 2–4:

| Thành phần | Vị trí hiện tại |
|------------|-----------------|
| `ICurrentUser*`, `ICurrentTenant`, `IWorkContext` contracts | `Jarvis.DDD.Domain.Services` |
| `CurrentUser<T>`, `CurrentUserAccessor`, `CurrentUserInfo` | 🟢 `Jarvis.Authentication` |
| `CurrentTenant`, `CurrentTenantAccessor` | 🟢 `Jarvis.Multitenancy` |
| Tenant resolvers | 🟢 `Jarvis.Multitenancy.DataStorages` (`AddTenantIdResolvers` / `AddCurrentTenant`) |
| DI Host | `AddCurrentUser<T>()` + `AddCurrentTenant()` — **đã xóa** `AddCoreCurrentContext` |
| OTEL WorkContext → attributes | 🟢 `Jarvis.OpenTelemetry.DDD` (không cast `CurrentUserInfo`) |

Vấn đề gốc (đã giải Phase 2–4): ISP đã tách interface nhưng impl + DI vẫn gộp trong Domain — trái Module Atomic / Clean Architecture.

---

## 2. Vấn đề (Debt)

| # | Debt | Hệ quả |
|---|------|--------|
| D1 | Impl `CurrentUser<T>` (+ `CurrentUserInfo`) trong Domain | Domain phụ thuộc HTTP auth — 🟢 đã move Auth |
| D2 | Impl `CurrentTenant` trong Domain; `AddCoreCurrentContext` gộp User+Tenant | Không bật độc lập Auth / Tenancy — 🟢 đã tách |
| D3 | Composition root giả nằm ở Domain | Host phải là composition — 🟢 Done |
| D4 | Enrich gắn WorkContext trong Domain | 🟢 Đã tách — ADR OTEL |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Domain **giữ contract** (`ICurrentUser*`, `ICurrentTenant`, `ICurrentTenantAccessor`, `ICurrentUserAccessor`, `IWorkContext`) |
| C2 | Authentication **không** reference Multitenancy; ngược lại cũng không |
| C3 | Auth / Multitenancy → Domain (một chiều) |
| C4 | Không gộp Tenant vào `Jarvis.Authentication` |
| C5 | UoW / EF dùng `ICurrentTenantAccessor` (interface Domain); **không** đăng ký impl accessor trong EF sau cut-over |
| C6 | OTEL.DDD chỉ phụ thuộc contract Domain — **không** cast `CurrentUserInfo` sau khi type chuyển Auth |
| C7 | Pattern OTEL bridge: Host compose extension mỏng |

---

## 4. Quyết định (Accepted 2026-08-06)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| P1 | Domain **chỉ giữ interface** (+ `WorkContext` zero-infra) | Không giữ `CurrentUser` / `CurrentTenant` / accessor **class** / `CurrentUserInfo` |
| P2 | User impl → **`Jarvis.Authentication`** | `CurrentUser<T>`, `CurrentUserAccessor<T>`, `CurrentUserInfo`, `AddCurrentUser<T>()` — namespace `Jarvis.Authentication` |
| P3 | Tenant impl → **`Jarvis.Multitenancy`** (package mới) | `CurrentTenant`, `CurrentTenantAccessor`, `AddCurrentTenant()` |
| P4 | Compose ở Host | **Xóa** `AddCoreCurrentContext` khỏi Domain → Sample: `AddCurrentUser` + `AddCurrentTenant` |
| P5 | `IWorkContext` / `WorkContext` | **Giữ Domain** — facade phụ thuộc cả User và Tenant; không đưa Auth hoặc Multitenancy (tránh C2) |
| P6 | Không đổi semantic API | Giữ R2, `GetAsync` / `GetIdAsync` / `Change` |

### Confirm §9 (2026-08-06)

| # | Câu hỏi | Trả lời |
|---|----------|---------|
| 1 | Accept P1–P6, C1–C7? | **OK** |
| 2 | Tên package tenant | **`Jarvis.Multitenancy`** |
| 3 | Accessor impl | Domain **chỉ interface**; class → Auth / Multitenancy |
| 4 | `AddCoreCurrentContext` | **Xóa** khỏi Domain (cut-over một lần) |

### Soft confirm (Phase 3 — 2026-08-06)

| # | Câu hỏi | Trả lời |
|---|----------|---------|
| 5 | Namespace sau move | **`Jarvis.Authentication`** / **`Jarvis.Multitenancy`** (không giữ type ở Domain) |
| 6 | OTEL `UserName` | Bỏ cast → `"anonymous"`; **không** mở rộng `ICurrentUserIdentity` đợt này |

### Target layout

```text
Jarvis.DDD.Domain
  ├─ Services/
  │    ICurrentUserIdentity, ICurrentUser<T>, ICurrentUserStore<T>
  │    ICurrentUserAccessor<T>
  │    ICurrentTenant, ICurrentTenantAccessor
  │    IWorkContext<T>, WorkContext<T>          ← zero-infra, giữ Domain
  ├─ DataStorages/
  │    ITenantIdResolver*, ITenantConnectionStringResolver*  ← ports
  │    ConfigConnectionStringResolver, TenantConnectionStringResolverFactory
  └─ (không: CurrentUser*, CurrentTenant*, HTTP TenantIdResolver* impl)

Jarvis.Authentication
  ├─ CurrentUser<T>, CurrentUserAccessor<T>
  ├─ CurrentUserInfo
  └─ AddCurrentUser<T>()

Jarvis.Multitenancy
  ├─ CurrentTenant*, CurrentTenantAccessor
  ├─ DataStorages/ Header|User|Query|Host TenantIdResolver + TenantIdResolverFactory
  └─ AddCurrentTenant() / AddTenantIdResolvers()

Host / Sample
  └─ AddCurrentUser<T>() + AddCurrentTenant()
     + ICurrentUserStore<T> (DB/cache/Sample store)
     + ICurrentTenantStore<T>
```

```text
                    ┌──────────────┐
                    │     Host     │  AddCurrentUser + AddCurrentTenant
                    └──────┬───────┘
           ┌───────────────┼───────────────┐
           ▼               ▼               ▼
   ┌───────────────┐ ┌───────────┐ ┌──────────────────┐
   │ Authentication│ │  Domain   │ │ Multitenancy     │
   │ CurrentUser*  │ │ contracts │ │ CurrentTenant*   │
   │ CurrentUserInfo│ │+WorkContext│ │ CurrentTenantAccessor│
   └───────┬───────┘ └─────▲─────┘ └────────┬─────────┘
           │               │                │
           └───────────────┴────────────────┘
                 reference contracts only
```

---

## 5. Nguyên tắc migrate

1. Contract ổn định — không đổi chữ ký `ICurrentUser` / `ICurrentTenant` / R2.
2. Một chiều: Auth / Multitenancy → Domain.
3. Cut-over DI một lần — Sample + tests; xóa `AddCoreCurrentContext`. 🟢
4. Một ambient mỗi loại — đăng ký accessor chỉ trong `AddCurrentUser` / `AddCurrentTenant`. 🟢
5. EF: bỏ `TryAdd` `CurrentTenantAccessor` impl; chỉ consume interface; Host gọi `AddCurrentTenant()`. 🟢 (Phase 2)
6. OTEL.DDD: bỏ cast `CurrentUserInfo` (UserName → `"anonymous"` hoặc chỉ `UserId`). 🟢
7. Dọn trùng `DataStorages/ICurrentTenantAccessor` + `CurrentTenantAccessor` nếu còn — thống nhất `Services` contract. 🟢 (Phase 2)
8. Domain vẫn giữ `FrameworkReference` AspNetCore đến Phase 5 (resolvers HTTP còn `DataStorages`).

---

## 6. Kế hoạch triển khai

| Phase | Việc | Trạng thái |
|-------|------|------------|
| 0 | Accept ADR (P1–P6, §9) | 🟢 **Done** (2026-08-06) |
| 1 | Inventory + chốt Multitenancy + accessor → package đích | 🟢 **Done** |
| 2 | Tạo Multitenancy + move Tenant impl | 🟢 **Done** — [jarvis-multitenancy-package](./2026-08-06-adr-jarvis-multitenancy-package.md) |
| 3 | Move user impl → Authentication; `AddCurrentUser<T>()` | 🟢 **Done** (2026-08-06) |
| 4 | Sample / UnitTest: xóa `AddCoreCurrentContext`; OTEL bỏ cast `CurrentUserInfo`; build + test | 🟢 **Done** (2026-08-06) |
| 5 | Move tenant resolvers `Domain.DataStorages` → Multitenancy | 🟢 **Done** (2026-08-07) — = Multitenancy.EF Phase A; B–D tiếp theo ([ADR](./2026-08-06-adr-jarvis-multitenancy-entityframework.md)) |
| 6 | Skill / README Host (`AddCurrentUser` + `AddCurrentTenant`) | 🔴 **Later** — docs Host |
| — | (Follow-up) `Jarvis.Multitenancy.EntityFramework` | 🟢 **Done** — [ADR](./2026-08-06-adr-jarvis-multitenancy-entityframework.md) |

### Phase 2 — xem ADR Multitenancy

Chi tiết package + cut-over nửa Tenant: [2026-08-06-adr-jarvis-multitenancy-package.md](./2026-08-06-adr-jarvis-multitenancy-package.md).

### Phase 3–4 chi tiết (Done 2026-08-06)

1. **Authentication**: `ProjectReference` Domain; move `CurrentUser*` + `CurrentUserInfo` + `AddCurrentUser<T>()` (namespace `Jarvis.Authentication`).
2. **Domain**: xóa user impl; **xóa** `AddCoreCurrentContext`; giữ `AddCoreDomain` + `WorkContext`.
3. **OTEL.DDD**: bỏ cast `CurrentUserInfo` → `UserName` = `"anonymous"`.
4. **Sample**: `AddCurrentUser<CurrentUserInfo>()` + `AddCurrentTenant()` + store.
5. **UnitTest** + verify build/tests.

---

## 7. Inventory (điểm xuất phát → đích)

### Giữ trong Domain

| File / type | Ghi chú |
|-------------|---------|
| `ICurrentUserIdentity`, `ICurrentUser<T>`, `ICurrentUserStore<T>`, `ICurrentUserAccessor<T>` | Ports |
| `ICurrentTenant`, `ICurrentTenantAccessor` | Ports |
| `IWorkContext<T>`, `WorkContext<T>` | Facade zero-infra |
| `ITenantIdResolver*` | Tạm `DataStorages/` đến Phase 5 |

### Đã chuyển khỏi Domain

| Thành phần | Đích | Phase |
|------------|------|-------|
| `CurrentUser<TUser>` | `Jarvis.Authentication` | 🟢 3 |
| `CurrentUserAccessor<TUser>` | `Jarvis.Authentication` | 🟢 3 |
| `CurrentUserInfo` | `Jarvis.Authentication` | 🟢 3 |
| `CurrentTenant` | `Jarvis.Multitenancy` | 🟢 2 |
| `CurrentTenantAccessor` | `Jarvis.Multitenancy` | 🟢 2 |
| `AddCoreCurrentContext<TUser>` | **Xóa** → `AddCurrentUser` + `AddCurrentTenant` | 🟢 4 |

### Đã tách (không thuộc ADR này)

| Thành phần | Đích |
|------------|------|
| Enrich WorkContext → OTEL | 🟢 `Jarvis.OpenTelemetry.DDD` |

---

## 8. Hệ quả

### Tích cực

- Domain sạch infra auth/tenancy impl; Module Atomic.
- Host bật độc lập `AddCurrentUser` / `AddCurrentTenant`.
- Cùng pattern OTEL bridge.

### Chi phí

- Breaking DI Sample + namespace `CurrentUserInfo` → `Jarvis.Authentication`.
- OTEL mất enrich `UserName` từ `CurrentUserInfo` (dùng anonymous) trừ khi mở rộng contract sau.
- Resolvers còn Domain đến Phase 5; Domain vẫn AspNetCore vì resolvers.

### Rủi ro

- Move nửa vời → hai ambient. Mitigation: một PR cut-over + xóa registration Domain/EF. 🟢 Phase 2–4 đã cut-over.
- Contract move → breaking OTEL/EF. Mitigation: **giữ contract Domain** (C1).

---

## 9. Checklist confirm

- [x] Accept ADR tech debt (P1–P6, C1–C7)?
- [x] Tên package tenant: **`Jarvis.Multitenancy`**
- [x] Accessor impl: Domain **chỉ interface**; class → Authentication / Multitenancy
- [x] `AddCoreCurrentContext`: **xóa** khỏi Domain
- [x] Enrich / OTEL: bridge Done; cut-over bỏ cast `CurrentUserInfo`
- [x] **Không** generic `ICurrentTenant<TTenant>` trong ADR này
- [x] Namespace move: `Jarvis.Authentication` / `Jarvis.Multitenancy`
- [x] OTEL `UserName`: anonymous (không mở rộng identity đợt này)

---

## 10. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** (2026-08-06) |
| Implementation | 🟢 **Done** Phase 2–5 (Phase 5 resolvers 2026-08-07) |
| Evidence | `Jarvis.Authentication` (`CurrentUser*`, `AddCurrentUser`); `Jarvis.Multitenancy` (+ `AddTenantIdResolvers` / HTTP resolvers); Domain chỉ contract + `WorkContext` + connection-string ports; Sample `AddCurrentUser` + `AddCurrentTenant`; OTEL.DDD không cast `CurrentUserInfo` |
| Pattern đã có | OTEL → `Jarvis.OpenTelemetry` + `Jarvis.OpenTelemetry.DDD` |
| Owner | Jarvis |

Còn mở: Phase 6 Skill/README Host; [ORM family](./2026-08-07-adr-jarvis-orm-packages.md) rename.
