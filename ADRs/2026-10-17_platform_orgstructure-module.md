# ADR — Module cơ cấu tổ chức (`Platform.OrgStructure`)

> **Trạng thái:** 🔴 **Proposed** (chờ confirm §7).  
> **Ngày:** 2026-10-17  
> **Loại:** Business module / Module Atomic (Library + Persistence + Abstractions)  
> **Liên quan:** [Orgs module](./2026-10-17_platform_orgs-module.md) (🔴 — Org A/B/C/D), [RBAC + ABAC](./2026-08-14_platform_rbac-abac-authorization.md) (🟡 — `DepartmentId`, `SameDepartment`, `ManagedDepartment`; bổ sung policy tập đoàn §5.6), [Tenants / Multitenancy](./2026-08-07_platform_tenants-module.md) · [Multitenancy package](./2026-08-06_platform_multitenancy-package.md), [architecture-rules.md](./architecture-rules.md) (§0.2).  
> **Phạm vi:** Module **`Platform.OrgStructure`** quản lý **cơ cấu tổ chức bên trong một Org platform** — cây đơn vị (tập đoàn, tổng công ty, chi nhánh, phòng ban…), pháp nhân (kế toán/thuế), chức danh và vị trí; hỗ trợ **doanh nghiệp nhỏ** (cây 1–2 cấp) và **tập đoàn phức tạp** (cùng model, khác độ sâu / `UnitType`).  
> **Ngoài phạm vi:** Catalog Org khách hàng A/B/C/D (module Orgs); CRUD Tenant; Authorization engine (chỉ publish event/DTO để Host cập nhật UserClaim); payroll; workflow duyệt theo OU; UI org-chart đầy đủ (Sample tối thiểu).  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

### 1.1 Vị trí trong mô hình SaaS

```text
Tenant X
  └── Org A  ("Tập đoàn ABC" — một khách hàng ERP)
        ├── LegalEntity L1, L2, …     ← pháp nhân (optional, kế toán)
        └── OrganizationalUnit (cây)  ← chi nhánh, khối, phòng, tổ…
              └── Position + JobTitle   ← vị trí / chức danh
```

| Lớp | Module | Ghi chú |
|-----|--------|---------|
| Khách hàng đại lý | Tenants | X, Y |
| Khách hàng cuối | **Orgs** | A, B — **một Org có thể là cả tập đoàn** về thương mại |
| Cơ cấu nội bộ | **OrgStructure** (ADR này) | **Mọi** bản ghi scoped bởi `OrgId` (+ `TenantId` qua Org / `ITenantEntity`) |

Doanh nghiệp **nhỏ**: seed hoặc wizard tạo cây tối giản (vd. một root `Company` + vài `Department`).  
**Tập đoàn**: cùng entity `OrganizationalUnit`, thêm `LegalEntity`, nhiều `UnitType`, subtree query.

### 1.2 Liên quan Authorization (hiện tại)

ADR RBAC+ABAC đã dùng:

- Claim **`DepartmentId`** — semantic ADR: **primary organizational unit** cho data scope, không bắt buộc “phòng ban” HR.
- Policy **`SameDepartment`**, **`ManagedDepartment`**.

OrgStructure là **nguồn sự thật** cho ID đó và (Phase sau) **`LegalEntityId`**, **`ManagedOrganizationalUnitIds`** — cập nhật IdentityAdmin qua facade Host, **không** sửa engine Policy trong module này.

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Không có module cây tổ chức | Mỗi host invent schema; `DepartmentId` trỏ UUID không validate |
| P2 | Tách module Department / Branch / Unit riêng | Ba cây lệch nhau; duplicate chi nhánh vs phòng |
| P3 | Nhét cây vào module Orgs | God-module; Org CRUD lẫn reorg hàng ngày |
| P4 | Tập đoàn cần cô lập pháp nhân | Chỉ `SameOrg` → CFO công ty con thấy dữ liệu anh em |
| P5 | Doanh nghiệp nhỏ cần đơn giản | Model quá nặng → adoption kém |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Mọi dữ liệu OrgStructure thuộc đúng **một `OrgId`** (Org platform — [Orgs ADR](./2026-10-17_platform_orgs-module.md)) |
| C2 | Module **phụ thuộc** `Platform.Orgs.Abstractions` — **không** reference `Platform.Orgs` runtime nặng nếu tránh được vòng |
| C3 | **Orgs không** reference OrgStructure (C4 Orgs ADR) |
| C4 | **Một cây OU** (`OrganizationalUnit`) + **`UnitType`** — không module Department riêng làm cây song song |
| C5 | Entity + migration host-owned; facade **`IOrgStructureManager`** (hoặc tách facade con — chốt §7) |
| C6 | **Teams** (nhóm dự án / collaboration) **ngoài** MVP — tránh nhầm với OU; Later entity riêng nếu cần |
| C7 | Soft-delete / `IsActive` trên node; hard delete chỉ khi không còn reference (HR, user assignment) |
| C8 | Reorg (đổi parent) phải validate **không cycle**, **cùng OrgId** |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|-----|--------|
| O1 | Module **Department** (phòng ban phẳng) | CRUD list | Đơn giản cho SME | Không tập đoàn; không chi nhánh cấp cao |
| O2 | Module **Teams** thay OrgStructure | Tên gần UX | Dễ hiểu nhóm nhỏ | Sai nghĩa ERP; không pháp nhân / chi nhánh |
| O3 | **OrganizationalUnit** + types + LegalEntity + Position | Một module OrgStructure | Một cây; SME + tập đoàn | Cần doc `UnitType` và seed mẫu |
| O4 | Gộp vào Orgs | Một package | Ít DI | Vi phạm bounded context |

**Chọn O3.**

---

## 5. Quyết định (Proposed — chờ §7)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Tên module | **`Platform.OrgStructure`** — `modules/orgstructure/` |
| D2 | Satellite | **`Platform.OrgStructure.EntityFramework`**, **`Platform.OrgStructure.Abstractions`** (DTO id/name cho lookup) |
| D3 | Facade chính | **`IOrgStructureManager`** — orchestrate OU / LegalEntity / JobTitle / Position; có thể tách `IOrganizationalUnitService` nội bộ package |
| D4 | **`OrganizationalUnit`** | Cây: `Id`, `OrgId`, `TenantId` (denormalized hoặc qua Org), `ParentId?`, `Code`, `Name`, `UnitType`, `SortOrder`, `IsActive`, optional `LegalEntityId?` |
| D5 | **`UnitType`** | Enum / lookup host mở rộng — seed platform: `Group`, `Company`, `LegalEntityNode`, `Region`, `Branch`, `Division`, `Department`, `Section`, `TeamNode` (node sơ đồ — **không** entity Teams collaboration) |
| D6 | **`LegalEntity`** | Optional Phase 1b: `Id`, `OrgId`, `Code`, `Name`, `TaxId?`, `IsActive` — dùng scope kế toán; OU có thể link `LegalEntityId` |
| D7 | **`JobTitle`** | Catalog chức danh trong Org (`OrgId`, `Code`, `Name`) |
| D8 | **`Position`** | Slot trên sơ đồ: `OrgId`, `OrganizationalUnitId`, `JobTitleId?`, `ReportsToPositionId?`, `IsActive`; assignment user → Phase 1b / Identity integration |
| D9 | **Doanh nghiệp nhỏ** | **`ApplySimpleTemplateAsync(orgId)`** — tạo root OU + optional vài Department; không bắt buộc LegalEntity |
| D10 | **Tập đoàn** | Cùng schema; UI/API list tree, move node, filter by `UnitType` / `LegalEntityId` |
| D11 | **User primary OU** | API **`SetUserPrimaryOrganizationalUnitAsync(userId, ouId)`** (Host) → cập nhật claim **`DepartmentId`** (giữ tên ABAC) |
| D12 | Query subtree | `GetDescendantsAsync(ouId)` / materialized `Path` — chốt implement §7 (closure table vs recursive CTE) |

### 5.1 Sơ đồ module

```text
Platform.Orgs.Abstractions          Platform.OrgStructure
         │                                    │
         └──────── validate OrgId ────────────┤
                                              ├─ OrganizationalUnit (tree)
                                              ├─ LegalEntity
                                              ├─ JobTitle
                                              └─ Position

Platform.Authorization (Host)  ←── DepartmentId, LegalEntityId claims
IdentityAdmin / HR module Later
```

### 5.2 `UnitType` — gợi ý seed (platform)

| UnitType | Ý nghĩa (VN) | Ghi chú |
|----------|--------------|---------|
| `Group` | Tập đoàn (node gốc logic) | Thường 1 root / Org |
| `Company` | Tổng công ty / công ty mẹ vận hành | |
| `Branch` | Chi nhánh | Có thể gắn LegalEntity |
| `Division` | Khối / ban | |
| `Department` | Phòng ban | Hay gắn `DepartmentId` user |
| `Section` | Bộ phận / tổ | |
| `TeamNode` | Nhóm trên sơ đồ | Khác product “Teams” |

Host được thêm type; module không hard-code UI label (i18n host).

### 5.3 Template doanh nghiệp nhỏ (D9)

```text
Org A (đã tạo bởi Orgs)
  └── OU root: UnitType=Company, Name="Công ty"
        ├── OU: Department "Kế toán"
        ├── OU: Department "Kinh doanh"
        └── OU: Department "Nhân sự"
```

Một lệnh seed; user admin sửa tên/xóa thêm node sau.

### 5.4 Tập đoàn (ví dụ)

```text
Org A
  ├── LegalEntity: CTCP Alpha
  ├── LegalEntity: CTCP Beta
  └── OU tree
        Group ABC
          ├── Branch HN (LegalEntity=Alpha)
          │     └── Department Sales
          └── Branch HCM (LegalEntity=Beta)
                └── Department Factory
```

### 5.5 Bổ sung Authorization (đề xuất — amend RBAC ADR Later)

| Policy code | Biểu thức (sketch) | Khi nào |
|-------------|-------------------|---------|
| `SameLegalEntity` | `Resource.LegalEntityId == User.LegalEntityId` | GL, hóa đơn, NS theo pháp nhân |
| `WithinOuSubtree` | `Resource.OrganizationalUnitId IN User.OuSubtree` | Trưởng chi nhánh |
| `ManagedOrganizationalUnits` | `Resource.OuId IN User.ManagedOuIds` | Giám đốc vùng |

MVP OrgStructure **không** bắt buộc implement evaluator mới — chỉ document operand `Resource.LegalEntityId`, `Resource.OrganizationalUnitId` cho host đăng ký.

### 5.6 MVP phạm vi chức năng

| # | Chức năng | Phase |
|---|-----------|-------|
| 1 | CRUD OU + move parent + list tree | MVP |
| 2 | `UnitType` seed + filter | MVP |
| 3 | `ApplySimpleTemplateAsync` | MVP |
| 4 | CRUD JobTitle | MVP |
| 5 | CRUD Position (không assignment) | MVP |
| 6 | CRUD LegalEntity + link OU | 1b |
| 7 | User primary OU → `DepartmentId` | 1b (với IdentityAdmin) |
| 8 | Position assignment → user | Later |

### 5.7 Inventory (sketch)

```text
modules/orgstructure/
  Platform.OrgStructure/
    ├─ Services/IOrgStructureManager.cs
    ├─ Models/OrganizationalUnitDto, UnitType.cs
    ├─ Templates/SimpleCompanyTemplate.cs
    └─ Extensions/AddPlatformOrgStructure(…)

  Platform.OrgStructure.EntityFramework/
    └─ configurations, repositories

  Platform.OrgStructure.Abstractions/
    └─ IOrganizationalUnitReference, IOrgStructureLookup

Host
  ├─ AddPlatformOrgs<…>()
  └─ AddPlatformOrgStructure<…, TUoW>()
```

---

## 6. Hệ quả

### Tích cực

- Một module phục vụ **SME và tập đoàn** (depth khác, model chung).
- `DepartmentId` ABAC có nguồn validate; sẵn sàng mở rộng pháp nhân.
- Tách khỏi Orgs — onboarding Org không kéo cả org-chart.

### Chi phí

- Cây OU: cần quy ước reorg, index, test cycle.
- Denormalized `TenantId` trên OU (nếu có) — đồng bộ khi đổi Org (hiếm).

### Rủi ro

| Rủi ro | Mitigation |
|--------|------------|
| Xóa OU đang có user/Position | C7; block delete; deactivate |
| Performance tree sâu | Path/closure; paging list con trực tiếp |
| Nhầm TeamNode vs Teams product | C6; naming doc |

---

## 7. Confirm

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D12, C1–C8, O3? | _chờ_ |
| Q2 | Tên package **`Platform.OrgStructure`** (không Teams / Department)? | _chờ_ |
| Q3 | Facade đơn **`IOrgStructureManager`** hay tách theo aggregate? | _chờ_ |
| Q4 | LegalEntity trong MVP hay Phase 1b? | _chờ_ |
| Q5 | Subtree storage: closure table / materialized path / CTE only? | _chờ_ |
| Q6 | Giữ claim name **`DepartmentId`** cho primary OU? | _chờ_ (đề xuất: Yes — compat RBAC) |
| Q7 | Template SME mặc định khi Create Org (Orgs module hook)? | Orgs event / Host orchestration |

- [ ] Accept D1–D12, O3
- [ ] Tên Platform.OrgStructure
- [ ] Facade shape
- [ ] LegalEntity phase
- [ ] Tree storage
- [ ] DepartmentId semantic
- [ ] Seed template wiring

---

## 8. Test cases *(khi implement)*

### 8.1 Smoke

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T1 | `ApplySimpleTemplateAsync` trên Org A | Root + ≥1 Department; cùng OrgId | 🔴 |
| T2 | Move OU sang parent khác cùng Org | OK; path/subtree cập nhật | 🔴 |
| T3 | Move OU khác OrgId | Reject | 🔴 |

### 8.2 Regression

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T4 | Policy tests dùng `DepartmentId` | Vẫn pass với OU id hợp lệ | 🟡 |

### 8.3 Mới

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T5 | Create edge parent→child→parent | Cycle rejected | 🔴 |
| T6 | Deactivate OU có Position active | Reject hoặc cascade rule | 🔴 |
| T7 | Deep tree (≥5 cấp) list descendants | Đúng số node; perf budget | 🔴 |

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 | Accepted | — |
| A | Abstractions + OU CRUD + tree read | T1,T3,T5 | [Orgs](./2026-10-17_platform_orgs-module.md) Phase B |
| B | Template SME + JobTitle + Position | T1 | A |
| C | LegalEntity + link OU | T2 | A |
| D | IdentityAdmin primary OU → claim | T4 | B; IdentityAdmin |
| E | RBAC amend: SameLegalEntity / operands | Doc + seed | C |
| F | Skill `orgstructure-dotnet` | Docs | B |

---

## 10. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🔴 **Proposed** |
| Implementation | 🔴 **Not started** |
| Blocked by | Confirm §7; Orgs Abstractions |
| Unblocks | HR modules; Data Scope theo OU/LegalEntity |
| Owner | Platform |
