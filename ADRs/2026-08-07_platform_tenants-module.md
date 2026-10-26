# ADR — Module quản lý Tenant (`Platform.Tenants`)

> **Trạng thái:** 🟢 **Accepted** (2026-10-25). Implementation 🟡 package + EF + API `/platform/tenants`, Host, migration `AddPlatformTenantColumns`, policy root, UI `/platform/tenants`.  
> **Ngày:** 2026-08-07 · Cập nhật phân cấp và §7: 2026-10-25  
> **Loại:** Business module / Module Atomic (Library + Persistence)  
> **Liên quan:** [Multitenancy package](./2026-08-06-adr-platform-multitenancy-package.md) (🟢 ambient), [generic `ICurrentTenant<TTenant>`](./2026-08-06-adr-generic-current-tenant.md) (🟢 profile + store), [Multitenancy.EF](./2026-08-06-adr-platform-multitenancy-entityframework.md) (🟢 — Persistence adapter; **khác** ADR này), [Setting](../modules/settings/Platform.Setting/doc/2026-07-30-adr-setting.md) (pattern module nghiệp vụ), [architecture-rules.md](./architecture-rules.md) (§0.2).  
> **Phạm vi:** quyết định **tách** quản lý catalog / CRUD tenant thành module nghiệp vụ **`Platform.Tenants`** (giống Setting); **không** nhét vào `Platform.Multitenancy`; facade + contract entity + opt-in Persistence; `ICurrentTenantStore` adapter. Tenant **có cha-con**: Tenant con tạo Org trong vùng của nó. Tenant không giữ bản ghi hóa đơn. Catalog tenant nằm ở Master DB. Mỗi tenant có chuỗi kết nối riêng, bắt buộc.  
> **Change log:** 2026-10-25 — Kéo trạng thái: library, Host, migration và UI `/platform/tenants` đã có. 2026-10-25 — Thêm T11 `ParentTenantId`. Chi nhánh và đại lý tự tạo Org khách là Tenant con. Sơ đồ: [multi-tenant, multi-org](../docs/guides/multitenancy/multi-tenant-multi-org.md). 2026-10-25 — Accept §7: T1–T14, C1–C8. `ITenantEntity` giữ Domain. Catalog là `ITenantCatalogEntity`. Chuỗi kết nối bắt buộc. Xóa mềm. User root một bản ghi trên Master. API vận hành `Platform.Tenants.Api`, route `/platform/tenants`.
> **Ngoài phạm vi:** ambient / R2 / `Change` / HTTP resolvers; `Platform.Multitenancy.EntityFramework` (interceptor / connection); Authorization engine; Identity User CRUD ngoài user root lúc cài đặt; SettingManagement; dedicated-DB provisioning tự động.  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

Platform đã có **hai lớp** liên quan tenant, nhưng **chưa** có module quản lý tenant:

| Lớp | Package | Việc |
|-----|---------|------|
| Ambient / working tenant | `Platform.Multitenancy` (+ Domain contracts) | *Request này đang làm việc trên tenant nào?* — `GetIdAsync` / `GetAsync` / `Change`, accessor Guid |
| Persistence adapter (đề xuất) | `Platform.Multitenancy.EntityFramework` | Connection string / interceptor theo tenant đang làm việc |
| Profile port | `ICurrentTenantStore<TTenant>` | Host (hoặc module) **load** `TTenant` theo Guid — chưa có nhà CRUD chuẩn |

`CurrentTenantInfo` mặc định chỉ `TenantId` + `Name`. Host muốn catalog thật (Code, Active, connection metadata, …) phải tự viết entity/API — lặp lại giữa các sản phẩm.

**Setting** đã là precedent module nghiệp vụ: facade (`ISettingManager`), contract entity host-owned, Library + satellite Persistence, **không** sở hữu tenant resolution.

Câu hỏi đã chốt hướng (thảo luận 2026-08-07): **không** elevate `Platform.Multitenancy` thành “Setting-for-tenants”. Cần ADR riêng cho phần **quản lý tenant**.

---

## 2. Vấn đề

| # | Debt | Hệ quả |
|---|------|--------|
| D1 | Không có package CRUD / catalog tenant | Mỗi host tự invent schema + API; lệch `ICurrentTenantIdentity` |
| D2 | Dễ nhầm gộp CRUD vào Multitenancy / Multitenancy.EF | Core ambient kéo ORM + nghiệp vụ; phá opt-in Atomic |
| D3 | `ICurrentTenantStore` bắt Host tự implement | Không có adapter mẫu “đọc từ bảng Tenant” |
| D4 | Setting / UoW cần tenant tồn tại nhưng Platform không định nghĩa lifecycle tenant | Onboarding / disable tenant ad-hoc |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | **`Platform.Multitenancy` giữ infra ambient** — không CRUD, không DbSet Tenant |
| C2 | **`Platform.Multitenancy.EntityFramework` chỉ** adapter connection/interceptor — **không** Tenants CRUD |
| C3 | Module Tenants → Domain contracts (+ Multitenancy chỉ nếu cần type default / compose Host); **Domain không** reference Tenants |
| C4 | Multitenancy **không** reference Tenants (tránh vòng; Host compose store) |
| C5 | Entity concrete + migration = **host-owned** (pattern Setting) |
| C6 | Quyền, audit và màn hình thuộc Host. Module cung cấp facade và package API. Route vận hành là `/platform/tenants` trên cùng binary Host, không trộn `/he-thong` |
| C7 | Không đổi semantic R2 / D5 / Guid `ICurrentTenantAccessor` |
| C8 | Pattern Atomic: `Platform.Tenants` + satellite `Platform.Tenants.EntityFramework` (và cache sau nếu cần) |

---

## 4. Quyết định (Accepted 2026-10-25)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| T1 | Tên module / package core | **`Platform.Tenants`** — thư mục `modules/tenants/` (cùng kiểu `modules/settings/`) |
| T2 | Vai trò | Catalog + lifecycle tenant: tạo / đọc / cập nhật / vô hiệu hóa (và list/filter tối thiểu). Vô hiệu hóa là xóa mềm (`IsActive = false`). Không hard delete |
| T3 | **Không** gộp vào Multitenancy | Multitenancy = ambient; Tenants = quản lý bản ghi tenant — hai package |
| T4 | Facade | **`ITenantManager`** — UI/API **không** query `DbSet` trực tiếp |
| T5 | Hai hợp đồng, không gộp tên | **`ITenantEntity`** và **`IOrgEntity`** giữ trong **`Platform.DDD.Domain`**: dấu trên dòng nghiệp vụ (hóa đơn thuộc tenant nào, org nào). Catalog tenant là **`ITenantCatalogEntity`** trong `Platform.Tenants`: `Id`, `Name`, `Code`, `IsActive`, `ParentTenantId`, chuỗi kết nối. Host thêm cột qua entity concrete |
| T6 | Satellite Persistence | **`Platform.Tenants.EntityFramework`** — đăng ký repo / mapping helpers; **không** thay Multitenancy.EF |
| T7 | Store bridge | **`ICurrentTenantStore<TTenant>`** đọc từ `ITenantManager` / repo — Host `AddPlatformTenants<…>().UseAsCurrentTenantStore()` |
| T8 | Connection string | **Bắt buộc** trên từng tenant. Resolve lúc request vẫn thuộc Multitenancy(.EF) / `ITenantConnectionStringResolver`. Tenants không copy interceptor |
| T9 | DI Host | `AddCurrentTenant` + `AddPlatformTenants<TEntity, TUoW>()` — Tenants **không** thay ambient |
| T10 | Skill | Khi implement: skill `tenants-dotnet` (workflows init/add) — Phase docs |
| T11 | Tenant nhiều cấp | `ParentTenantId` nullable. Tenant gốc (Minvoice, EPAY, VNA) để trống. Tenant con (chi nhánh HCM, HN, đại lý) trỏ Tenant cha. Đại lý thuộc HCM thì cha là Tenant HCM. Tenant con tạo Org của riêng nó. Không có Org cha-con. Ngữ cảnh làm việc là tenant con mà user đang đăng nhập |
| T12 | Catalog và user root | Bảng tenant và user root nằm ở **Master DB**. Đúng một user tạo lúc cài đặt: `TenantId = null`, `OrgId = null`, không thuộc tenant hay org. Đăng nhập chỉ chấp nhận hai giá trị null khi user đó có quyền Platform. Request thiếu `X-Tenant-Id` / `X-Org-Id` không được thành root |
| T13 | API vận hành | Package **`Platform.Tenants.Api`**. Cùng binary Host. Route `/platform/tenants`, policy Platform. Không nằm trong `/he-thong` |
| T14 | Nhìn Org theo cây | Tenant thấy Org của chính nó và Org của tenant thấp hơn. Không thấy Org của tenant cùng cấp hoặc cao hơn. Luật danh sách nằm ở [ADR Orgs](./2026-10-17_platform_orgs-module.md) |

### 4.1 Ranh giới với Multitenancy / Setting

```text
  [Authorization]     [Audit]        [UI Admin]
         │               │                │
         └──────── Host ─┴────────────────┘
                          │
          ┌───────────────┼───────────────────────┐
          ▼               ▼                       ▼
  Platform.Multitenancy   Platform.Tenants    Platform.Setting
  ambient / R2 / Change   catalog + CRUD      settings theo tenant
  + Multitenancy.EF       + Tenants.EF        + Setting.EF
  (connection)            (bảng Tenant)       (bảng Setting)
          │                       │
          │    ICurrentTenantStore (optional bridge)
          └───────────────────────┘
```

| Concern | Thuộc |
|---------|--------|
| Header / claim → Guid đang làm việc | Multitenancy (+ resolvers) |
| Đổi ambient `Change(Guid)` | Multitenancy |
| Interceptor / DbContext tenant connection | Multitenancy.EF |
| Tạo tenant mới, đổi Name/Code, Active=false | **Tenants** |
| Giá trị SMTP / feature config của tenant | Setting |
| Load profile cho `GetAsync()` | Store — Host **hoặc** bridge Tenants |

### 4.2 Inventory (đích — sketch)

```text
modules/tenants/
  Platform.Tenants/
    ├─ Services/ITenantManager.cs, TenantManager.cs
    ├─ Models/… (DTO đọc/ghi nếu tách entity)
    ├─ Extensions/AddPlatformTenants(…)
    └─ (optional) CurrentTenantStoreFromTenants<T>…

  Platform.Tenants.EntityFramework/
    └─ mapping helpers / AddPlatformTenantsEntityFramework (nếu cần)

Platform.DDD.Domain
  └─ ITenantEntity, IOrgEntity     // dấu TenantId / OrgId trên dòng nghiệp vụ

Platform.Tenants
  └─ ITenantCatalogEntity          // bản ghi catalog: Id, Code, Name, ParentTenantId, ConnectionString

Host
  ├─ AddCurrentTenant<TTenant>()
  ├─ AddPlatformTenants<Tenant, IMasterUnitOfWork>()
  ├─ Platform.Tenants.Api → /platform/tenants
  └─ entity Tenant : ITenantCatalogEntity (Master DB)
```

### 4.3 Phạm vi chức năng tối thiểu (MVP khi implement)

| # | Chức năng | Ghi chú |
|---|-----------|---------|
| 1 | Create / GetById / Update / SetActive | Xóa mềm. Không hard delete |
| 2 | List + filter (Active, search Name/Code) | Phân trang tối thiểu |
| 3 | Unique `Code` (nếu có) | Index host-owned |
| 4 | Bridge `ICurrentTenantStore` | Opt-in |
| 5 | Sample API + skill | Sau accept |

**Không** MVP: auto-provision DB per tenant, billing, tenant-user membership (Identity), feature flags engine.

---

## 5. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 (T1–T10) | Status → Accepted | — |
| A | Contract `ITenantEntity` + `ITenantManager` skeleton + DI | Build | 0 |
| B | CRUD MVP + host entity Sample (master DB) | Test xanh | A |
| C | Optional `ICurrentTenantStore` bridge | Sample `GetAsync` từ bảng Tenant | B |
| D | Satellite `Platform.Tenants.EntityFramework` nếu còn logic EF tách được | Opt-in Persistence | B |
| E | Skill `tenants-dotnet` + ADR/README Done | Docs đồng bộ | C–D |

**Thứ tự so với Multitenancy.EF:** độc lập. Multitenancy.EF / Phase 5 resolvers **không** block Tenants; Tenants **không** block Multitenancy.EF. Làm Tenants khi có nhu cầu sản phẩm catalog; làm Multitenancy.EF theo ưu tiên package boundary.

---

## 6. Hệ quả

### Tích cực

- Ranh giới rõ: ambient ≠ catalog (tránh God-package Multitenancy).
- Cùng pattern Setting: Atomic, host-owned schema, facade.
- Có chỗ chuẩn cho `ICurrentTenantStore` mà không buộc mọi host tự viết.

### Chi phí

- Thêm module + Sample + skill.
- Host phải phân biệt master DB (catalog Tenant) vs tenant-scoped DB (Setting, …).

### Rủi ro

| Rủi ro | Mitigation |
|--------|------------|
| Nhầm Tenants.EF ↔ Multitenancy.EF | T6 + bảng §4.1; skill tách workflow |
| Module Tenants reference Multitenancy vòng | C4: bridge đăng ký ở Host hoặc Tenants → Domain store port only |
| Soft-delete vs hard-delete phá FKs | Đã chốt: chỉ `IsActive = false` |
| Connection string trên entity | Bắt buộc theo tenant. Mã hóa lúc lưu thuộc Host, không đổi luật “mỗi tenant một chuỗi” |

---

## 7. Checklist confirm

Đã chốt 2026-10-25.

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 1 | Accept T1–T11, C1–C8? | Có. Thêm T12–T14 |
| 2 | Tên package | `Platform.Tenants`, `Platform.Tenants.EntityFramework`, `Platform.Tenants.Api` |
| 3 | Facade | `ITenantManager` |
| 4 | `ITenantEntity` đặt ở | Giữ trong Domain, cạnh `IOrgEntity`. Catalog là `ITenantCatalogEntity` |
| 5 | Xóa tenant | Xóa mềm |
| 6 | Bridge `ICurrentTenantStore` | Có |
| 7 | Connection string | Bắt buộc trên từng tenant |
| 8 | Thứ tự vs Multitenancy.EF | Độc lập |
| 9 | Tenant đang làm việc | Tenant con mà user đăng nhập |
| 10 | User root | Một user lúc cài đặt. `TenantId` và `OrgId` null. Nằm ở Master DB. Chỉ chấp nhận khi có quyền Platform |
| 11 | Catalog tenant | Master DB |
| 12 | Màn vận hành | Cùng binary. Route `/platform/tenants` và policy Platform |

- [x] Accept T1–T14, C1–C8
- [x] Tên package và `ITenantManager`
- [x] `ITenantEntity` ở Domain; `ITenantCatalogEntity` cho catalog
- [x] Xóa mềm, store bridge, chuỗi kết nối bắt buộc
- [x] User root và route vận hành

---

## 8. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** 2026-10-25 (T1–T14, C1–C8) |
| Implementation | 🟡 **Đang làm** — package `Platform.Tenants` + EF + API `/platform/tenants`, test `TenantManagerTests` (7). Host gắn catalog lên `identity.Tenants`, user cài đặt là root (`TenantId`/`OrgId` null), migration `AddPlatformTenantColumns`, policy root. UI `/platform/tenants` đã có (danh sách, tạo, sửa, bật/tắt, xem chuỗi kết nối; không xóa cứng) |
| Blocked by | Không |
| Unblocks | Catalog tenant chuẩn; optional store cho `ICurrentTenant.GetAsync`; Sample onboarding |
| Không thay thế | [Multitenancy.EF](./2026-08-06_platform_multitenancy-entityframework.md), split Phase 5 resolvers |
| Owner | Platform |
