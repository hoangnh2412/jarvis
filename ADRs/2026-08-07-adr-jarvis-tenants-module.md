# ADR — Module quản lý Tenant (`Jarvis.Tenants`)

> **Trạng thái:** 🔴 **Proposed** (chờ confirm §7).  
> **Ngày:** 2026-08-07  
> **Loại:** Business module / Module Atomic (Library + Persistence)  
> **Liên quan:** [Multitenancy package](./2026-08-06-adr-jarvis-multitenancy-package.md) (🟢 ambient), [generic `ICurrentTenant<TTenant>`](./2026-08-06-adr-generic-current-tenant.md) (🟢 profile + store), [Multitenancy.EF](./2026-08-06-adr-jarvis-multitenancy-entityframework.md) (🟢 — Persistence adapter; **khác** ADR này), [Setting](../modules/settings/Jarvis.Setting/doc/2026-07-30-adr-setting.md) (pattern module nghiệp vụ), [architecture-software.md](./architecture-software.md) (§0.2).  
> **Phạm vi:** quyết định **tách** quản lý catalog / CRUD tenant thành module nghiệp vụ **`Jarvis.Tenants`** (giống Setting); **không** nhét vào `Jarvis.Multitenancy`; facade + contract entity + opt-in Persistence; optional `ICurrentTenantStore` adapter.  
> **Ngoài phạm vi:** ambient / R2 / `Change` / HTTP resolvers; `Jarvis.Multitenancy.EntityFramework` (interceptor / connection); Authorization / RBAC; Identity User CRUD; SettingManagement; UI admin đầy đủ (chỉ Sample/API tối thiểu khi implement); dedicated-DB provisioning tự động.  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

Jarvis đã có **hai lớp** liên quan tenant, nhưng **chưa** có module quản lý tenant:

| Lớp | Package | Việc |
|-----|---------|------|
| Ambient / working tenant | `Jarvis.Multitenancy` (+ Domain contracts) | *Request này đang làm việc trên tenant nào?* — `GetIdAsync` / `GetAsync` / `Change`, accessor Guid |
| Persistence adapter (đề xuất) | `Jarvis.Multitenancy.EntityFramework` | Connection string / interceptor theo tenant đang làm việc |
| Profile port | `ICurrentTenantStore<TTenant>` | Host (hoặc module) **load** `TTenant` theo Guid — chưa có nhà CRUD chuẩn |

`CurrentTenantInfo` mặc định chỉ `TenantId` + `Name`. Host muốn catalog thật (Code, Active, connection metadata, …) phải tự viết entity/API — lặp lại giữa các sản phẩm.

**Setting** đã là precedent module nghiệp vụ: facade (`ISettingManager`), contract entity host-owned, Library + satellite Persistence, **không** sở hữu tenant resolution.

Câu hỏi đã chốt hướng (thảo luận 2026-08-07): **không** elevate `Jarvis.Multitenancy` thành “Setting-for-tenants”. Cần ADR riêng cho phần **quản lý tenant**.

---

## 2. Vấn đề

| # | Debt | Hệ quả |
|---|------|--------|
| D1 | Không có package CRUD / catalog tenant | Mỗi host tự invent schema + API; lệch `ICurrentTenantIdentity` |
| D2 | Dễ nhầm gộp CRUD vào Multitenancy / Multitenancy.EF | Core ambient kéo ORM + nghiệp vụ; phá opt-in Atomic |
| D3 | `ICurrentTenantStore` bắt Host tự implement | Không có adapter mẫu “đọc từ bảng Tenant” |
| D4 | Setting / UoW cần tenant tồn tại nhưng Jarvis không định nghĩa lifecycle tenant | Onboarding / disable tenant ad-hoc |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | **`Jarvis.Multitenancy` giữ infra ambient** — không CRUD, không DbSet Tenant |
| C2 | **`Jarvis.Multitenancy.EntityFramework` chỉ** adapter connection/interceptor — **không** Tenants CRUD |
| C3 | Module Tenants → Domain contracts (+ Multitenancy chỉ nếu cần type default / compose Host); **Domain không** reference Tenants |
| C4 | Multitenancy **không** reference Tenants (tránh vòng; Host compose store) |
| C5 | Entity concrete + migration = **host-owned** (pattern Setting) |
| C6 | Authorization, audit, UI production = Host — module chỉ facade nghiệp vụ |
| C7 | Không đổi semantic R2 / D5 / Guid `ICurrentTenantAccessor` |
| C8 | Pattern Atomic: `Jarvis.Tenants` + satellite `Jarvis.Tenants.EntityFramework` (và cache sau nếu cần) |

---

## 4. Quyết định (Proposed — chờ §7)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| T1 | Tên module / package core | **`Jarvis.Tenants`** — thư mục `modules/tenants/` (cùng kiểu `modules/settings/`) |
| T2 | Vai trò | Catalog + lifecycle tenant: tạo / đọc / cập nhật / vô hiệu hóa (và list/filter tối thiểu) |
| T3 | **Không** gộp vào Multitenancy | Multitenancy = ambient; Tenants = quản lý bản ghi tenant — hai package |
| T4 | Facade | **`ITenantManager`** (tên chốt §7) — UI/API **không** query `DbSet` trực tiếp |
| T5 | Contract persistence | **`ITenantEntity`** (Domain hoặc Tenants.Abstractions — chốt §7) — tối thiểu Id + Name + Active; field mở rộng host |
| T6 | Satellite Persistence | **`Jarvis.Tenants.EntityFramework`** — đăng ký repo / mapping helpers; **không** thay Multitenancy.EF |
| T7 | Store bridge | Optional **`ICurrentTenantStore<TTenant>`** impl đọc từ `ITenantManager` / repo — Host `AddJarvisTenants<…>().UseAsCurrentTenantStore()` (API chốt khi implement) |
| T8 | Connection string trên entity | **Cho phép** field optional (vd. `ConnectionString` / encrypted) cho dedicated-DB — **resolve runtime** vẫn thuộc Multitenancy(.EF) / `ITenantConnectionStringResolver`, không copy interceptor vào Tenants |
| T9 | DI Host | `AddCurrentTenant` + (opt-in) `AddJarvisTenants<TEntity, TUoW>()` — Tenants **không** thay ambient |
| T10 | Skill | Khi implement: skill `tenants-dotnet` (workflows init/add) — Phase docs |

### 4.1 Ranh giới với Multitenancy / Setting

```text
  [Authorization]     [Audit]        [UI Admin]
         │               │                │
         └──────── Host ─┴────────────────┘
                          │
          ┌───────────────┼───────────────────────┐
          ▼               ▼                       ▼
  Jarvis.Multitenancy   Jarvis.Tenants    Jarvis.Setting
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
  Jarvis.Tenants/
    ├─ Services/ITenantManager.cs, TenantManager.cs
    ├─ Models/… (DTO đọc/ghi nếu tách entity)
    ├─ Extensions/AddJarvisTenants(…)
    └─ (optional) CurrentTenantStoreFromTenants<T>…

  Jarvis.Tenants.EntityFramework/
    └─ mapping helpers / AddJarvisTenantsEntityFramework (nếu cần)

Jarvis.DDD.Domain (hoặc Tenants)
  └─ Entities/ITenantEntity.cs     // Id, Name, IsActive, … — chốt chỗ đặt §7

Host
  ├─ AddCurrentTenant<TTenant>()
  ├─ AddJarvisTenants<Tenant, IMasterUnitOfWork>()
  └─ entity Tenant : ITenantEntity (+ map master DB)
```

### 4.3 Phạm vi chức năng tối thiểu (MVP khi implement)

| # | Chức năng | Ghi chú |
|---|-----------|---------|
| 1 | Create / GetById / Update / SetActive | Soft-disable ưu tiên hơn hard delete (chốt §7) |
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
| D | Satellite `Jarvis.Tenants.EntityFramework` nếu còn logic EF tách được | Opt-in Persistence | B |
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
| Soft-delete vs hard-delete phá FKs | MVP: `IsActive`; hard delete Later + ADR follow-up |
| Connection string plaintext trên entity | Encrypt at rest (pattern Setting) hoặc secret store — Phase sau MVP |

---

## 7. Checklist confirm

### Confirm §7 (điền khi accept)

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 1 | Accept T1–T10, C1–C8? | _chờ_ |
| 2 | Tên package | **`Jarvis.Tenants`** (+ `Jarvis.Tenants.EntityFramework`) — OK? |
| 3 | Facade | **`ITenantManager`** — OK / tên khác? |
| 4 | `ITenantEntity` đặt ở | Domain **hay** trong `Jarvis.Tenants`? |
| 5 | Soft-disable (`IsActive`) mặc định, hard delete Later? | _chờ_ |
| 6 | Bridge `ICurrentTenantStore` trong MVP? | Yes / Later |
| 7 | Connection string trên entity | Optional field MVP / chỉ Later |
| 8 | Thứ tự vs Multitenancy.EF | Độc lập (không block nhau) — OK? |

- [ ] Accept T1–T10, C1–C8
- [ ] Tên: `Jarvis.Tenants` (+ satellite EF)
- [ ] Facade: `ITenantManager`
- [ ] Chốt chỗ `ITenantEntity`
- [ ] Soft-disable vs hard delete
- [ ] Store bridge MVP?
- [ ] Connection string trên entity?
- [ ] Độc lập lịch với Multitenancy.EF

---

## 8. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🔴 **Proposed** |
| Implementation | 🔴 **Not started** |
| Blocked by | Confirm §7 |
| Unblocks | Catalog tenant chuẩn; optional store cho `ICurrentTenant.GetAsync`; Sample onboarding |
| Không thay thế | [Multitenancy.EF](./2026-08-06-adr-jarvis-multitenancy-entityframework.md), split Phase 5 resolvers |
| Owner | Jarvis |
