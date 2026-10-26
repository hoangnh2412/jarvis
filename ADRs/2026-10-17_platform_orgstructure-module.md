# ADR — Module cơ cấu tổ chức (`Platform.OrgStructure`)

> **Trạng thái:** 🟢 **Accepted** (2026-10-25). Implementation 🟡 Host API + migration `AddOrgStructure`, màn `/system/org-structure`. Chưa gán Position cho user.  
> **Ngày:** 2026-10-17 · Cập nhật phân cấp và §7: 2026-10-25  
> **Loại:** Business module / Module Atomic (Library + Persistence + Abstractions)  
> **Liên quan:** [Orgs module](./2026-10-17_platform_orgs-module.md) (🟡 catalog, không seed, không xóa cứng), [RBAC + ABAC](./2026-08-14_platform_rbac-abac-authorization.md) (🟡 — tên chuẩn `OrganizationalUnitId` / `UnitType`; `DepartmentId` chỉ còn cho rule cũ), [Tenants / Multitenancy](./2026-08-07_platform_tenants-module.md) · [Multitenancy package](./2026-08-06_platform_multitenancy-package.md), [architecture-rules.md](./architecture-rules.md) (§0.2).  
> **Phạm vi:** Cây tổ chức **bên trong một Org**: phòng ban, và các node **không tạo Org** (chi nhánh, phòng vé, đại lý của VNA — hóa đơn vẫn thuộc Org VNA). Pháp nhân, chức danh, vị trí giữ trong module này.  
> **Ngoài phạm vi:** Catalog Org; Tenant và Tenant con. Chi nhánh HCM, HN và đại lý của Minvoice **là Tenant con**, không phải `OrganizationalUnit`. CRUD Tenant; Authorization engine; payroll; workflow duyệt; UI org-chart đầy đủ.  
> **Change log:** 2026-10-25 — Host: entity `sys`, API `api/v1/org-structure`, migration `AddOrgStructure`, UnitType trên Host, UI `/system/org-structure` xem trước mẫu rồi xác nhận. Gán Position cho user vẫn để sau. 2026-10-25 — Library `Platform.OrgStructure`: facade, `Path`, mẫu `simple-company` có xem trước, test manager. Chưa Host/API. 2026-10-25 — `UnitType.Branch` chỉ cho node không tạo Org khách (mẫu VNA). Kênh tạo Org khách chuyển sang ADR Tenants. 2026-10-25 — Accept §7: D1–D12, C1–C8, O3. Một facade. Pháp nhân trong cùng lượt. Cây lưu `Path`. Không tự seed. Seed chỉ khi user chọn và đã xem trước dữ liệu. Bỏ `DepartmentId`. Thêm `IOrganizationalUnitScope`.
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

### 1.1 Vị trí trong mô hình SaaS

```text
Tenant VNA
  └── Org VNA                         ← hóa đơn thuộc Org này
        └── OrganizationalUnit
              ├── Chi nhánh           ← không tạo Org khách
              │     └── Phòng ban
              ├── Phòng vé
              └── Đại lý

Tenant HCM                            ← Tenant con, KHÔNG phải OU
  └── Org Chi nhánh HCM
        └── OrganizationalUnit        ← chỉ phòng ban của Org này
```

| Lớp | Module | Ghi chú |
|-----|--------|---------|
| Tenant và Tenant con | Tenants | Minvoice, HCM, đại lý khi chúng tạo Org khách |
| Người bán | **Orgs** | Mỗi Org một cây OrgStructure |
| Cây trong Org | **OrgStructure** (ADR này) | Scoped bởi `OrgId`. Không đựng kênh tạo Org |

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

## 5. Quyết định (Accepted 2026-10-25)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Tên module | **`Platform.OrgStructure`** — `modules/orgstructure/` |
| D2 | Satellite | **`Platform.OrgStructure.EntityFramework`**, **`Platform.OrgStructure.Abstractions`** (DTO id/name cho lookup) |
| D3 | Facade chính | **Một** **`IOrgStructureManager`** cho OU, LegalEntity, JobTitle, Position |
| D4 | **`OrganizationalUnit`** | Cây: `Id`, `OrgId`, `TenantId`, `ParentId?`, `Code`, `Name`, `UnitType`, `SortOrder`, `IsActive`, `LegalEntityId?`, **`Path`** |
| D5 | **`UnitType`** | Lookup do host mở rộng. Danh sách gợi ý: `Group`, `Company`, `LegalEntityNode`, `Region`, `Branch`, `Division`, `Department`, `Section`, `TeamNode`. Platform không seed `UnitType` của VNA. Host VNA seed |
| D6 | **`LegalEntity`** | Làm trong cùng lượt platform: `Id`, `OrgId`, `Code`, `Name`, `TaxId?`, `IsActive`. OU gắn `LegalEntityId`. Chi nhánh VNA có pháp nhân |
| D7 | **`JobTitle`** | Catalog chức danh trong Org (`OrgId`, `Code`, `Name`) |
| D8 | **`Position`** | Slot trên sơ đồ: `OrgId`, `OrganizationalUnitId`, `JobTitleId?`, `ReportsToPositionId?`, `IsActive` |
| D9 | **Cây mẫu** | Không tự tạo khi tạo Org. Có lệnh chọn mẫu. Trước khi ghi, API và UI trả danh sách đơn vị sẽ được tạo để người dùng thấy rồi xác nhận |
| D10 | **Tập đoàn** | Cùng schema; UI/API list tree, move node, filter by `UnitType` / `LegalEntityId` |
| D11 | **Phạm vi đơn vị** | **`IOrganizationalUnitScope`** trong Abstractions: `OrganizationalUnitId` + `UnitType` trên bản ghi nghiệp vụ và trên user. Bỏ `DepartmentId`. Alias `DepartmentId` chỉ để tương thích tạm, đánh deprecated, sửa trong cùng lượt platform. Sửa [ADR RBAC](./2026-08-14_platform_rbac-abac-authorization.md) khi implement |
| D12 | Query subtree | Cột **`Path`** (materialized path), ví dụ `/vna/ha-noi/phong-ve-01`. `GetDescendantsAsync` lọc theo tiền tố path. Đổi cha thì ghi lại path của nút và của các con |

### 5.1 Sơ đồ module

```text
Platform.Orgs.Abstractions          Platform.OrgStructure
         │                                    │
         └──────── validate OrgId ────────────┤
                                              ├─ OrganizationalUnit (tree)
                                              ├─ LegalEntity
                                              ├─ JobTitle
                                              └─ Position

Platform.Authorization (Host)  ←── OrganizationalUnitId, UnitType, LegalEntityId
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

### 5.3 Cây mẫu (D9)

Tạo Org không tạo đơn vị. Khi người dùng chọn một mẫu, hệ thống trả trước đúng các dòng sẽ ghi, rồi mới ghi sau khi người dùng xác nhận.

```text
Mẫu "Công ty đơn giản" — xem trước:
  └── Company "Công ty"
        ├── Department "Kế toán"
        ├── Department "Kinh doanh"
        └── Department "Nhân sự"
```

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

### 5.5 Bổ sung Authorization

| Policy code | Biểu thức | Khi nào |
|-------------|-----------|---------|
| `SameOrganizationalUnit` | `Resource.OrganizationalUnitId == User.OrganizationalUnitId` | Thay `SameDepartment` |
| `SameLegalEntity` | `Resource.LegalEntityId == User.LegalEntityId` | Hóa đơn, kế toán theo pháp nhân |
| `WithinOuSubtree` | path của resource bắt đầu bằng path đơn vị của user | Trưởng chi nhánh |

`DepartmentId` và `SameDepartment` deprecated trong lượt này. Operand mới đăng ký qua `IOrganizationalUnitScope`. Sửa ADR RBAC khi implement, không giữ `DepartmentId` làm tên chuẩn.

### 5.6 MVP phạm vi chức năng

| # | Chức năng | Phase |
|---|-----------|-------|
| 1 | CRUD OU + move parent + list tree | MVP |
| 2 | `UnitType` seed + filter | MVP |
| 3 | Cây mẫu có xem trước, không tự chạy | Cùng lượt |
| 4 | CRUD JobTitle | Cùng lượt |
| 5 | CRUD Position (không assignment) | Cùng lượt |
| 6 | CRUD LegalEntity + link OU | Cùng lượt |
| 7 | User primary OU → `OrganizationalUnitId` + `UnitType` | Cùng lượt |
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
- Phạm vi đơn vị có nguồn `OrganizationalUnitId` và `UnitType`. Pháp nhân nằm trong cùng lượt.
- Tách khỏi Orgs — onboarding Org không kéo cả org-chart.

### Chi phí

- Cây OU: cần quy ước reorg, index, test cycle.
- Denormalized `TenantId` trên OU (nếu có) — đồng bộ khi đổi Org (hiếm).

### Rủi ro

| Rủi ro | Mitigation |
|--------|------------|
| Xóa OU đang có user/Position | C7; block delete; deactivate |
| Performance tree sâu | Cột `Path`; phân trang danh sách con trực tiếp. Đổi cha ghi lại path của cả nhánh |
| Nhầm TeamNode vs Teams product | C6; naming doc |

---

## 7. Confirm

Đã chốt 2026-10-25.

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D12, C1–C8, O3? | Có |
| Q2 | Tên package | `Platform.OrgStructure` |
| Q3 | Facade | Một `IOrgStructureManager` |
| Q4 | LegalEntity | Cùng lượt platform. Chi nhánh VNA có pháp nhân |
| Q5 | Lưu cây | Cách B: cột `Path` |
| Q6 | `DepartmentId` | Bỏ. Dùng `OrganizationalUnitId` và `UnitType` |
| Q7 | Cây mẫu | Không tự seed khi tạo Org. User chọn mẫu. UI và API hiển thị dữ liệu sẽ seed trước khi ghi |
| Q8 | Phạm vi | `IOrganizationalUnitScope` trong Abstractions |

- [x] Accept D1–D12, C1–C8, O3
- [x] Một facade, pháp nhân cùng lượt, cột `Path`
- [x] Bỏ `DepartmentId`
- [x] Cây mẫu có xem trước, không tự chạy

---

## 8. Test cases *(khi implement)*

### 8.1 Smoke

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T1 | `ApplySimpleTemplateAsync` trên Org A | Root + ≥1 Department; cùng OrgId | 🟢 library |
| T2 | Move OU sang parent khác cùng Org | OK; path/subtree cập nhật | 🟢 library |
| T3 | Move OU khác OrgId | Reject | 🟢 library |

### 8.2 Regression

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T4 | Policy dùng `OrganizationalUnitId` | Pass với OU id hợp lệ. `DepartmentId` không còn là tên chuẩn | 🔴 |

### 8.3 Mới

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T5 | Create edge parent→child→parent | Cycle rejected | 🟢 library |
| T6 | Deactivate OU có Position active | Reject hoặc cascade rule | 🟢 library, reject |
| T7 | Deep tree (≥5 cấp) list descendants | Đúng số node; perf budget | 🟢 library, chưa đo perf |

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
| Decision | 🟢 **Accepted** 2026-10-25 (D1–D12, C1–C8, O3) |
| Implementation | 🟡 **Đang làm** — library + Host entity/API/migration `AddOrgStructure` + UI `/system/org-structure`. Chưa gán Position cho user |
| Blocked by | Không. Phụ thuộc `Platform.Orgs.Abstractions` đã có |
| Unblocks | HR modules; Data Scope theo OU/LegalEntity |
| Owner | Platform |
