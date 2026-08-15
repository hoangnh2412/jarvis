# ADR — `Jarvis.Multitenancy.EntityFramework` (satellite Persistence)

> **Trạng thái:** 🟢 **Accepted + Implemented** (confirm §7 2026-08-07; Phase A–E 2026-08-07).  
> **Ngày:** 2026-08-06 · Accept §7: 2026-08-07  
> **Loại:** Package boundary / Module Atomic (core + satellite)  
> **Liên quan:** [jarvis-multitenancy-package](./2026-08-06-adr-jarvis-multitenancy-package.md) (🟢 Done), [split packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) (Phase 5 = Phase A **trước** B–D), [ORM family](./2026-08-07-adr-jarvis-orm-packages.md) (🟢 Done — `Jarvis.ORM.EntityFramework`), [architecture-rules.md](./architecture-rules.md) (§0.2 Core + satellite).  
> **Phạm vi:** tách **adapter EF gắn Multitenancy** khỏi `Jarvis.EntityFramework` (sau rename: `Jarvis.ORM.EntityFramework`) → package **`Jarvis.Multitenancy.EntityFramework`**; Host compose opt-in; giữ ORM.EF = infra EF chung.  
> **Ngoài phạm vi:** `Jarvis.Multitenancy.Dapper` (chỉ nêu pattern; foundation Dapper → [ORM ADR](./2026-08-07-adr-jarvis-orm-packages.md)); Tenants CRUD / Identity → [ADR `Jarvis.Tenants`](./2026-08-07-adr-jarvis-tenants-module.md) (🔴 Proposed); đổi semantic R2 / dedicated-DB vs shared-DB; Auth User move (split Phase 3–4).  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

Đã có:

| Package | Nội dung |
|---------|----------|
| `Jarvis.Multitenancy` | `CurrentTenant`, `CurrentTenantAccessor`, `AddCurrentTenant()` |
| `Jarvis.DDD.Domain` | Contract `ICurrentTenant*`; resolvers `DataStorages` (tạm — split Phase 5) |
| `Jarvis.EntityFramework` | UoW / repos / `BaseStorageContext` **và** wiring multitenancy EF |

ADR Multitenancy chủ động **không** tạo `Jarvis.Multitenancy.EntityFramework` trong slice core — để cắt nhỏ cut-over ambient.

Hệ quả hiện tại:

- Host gọi `AddEntityFramework()` là **kéo luôn** đăng ký `ITenantIdResolver*` + factory (private `AddMultitenancy`).
- Adapter tenant–EF (`TenantDbConnectionInterceptor`, `DbTenantConnectionStringResolver`, cache connection string, overload `AddCoreDbContext` có interceptor) nằm trong package EF chung.
- App muốn **Multitenancy + Dapper** (hoặc persistence khác) vẫn bị phụ thuộc khái niệm / wiring EF, hoặc phải fork copy.

Module Atomic ([architecture-rules](./architecture-rules.md) §0.2): **core + satellite** — cùng pattern `Jarvis.Caching` + `Jarvis.Caching.Redis`, `Jarvis.Authentication` + `Jarvis.Authentication.Jwt`.

---

## 2. Vấn đề

| # | Debt | Hệ quả |
|---|------|--------|
| D1 | Tenant–EF wiring trong `Jarvis.EntityFramework` | Không opt-in Multitenancy persistence; EF “biết” tenancy quá sâu |
| D2 | `AddEntityFramework` gọi `AddMultitenancy` | Host không chọn được “chỉ EF, không tenant resolvers” |
| D3 | Chưa có nhà cho satellite Persistence | Khó thêm `*.Dapper` / provider khác mà không phình EF core |
| D4 | Resolvers HTTP vẫn Domain; đăng ký DI nằm EF | Boundary lệch (Phase 5 split + ADR này cần phối hợp) |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | `Jarvis.Multitenancy` **không** reference EF / Dapper |
| C2 | `Jarvis.Multitenancy.EntityFramework` → `Jarvis.Multitenancy` + `Jarvis.EntityFramework` + Domain contracts (một chiều) |
| C3 | `Jarvis.EntityFramework` **không** reference `Jarvis.Multitenancy` / `Jarvis.Multitenancy.EntityFramework` |
| C4 | Domain **không** reference Multitenancy.* ; UoW/EF chỉ consume `ICurrentTenantAccessor` / `ITenantIdResolver*` (ports) |
| C5 | Semantic R2 / dedicated vs shared DB / interceptor behavior **không** đổi trong ADR này — chỉ đổi **chỗ package + DI compose** |
| C6 | Host là composition: `AddCurrentTenant` + (opt-in) `AddMultitenancyEntityFramework` / `AddCoreDbContext*` tenant |

---

## 4. Quyết định (Accepted 2026-08-07)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| E1 | Tên package | **`Jarvis.Multitenancy.EntityFramework`** (`frameworks/Jarvis.Multitenancy.EntityFramework`) |
| E2 | Vai trò | Satellite Persistence: interceptor, resolver connection string kiểu EF, DI tenant–DbContext |
| E3 | `Jarvis.EntityFramework` (sau rename: [`Jarvis.ORM.EntityFramework`](./2026-08-07-adr-jarvis-orm-packages.md)) giữ | Repos, `BaseUnitOfWork`, `BaseStorageContext` (+ global query filter `ITenantEntity`), helpers query — **infra EF chung**, consume port Domain |
| E4 | Move sang Multitenancy.EF | Xem inventory §4.1 |
| E5 | DI Host | `AddEntityFramework()` **không** còn gọi `AddMultitenancy`; Host opt-in `AddMultitenancyEntityFramework()` (tên cuối chốt §7) |
| E6 | Resolvers HTTP / factory (không EF) | **Không** để lâu trong Multitenancy.EF — đích **`Jarvis.Multitenancy`** (đồng bộ [split Phase 5](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md)); tạm có thể move đăng ký DI khỏi EF trước hoặc cùng PR Phase A |
| E7 | Dapper / provider khác | Foundation → [`Jarvis.ORM.Dapper`](./2026-08-07-adr-jarvis-orm-packages.md); satellite tenant → `Jarvis.Multitenancy.Dapper` (Later, ADR riêng) — **không** nhét vào Multitenancy.EF |
| E8 | `AddCoreDbContext*` | Overload **có** `TenantDbConnectionInterceptor` / keyed tenant connection resolver → Multitenancy.EF; overload shared/master cố định connection có thể giữ EF **hoặc** chuyển nếu chỉ phục vụ tenancy — chốt inventory khi implement (ưu tiên: mọi API “tenant connection” → Multitenancy.EF) |

### 4.1 Inventory (đích)

```text
Jarvis.Multitenancy                          (core — đã có + Phase 5)
  ├─ CurrentTenant*, AddCurrentTenant()
  └─ (Phase 5) ITenantIdResolver* impl, factories không-EF,
                AddTenantIdResolvers() / gộp vào AddCurrentTenant

Jarvis.Multitenancy.EntityFramework          (satellite — ADR này)
  ├─ TenantDbConnectionInterceptor
  ├─ DbTenantConnectionStringResolver<,>
  ├─ AddMultitenancyEntityFramework()          // opt-in
  └─ AddCoreDbContext<TDb, TResolver>() …      // tenant-aware overloads

Jarvis.EntityFramework                       (infra chung; sau rename → Jarvis.ORM.EntityFramework)
  ├─ BaseStorageContext, BaseUnitOfWork, repos
  ├─ CachingTenantConnectionStringResolver     // shared (config + tenant keys)
  ├─ AddEntityFramework()                      // repos + config keyed cache
  ├─ AddCoreDbContext<T>()                     // shared/master, không interceptor
  └─ (không) interceptor tenant / AddCoreDbContext<T, TResolver>

Host
  ├─ AddCurrentTenant()
  ├─ AddEntityFramework()                      // optional
  └─ AddMultitenancyEntityFramework()          // khi dùng EF + tenant DB
       + AddCoreDbContext<…>(…)
```

```text
                    ┌──────────────┐
                    │     Host     │
                    └──────┬───────┘
           ┌───────────────┼────────────────┐
           ▼               ▼                ▼
   ┌───────────────┐ ┌──────────┐  ┌──────────────────────────┐
   │ Multitenancy  │ │ Domain   │  │ Multitenancy.EntityFramework │
   │ ambient+id    │ │ ports    │  │ interceptor / DbTenant*      │
   └───────┬───────┘ └────▲─────┘  └────────────┬─────────────┘
           │              │                     │
           └──────────────┴─────────────────────┘
                          │
                          ▼
                 ┌─────────────────┐
                 │ EntityFramework │  UoW / BaseStorageContext
                 │ (không ref MT)  │
                 └─────────────────┘
```

---

## 5. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 (E1–E8) | Status → Accepted | — 🟢 |
| A | Resolvers không-EF: Domain → Multitenancy (+ DI ra khỏi `AddEntityFramework`) | Split Phase 5 🟢 | 🟢 Done 2026-08-07 |
| B | Tạo `Jarvis.Multitenancy.EntityFramework.csproj` + `Jarvis.sln` | Project build | 🟢 |
| C | Move interceptor / `DbTenant*`; `AddCoreDbContext<T,TResolver>` → Multitenancy.EF | EF không còn interceptor tenant | 🟢 |
| D | Sample + UnitTest + skill | Build + test xanh | 🟢 |
| E | ADR / README Done | Docs đồng bộ | 🟢 |

**E8 chốt implement:** `CachingTenantConnectionStringResolver` **giữ** EF (dùng chung config + tenant); overload `AddCoreDbContext<T>` giữ EF; overload `AddCoreDbContext<T, TResolver>` + interceptor + `DbTenant*` → Multitenancy.EF.

### Host compose (sketch sau cut-over)

```csharp
builder.AddCurrentTenant();                 // Multitenancy
builder.AddEntityFramework();               // repos / UoW base — không kéo tenant DI
builder.Services.AddMultitenancyEntityFramework(); // hoặc AddCoreDbContext tenant từ package này
// shared master:
builder.Services.AddCoreDbContext<MasterDbContext>(…);           // chốt package theo E8
// dedicated tenant DB:
builder.Services.AddCoreDbContext<TenantDbContext, DbTenantConnectionStringResolver<…>>(…);
```

---

## 6. Hệ quả

### Tích cực

- Opt-in Persistence theo module; mở đường `*.Dapper` không phình EF.
- `Jarvis.EntityFramework` trở lại concern “EF chung”.
- Khớp Module Atomic + ADR Multitenancy đã hẹn.

### Chi phí

- Breaking DI Host/Sample: thêm reference + gọi extension Multitenancy.EF.
- Phase A (= split Phase 5) **bắt buộc trước** B–D — không để resolvers HTTP tạm trong Multitenancy.EF.

### Rủi ro

| Rủi ro | Mitigation |
|--------|------------|
| Quên gọi `AddMultitenancyEntityFramework` | Sample + test + skill `entityframework-dotnet` / pattern multitenancy |
| Vòng phụ thuộc EF ↔ Multitenancy | C3: EF không ref Multitenancy.*; UoW chỉ port Domain |
| Move `BaseStorageContext` nhầm sang Multitenancy.EF | E3: **giữ** EF — filter `ITenantEntity` là behavior storage, không phải package boundary Persistence provider |

---

## 7. Checklist confirm

### Confirm §7 (2026-08-07)

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 1 | Accept E1–E8, C1–C6? | **OK** |
| 2 | Tên package | **`Jarvis.Multitenancy.EntityFramework`** — **OK** |
| 3 | `BaseStorageContext` / `BaseUnitOfWork` | **Giữ** EF foundation (E3; sau rename `Jarvis.ORM.EntityFramework`) — **OK** |
| 4 | `AddEntityFramework` bỏ auto `AddMultitenancy` | **OK** (breaking Host) |
| 5 | Thứ tự | **Phase A (split Phase 5) trước**, rồi B–D — **OK** (rename ORM: xem [ORM ADR](./2026-08-07-adr-jarvis-orm-packages.md) O10) |
| 6 | `Jarvis.Multitenancy.Dapper` | Ngoài phạm vi; foundation = `Jarvis.ORM.Dapper` — **OK để sau** |
| 7 | Tên extension DI | **`AddMultitenancyEntityFramework`** — **OK** |

- [x] Accept E1–E8, C1–C6
- [x] Tên package: `Jarvis.Multitenancy.EntityFramework`
- [x] Giữ `BaseStorageContext` / UoW trong EF
- [x] `AddEntityFramework` không còn kéo tenant DI
- [x] Phase A = split Phase 5 **trước** B–D
- [x] Dapper ngoài phạm vi (Later)
- [x] Extension DI: `AddMultitenancyEntityFramework`

---

## 8. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** (2026-08-07) |
| Implementation | 🟢 **Done** Phase A–E (2026-08-07) |
| Blocked by | — |
| Unblocks | Host chọn EF vs Dapper cho Multitenancy; EF package sạch concern; ORM rename (O10) ít churn hơn |
| Owner | Jarvis |
