# ADR — Module quản lý Organization (`Platform.Orgs`)

> **Trạng thái:** 🔴 **Proposed** (chờ confirm §7).  
> **Ngày:** 2026-10-17  
> **Loại:** Business module / Module Atomic (Library + Persistence + optional Abstractions)  
> **Liên quan:** [Tenants module](./2026-08-07_platform_tenants-module.md) (🔴 — pattern catalog song song), [Multitenancy package](./2026-08-06_platform_multitenancy-package.md) (🟢 ambient), [generic `ICurrentTenant<TTenant>`](./2026-08-06_platform_generic-current-tenant.md) (🟢), [Current User / Tenant](./2026-08-01_platform_current-user-tenant.md) (🟢 — D5 home vs working), [RBAC + ABAC](./2026-08-14_platform_rbac-abac-authorization.md) (🟡 — `User.OrgId`, `SameOrg`, `OrgAdmin`), [OrgStructure module](./2026-10-17_platform_orgstructure-module.md) (🔴 — cây nội bộ **trong** một Org), [architecture-rules.md](./architecture-rules.md) (§0.2).  
> **Phạm vi:** Tách **catalog / lifecycle Organization (Org)** — khách hàng cuối A, B, C, D thuộc đại lý Tenant X, Y — thành module **`Platform.Orgs`**; contract `IOrgEntity`; facade `IOrgManager`; optional **`ICurrentOrg`** / store; **không** chứa cây phòng ban / pháp nhân / vị trí (thuộc OrgStructure).  
> **Ngoài phạm vi:** Cơ cấu tổ chức nội bộ tập đoàn; CRUD Tenant; ambient tenant R2 / `Change`; Authorization engine; Identity User CRUD; billing / subscription; dedicated-DB provisioning; UI admin production đầy đủ (Sample/API tối thiểu khi implement).  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

### 1.1 Mô hình kinh doanh (ERP SaaS)

```text
ERP (root — hạ tầng của chủ sản phẩm)
  ├── Tenant X, Y          ← đại lý / white-label (module Tenants + Multitenancy)
  │     ├── Org A, B       ← khách hàng cuối của X (module Orgs — ADR này)
  │     └── …
  └── Tenant Y
        ├── Org C, D
        └── …
```

| Khái niệm | Ví dụ | Ranh giới |
|----------|-------|-----------|
| **Tenant** | X, Y | Isolation đại lý, branding, catalog tenant |
| **Org** | A, B, C, D | **Một khách hàng ERP** (hợp đồng / admin gốc / phạm vi `OrgAdmin`); có thể là **cả tập đoàn** về mặt thương mại — cơ cấu nội bộ (công ty con, chi nhánh, phòng) **không** là Org platform |
| **OrgStructure** | Cây OU, pháp nhân, vị trí | Trong **một** Org — [ADR riêng](./2026-10-17_platform_orgstructure-module.md) |

Platform đã có:

| Thành phần | Trạng thái | Liên quan Org |
|------------|------------|---------------|
| `ICurrentTenant` / `ITenantEntity` | 🟢 | Mọi Org thuộc đúng một `TenantId` |
| `User.OrgId` trên `AspNetUsers` | 🟢 | Cột subject ABAC — cần catalog Org làm nguồn sự thật |
| Policy `SameOrg`, role `OrgAdmin` | 🟡 | Giả định tồn tại bản ghi Org O001… |
| Module Tenants (catalog) | 🔴 Proposed | Precedent Atomic cho Orgs |

**Setting** và **Tenants** ADR: module nghiệp vụ = facade + contract entity + host-owned migration; **không** nhét vào package ambient (`Platform.Multitenancy`).

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Không có module catalog Org | Host tự invent bảng/API; `User.OrgId` trỏ UUID ad-hoc; lệch `SameOrg` / Data Scope |
| P2 | Nhầm Org platform với công ty con / chi nhánh | Mỗi pháp nhân thành một Org → nở Org, billing lẫn sơ đồ; `SameOrg` quá hẹp hoặc quá rộng |
| P3 | Gộp cây tổ chức vào module Org | God-module; lifecycle khác (ít Org vs nhiều OU); phá Module Atomic |
| P4 | Không có `ICurrentOrg` chuẩn | App đọc `OrgId` từ DbContext/claim tùy ý; khó “switch org” (tenant-wide user) |
| P5 | Nghiệp vụ thiếu contract scope | Không có `IOrgScopedEntity` — filter/list cross-Org trong cùng tenant |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | **`Platform.Multitenancy` không CRUD Org** — ambient tenant giữ nguyên semantic R2 / D5 |
| C2 | **Org luôn thuộc một Tenant** — `ITenantEntity` (hoặc tương đương) trên entity Org |
| C3 | **Một Org platform = một khách hàng ERP** (A/B/C/D); cấu trúc tập đoàn **bên trong** Org qua OrgStructure |
| C4 | Module Orgs **không** reference OrgStructure runtime nặng; OrgStructure chỉ cần `Platform.Orgs.Abstractions` |
| C5 | Entity concrete + migration = **host-owned** (pattern Setting / Tenants) |
| C6 | Authorization rules (`SameOrg`, Role Governance) = `Platform.Authorization` — Orgs chỉ cung cấp **dữ liệu tham chiếu** |
| C7 | Pattern Atomic: `Platform.Orgs` + `Platform.Orgs.EntityFramework` + `Platform.Orgs.Abstractions` (SPI mỏng) |
| C8 | `User.OrgId` (Identity) **không** thay thế catalog — IdentityAdmin gán user vào Org qua facade Orgs / validation |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|-----|--------|
| O1 | Không module — Org chỉ là enum/config | Nhanh | Không onboarding, không lifecycle |
| O2 | Gộp Org + OrgStructure một module | Một package | God-module; khó opt-in |
| O3 | **Orgs catalog mỏng + OrgStructure riêng** | Hai module linked Abstractions | Rõ SaaS vs master data nội bộ; mirror Tenant | Hai module cần compose Host |
| O4 | Mỗi pháp nhân = một Org platform | Org = legal entity | Tách license từng công ty con | Trái mô hình đại lý bán “một tập đoàn một Org”; nở Org |

**Chọn O3** (Org = khách A/B/C/D; tập đoàn = OrgStructure trong Org đó). **Không** chọn O4 làm mặc định platform (host có thể map thương mại khác ở layer billing — ngoài phạm vi ADR).

---

## 5. Quyết định (Proposed — chờ §7)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Tên module / package core | **`Platform.Orgs`** — thư mục `modules/orgs/` |
| D2 | Satellite | **`Platform.Orgs.EntityFramework`**, **`Platform.Orgs.Abstractions`** (`IOrgReference`, validate Org tồn tại / thuộc tenant) |
| D3 | Vai trò | Catalog + lifecycle Org: create / read / update / soft-disable; list/filter theo tenant |
| D4 | Facade | **`IOrgManager`** — API/UI **không** query `DbSet` trực tiếp |
| D5 | Contract catalog | **`IOrgEntity`** — bản ghi Org: tối thiểu `Id`, `TenantId`, `Name`, `Code?`, `IsActive`; field mở rộng host |
| D6 | Scope nghiệp vụ | **`IOrgScopedEntity`** (marker): `OrgId` (+ thường kèm `ITenantEntity`) — Data Scope / query filter **khi Host bật** (Phase ORM follow-up; tách tên khỏi catalog `IOrgEntity`) |
| D7 | Working Org | Optional **`ICurrentOrg<TOrg>`** + accessor + store port — mirror `ICurrentTenant`; **home** Org = `User.OrgId`; **working** = ambient/header (semantic chốt §7) |
| D8 | **Không** chứa OrgStructure | Phòng ban, chi nhánh, pháp nhân, job title → [OrgStructure ADR](./2026-10-17_platform_orgstructure-module.md) |
| D9 | DI Host | `AddPlatformOrgs<TOrgEntity, TUoW>()` opt-in; **không** thay `AddCurrentTenant` |
| D10 | DB placement | Org catalog **tenant-scoped** (bảng có `TenantId`, cùng DB nghiệp vụ tenant) **hoặc** master catalog — **mặc định đề xuất: tenant-scoped DB** (đại lý X chỉ thấy A,B khi ambient tenant X); host dedicated-DB chốt §7 |

### 5.1 Ranh giới Tenant / Org / OrgStructure

```text
  [Authorization]     [IdentityAdmin]      [UI Admin]
         │                    │                  │
         └──────── Host ──────┴──────────────────┘
                          │
          ┌───────────────┼───────────────────────┐
          ▼               ▼                       ▼
  Platform.Multitenancy   Platform.Orgs      Platform.OrgStructure
  + Tenants (catalog)     catalog Org A,B…   OU / LegalEntity / Position
  ambient TenantId        + Orgs.EF          (OrgId FK — Abstractions → Orgs)
          │                       │
          │    ICurrentOrg (optional)
          └───────────────────────┘
```

| Concern | Thuộc |
|---------|--------|
| Header / claim → tenant đang làm việc | Multitenancy |
| Danh sách Org của tenant X; tạo Org A | **Orgs** |
| `User.OrgId` gán user vào Org home | IdentityAdmin + validate qua **Orgs** |
| Cây phòng ban trong Org A | **OrgStructure** |
| Policy `SameOrg` | Authorization (operand `User.OrgId` / `Resource.OrgId`) |

### 5.2 Model dữ liệu (sketch)

**Bảng Org (host entity `: IOrgEntity`)**

| Cột | Ghi chú |
|-----|---------|
| `Id` | Guid PK |
| `TenantId` | FK logic tới tenant catalog |
| `Code` | Unique trong tenant (index host-owned) |
| `Name` | Tên hiển thị |
| `IsActive` | Soft-disable |
| `…` | Logo, locale, timezone — host extension |

**Không** có `ParentOrgId` trên Org platform — quan hệ cha-con tập đoàn nằm ở OrgStructure (`OrganizationalUnit` / `LegalEntity`).

### 5.3 MVP chức năng (khi implement)

| # | Chức năng | Ghi chú |
|---|-----------|---------|
| 1 | Create / GetById / Update / SetActive | Soft-disable ưu tiên hard delete |
| 2 | List + filter (`TenantId` từ ambient, `IsActive`, search Code/Name) | TenantAdmin / đại lý |
| 3 | `GetByCodeAsync(tenantId, code)` | Onboarding |
| 4 | Validate `IOrgReference` cho module khác | Abstractions |
| 5 | Optional bridge `ICurrentOrgStore` | Opt-in §7 |
| 6 | Sample API + permission keys (`Org.View`, `Org.Manage`) | Host wiring Authorization |

**Không** MVP: org-user membership table (dùng Identity role + `User.OrgId` trước); billing per Org; auto-seed OrgStructure.

### 5.4 Inventory (đích — sketch)

```text
modules/orgs/
  Platform.Orgs/
    ├─ Services/IOrgManager.cs, OrgManager.cs
    ├─ Extensions/AddPlatformOrgs(…)
    └─ (optional) CurrentOrgStoreFromOrgs…

  Platform.Orgs.Abstractions/
    └─ IOrgReference.cs, IOrgExistenceValidator.cs

  Platform.Orgs.EntityFramework/
    └─ mapping helpers / AddPlatformOrgsEntityFramework

Platform.DDD.Domain (hoặc Orgs.Abstractions)
  ├─ Entities/IOrgEntity.cs          // catalog Org
  └─ Entities/IOrgScopedEntity.cs    // OrgId trên nghiệp vụ — chốt §7

Host
  ├─ AddCurrentTenant<…>()
  ├─ AddPlatformOrgs<Org, ITenantUnitOfWork>()
  └─ entity Org : IOrgEntity
```

---

## 6. Hệ quả

### Tích cực

- Ranh giới rõ: **Tenant (đại lý)** vs **Org (khách cuối)** vs **OrgStructure (nội bộ)**.
- Catalog chuẩn cho `User.OrgId` và `Resource.OrgId` trong ABAC.
- Cùng pattern Tenants/Setting: Atomic, host-owned schema.

### Chi phí

- Thêm module + Sample; Host compose thêm `AddPlatformOrgs`.
- Cần quy ước HTTP header / resolver cho `ICurrentOrg` (nếu bật) — ADR follow-up hoặc chốt §7.

### Rủi ro

| Rủi ro | Mitigation |
|--------|------------|
| Nhầm Org vs LegalEntity | Doc + OrgStructure ADR; không `ParentOrgId` trên Org |
| Cross-Org leak trong tenant | `IOrgEntity` + Data Scope; test T* |
| User `OrgId` trỏ Org inactive | `IOrgManager` validate khi gán; login policy |

---

## 7. Confirm

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D10, C1–C8, phương án O3? | _chờ_ |
| Q2 | Tên package | **`Platform.Orgs`** (+ EF + Abstractions) — OK? |
| Q3 | Facade | **`IOrgManager`** — OK? |
| Q4 | `IOrgEntity` (catalog) và `IOrgScopedEntity` (marker) đặt ở | Domain **hay** `Platform.Orgs.Abstractions`? |
| Q5 | Org catalog DB | Tenant-scoped (đề xuất D10) **hay** Master? |
| Q6 | `ICurrentOrg` trong MVP? | Yes / Later |
| Q7 | Header working org (vd. `X-Org-Id`) song song tenant? | _chờ_ |
| Q8 | Soft-disable mặc định, hard delete Later? | _chờ_ |

- [ ] Accept D1–D10, C1–C8, O3
- [ ] Tên `Platform.Orgs` (+ satellite)
- [ ] Facade `IOrgManager`
- [ ] Chốt vị trí `IOrgEntity`
- [ ] Chốt DB placement
- [ ] `ICurrentOrg` MVP?
- [ ] Working org resolver
- [ ] Soft-disable

---

## 8. Test cases *(khi implement)*

### 8.1 Smoke

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T1 | Host `AddPlatformOrgs` + Create Org under tenant X | Persist; `TenantId` = X | 🔴 |
| T2 | List Org khi ambient tenant X | Chỉ A,B; không C,D (tenant Y) | 🔴 |

### 8.2 Regression

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T3 | Authorization seed / `User.OrgId` Sample | Không gãy khi thêm bảng Org | 🟡 |
| T4 | `ITenantEntity` query filter hiện có | Không đổi semantic tenant | 🟢 |

### 8.3 Mới

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T5 | SetActive=false → gán user mới | Reject hoặc policy host | 🔴 |
| T6 | `IOrgReference` validator | Org không thuộc tenant → fail | 🔴 |

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 | Accepted | — |
| A | Abstractions + `IOrgManager` skeleton + DI | Build | 0 |
| B | CRUD MVP + entity Sample | T1–T2 | A |
| C | Permission + IdentityAdmin validate OrgId | Sample | B; Authorization 🟡 |
| D | Optional `ICurrentOrg` + resolver | T6 | B |
| E | `IOrgScopedEntity` query filter (ORM) nếu cần | ADR ORM follow-up | B |
| F | Skill `orgs-dotnet` | Docs | B |

**Thứ tự vs OrgStructure:** Orgs **trước** (OrgStructure phụ thuộc Abstractions). Có thể song song Tenants module.

---

## 10. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🔴 **Proposed** |
| Implementation | 🔴 **Not started** |
| Blocked by | Confirm §7 |
| Unblocks | Catalog Org cho RBAC; module OrgStructure |
| Owner | Platform |
