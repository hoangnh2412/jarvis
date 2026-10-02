# ADR — RBAC + ABAC Authorization + ranh giới Account / IdentityAdmin

> **Trạng thái:** 🟢 Accepted · **Amendment 2026-10-03: 🟡 Proposed** (chờ anh Hoàng duyệt — §7.3 phát hiện rà soát, V6–V14, test §8.9; chưa được implement khi chưa duyệt)  
> **Ngày:** 2026-08-14 · Accept: 2026-09-29 · Implement: 🟡 đang làm (rà soát code 2026-10-02, cập nhật Data Scope + Sample 2026-10-03 — đối chiếu C/D/Q: §7.2 · Data Scope: §5.11 · checklist: §10 · cấu trúc: §11 · Sample: §11.8 · phase: §12)  
> **Loại:** Authorization / package boundary / module boundary / data model  
> **Liên quan:** [Refactor Authentication](./refactor-authentication.md) (Auth ≠ Authorization), [Current User / Tenant](./2026-08-01_platform_current-user-tenant.md), [Multitenancy](./2026-08-06_platform_multitenancy-package.md), [refactoring-rules.md](./refactoring-rules.md) (§0.2 Module Atomic)  
> **Thuật ngữ:** **Authentication = xác thực** (người gọi là ai) · **Authorization = phân quyền** (được làm gì, trên dữ liệu nào). Không dùng "ủy quyền" cho Authorization — dành chữ đó cho delegation nếu sau này có.  
> **Phạm vi:** Mô hình RBAC + ABAC; Role Governance; Policy/PolicyRule; Data Scope; ranh giới **`Platform.Authentication.*` (xác thực) / `Platform.Authorization` (phân quyền) / module Account / module IdentityAdmin**; map 8 chức năng cơ bản (User/Role/Permission/Login/Logout/Profile/Forgot/Change password).  
> **Ngoài phạm vi:** Chi tiết implement từng Auth scheme (JWT/ApiKey/Basic — xem refactor Auth); UI/UX chi tiết; Policy Engine DSL phức tạp (nested expression / function calls); OpenIddict AS/RS scopes; gộp Login vào `Platform.Authentication` core.  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm

---

## 1. Bối cảnh

Hệ thống ERP cần phân quyền đồng thời theo **hành động** (RBAC) và theo **phạm vi dữ liệu / thuộc tính** (ABAC), trong môi trường:

- Multi-tenant
- Multi-organization (Org) trong Tenant
- Department / Team scope
- Ownership scope (dữ liệu user tạo)
- Managed resource scope (resource user phụ trách)
- Numeric threshold (ví dụ hạn mức duyệt phiếu chi)
- Dynamic business roles (khách hàng tạo/sửa Role nghiệp vụ)
- System roles cố định (`Admin`, `TenantAdmin`, `OrgAdmin`) không chỉnh sửa quyền
- Role Governance: user trong Org chỉ tạo/sửa Role có phạm vi quyền **thấp hơn** mình

Nền tảng: ASP.NET Core + ASP.NET Identity + C#. `Platform.Authentication.*` đã có (scheme, password **policy**, CurrentUser). ADR này chốt **Authorization** và **ranh giới module** cho các chức năng tài khoản / quản trị identity.

### Chức năng cơ bản cần đáp ứng (v1 product)

| # | Chức năng | Nhóm |
|---|-----------|------|
| 1 | CRUD User, lock / unlock | Admin |
| 2 | CRUD Role | Admin |
| 3 | Permission (định nghĩa hệ thống + gán vào Role) | Engine + Admin |
| 4 | Login | Self-service |
| 5 | Logout | Self-service |
| 6 | Profile | Self-service |
| 7 | Forgot Password | Self-service |
| 8 | Change Password | Self-service |

### Mục tiêu

| # | Mục tiêu |
|---|----------|
| — | RBAC: user được phép thực hiện **action** nào |
| — | ABAC: user được phép thực hiện action trên **resource/data** nào |
| — | Tận dụng tối đa ASP.NET Identity; hạn chế bảng phụ không cần thiết |
| — | Policy do hệ thống định nghĩa trước; khách hàng cấu hình gán Policy/Permission + attribute |
| — | PolicyRule biểu diễn biểu thức động (`Resource.*` vs `User.*`) |
| — | Tách rõ foundation AuthN / engine AuthZ / module Account / module IdentityAdmin (Module Atomic) |

Công thức:

```text
Authorization = RBAC Permission Check AND ABAC Policy Check
```

Một số permission (ví dụ `HealthCheck.View`) chỉ cần RBAC; permission như `Expense.Approve` cần cả RBAC lẫn ABAC.

---

## 2. Vấn đề

P1–P11 mô tả các **anti-pattern / tình huống nếu KHÔNG áp dụng thiết kế ở §5** (tức nếu chọn O1/O4/O5/O6 hoặc không làm gì). Đây là lực kéo (forces) dẫn tới quyết định — không phải mô tả thiết kế đã chọn. Thiết kế §5 (Policies/PolicyRules, RoleClaim tham chiếu PolicyCode, module boundary) chính là lời giải cho các vấn đề này.

| # | Vấn đề (nếu không có thiết kế §5) | Hệ quả |
|---|--------|--------|
| P1 | Gộp toàn bộ logic quyền vào Role / `ClaimValue` JSON | Khó audit bằng SQL; khó reuse rule giữa Role; RoleClaim phình; đổi 1 rule phải sửa nhiều Role; cache/token stale khó kiểm soát |
| P2 | Chỉ RBAC không đủ cho Org / Dept / Owner / hạn mức / managed resource | Hard-code `if` theo Role/user trong từng API, hoặc nhân bản Role theo từng Org/Dept/limit — nở Role, lệch logic, dễ lộ data cross-scope |
| P3 | Chỉ ABAC thuần (mọi rule là attribute) | Mất mô hình Permission rõ ràng; khó governance “user được làm gì” |
| P4 | Nhồi Permission vào `UserClaims` | Bypass Role; khó revoke theo Role; lệch Identity best practice |
| P5 | Nhồi danh sách quan hệ lớn (`ManagedProjectIds`) vào JWT | Token phình, stale, không phù hợp list động |
| P6 | Cho khách hàng viết DSL tùy ý ngay từ đầu | Rủi ro bảo mật / Privilege Escalation / rule không validate được |
| P7 | Không có RoleLevel / IsSystemRole | OrgAdmin có thể tự nâng Role ngang hoặc vượt quyền mình |
| P8 | Auth đã tách nhưng chưa có ADR Authorization | Host/module thiếu chuẩn Permission / Policy / Data Scope |
| P9 | Nhét Login/Profile/Password vào `Platform.Authentication` core | Phá Module Atomic; Host chỉ JWT/ApiKey vẫn kéo Account business; core phình |
| P10 | Gọi module quyền là `Identity` | Trùng nghĩa ASP.NET Identity (store Users/Roles/Claims dùng chung) |
| P11 | Gộp Account self-service + User/Role admin vào một module | Account phải phụ thuộc cứng Authorization dù Login không cần |

### 2.1 P1 — “Khó” cụ thể khi nhồi logic vào Role / claim JSON

*(Đây là phương án O4 — bị loại. Thiết kế đã chọn dùng `Policies`/`PolicyRules`, RoleClaim chỉ tham chiếu PolicyCode.)*

Anti-pattern điển hình: mỗi RoleClaim chứa cả Permission lẫn biểu thức điều kiện trong một JSON:

```json
{
  "permission": "Expense.Approve",
  "rules": [
    { "left": "Resource.OrgId", "op": "==", "right": "User.OrgId" },
    { "left": "Resource.Amount", "op": "<=", "right": "User.ApproveLimit" }
  ]
}
```

lặp lại trên `R_CFO`, `R_CTO`, `R_DEPT_MGR`, …

| Khó khăn | Cụ thể |
|----------|--------|
| **Audit** | Không trả lời nhanh bằng SQL kiểu “Role nào đang dùng điều kiện `Resource.OrgId`?” — phải parse JSON từng `ClaimValue`, khác dialect DB, không index được. Phụ lục C sẽ **không viết được** nếu rule nằm trong JSON. |
| **Reuse** | Cùng rule “SameOrg” copy-paste vào hàng chục Role. Sửa semantic (ví dụ đổi `OrgId` → hỗ trợ multi-Org) phải tìm/sửa N chỗ; dễ sót → lệch quyền giữa CFO và CTO. |
| **God-object Role** | Role không còn là “gán Permission + tham chiếu Policy” mà chứa cả catalog business rule. UI/API Role vừa quản lý tên/level vừa edit expression → khó governance, khó phân quyền “ai được sửa rule hệ thống”. |
| **ClaimValue phình** | Identity/`AspNetRoleClaims.ClaimValue` thường là string ngắn; JSON nhiều rule + nhiều permission → hàng claim lớn, khó đọc log, dễ vượt giới hạn cột / token nếu đem nguyên JSON vào JWT. |
| **So sánh / diff** | Hai Role “gần giống” khó diff có ý nghĩa (whitespace JSON, thứ tự rule). Review PR seed/migration không đọc được như bảng `PolicyRules`. |
| **Cache & invalidate** | Đổi 1 điều kiện “SameOrg” lý thuyết phải invalidate mọi Role chứa JSON đó — không có `PolicyId` chung để `Invalidate(Policy:P_SAME_ORG)`. |
| **Gán nhầm / escalation** | OrgAdmin copy JSON từ TenantAdmin (có `SameTenant`) vào Role Org vì “cùng format” — không có entity Policy để kiểm tra grant-scope theo `PolicyCode` whitelist. |
| **Test** | Không seed một lần `P_SAME_ORG` rồi gắn nhiều Role (T20–T25, T60+); mỗi Role cần fixture JSON riêng → test config nở và trùng lặp. |

**Cách ADR giải quyết:** RoleClaim chỉ lưu `Permission` hoặc `Policy` = mã (`P_SAME_ORG`); biểu thức nằm ở `Policies` / `PolicyRules` — một chỗ sửa, nhiều Role reuse, audit SQL được (Phụ lục C), cache theo `PolicyCode`.

### 2.2 P2 — Hard-code / nhân bản Role cụ thể thế nào?

*(Đây là phương án O1 — chỉ RBAC, **không có** `Policies`/`PolicyRules` — bị loại. Chính vấn đề này là lý do thiết kế §5 thêm hai bảng đó.)*

RBAC thuần chỉ trả lời: *user có `Expense.Approve` không?*  
Không trả lời: *approve được phiếu **Org nào**, **phòng nào**, **dưới bao nhiêu tiền**, **project nào**?*

Nếu chỉ có RBAC mà thiếu ABAC / Data Scope, code buộc rơi vào một trong hai hướng:

#### Hướng A — Hard-code trong từng API / handler

```csharp
// ExpenseController.Approve
if (!User.HasPermission("Expense.Approve"))
    return Forbid();

// Phải tự viết scope — lặp lại ở mọi action liên quan
if (expense.OrgId != currentUser.OrgId)
    return Forbid();

if (User.IsInRole("CFO") && expense.Amount > 10_000_000)
    return Forbid();

if (User.IsInRole("DepartmentManager") && expense.Amount > 2_000_000)
    return Forbid();

if (User.IsInRole("DepartmentManager")
    && expense.DepartmentId != currentUser.DepartmentId)
    return Forbid();

// Technical Manager — lại một cụm if khác trong ProjectController
// Employee — lại Owner || SameDept trong CustomerController
// ...
```

| Khó khăn | Cụ thể |
|----------|--------|
| **Lặp & lệch** | Cùng rule “cùng Org” copy sang Expense, Accounting, Project, Sales… Đổi semantic (multi-Org) phải sửa N controller; sót một chỗ = lỗ hổng cross-Org. |
| **Gắn chết tên Role** | `IsInRole("CFO")` + magic number `10_000_000` — đổi tên Role hiển thị, thêm CTO cùng limit, hoặc limit theo UserClaim đều phải sửa code + redeploy. |
| **List/search không an toàn** | API list hay `GetAll()` rồi filter trong memory, hoặc quên `Where(OrgId == …)` → lộ bản ghi Org khác trước khi check từng item (đúng pain §5.11). |
| **Khó test / audit** | Không có PolicyCode; test phải cover từng nhánh Role×API. Không SQL được “ai đang dùng SameOrg”. |
| **UI cấu hình vô dụng** | Khách tạo Role mới “Deputy CFO” + gán `Expense.Approve` vẫn **không** có hạn mức/scope trừ khi dev viết thêm `if`. |

#### Hướng B — Nhân bản Role theo scope (vẫn chỉ RBAC)

```text
CFO_O001_10M
CFO_O001_20M
CFO_O002_10M
DeptMgr_D-ACC_2M
DeptMgr_D-TECH_2M
Employee_O001_D-TECH
…
```

Mỗi biến thể Org × Dept × hạn mức × resource = một Role + bộ Permission gần giống nhau.

| Khó khăn | Cụ thể |
|----------|--------|
| **Nổ số Role** | 10 Org × 5 mức duyệt × vài Dept → hàng trăm Role gần trùng; IdentityAdmin/UI không quản lý nổi. |
| **Gán User cứng** | Chuyển user sang Org/Dept khác = đổi Role (hoặc giữ Role cũ → sai scope). ApproveLimit không tách được thành attribute. |
| **Sửa Permission hàng loạt** | Thêm `Expense.View` phải cập nhật mọi biến thể `CFO_*`; sót một Role = lệch quyền thầm lặng. |
| **Vẫn thiếu Owner / OR** | `Employee` cần Owner **hoặc** SameDept — nhân bản Role không diễn tả OR trên resource; vẫn phải hard-code ở API. |
| **ManagedProject** | Không thể tạo Role cho từng tập project động; lại quay về hard-code hoặc nhét ID vào claim (P5). |

#### Ví dụ nghiệp vụ “gãy” nếu chỉ RBAC

| Nhu cầu | Chỉ RBAC thấy gì | Thực tế cần thêm |
|---------|------------------|------------------|
| CFO và DeptMgr cùng `Expense.Approve` | Giống hệt nhau | Limit 10M vs 2M + scope Org vs Dept |
| Director `Accounting.View` | Được xem mọi bản ghi nếu có permission | Chỉ trong Org của Director |
| Sales Employee `SalesResult.View` | Xem được mọi doanh số | Chỉ bản ghi `EmployeeId == User.Id` |
| Technical Manager `Project.Edit` | Sửa mọi project | Chỉ project thuộc Dept / managed set |

**Cách ADR giải quyết:**  
`Permission` (RBAC) + gán `Policy` SameOrg / ApproveLimit / OwnerOrSameDept… (ABAC) + attribute trên UserClaim (`OrgId`, `ApproveLimit`) + Data Scope trên query — **một** Role `CFO`, nhiều user khác limit/Org; không `if (IsInRole("CFO"))` trong từng API.

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Tận dụng ASP.NET Identity: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims` |
| C2 | Chỉ thêm bảng Authorization extension tối thiểu: **`Policies`**, **`PolicyRules`** (v1) |
| C3 | Không tạo ngay `Permissions` / `RolePermissions` / `UserPermissions` / `PolicyExpressions` trừ khi sau này cần metadata sâu hơn |
| C4 | System roles cố định: `Admin`, `TenantAdmin`, `OrgAdmin` — không cho khách hàng thêm/xóa Permission/Policy/Role |
| C5 | Role Governance: Actor chỉ tạo/sửa Role khi `Actor.RoleLevel > TargetRole.RoleLevel` (**strictly lower**) |
| C6 | Actor chỉ cấp Permission/Policy mà Actor **đang có** (ngăn Privilege Escalation) |
| C7 | Policy definition do hệ thống seed/whitelist operand & operator; khách hàng **gán**, không viết DSL tự do ở v1. Whitelist operand = **operand registry** đăng ký bằng code lúc khởi động (platform: operand generic; sản phẩm/module: operand nghiệp vụ) — §5.6 |
| C8 | JWT/token không chứa collection domain lớn (ví dụ hàng nghìn `ManagedProjectIds`) — resolve từ domain/cache |
| C9 | Authorization = RBAC **AND** ABAC (khi permission yêu cầu policy); fail-fast RBAC trước |
| C10 | `Platform.Authentication.*` = foundation AuthN (scheme, options, password **policy**, CurrentUser) — **không** chứa Login/Logout/Profile/Forgot/Change password |
| C11 | Engine phân quyền = **`Platform.Authorization`** (§7 Q4 — amendment 2026-10-02; tên `Platform.IdentityAccessManagement` dùng từ 2026-09-29 đến 2026-10-02) — **không** đặt tên package/module `Identity` cho RBAC/ABAC |
| C12 | Module **Account** = self-service (Login…Change password); **không** phụ thuộc cứng Authorization |
| C13 | Module **IdentityAdmin** = CRUD User/Role + gán Permission/Policy; phụ thuộc Authentication + Authorization |
| C14 | Identity store (AspNet*) là **shared persistence** — không thuộc riêng Account hay Authorization |
| C15 | Host compose opt-in; Authentication **không** reference Account / Authorization / IdentityAdmin |
| C16 | Login **không** evaluate mọi Policy — chỉ build Authorization Context (permissions/policy codes + attributes) vào token/cache |

---

## 4. Phương án đã cân nhắc

### 4.1 Mô hình quyền

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | Chỉ ASP.NET Identity Roles + RoleClaims Permission | Đơn giản, Identity-native | Ít bảng | Không ABAC / data scope / threshold |
| O2 | Custom Permission tables + ABAC engine đầy đủ (DSL) | Linh hoạt tối đa | Biểu thức phức tạp | Over-engineer; lệch Identity; chi phí bảo trì cao |
| O3 | **RBAC qua RoleClaims + ABAC qua Policies/PolicyRules** (Identity-first) | Permission/Policy trên RoleClaim; attribute trên UserClaim; Policy seed sẵn | Tái sử dụng Identity; audit dễ; đủ ERP scope | Cần semantics AND/OR rõ; engine evaluate + data filter |
| O4 | Đưa mọi rule vào ClaimValue JSON trên RoleClaim | Không thêm bảng | Ít schema | Khó query/audit; JSON phình; không cache policy độc lập |

**Đề xuất chọn (quyền):** **O3**.

### 4.2 Ranh giới package / module

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O5 | Nhét Login/Profile/Password vào `Platform.Authentication` | Ít package | Nhanh “có login” | Phá Atomic; Host service-only kéo Account; lệch refactor Auth |
| O6 | Một module `Identity` = Account + Admin + AuthZ | Một chỗ | Ít tên | God-module; trùng tên ASP.NET Identity; Account kéo AuthZ |
| O7 | **Authentication (giữ) + Authorization (mới) + Account + IdentityAdmin** | Đúng lớp; opt-in | Rõ trách nhiệm; phụ thuộc sạch | Nhiều package/module hơn |

**Đề xuất chọn (ranh giới):** **O7**.

---

## 5. Quyết định

Chúng ta sẽ thiết kế Authorization theo mô hình **RBAC (Permission) + ABAC (Policy/PolicyRule) + Subject Attributes**, tận dụng ASP.NET Identity và chỉ mở rộng `Policies` / `PolicyRules`; đồng thời tách ranh giới **Authentication / Authorization / Account / IdentityAdmin**.

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Tách trách nhiệm | **RBAC** = “được làm gì?”; **ABAC** = “trên dữ liệu nào / điều kiện nào?” |
| D2 | Schema Identity | Giữ Users / Roles / UserRoles / UserClaims / RoleClaims |
| D3 | Schema extension | Thêm `Policies` + `PolicyRules` |
| D4 | RoleClaim types | `ClaimType = Permission` \| `Policy`; Policy claim chỉ lưu **PolicyCode/Id**, không nhồi rule JSON |
| D5 | UserClaim | Subject attributes (`TenantId`, `OrgId`, `DepartmentId`, `ApproveLimit`, …); **không** lưu Permission trên UserClaim |
| D6 | Role | Mở rộng `IsSystemRole`, `RoleLevel` — entity `Role` (trước đây `ApplicationRole`) cùng `User` đặt ở `Platform.Authentication.Identity` (§11.1) |
| D7 | System roles | Identifier ổn định `ADMIN` / `TENANT_ADMIN` / `ORG_ADMIN` — không dùng display name làm logic |
| D8 | Wildcard | `Permission = System.All` (ưu tiên hơn `*`); Admin = global; TenantAdmin/OrgAdmin vẫn qua ABAC scope |
| D9 | Policy v1 | Seed: SameTenant, SameOrg, SameDepartment, OwnerOnly, ApproveLimit, ManagedProject, ManagedDepartment |
| D10 | Operators v1 | `==`, `!=`, `>`, `>=`, `<`, `<=`, `IN` — whitelist cố định. Operand: `IOperandRegistry`, kiểm cả khi lưu rule lẫn lúc evaluate |
| D11 | AND/OR | Nhiều Policy trên Role mặc định **AND**; case OR (OwnerOnly ∨ SameDepartment) biểu diễn bằng Policy Group / AuthorizationRuleSet — không implicit OR từ danh sách phẳng |
| D12 | Runtime 2 lớp | (1) Permission check; (2) Data Scope / Policy evaluate (+ query filter cho list) |
| D13 | Cache | Snapshot Authorization Context + Policy definition cache; invalidate khi Role/Claim/Policy đổi |
| D14 | Framework AuthZ (phân quyền) | **`Platform.Authorization`** (Q4 amendment 2026-10-02), DI `AddPlatformAuthorization()` — model + engine RBAC+ABAC, abstraction Role Governance, Data Scope; **không phụ thuộc EF Core / store user**. Store EF tách riêng: **`Platform.Authorization.EntityFramework`**, DI `AddPlatformAuthorizationEntityFramework()` (provider, `IdentityDbContextBase`, cấu hình EF, seeder) — 2026-10-02. Host opt-in; tách khỏi `Platform.Authentication.*` |
| D15 | Phạm vi engine v1 | Không nested DSL / NOT / function / external environment context |
| D16 | Giữ Authentication | `Platform.Authentication.*` giữ nguyên vai trò foundation AuthN — **không** thêm Login/Logout/Profile/Password flows |
| D17 | Module Account | Self-service: Login, Logout, Profile, Forgot Password, Change Password → chỉ phụ thuộc Authentication (+ Identity store) |
| D18 | Module IdentityAdmin | Admin: CRUD User (lock/unlock), CRUD Role, gán Permission/Policy (+ User↔Role) → phụ thuộc Authentication + Authorization |
| D19 | Permission | Hệ thống **seed/whitelist**; IdentityAdmin **gán** vào Role — không CRUD Permission string tùy ý ở v1 |
| D20 | Naming | Không đặt tên module/package quyền là `Identity`; Identity = ASP.NET Identity store dùng chung. Tên thực tế: phân quyền `Platform.Authorization` + `Platform.Authorization.EntityFramework`; store danh tính `Platform.Authentication.Identity` (chữ "Identity" ở đây đúng nghĩa *danh tính* / ASP.NET Identity — D20 chỉ cấm cho package phân quyền; tên cũ `Platform.Authentication.AspNetIdentity`, đổi 2026-10-02 để bỏ tiền tố công nghệ "Asp"); module `Platform.Modules.Account` / `Platform.Modules.IdentityAdmin`. Tránh trùng ASP.NET Core: DI là `AddPlatformAuthorization()` (không phải `AddAuthorization()`), kết quả evaluate là `AccessDecision` (không phải `AuthorizationResult`); "Policy" trong ADR này là bộ điều kiện ABAC, **khác** authorization policy của ASP.NET (`[Authorize(Policy = …)]`) |

### 5.0 Ranh giới package / module & map chức năng

```text
erp_core/platform/frameworks/
  Platform.Authentication.*                 ← xác thực: scheme JWT/ApiKey/Basic/Cognito, password policy, CurrentUser
  Platform.Authentication.Identity          ← store danh tính dùng chung: entity User, Role (C14, Q10) — không EF
  Platform.Authorization                    ← phân quyền: model + engine RBAC/ABAC, abstraction Role Governance, Data Scope (Q4) — không EF
  Platform.Authorization.EntityFramework    ← store EF của phân quyền: provider, IdentityDbContextBase, cấu hình EF, seeder

erp_core/platform/modules/identity/
  Platform.Modules.Account                  ← Login, Logout, Profile, Forgot/Change Password
  Platform.Modules.IdentityAdmin            ← CRUD User (lock/unlock), CRUD Role, gán Permission/Policy

Host (Lexora.Host) = composition root
  AddPlatformAuthentication(...)            // + AddIdentity<User, Role>()
  AddPlatformAuthorization()
  AddPlatformAuthorizationEntityFramework()
  AddAccountApplication()
  AddIdentityAdminApplication()
```

Account + IdentityAdmin cùng family thư mục `modules/identity/` nhưng **tách package/API** theo self-service vs admin. Cấu trúc file chi tiết: §11.

#### Map 8 chức năng

| # | Chức năng | Đặt ở | Phụ thuộc chính |
|---|-----------|--------|-----------------|
| 1 | CRUD User, lock/unlock | **IdentityAdmin** | Authentication + Authorization (`User.Manage`, …) |
| 2 | CRUD Role | **IdentityAdmin** | Authorization (Role Governance) |
| 3 | Permission (định nghĩa + evaluate) | **Authorization** | — |
| 3b | Permission / Policy **gán vào Role** | **IdentityAdmin** | Authorization |
| 4 | Login | **Account** | Authentication |
| 5 | Logout | **Account** | Authentication |
| 6 | Profile | **Account** | Authentication (thường chỉ “đã đăng nhập”) |
| 7 | Forgot Password | **Account** | Authentication / Identity token + email |
| 8 | Change Password | **Account** | Authentication |

#### Phụ thuộc (cấm chiều ngược)

```text
Account          → Authentication + Identity store
IdentityAdmin    → Authentication + Identity store + Authorization
Authorization    → (không gì trong khối này — chỉ làm việc qua userId và interface)
Authorization.EF → Authorization + Identity store + ORM.EntityFramework
Identity store   → Authentication
Authentication   → (không) Account | Authorization | IdentityAdmin
Authorization    → (không) Account | IdentityAdmin | EF Core

Ánh xạ package: Authentication = Platform.Authentication.* · Identity store = Platform.Authentication.Identity
                Authorization = Platform.Authorization · Authorization.EF = Platform.Authorization.EntityFramework
                Account / IdentityAdmin = Platform.Modules.Account / .IdentityAdmin
Chỉ Authorization.EF (và product Infrastructure) phụ thuộc EF Core.
```

Account **không** phụ thuộc cứng Authorization cho flow 4–8. Host áp dụng `[Authorize]` / pipeline chung khi cần.

Satellite `Platform.Authentication.Identity` (Q10 — làm ngay): đã có entity `User` / `Role`; 🔴 chưa wire `UserManager` / `SignInManager` (Host đang tự gọi `AddIdentity<User, Role>()`). Vẫn **không** chứa API Login/Profile trong core Authentication.

#### Runtime sau Login

```text
Login (Account)
  → Authentication + Identity store
  → Build Authorization Context (permissions, policy codes, attributes)
  → Token / cookie / cache

Request sau đó
  → Authentication xác định user
  → Authorization: HasPermission? → Policy / Data Scope?
```

### 5.1 Mô hình dữ liệu tổng thể

```text
┌─────────────────┐
│    AspNetUsers  │
└────────┬────────┘
         │ UserRoles
         ▼
┌─────────────────┐
│   AspNetRoles   │  (+ IsSystemRole, RoleLevel)
└────────┬────────┘
         │ RoleClaims
         ├──────────────────────────┐
         ▼                          ▼
┌─────────────────┐          ┌───────────────┐
│   Permission    │          │    Policy     │
│ (ClaimType/Val) │          └───────┬───────┘
└─────────────────┘                  │
                                     ▼
                              ┌───────────────┐
                              │  PolicyRules  │
                              └───────────────┘

AspNetUsers ──UserClaims──► Subject Attributes
                            (TenantId / OrgId / DeptId / ApproveLimit / …)
```

Bảng chính:

```text
AspNetUsers
AspNetRoles
AspNetUserRoles
AspNetUserClaims
AspNetRoleClaims
Policies
PolicyRules
```

Dữ liệu mẫu đầy đủ theo từng cấu hình nghiệp vụ: **[Phụ lục D](#phụ-lục-d--dữ-liệu-mẫu-theo-cấu-hình)**.

### 5.2 AspNetUsers & UserClaims

Identity vẫn là nguồn user. Attribute authorization có thể nằm ở UserClaims nếu muốn giữ context dạng claim:

| UserId | ClaimType | ClaimValue |
|--------|-----------|------------|
| U001 | TenantId | T001 |
| U001 | OrgId | O001 |
| U001 | DepartmentId | D010 |
| U001 | ApproveLimit | 10000000 |

**Không** dùng UserClaim cho Permission kiểu `CanEditCustomer=true`. Permission chỉ trên RoleClaim.

Field quan hệ cốt lõi của domain (nếu đã có cột trên User entity) không bắt buộc nhân bản sang claim — nhưng Authorization Context phải resolve được cùng semantic.

### 5.3 AspNetRoles — System vs Business

```csharp
// Platform.Authentication.Identity/Domain/Role.cs (trước đây ApplicationRole)
public sealed class Role : IdentityRole<Guid>
{
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Cấp độ quyền. Level càng cao thì quyền càng lớn.
    /// </summary>
    public int RoleLevel { get; set; } = 100;

    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

| Role | IsSystemRole | RoleLevel |
|------|-------------:|----------:|
| Admin | true | 1000 |
| TenantAdmin | true | 900 |
| OrgAdmin | true | 800 |
| Director | false | 700 |
| CFO / CTO | false | 650 |
| DepartmentManager | false | 500 |
| Employee | false | 100 |

System Role (`IsSystemRole = true`): không đổi tên (nếu hệ thống khóa), không thêm/xóa Permission/Policy, không xóa Role.

```csharp
public static class SystemRoles
{
    public const string Admin = "ADMIN";
    public const string TenantAdmin = "TENANT_ADMIN";
    public const string OrgAdmin = "ORG_ADMIN";
}
```

> 🔴 Hằng số `SystemRoles` chưa có trong code — tên role đang là string literal trong `AuthorizationSeeder`.

### 5.4 Role Governance

```text
Actor.RoleLevel > TargetRole.RoleLevel   // strictly greater
```

Đồng thời kiểm tra:

1. RoleLevel  
2. Permission grant scope (chỉ cấp Permission Actor có)  
3. Policy grant scope (chỉ gán Policy Actor có)  
4. Tenant scope  
5. Org scope  

Ví dụ: user Org O001 **không** được tạo Role gắn `SameTenant` nếu bản thân không được vượt Org.

### 5.5 AspNetRoleClaims

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_CFO | Permission | Expense.View |
| R_CFO | Permission | Expense.Approve |
| R_CFO | Policy | P_SAME_ORG |
| R_CFO | Policy | P_APPROVE_LIMIT |

Lý do Policy chỉ reference code/id: tận dụng RoleClaim, rule có cấu trúc riêng, query/audit độc lập, không nhồi JSON vào `ClaimValue`.

User có nhiều Role → **hợp nhất** Permission/Policy từ toàn bộ Role.

### 5.6 Policies & PolicyRules

```csharp
public class Policy
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ResourceType { get; set; }
    public string? Action { get; set; }
    public bool IsSystemPolicy { get; set; }
    public ICollection<PolicyRule> Rules { get; set; } = new List<PolicyRule>();
}

public class PolicyRule
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string LeftOperand { get; set; } = default!;
    public string Operator { get; set; } = default!;
    public string RightOperand { get; set; } = default!;
    public int Order { get; set; }
}
```

Policy chuẩn ERP (v1):

| Code | Rule (tóm tắt) | Dùng cho |
|------|----------------|----------|
| SameTenant | `Resource.TenantId == User.TenantId` | TenantAdmin, IT tập đoàn |
| SameOrg | `Resource.OrgId == User.OrgId` | OrgAdmin, Director, CFO, … |
| SameDepartment | `Resource.DepartmentId == User.DepartmentId` | Dept Manager, nhân viên phòng |
| OwnerOnly | `Resource.CreatedBy == User.Id` | Employee, Sales Employee |
| ApproveLimit | `Resource.Amount <= User.ApproveLimit` | CFO/CTO, Dept Manager |
| ManagedProject | Dept match hoặc `ProjectId IN ManagedProjectIds` (resolve domain) | Technical Manager |
| ManagedDepartment | `DepartmentId IN ManagedDepartmentIds` | Sales Manager đa phòng |

Operand whitelist — **operand registry** (2026-10-02; thay danh sách hardcode trong `OperandResolver` / `OperandValidator`):

```text
Platform (mặc định, generic):   User.Id (subject) · User.TenantId · User.OrgId (cột AspNetUsers — Q3)
                                Resource.TenantId · Resource.OrgId · Resource.CreatedBy
Sản phẩm / module (đăng ký):    Lexora — AddLexoraAuthorizationOperands():
                                User.DepartmentId · User.ApproveLimit · User.ManagedProjectIds[] · User.ManagedDepartmentIds[] (UserClaim)
                                Resource.DepartmentId · Resource.Amount · Resource.ProjectId · Resource.EmployeeId
```

- Đăng ký **bằng code lúc khởi động**: `builder.AddPlatformAuthorizationOperands(o => o.AddUserClaim("ApproveLimit").AddResource("Amount"))`; nhiều module cùng đóng góp, trùng định nghĩa khác nhau → lỗi lúc start.
- **Không** cho thêm operand lúc runtime (bảng DB / API): whitelist thành dữ liệu sửa được thì C7 mất tác dụng. Nếu cần theo tenant, chỉ bật/tắt trong số đã đăng ký.
- Operator (`==`, `!=`, `<`, `<=`, `>`, `>=`, `IN`) giữ cố định trong code — là ngữ nghĩa của engine, không phải nghiệp vụ.
- `[]` = `OperandValueKind.Collection` (CSV trong claim, parse từng phần tử Guid → decimal → string); `IN` chỉ nhận vế phải là collection đã đăng ký — không còn đoán theo hậu tố `Ids`.

Validator reject operand/operator ngoài registry khi lưu rule (ví dụ `Resource.Password`); resolver **cũng** từ chối operand chưa đăng ký lúc evaluate (D10) — rule đó fail-closed.

### 5.7 Mapping requirement → RBAC + ABAC

| Requirement | RBAC | ABAC |
|-------------|------|------|
| Admin xem toàn hệ thống | `System.All` | Global |
| TenantAdmin xem toàn Tenant | `System.All` | SameTenant |
| OrgAdmin xem toàn Org | `System.All` | SameOrg |
| Director xem toàn Org, không sửa | `*.View` (các resource) | SameOrg |
| IT tập đoàn sửa config công ty thành viên | `Company.Config.Edit` | SameTenant (hoặc scope riêng) |
| Kế toán trưởng xem/sửa kế toán | `Accounting.View/Edit` | SameOrg |
| Trưởng phòng kỹ thuật xem/sửa project | `Project.View/Edit` | ManagedProject / SameDepartment |
| Employee dữ liệu của mình / phòng | `<domain>.View/Edit` | OwnerOnly **OR** SameDepartment |
| CFO/CTO duyệt &lt; 10M | `Expense.Approve` | ApproveLimit + SameOrg |
| Trưởng phòng duyệt &lt; 2M | `Expense.Approve` | ApproveLimit + SameDepartment |
| Sales Manager doanh số phòng | `SalesResult.View/Edit` | ManagedDepartment / SameDepartment |
| Sales Employee doanh số mình | `SalesResult.View` | OwnerOnly |

### 5.8 Role mẫu (tóm tắt)

| Role | Type | Level | Permission | Policy |
|------|------|------:|------------|--------|
| Admin | System | 1000 | `System.All` | none (global) |
| TenantAdmin | System | 900 | `System.All` | SameTenant |
| OrgAdmin | System | 800 | `System.All` | SameOrg |
| Director | Custom | 700 | các `*.View` | SameOrg |
| CFO | Custom | 650 | Accounting.*, Expense.View/Approve | SameOrg + ApproveLimit |
| TechnicalManager | Custom | 500 | Project.View/Edit | SameDepartment / ManagedProject |
| Employee | Custom | 100 | `<business>.View/Edit` | OwnerOnly **OR** SameDepartment |

### 5.9 Authentication vs Authorization flow

**Login (Account module)** — không evaluate mọi Policy trên resource:

```text
Username + Password → Identity (User, Roles, Claims)
  → Authorization Context (permissions + policy codes + attributes)
  → Cache / Access Token
```

JWT nên chứa subject attributes + permission codes + policy codes; **không** nhét collection quan hệ lớn.

> **Hiện trạng (2026-10-02):** JWT do `LoginCommandHandler` phát chỉ có `sub`, `jti`, `name` và tenant claim (`TenantClaimName`, mặc định `GroupSid`). Engine đọc permission / policy / attribute từ DB ở mỗi lần evaluate; chưa có Authorization Context trong token hay cache (C16 🟡, D13 🔴). Chưa endpoint nào chạy luồng RBAC → ABAC dưới đây (D14 🟡).

**AuthZ (`Platform.Authorization`)** trên request được bảo vệ:

```text
Request → Authentication → User Context
  → RBAC Permission Check (Deny → 403)
  → ABAC Policy Check (Deny → 403)
  → Allow
```

Decision model:

```text
HTTP Request → Authentication → User Context
        ├─ RBAC Permission Check
        └─ ABAC Context Resolution
                → Policy Evaluate → ALLOW | DENY
```

### 5.10 Runtime — Permission, Operand, Rule, Policy

```csharp
public static bool HasPermission(this ClaimsPrincipal user, string permission)
{
    if (user.HasClaim("Permission", "System.All") || user.HasClaim("Permission", "*"))
        return true;

    return user.Claims.Any(x =>
        x.Type == "Permission" && x.Value == permission);
}
```

Policy codes từ RoleClaim → load Policy → PolicyRules (cache theo `PolicyCode`).

```csharp
public sealed class AuthorizationContext
{
    public required ClaimsPrincipal User { get; init; }
    public required object Resource { get; init; }
    public required string ResourceType { get; init; }
    public required string Action { get; init; }
}
```

`IOperandResolver` resolve `User.*` từ claims và `Resource.*` từ property resource. Production nên dùng compiled accessor / domain resolver thay vì reflection mọi request.

`IRuleEvaluator` hỗ trợ operators v1; `IN` cho collection đã resolve.

**Semantics:** nhiều Policy trên một authorization requirement mặc định **AND**. OR cần `AuthorizationRuleSet` / Policy Group rõ ràng — không suy luận OR từ danh sách RoleClaim phẳng.

> **Hiện trạng (2026-10-02)** — code mẫu ở trên là minh hoạ, API thật khác:
> - Không có extension `HasPermission(ClaimsPrincipal)`. RBAC kiểm qua `IPermissionProvider.HasPermissionAsync(userId, permission)` / `IAuthorizationEngine.AuthorizeAsync(userId, permission, resource)`, đọc `AspNetUserRoles` → `AspNetRoleClaims` từ DB.
> - Chưa có class `AuthorizationContext`; engine nhận trực tiếp `(userId, permission, resource)`; `ResourceType` / `Action` của `Policy` chưa được dùng khi evaluate.
> - `IRuleEvaluator` hiện thực là `IPolicyEvaluator` / `PolicyEvaluator`; `Resource.*` vẫn đọc bằng reflection, mỗi `User.*` một query.
> - Semantics đã đúng: AND giữa policy trong một role, OR giữa các role cấp permission (Q8), OR giữa rule trong một policy (Q5); thiếu thuộc tính ở vế nào thì rule không đạt.

Ví dụ Expense Approve (CFO, Amount 8M, Limit 10M, cùng Org) → RBAC PASS + SameOrg PASS + ApproveLimit PASS → **ALLOW**. Amount 15M → ApproveLimit FAIL → **DENY**.

### 5.11 Data Scope / Query filter

ABAC không chỉ true/false sau khi load record. List/search phải đẩy scope vào query:

```csharp
query = query.Where(x => x.OrgId == userOrgId);
// Employee:
query = query.Where(x =>
    x.CreatedBy == userId || x.DepartmentId == userDepartmentId);
```

Hai lớp runtime:

1. **Permission Authorization** — action có được phép không?  
2. **Data Scope Authorization** — resource có trong scope không? (+ filter query)

ManagedProjectIds / ManagedDepartmentIds: resolve từ domain relationship (Department ↔ Project, …), không JWT blob.

> **Hiện trạng (2026-10-03):** 🟢 `DataScopeFilter.ApplyFilterAsync` dịch policy của user thành `Expression<Func<T,bool>>` → provider đẩy xuống SQL `WHERE` (giá trị user là tham số SQL, `IN` → `= ANY (@p)`, không quyền → `WHERE FALSE`, wildcard → không lọc). Ngữ nghĩa trùng engine: chỉ role cấp permission tạo đường dẫn; AND policy trong role, OR giữa role (Q8); OR rule trong policy (Q5); thiếu thuộc tính / operand chưa đăng ký / policy không resolve được → không đạt; giá trị user phải cùng kiểu với thuộc tính resource. Test `Authorization/DataScopeFilterTests` (16, gồm đối chiếu từng bản ghi với engine và kiểm tra SQL Npgsql) + Sample `GET api/_authz-demo/scope-check` trên Postgres: 21/21 khớp. Chưa hỗ trợ trong query: `User.X IN Resource.Collection`, so sánh thứ tự cho Guid/string (rule → không đạt). `ManagedProjectIds` / `ManagedDepartmentIds` vẫn đọc UserClaim dạng CSV, chưa resolve từ domain (Q6 🔴).

#### 5.11.1 Luồng `DataScopeFilter.ApplyFilterAsync` (code thực tế 2026-10-03)

```text
ApplyFilterAsync<T>(query, userId, permission)
 ├─ IPermissionProvider.GetPermissionsByRoleAsync(userId)
 ├─ giữ role cấp permission (PermissionMatcher) ── không có ──► query.Where(x => false)   → SQL WHERE FALSE
 ├─ có role wildcard "*" / "System.All" ───────────────────────► trả nguyên query (không lọc, Q7)
 ├─ mỗi role cấp permission: IPolicyProvider.GetPoliciesByRoleAsync(roleId)
 │     UnresolvedPolicyException / không có policy ──► bỏ đường dẫn role này (role khác vẫn xét)
 │     path(role) = AND các policy;  policy = OR các rule (theo Order)
 ├─ predicate = OR path(role)  → Expression<Func<T,bool>>
 └─ query.Where(predicate)     → EF dịch SQL WHERE, giá trị user là tham số

Dịch một PolicyRule (tra IOperandRegistry cho từng vế):
  chưa đăng ký                → false
  Resource.X op User.Y        → x.X != null && x.X op @p          (IN → x.X = ANY(@list); User.Y op Resource.X → đảo toán tử)
  User.Y op User.Z            → hằng true/false (đánh giá bằng PolicyEvaluator)
  Resource.X op Resource.Z    → x.X op x.Z (cùng kiểu, cả hai != null)
  giá trị user null · khác kiểu · T không có property X → false
```

#### 5.11.2 Mẫu dùng trong endpoint (theo `Sample/Controllers/ExpenseSampleController.cs`)

```csharp
// Danh sách: RBAC trước (403 khác "không có dữ liệu"), rồi thu hẹp query, rồi lọc nghiệp vụ + phân trang — một câu SQL.
if (!await engine.HasPermissionAsync(userId, "Expense.View", ct))
    return Problem(statusCode: 403, detail: "Thiếu quyền Expense.View.");

var query = await scopeFilter.ApplyFilterAsync(db.Expenses.AsNoTracking(), userId, "Expense.View", ct);
if (minAmount != null) query = query.Where(e => e.Amount >= minAmount);   // AND với phạm vi
var total = await query.CountAsync(ct);
var items = await query.OrderBy(e => e.Code).Skip(skip).Take(take).Select(...).ToListAsync(ct);

// Thao tác trên một bản ghi: tìm trong phạm vi xem (ngoài phạm vi → 404, không lộ tồn tại),
// rồi engine đánh giá policy của permission hành động trên chính bản ghi (từ chối → 403 + Reason).
var expense = await (await scopeFilter.ApplyFilterAsync(db.Expenses, userId, "Expense.View", ct))
    .FirstOrDefaultAsync(e => e.Code == code, ct);
if (expense is null) return NotFound();

var decision = await engine.EvaluatePoliciesAsync(userId, "Expense.Approve", expense, ct);
if (!decision.IsAllowed) return Problem(statusCode: 403, detail: decision.Reason);
```

| Tình huống | Quy ước phản hồi |
|---|---|
| Không có permission của danh sách | 403 |
| Có permission, không bản ghi nào trong phạm vi | 200, danh sách rỗng |
| Bản ghi ngoài phạm vi xem | 404 |
| Trong phạm vi xem nhưng policy của hành động không đạt (vd. vượt hạn mức) | 403 + `AccessDecision.Reason` |

Kiểm thử trực tiếp trên PostgreSQL 2026-10-03 (xem §11.8): danh sách `employee` → `WHERE "CreatedBy" = @p AND "Amount" >= @p LIMIT/OFFSET`; `manager.sales` duyệt E-01 (8M) → 200, E-02 (15M) → 403; xem E-03 khác phòng → 404.

### 5.12 Cache & Audit

Snapshot gợi ý:

```csharp
public sealed class AuthorizationSnapshot
{
    public Guid UserId { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OrgId { get; init; }
    public Guid? DepartmentId { get; init; }
    public IReadOnlySet<string> Permissions { get; init; } = new HashSet<string>();
    public IReadOnlySet<string> Policies { get; init; } = new HashSet<string>();
    public decimal? ApproveLimit { get; init; }
}
```

Invalidate khi đổi UserRole / RoleClaim / UserClaim / Role / Policy / PolicyRule.

> **Hiện trạng (2026-10-02):** 🔴 chưa có `AuthorizationSnapshot`. `CachedPolicyProvider` chỉ cache `GetPolicyByCodeAsync` (engine không gọi hàm này) và chưa được đăng ký DI. Chưa ghi authorization trace; `AccessDecision` có `Reason` / `PolicyCode` nhưng không được log.

Authorization trace (ERP audit) nên log Permission/Policy/result/reason; tránh log dữ liệu nhạy cảm thừa.

### 5.13 Nguyên tắc quan trọng

1. Role chỉ chứa **Permission + Policy assignment** — không nhồi hàng trăm business rule cụ thể.  
2. Không đưa domain relationship lớn vào JWT.  
3. Không cho khách hàng viết DSL tùy ý ở v1 — whitelist operand/operator.  
4. Validate Policy khi cấu hình; reject field/operator lạ.  
5. Privilege Escalation: RoleLevel **và** Permission/Policy/Tenant/Org grant scope.

### 5.14 ERD logic

```text
AspNetUsers ──1:N── AspNetUserClaims
AspNetUsers ──N:M── AspNetRoles (qua AspNetUserRoles)
AspNetRoles ──1:N── AspNetRoleClaims  (Permission | Policy→Code)
Policies    ──1:N── PolicyRules
```

### 5.15 Tư tưởng thiết kế cuối

Không: `Role = tất cả logic quyền` · Không: `Authentication = Account` · Không: `Identity = Authorization`.

Bốn khối:

```text
Authentication  = How is the caller authenticated?
Authorization   = What + under which conditions + on which data?
Account         = Self-service session & credential (login…password)
IdentityAdmin   = Administer users, roles, assignments
```

Ba lớp quyền trong Authorization:

```text
1. Permission  = What can the user do?
2. Policy      = Under what conditions?
3. Scope/Attr  = On which data?
```

Ví dụ CFO: `Expense.Approve` + SameOrg + ApproveLimit + attributes `OrgId` / `ApproveLimit` — cả ba khớp mới **ALLOW**.

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Tách RBAC/ABAC rõ; tận dụng Identity; audit Permission/Policy bằng SQL trên RoleClaims; đủ use case ERP v1 |
| Tốt | Role Governance + grant-scope chống leo thang quyền |
| Tốt | Data Scope đẩy vào query — tránh load-all-then-filter |
| Tốt | Account self-service không kéo Policy engine; Host chỉ JWT vẫn mỏng |
| Tốt | IdentityAdmin rõ chỗ cho CRUD User/Role + gán quyền |
| Xấu / chi phí | Cần Policy engine + cache invalidation + validator whitelist |
| Xấu / chi phí | AND/OR semantics phải thiết kế cẩn (Policy Group) — Employee OR case |
| Xấu / chi phí | Thêm `Platform.Authorization` + `Platform.Authentication.Identity` + 2 module (Account, IdentityAdmin); Host cut-over |
| Trung lập | `Platform.Authentication.*` giữ nguyên; JWT chỉ thêm permission/policy codes + attributes |
| Trung lập | Managed* collections resolve domain — có thể cần API/cache riêng |
| Trung lập | Account + IdentityAdmin có thể cùng folder family nhưng tách package |

---

## 7. Confirm — Chốt quyết định (Accepted 2026-09-29)

| # | Câu hỏi | Trả lời | Ghi chú |
|---|---------|---------|---------|
| Q1 | Accept O3 + O7 + D1–D20? | ✅ YES | RBAC+ABAC model + 4 khối tách biệt |
| Q2 | Wildcard dùng `System.All` hay cho phép cả `*`? | `*` | Đơn giản; Admin = `Permission = *` |
| Q3 | User attributes: cột User hay UserClaims? | **Cột User**: TenantId, OrgId; **UserClaims**: ApproveLimit, DepartmentId, … | Không cần fallback UserClaim cho TenantId/OrgId |
| Q4 | Tên package? | `Platform.Authorization` *(amendment 2026-10-02)* | Chốt ban đầu 2026-09-29: `Platform.IdentityAccessManagement`. Đổi lại vì: phần "Identity" (`User`/`Role` store) đã tách sang `Platform.Authentication.Identity`, package chỉ còn Permission / Policy / PolicyRule / Role Governance / Data Scope — đúng nghĩa **phân quyền**; đối xứng với `Platform.Authentication.*` (xác thực); và vẫn tránh chữ `Identity` (D20) |
| Q5 | OR semantics v1? | **A: Seed Policy với Order tag** | v1 = seed `P_OWNER_OR_SAME_DEPT` (Order 1=Owner, 2=SameDept); v9+ = DSL expression |
| Q6 | `ManagedProject` v1? | **B: Domain relationship** | Project có ProjectMember + ProjectPosition; resolve `ManagedProjectIds` từ DB |
| Q7 | Admin bypass ABAC? | ✅ YES | `Permission = *` → skip ABAC (global) |
| Q8 | Multi-role policy evaluation? | **C: Per-role path (union)** | Mỗi role = 1 path; result = any path pass → ALLOW |
| Q9 | Account + IdentityAdmin ở đâu? | **Gộp (1 Host Lexora.Host)** | Đơn giản v1; tách sau nếu cần scaling |
| Q10 | Satellite ASP.NET Identity? | **Phase 5 (ngay)** | `Platform.Authentication.Identity` wire UserManager/SignInManager *(tên chốt ban đầu `Platform.Authentication.AspNetIdentity`; đổi 2026-10-02 — xem D20)* |

### 7.1 Làm rõ thêm

#### Q3: Resolver Strategy
- **TenantId, OrgId:** Lấy từ **cột AspNetUsers** (không UserClaims)
- **ApproveLimit, DepartmentId:** Từ **AspNetUserClaims**
- **ManagedProjectIds, ManagedDepartmentIds:** Resolve từ **domain** (query DB ProjectMember, etc.)

#### Q5: OR Semantics v1
Để biểu diễn **Employee được xem/sửa Owner HOẶC SameDept**, seed:
```
P_OWNER_OR_SAME_DEPT
├─ PR08: Order=1, Resource.CreatedBy == User.Id
└─ PR09: Order=2, Resource.DepartmentId == User.DepartmentId
```
Runtime evaluator: nếu **nhiều rule cùng Policy**, logic = **OR** (check Order/Group).

#### Q8: Multi-role Evaluation (Per-Role Path)
```
User có Role1 (Expense.View + P_SAME_ORG) + Role2 (Expense.View + P_APPROVE_LIMIT)

RBAC Union: Permission = {Expense.View} ✓

ABAC Per-Role Path:
├─ Path1: P_SAME_ORG (Resource.OrgId == User.OrgId)
├─ Path2: P_APPROVE_LIMIT (Resource.Amount <= User.ApproveLimit)

Result = Path1 OR Path2 (nếu ≥1 thỏa → ALLOW)
```

**Không có deny-override:** User không được phân quyền = không có permission → DENY.

#### Q9: Host Architecture (Gộp 1 Host)
```
Lexora.Host (1 process)
├─ Authentication (JWT + ApiKey + Basic)
├─ Authorization (RBAC + ABAC engine)
├─ Account module (/api/account/*)
├─ IdentityAdmin module (/api/identity-admin/* → [Authorize(Roles="Admin")])
└─ Business APIs (/api/expenses, /api/projects, … → [Authorize])
```

Lý do gộp:
- ✅ Đơn giản v1; 1 Host, 1 process
- ✅ Transaction chung (tạo user + assign role = 1 tx)
- ✅ Deploy nhanh (1 lần)
- ⏳ Tách sau nếu IdentityAdmin thành bottleneck

Status: 🟢 **Accepted** (anh duyệt)

### 7.2 Đối chiếu quyết định ↔ code (rà soát 2026-10-02)

Icon: 🟢 code đúng quyết định · 🟡 đúng một phần · 🔴 chưa làm · ⚠️ mâu thuẫn cần chốt. Chi tiết file: §11; test: §8.

#### Ràng buộc C1–C16

| # | Trạng thái | Hiện trạng trong code |
|---|------------|-----------------------|
| C1 | 🟢 | Dùng `AspNetUsers/Roles/UserRoles/UserClaims/RoleClaims` (`IdentityDbContext<User, Role, Guid>`) |
| C2 | 🟢 | Chỉ thêm `lexora.policies`, `lexora.policy_rules` |
| C3 | 🟢 | Không có bảng `Permissions` / `RolePermissions` / `UserPermissions` |
| C4 | 🟡 | Assign/Remove permission/policy chặn `IsSystemRole`; `CreateRole` luôn tạo business role. Chưa có lệnh sửa/xoá role nên chưa có luồng nào cần chặn đổi tên/xoá; hằng số `SystemRoles` chưa có |
| C5 | 🟢 | `RoleGovernanceValidator`: level actor **>** level đích, áp cho mọi lệnh IdentityAdmin |
| C6 | 🟡 | Grant-scope cho Permission (wildcard theo quyền đang có) và Policy 🟢. Admin (không có policy nào) **không gán được policy** — cần chốt ngoại lệ cho `*` |
| C7 | 🟢 | Policy chỉ đến từ seed; chưa có API custom policy. Operand qua registry (platform generic + Lexora nghiệp vụ), không còn hardcode nghiệp vụ trong `Platform.Authorization*` |
| C8 | 🟢 | JWT chỉ có `sub`, `jti`, `name`, tenant claim — không chứa collection |
| C9 | 🟡 | RBAC fail-fast trước ABAC 🟢. Permission chỉ cần RBAC (vd `HealthCheck.View`, §1) chỉ chạy được qua `AuthorizeAsync(resource: null)`; nếu có resource mà role không gán policy thì **DENY** — xem vấn đề mở V2 |
| C10 | 🟢 | `Platform.Authentication.*` không chứa Login/Profile/Password |
| C11 | 🟢 | Engine = `Platform.Authorization` (Q4) |
| C12 | 🟢 | `Platform.Modules.Account` không reference engine (T13 chưa có test tự động) |
| C13 | 🟢 | `Platform.Modules.IdentityAdmin` → Authentication + Authentication.Identity + Authorization (core, qua interface — kể cả `IRoleGovernanceValidator`); **không** phụ thuộc EF Core |
| C14 | 🟢 | Store dùng chung `Platform.Authentication.Identity` (`User`, `Role`) |
| C15 | 🟢 | Host opt-in từng khối; Authentication không reference khối nào khác |
| C16 | 🟡 | Login không evaluate policy 🟢; nhưng **chưa build Authorization Context** (permission/policy/attribute) vào token/cache |

#### Quyết định D1–D20

| # | Trạng thái | Hiện trạng trong code |
|---|------------|-----------------------|
| D1–D3 | 🟢 | RBAC (RoleClaim `Permission`) + ABAC (`Policy`/`PolicyRule`) |
| D4 | 🟢 | RoleClaim `Policy` chỉ lưu mã; mã không tồn tại → `UnresolvedPolicyException`, path fail-closed |
| D5 | 🟢 | Permission chỉ đọc từ RoleClaim; UserClaim chỉ giữ attribute (TenantId/OrgId đã chuyển sang cột — Q3) |
| D6 | 🟢 | `Role.IsSystemRole`, `Role.RoleLevel` |
| D7 | 🟡 | Seed dùng tên ổn định `ADMIN` / `TENANT_ADMIN` / `ORG_ADMIN`; chưa có hằng số dùng chung |
| D8 | 🟡 | Chấp nhận cả `*` và `System.All`; Admin bypass ABAC 🟢; "TenantAdmin/OrgAdmin vẫn qua ABAC scope" **không** đạt nếu seed `System.All` cho họ — ⚠️ V1 |
| D9 | 🟡 | Seed 7 policy: có `P_OWNER_OR_SAME_DEPT` (Q5), **thiếu** `P_MANAGED_DEPARTMENT`; `P_MANAGED_PROJECT` dùng `DepartmentId IN User.ManagedDepartmentIds` (khác D.2 và Q6). Từ 2026-10-02 (V5): platform chỉ seed 3 policy chung (`P_SAME_TENANT`, `P_SAME_ORG`, `P_OWNER_ONLY`); 4 policy nghiệp vụ do Lexora seed (`AddLexoraAuthorizationSeed`) |
| D10 | 🟢 | Operator whitelist cố định; operand whitelist = `IOperandRegistry` — `OperandValidator` và `OperandResolver` dùng chung, resolver từ chối operand chưa đăng ký lúc evaluate (kể cả khi resource có property đó). Resource vẫn đọc bằng reflection (chỉ property đã đăng ký) |
| D11 | 🟢 | AND giữa policy trong một role; OR giữa role (Q8); OR giữa rule trong policy theo `Order` (Q5) |
| D12 | 🟢 | Lớp 1 Permission 🟢 · Lớp 2 evaluate trên 1 resource 🟢 · query filter cho list 🟢 (`DataScopeFilter` → SQL WHERE, 2026-10-03); endpoint nghiệp vụ chưa gọi |
| D13 | 🔴 | `CachedPolicyProvider` chưa đăng ký DI; chưa có snapshot / invalidation |
| D14 | 🟡 | Engine + governance có; core tách khỏi EF (`Platform.Authorization` / `.EntityFramework`) 🟢; Host opt-in 🟢; chưa tích hợp `IAuthorizationHandler`/attribute — **chưa endpoint nào gọi engine** |
| D15 | 🟢 | Không có DSL / NOT / function |
| D16 | 🟢 | Authentication core giữ nguyên vai trò |
| D17 | 🟡 | Handler Login/Logout/Profile/Forgot/Change/Reset đủ; Logout chưa revoke token; email chỉ log; chưa có API |
| D18 | 🟡 | Handler CRUD User (lock/unlock), tạo Role, gán/gỡ Permission/Policy đủ; chưa sửa/xoá Role; chưa có API |
| D19 | 🔴 | **Chưa có catalog/whitelist permission** — gán được chuỗi bất kỳ actor đang giữ (vd gõ nhầm `Expense.Aprove`) |
| D20 | 🟢 | Không package nào tên `Identity` cho engine phân quyền; tên DI / type tránh trùng ASP.NET (`AddPlatformAuthorization`, `AccessDecision`) |

#### Câu hỏi đã chốt Q1–Q10

| # | Trạng thái | Hiện trạng trong code |
|---|------------|-----------------------|
| Q1 | 🟡 | O3 + O7 đã phản ánh trong cấu trúc package; nhiều D còn 🟡/🔴 (bảng trên) |
| Q2 | 🟡 | `*` hoạt động; `System.All` vẫn được chấp nhận song song (Phụ lục D.4 vẫn ghi `System.All`; seeder chưa seed RoleClaims nên chưa role nào có wildcard) |
| Q3 | 🟢 | `User.TenantId` / `User.OrgId` là cột AspNetUsers; `DepartmentId`, `ApproveLimit` từ UserClaims; `User.Id` = subject |
| Q4 | 🟢 | `Platform.Authorization` (đổi từ `Platform.IdentityAccessManagement` ngày 2026-10-02; folder, namespace, csproj, test `UnitTest/Authorization/`, migration snapshot đã đồng bộ) |
| Q5 | 🟢 | `P_OWNER_OR_SAME_DEPT` seed 2 rule Order 1/2; evaluator OR theo `Order` |
| Q6 | 🔴 | `ManagedProjectIds` chưa resolve từ ProjectMember — đang đọc UserClaim CSV (registry đã parse đúng collection; còn thiếu `OperandSource` dạng truy vấn domain để cắm resolver ProjectMember) |
| Q7 | 🟢 | Có `*` → bỏ qua ABAC (xem ⚠️ V1) |
| Q8 | 🟢 | Per-role path (union) — `AuthorizationEngine` |
| Q9 | 🟡 | Một Host `Lexora.Host` đăng ký đủ 4 khối; chưa map `/api/account/*`, `/api/identity-admin/*` |
| Q10 | 🟡 | Package `Platform.Authentication.Identity` có entity; chưa wire `UserManager`/`SignInManager` (Host tự gọi `AddIdentity<User, Role>()`) |

#### Vấn đề mở cần chốt

| # | Vấn đề | Hệ quả hiện tại | Owner |
|---|--------|-----------------|-------|
| V1 ⚠️ | Q7 (`*` bỏ qua ABAC) mâu thuẫn D8 + Phụ lục D.4 (TenantAdmin/OrgAdmin = `System.All` + SameTenant/SameOrg) | Nếu seed đúng D.4, TenantAdmin/OrgAdmin **truy cập ngoài tenant/org**. Hướng gợi ý: bypass chỉ khi role có wildcard **và không gán policy**, hoặc TenantAdmin/OrgAdmin dùng danh sách permission tường minh | SA / anh Hoàng |
| V2 | Permission chỉ-RBAC khi kèm resource | Role cấp permission nhưng không gán policy → DENY (deny-by-default). Cần chốt: giữ deny, hay cho phép "role không policy = toàn quyền theo RBAC" | SA |
| V3 | C6 với Admin | Admin có `*` nhưng không có policy nên không gán được policy cho role khác | SA |
| V4 | D19 catalog permission | Chưa có nguồn whitelist; cần chốt nơi khai báo (code từng module) trước khi mở API gán permission. Có thể dùng cùng cơ chế đóng góp như operand registry | SA / Dev |
| V5 | ~~Seed policy nghiệp vụ nằm ở platform~~ 🟢 **Đã xử lý 2026-10-02** | Platform `AuthorizationSeeder.AddPlatformDefaults` chỉ seed role `ADMIN`/`TENANT_ADMIN`/`ORG_ADMIN` + policy `P_SAME_TENANT`/`P_SAME_ORG`/`P_OWNER_ONLY`. Sản phẩm đóng góp qua `AddPlatformAuthorizationSeed(...)` (Lexora: `AddLexoraAuthorizationSeed()` — role `DIRECTOR`/`CFO`/`DEPARTMENT_MANAGER`/`EMPLOYEE`, policy `P_SAME_DEPARTMENT`/`P_APPROVE_LIMIT`/`P_MANAGED_PROJECT`/`P_OWNER_OR_SAME_DEPT`). `AuthorizationDataSeeder` kiểm tra mọi rule với operand registry **trước khi ghi** (operand chưa đăng ký → lỗi lúc seed), chỉ thêm bản ghi còn thiếu (theo `NormalizedName` / `Code`), không ghi đè bản ghi đã có. Còn mở: Lexora chưa gọi seed lúc start (chờ migration/AutoMigrate) | SA / Dev |
| V6 🟡 *Proposed* | Login không ghi nhận lần sai mật khẩu (§7.3 A) | Lockout tự động không bao giờ kích hoạt; brute-force không bị chặn | Dev |
| V7 🟡 *Proposed* | IdentityAdmin không kiểm tenant/org (§7.3 B) | TenantAdmin T001 tạo/khoá/xoá/liệt kê user T002 | SA / Dev |
| V8 🟡 *Proposed* | Gỡ policy mở rộng phạm vi không bị kiểm soát (§7.3 C) | Actor gỡ `P_SAME_ORG` khỏi role thấp hơn → role đó vượt Org | SA |
| V9 🟡 *Proposed* | IdentityAdmin không kiểm permission (§7.3 D) | Mọi role có level cao hơn (vd DIRECTOR 700) quản trị được user/role thấp hơn | SA |
| V10 🟡 *Proposed* | Grant-scope xét theo tập hợp, không theo path (§7.3 E) | Actor tạo được role có phạm vi rộng hơn phạm vi của chính mình | SA |
| V11 🟡 *Proposed* | `CachedPolicyProvider` invalidate là no-op (§7.3 F) | Nếu đăng ký DI như hiện tại, policy cũ còn hiệu lực tới 1 giờ | Dev |
| V12 🟡 *Proposed* | API engine dễ dùng sai + so sánh lệch kiểu (§7.3 G) | `AuthorizeAsync(resource: null)` bỏ ABAC; `Compare` trả 0 → `<=`/`>=` đạt; claim đoán kiểu → deny âm thầm | SA / Dev |
| V13 🟡 *Proposed* | Chưa có bootstrap quyền ban đầu (§7.3 H) | Sau migrate không ai có permission; IdentityAdmin không dùng được | SA / Dev |
| V14 🟡 *Proposed* | JWT thiếu `iss`/`aud`, RefreshToken rỗng, hạn token hardcode (§7.3 I) | Lexora phải tắt `ValidateIssuer`/`ValidateAudience` | Dev |

### 7.3 Amendment 2026-10-03 — Phát hiện rà soát code *(🟡 Proposed — chờ duyệt)*

Rà soát 2026-10-03 đối chiếu ADR ↔ code (`Platform.Authorization*`, `Platform.Modules.*`, `Lexora.Infrastructure`, `Sample`, `UnitTest`). Suite `UnitTest.Authorization|Account|IdentityAdmin`: **178/178 xanh** — các lỗi dưới đây **không** bị test hiện có bắt. Mục này **chưa** là quyết định implement: hướng xử lý chỉ là đề xuất; khi được duyệt mới chuyển 🟢 và code theo.

#### 7.3.1 Lỗi / lỗ hổng chưa được ADR ghi nhận

| # | V | Mức | Hiện trạng code | Vị trí | Hướng xử lý đề xuất |
|---|---|---|---|---|---|
| A | V6 | Cao | Sai mật khẩu chỉ `throw`, không gọi `AccessFailedAsync`; thành công không `ResetAccessFailedCountAsync` → `AccessFailedCount` không tăng, lockout chỉ có khi admin khoá tay. §8.8 ghi "Login chặn lockout 🟢" chỉ đúng cho khoá tay | `Platform.Modules.Account/Commands/LoginCommandHandler.cs` | Gọi `AccessFailedAsync` khi sai, reset khi đúng (hoặc dùng `SignInManager.CheckPasswordSignInAsync(lockoutOnFailure: true)`); cấu hình `LockoutOptions` ở Host |
| B | V7 | Cao | `CreateUser` ghi `TenantId`/`OrgId` từ command, không đối chiếu actor; `UpdateUser`/`DeleteUser` chỉ so RoleLevel; `GetUsers` trả mọi tenant | `IdentityAdmin/Commands/CreateUserCommandHandler.cs`, `UpdateUserCommandHandler.cs`, `DeleteUserCommandHandler.cs`, `Queries/GetUsersQueryHandler.cs` | Hiện thực §5.4 mục 4–5 trong `IRoleGovernanceValidator` (vd `CanManageScopeAsync(actor, tenantId, orgId)`): actor có tenant → user đích phải cùng tenant; actor có org → cùng org; Admin global (không tenant) mới vượt tenant. `GetUsers` lọc theo scope actor |
| C | V8 | Cao | Policy **thu hẹp** phạm vi (AND trong role). Gán policy bị C6 chặn, nhưng **gỡ** policy (mở rộng phạm vi) chỉ kiểm C5 → actor level 700 gỡ `P_SAME_ORG` khỏi CFO (650) ⇒ CFO duyệt xuyên Org | `RemovePolicyFromRoleCommandHandler.cs`, `AssignPolicyToRoleCommandHandler.cs` | Đảo ngữ nghĩa C6 cho policy: **gỡ** policy chỉ được khi phạm vi role sau khi gỡ vẫn nằm trong phạm vi actor (thực tế v1: actor phải có `*` hoặc giữ đúng tập policy còn lại của role đó); **gán** policy (thu hẹp) không cần actor giữ policy → đồng thời giải quyết V3 |
| D | V9 | Cao | Không lệnh IdentityAdmin nào kiểm permission; chỉ RoleLevel (C5). DIRECTOR (700) tạo/khoá/xoá user EMPLOYEE, tạo role. §11.4 để việc này cho "tầng endpoint" nhưng chưa có endpoint | Toàn bộ `IdentityAdmin/Commands` | Kiểm permission **trong handler** (không phụ thuộc API có `[Authorize]` hay không): `User.Manage`, `Role.Manage`, `Role.AssignPermission`, `Role.AssignPolicy` — khai báo vào catalog permission (V4/D19) |
| E | V10 | TB | C6 kiểm "actor có permission X" và "actor có policy P" **riêng rẽ** trên toàn bộ role của actor. Actor duyệt chỉ trong phòng (role A: `Expense.Approve` + `P_SAME_DEPARTMENT`) nhưng có `P_SAME_TENANT` ở role B → tạo được role `Expense.Approve` + `P_SAME_TENANT` | `AssignPermissionToRoleCommandHandler.cs`, `AssignPolicyToRoleCommandHandler.cs` | v1: chấp nhận rủi ro và ghi rõ, **hoặc** kiểm theo path: tồn tại role của actor cấp X với tập policy ⊆ tập policy của role đích (role đích hẹp hơn hoặc bằng) |
| F | V11 | TB | `ClearPolicyCaches()` rỗng, `InvalidateCacheAsync` không xoá gì; `GetPoliciesByRoleAsync` (đường engine dùng) không cache | `Platform.Authorization/Services/CachedPolicyProvider.cs` | Không đăng ký DI cho tới Phase 7; khi làm: cache theo `roleId` + key-version/`CancellationChangeToken` để invalidate được; IdentityAdmin gọi invalidate sau mọi lệnh ghi |
| G | V12 | TB | (1) `AuthorizeAsync(userId, perm, resource: null)` trả RBAC-only kể cả khi role có policy → quên truyền resource là lọt ABAC. (2) `PolicyEvaluator.Compare` trả 0 khi không `IComparable` → `<=`/`>=` đạt (fail-open). (3) `OperandResolver.ParseScalar` đoán kiểu Guid → decimal → string; resource `int`/`long`/`string` lệch kiểu → deny âm thầm | `AuthorizationEngine.cs`, `PolicyEvaluator.cs`, `Platform.Authorization.EntityFramework/Services/OperandResolver.cs` | (1) Tách rõ `HasPermissionAsync` (RBAC) và `AuthorizeAsync(resource bắt buộc)`, hoặc `AuthorizeAsync(null)` deny khi role cấp permission có policy — gộp quyết định với V2. (2) `Compare` không so được → rule không đạt. (3) Khai báo kiểu trong `OperandDefinition` (Guid/decimal/string) → parse theo kiểu, validator chặn `<`/`>` trên Guid/string (T37) |
| H | V13 | TB | Seeder không seed RoleClaims và không tạo user nào → sau migrate `ADMIN` không có `*`, không actor nào chạy được IdentityAdmin | `AuthorizationSeeder.cs`, Lexora Host | Seed RoleClaim `ADMIN` = `*`; tạo admin đầu tiên bằng lệnh/endpoint bootstrap một lần (mật khẩu từ user-secrets/env, không commit) — chốt cơ chế cụ thể khi duyệt |
| I | V14 | Thấp | JWT không có `iss`/`aud`; `RefreshToken = ""`; hạn 1 giờ hardcode; Lexora `appsettings.json` tắt `ValidateIssuer`/`ValidateAudience` | `LoginCommandHandler.cs`, `Lexora.Host/appsettings.json` | Phát `iss`/`aud` theo `AuthenticationJwtOption`, bật lại validate; hạn token lấy từ config; RefreshToken để phase Account API |

Ghi nhận phụ (không tạo V riêng): `OperandResolver.ResolveUserClaimAsync` dùng `FirstOrDefault` → user có 2 claim cùng type cho kết quả không xác định; `UpdateProfile` đổi email không đặt lại `EmailConfirmed`; `GetUsersQueryHandler` N+1 (`GetRolesAsync` từng user).

#### 7.3.2 Sample — thiếu so với vai trò "code mẫu cho dev sản phẩm"

| # | Thiếu | Hệ quả | Đề xuất |
|---|---|---|---|
| S-1 | User lấy từ header `X-Demo-User`, controller `[AllowAnonymous]` | Không có mẫu lấy userId từ `ICurrentUser`/JWT, không có mẫu `[Authorize]` + permission | Thêm biến thể dùng xác thực thật (sau khi có D14 `IAuthorizationHandler`) |
| S-2 | Logic + `DbContext` nằm trong controller | Trái quy ước "controller mỏng / CQRS handler" của sản phẩm — dev copy sẽ sai tầng | Mẫu Query/Command handler (Application) gọi `IDataScopeFilter` qua repository; controller chỉ dispatch |
| S-3 | Không có mẫu ghi: Create / Update / Delete | Không minh hoạ "bản ghi mới phải nằm trong phạm vi", "Update kiểm cả trước và sau khi đổi `DepartmentId`" | Thêm 3 endpoint mẫu + kịch bản AZ tương ứng |
| S-4 | `GET summary` không kiểm RBAC trước | Không quyền → 200 rỗng, lệch quy ước §5.11.2 (403) | Thêm `HasPermissionAsync` như `List` |
| S-5 | Không demo Account / IdentityAdmin | Chưa có E2E login → JWT → gọi API, gán permission/policy + governance (Phase 8) | Thêm khi có API Account / IdentityAdmin |
| S-6 | Bộ role `SAMPLE_*` riêng, thiếu kịch bản | Không có OR-policy (`P_OWNER_OR_SAME_DEPT`), role có permission không policy (V2), TenantAdmin `*` + `P_SAME_TENANT` (V1), thiếu thuộc tính | Bổ sung AZ-14+ theo Phụ lục D |
| S-7 | Không minh hoạ phối hợp với query filter tenant của Platform Multitenancy | Chưa rõ `P_SAME_TENANT` và tenant filter chồng nhau thế nào | Thêm ghi chú + 1 endpoint mẫu |

---

## 8. Test cases

Quy ước icon: 🟢 có test tự động, đang xanh và đạt đúng Expect · 🟡 suite regression có sẵn, cần giữ xanh · 🔴 chưa có test (ghi chú nếu logic đã có).  
Dữ liệu seed tham chiếu **[Phụ lục D](#phụ-lục-d--dữ-liệu-mẫu-theo-cấu-hình)** (`P001`–`P008`, `PR01`–`PR09`).  
*Cập nhật trạng thái 2026-10-02.* Đường dẫn test viết tắt: `Authorization/` = `erp_core/platform/UnitTest/Authorization/`, `Account/` = `UnitTest/Account/`, `IdentityAdmin/` = `UnitTest/IdentityAdmin/`. Test cũ trong `AuthorizationEngineTests` / `PolicyEvaluatorTests` có tên T2, T5, T77… **không trùng nghĩa** với mã ở đây; test viết từ 2026-10-02 ghi mã §8 trong `DisplayName`.

### 8.1 Smoke *(≥ 1)*

| # | Case | Expect | Nơi (file / filter) | Trạng thái |
|---|------|--------|---------------------|------------|
| T1 | Host đăng ký Authentication + Authorization + Account (+ IdentityAdmin); permission check cơ bản | Không throw; `HasPermission` đúng với RoleClaim seed | `Authorization/AuthorizationEngineTests.T1_HostRegistrationSucceeds` (chỉ DI Authorization) · `Authorization/PermissionProviderTests` | 🔴 Mới có smoke DI cho Authorization + HasPermission trên RoleClaim; chưa có smoke compose đủ 4 khối |
| T1b | Seed Policies + PolicyRules chuẩn load được qua `IPolicyProvider` | `P_SAME_ORG`, `P_APPROVE_LIMIT`… resolve đủ rules | `Authorization/AuthorizationSeederTests.T1b_*` | 🟢 |

### 8.2 Regression

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T2 | Authentication schemes (JWT/ApiKey/Basic) không bị phá khi thêm Authorization | Suite `UnitTest/Authentication` xanh | `UnitTest/Authentication` | 🟡 Xanh trừ `CFG_01_Sample_appsettings_has_no_plaintext_api_keys` (appsettings Sample — không liên quan Authorization) |
| T3 | Multitenancy ambient / current user không đổi semantic ngoài claim mới | Suite Multitenancy / CurrentUser liên quan xanh | `UnitTest/Multitenancy`, `UnitTest/Services/CurrentUserTenantTests` | 🟡 Xanh (2026-10-02) |

### 8.3 Runtime AuthZ (RBAC + ABAC evaluate)

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T4 | RBAC deny → 403, không chạy ABAC | Fail-fast | `Authorization/AuthorizationEngineTests.T4_RbacDenyShortCircuits` | 🟢 (engine; 403 HTTP chưa có vì chưa tích hợp endpoint) |
| T5 | Expense.Approve: SameOrg + ApproveLimit PASS/FAIL (Phụ lục D.9 EXP-001…003) | Đúng bảng quyết định | `Authorization/AuthorizationEnginePerRolePathTests` (`Cfo_*`) | 🟢 |
| T6 | Employee OwnerOnly OR SameDepartment (CUS-OWN / CUS-DEPT / CUS-OUT) | PASS / PASS / DENY | `Authorization/AuthorizationEnginePerRolePathTests.OwnerOrSameDept_RealStore` | 🟢 |
| T10 | List query áp dụng Data Scope (Org / Owner∨Dept) | SQL filter đúng; không load all | `Authorization/DataScopeFilterTests` (16) · Sample `api/_authz-demo/scope-check` | 🟢 SQL WHERE tham số hoá; danh sách = quyết định engine từng bản ghi |

### 8.4 IdentityAdmin / Account / boundary

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T7 | Role Governance: RoleLevel và grant Permission/Policy ngoài scope → Forbidden | Deny escalation | `Authorization/RoleGovernanceValidatorTests` · `IdentityAdmin/GovernanceEnforcementTests` · `IdentityAdmin/AssignPermissionEscalationTests` · `*_target_role_not_lower` | 🟢 (RoleLevel + C6; tenant/org scope §5.4 mục 4–5 chưa có) |
| T8 | System Role không sửa được Permission/Policy qua API | Reject | — | 🔴 Handler đã chặn `IsSystemRole`; chưa có test, chưa có API |
| T11 | Account: Login / Logout / Change / Forgot password happy path | Token hợp lệ; không reference Authorization | `Account/LoginCommandHandlerTests` | 🔴 Mới có Login (token, lockout, tenant claim); Logout / Change / Forgot chưa có test |
| T12 | IdentityAdmin: CRUD User lock/unlock + CRUD Role + gán Permission | Đúng Permission; governance | `IdentityAdmin/CreateUserCommandHandlerTests` · `GovernanceEnforcementTests` (lock/unlock, delete, create role) · `AssignPermissionToRoleCommandHandlerTests` | 🟢 (tầng handler; chưa có API) |
| T13 | Account csproj **không** ProjectReference `Platform.Authorization` | Enforce boundary | Arch / csproj test | 🔴 (csproj hiện đúng; chưa có test tự động) |

### 8.5 Cấu hình Policy & PolicyRule *(bắt buộc cho seed + admin config)*

Phạm vi: CRUD/validate/seed/cache của bảng `Policies` / `PolicyRules`; gán Policy vào Role; **không** thay thế runtime evaluate (đã ở §8.3).

#### 8.5.1 Seed & đọc Policy definition

| # | Case | Input / bước | Expect | Trạng thái |
|---|------|--------------|--------|------------|
| T20 | Seed idempotent Policies P001–P008 | Chạy seed 2 lần | Không duplicate `Code`; đủ 8 policy `IsSystemPolicy=true` | 🔴 Test idempotent xanh nhưng seed mới **7/8** (thiếu `P_MANAGED_DEPARTMENT`) — `Authorization/AuthorizationSeederTests.T20_*`; 7 = 3 platform + 4 đóng góp nghiệp vụ (V5, test dùng `Helpers/TestSeeds` mô phỏng Lexora) |
| T21 | Seed idempotent PolicyRules PR01–PR09 | Chạy seed 2 lần | Mỗi Policy đủ rules; `Order` đúng Phụ lục D.2 | 🔴 Test xanh với **8/9** rule (thiếu PR07); PR06 seed là `DepartmentId IN ManagedDepartmentIds`, khác D.2 |
| T22 | `IPolicyProvider.GetAsync("P_SAME_ORG")` | Code `P_SAME_ORG` | Trả Policy + 1 rule `Resource.OrgId == User.OrgId` | 🟢 `T22_*` |
| T23 | `GetAsync` policy không tồn tại | Code `P_UNKNOWN` | `null` / NotFound — không throw unhandled | 🟢 `T23_*` |
| T24 | List policies (admin/read) | Gọi list | Có đủ Code seed; filter `IsSystemPolicy` đúng | 🟢 `T24_*` (đủ theo seed hiện tại — xem T20) |
| T25 | Get Policy kèm Rules theo `Order` | `P_OWNER_OR_SAME_DEPT` | Rules Order 1 = Owner, Order 2 = SameDept | 🟢 `T25_*` |

#### 8.5.2 Validate khi tạo / cập nhật Policy

| # | Case | Input | Expect | Trạng thái |
|---|------|-------|--------|------------|
| T26 | Tạo Policy Code hợp lệ (custom, non-system) | Code unique, Name, optional ResourceType/Action | 201/OK; persist | 🔴 Chưa có API (Phase 4b) |
| T27 | Tạo Policy trùng `Code` | Code = `P_SAME_ORG` | 409 / validation fail | 🔴 (DB có unique index `Code`) |
| T28 | Code rỗng / invalid format | `""`, space, quá dài | Reject | 🔴 |
| T29 | Sửa metadata Policy non-system | Đổi Name / Description | OK; Rules giữ nguyên nếu không gửi | 🔴 |
| T30 | Sửa / xóa **system** Policy (`IsSystemPolicy=true`) | Update/Delete `P_SAME_ORG` | **Forbidden / Reject** (C7, D.10) | 🔴 |
| T31 | Xóa Policy custom **đang được RoleClaim tham chiếu** | Role có `Policy=P_CUSTOM_X` | Reject hoặc cascade policy rõ (chốt: **Reject** v1) | 🔴 (nếu lỡ xoá, engine fail-closed path của role — xem §8.8) |
| T32 | Xóa Policy custom không còn tham chiếu | Không RoleClaim trỏ tới | Xóa Policy + Rules | 🔴 |

#### 8.5.3 Validate khi tạo / cập nhật PolicyRule

| # | Case | Input | Expect | Trạng thái |
|---|------|-------|--------|------------|
| T33 | Thêm rule hợp lệ | Left=`Resource.OrgId`, Op=`==`, Right=`User.OrgId` | OK; gắn đúng PolicyId | 🔴 Validator chấp nhận (`Authorization/OperandValidatorTests.T9a_*`); chưa có lệnh lưu rule |
| T34 | Operand ngoài whitelist | Left=`Resource.Password` hoặc `User.Password` | **Reject** (T9 mở rộng) | 🟢 `Authorization/OperandValidatorTests.T34_*` (tầng validator) |
| T35 | Operand không thuộc `Resource.*` / `User.*` | `Env.Time`, `Foo.Bar` | Reject | 🟢 `T35_*` (tầng validator) |
| T36 | Operator ngoài whitelist | `LIKE`, `REGEX`, `&&` | Reject | 🟢 `T36_*` (tầng validator) |
| T37 | Operator so sánh trên operand không numeric | `Resource.OrgId` `<=` `User.OrgId` | Reject hoặc cảnh báo theo rule (v1: **Reject** nếu typed metadata có) | 🔴 Chưa có typed metadata |
| T38 | `IN` với Right không phải collection attribute | Right=`User.OrgId` (scalar) | Reject; Right phải `User.ManagedDepartmentIds` / tương đương whitelist | 🟢 `T38_*`, `T38b_*` (tầng validator) |
| T39 | Rule thiếu Left/Operator/Right | null/empty | Reject | 🔴 Validator đã reject chuỗi rỗng; chưa có test |
| T40 | `Order` trùng trong cùng Policy | Hai rule cùng Order=1 | Reject hoặc auto-renumber (v1: **Reject**) | 🔴 DB có unique (PolicyId, Order); chưa validate trước khi lưu, chưa có test |
| T41 | Cập nhật rule system Policy | Sửa PR02 của `P_SAME_ORG` | **Reject** (system immutable) | 🔴 |
| T42 | Xóa rule khỏi Policy custom | Delete một PolicyRule | OK; evaluate chỉ còn rules còn lại | 🔴 |
| T43 | Policy không còn rule nào | Xóa hết rules rồi evaluate | Deny / invalid Policy (v1: **không cho save** Policy active 0 rule) | 🔴 Evaluate đã deny policy 0 rule (`PolicyEvaluatorTests.T80_EmptyPolicyFails`); chưa chặn khi save |
| T44 | Thêm rule vào Policy OR (`P_OWNER_OR_SAME_DEPT`) | Thêm nhánh 3 hợp lệ | OK nếu custom; system → Reject | 🔴 |

#### 8.5.4 Gán Policy vào Role (RoleClaim) — cấu hình liên quan

| # | Case | Input | Expect | Trạng thái |
|---|------|-------|--------|------------|
| T45 | Gán Policy tồn tại vào Role business | `R_CFO` ← `P_APPROVE_LIMIT` | RoleClaim `ClaimType=Policy`, Value=`P_APPROVE_LIMIT` | 🟢 `IdentityAdmin/AssignPolicyToRoleCommandHandlerTests.HandleAsync_adds_policy_claim_when_actor_has_scope` |
| T46 | Gán Policy **không tồn tại** | Value=`P_NOPE` | Reject | 🟢 `HandleAsync_throws_when_policy_code_not_found` |
| T47 | Gán trùng Policy cùng Role | Gán 2 lần `P_SAME_ORG` | Idempotent hoặc Reject duplicate | 🔴 Handler idempotent; chưa có test (bản Permission có: `AssignPermissionToRoleCommandHandlerTests.HandleAsync_is_idempotent_*`) |
| T48 | Actor không có Policy trong grant scope | OrgAdmin gán `P_SAME_TENANT` dù không có | **Forbidden** (C6) | 🟢 `HandleAsync_throws_unauthorized_when_actor_lacks_policy_scope` |
| T49 | Gán Policy vào System Role | `R_ORG_ADMIN` thêm Policy | **Reject** (T8) | 🔴 Handler đã chặn `IsSystemRole`; chưa có test |
| T50 | Gỡ Policy khỏi Role business | Remove RoleClaim Policy | OK; evaluate không còn policy đó | 🟢 `IdentityAdmin/GovernanceEnforcementTests.RemovePolicy_from_lower_role_allowed` (gỡ claim; chưa kiểm evaluate sau gỡ) |
| T51 | Đổi ApproveLimit UserClaim, giữ nguyên Policy gán | U_CFO 10M → 5M; EXP 8M | RBAC+Policy same; evaluate → DENY | 🔴 Đúng theo thiết kế (resolver đọc DB mỗi lần, không cache); chưa có test riêng |

#### 8.5.5 Compile / cache sau cấu hình

| # | Case | Input / bước | Expect | Trạng thái |
|---|------|--------------|--------|------------|
| T52 | Sau update PolicyRule, cache Policy invalid | Sửa custom rule rồi `GetAsync` | Trả definition mới; evaluate dùng rule mới | 🔴 `CachedPolicyProvider` chưa đăng ký DI |
| T53 | Sau gán/gỡ Policy RoleClaim, snapshot user invalid | Đổi RoleClaim CFO | Request sau không còn/ còn policy cũ | 🔴 Chưa có snapshot |
| T54 | Concurrent read khi seed | Nhiều `GetAsync` song song sau seed | Kết quả ổn định, không partial rules | 🔴 |

#### 8.5.6 Ma trận evaluate theo Policy đã cấu hình (seed Phụ lục D)

Mỗi case: load Policy/Rules từ store (không hard-code rule trong test) rồi evaluate. Nơi: `Authorization/AuthorizationEnginePerRolePathTests` (store InMemory + seeder thật + resolver thật). `Authorization/PolicyEvaluatorTests` có T60/T61/T74–T78 nhưng dùng resolver **mock** và rule hard-code → không tính.

| # | PolicyCode | Actor (Phụ lục D) | Resource | Expect | Trạng thái |
|---|------------|-------------------|----------|--------|------------|
| T60 | P_SAME_TENANT | U_TA (T001) | TenantId=T001 | PASS | 🟢 `SameTenant_UsesTenantColumn` |
| T61 | P_SAME_TENANT | U_TA | TenantId=T002 | FAIL | 🟢 `SameTenant_UsesTenantColumn` |
| T62 | P_SAME_ORG | U_CFO (O001) | OrgId=O001 | PASS | 🟢 `OrgIdClaim_IsIgnored` (P_SAME_ORG riêng) |
| T63 | P_SAME_ORG | U_CFO | OrgId=O002 | FAIL | 🟢 `OrgIdClaim_IsIgnored` |
| T64 | P_SAME_DEPARTMENT | U_TM (D-TECH) | Dept=D-TECH | PASS | 🔴 Chỉ phủ gián tiếp qua chuỗi DeptMgr (T79) |
| T65 | P_SAME_DEPARTMENT | U_TM | Dept=D-SALES | FAIL | 🔴 Chỉ phủ gián tiếp qua chuỗi DeptMgr (T80) |
| T66 | P_OWNER_ONLY | U_SE | CreatedBy/EmployeeId=U_SE | PASS | 🟢 `OwnerOnly_UsesSubjectId` |
| T67 | P_OWNER_ONLY | U_SE | CreatedBy=U_OTHER | FAIL | 🟢 `OwnerOnly_UsesSubjectId` |
| T68 | P_APPROVE_LIMIT | U_CFO (limit 10M) | Amount=8M | PASS | 🟢 `Cfo_SameOrg_WithinLimit_Allows` (qua role CFO, SameOrg đạt) |
| T69 | P_APPROVE_LIMIT | U_CFO | Amount=15M | FAIL | 🟢 `Cfo_SameOrg_OverLimit_Denies` |
| T70 | P_APPROVE_LIMIT | U_DM (limit 2M) | Amount=1.5M | PASS | 🟢 `DeptMgr_AllThreePoliciesPass_Allows` |
| T71 | P_APPROVE_LIMIT | U_DM | Amount=8M | FAIL | 🔴 |
| T72 | P_MANAGED_PROJECT | U_TM | Dept=D-TECH | PASS | 🔴 Seed rule khác D.2 (dùng `ManagedDepartmentIds` từ claim; Q6 chưa resolve từ domain). Rule seed hiện tại đã evaluate được nhờ collection operand: `AuthorizationEnginePerRolePathTests.ManagedProject_InCollection` |
| T73 | P_MANAGED_PROJECT | U_TM | Dept=D-SALES | FAIL | 🔴 |
| T74 | P_OWNER_OR_SAME_DEPT | U_EMP | Owner khác Dept | PASS (OR) | 🟢 `OwnerOrSameDept_RealStore` |
| T75 | P_OWNER_OR_SAME_DEPT | U_EMP | SameDept không owner | PASS (OR) | 🟢 `OwnerOrSameDept_RealStore` |
| T76 | P_OWNER_OR_SAME_DEPT | U_EMP | Không owner, khác Dept | FAIL | 🟢 `OwnerOrSameDept_RealStore` |
| T77 | P_SAME_ORG **AND** P_APPROVE_LIMIT (Role CFO) | U_CFO | Org=O001, Amount=8M | PASS | 🟢 `Cfo_SameOrg_WithinLimit_Allows` |
| T78 | P_SAME_ORG **AND** P_APPROVE_LIMIT | U_CFO | Org=O002, Amount=1M | FAIL (Org) | 🟢 `Cfo_OtherOrg_WithinLimit_Denies` |
| T79 | P_SAME_ORG + P_SAME_DEPARTMENT + P_APPROVE_LIMIT (DeptMgr) | U_DM | Org=O001, Dept=D-ACC, 1.5M | PASS | 🟢 `DeptMgr_AllThreePoliciesPass_Allows` |
| T80 | Chuỗi AND DeptMgr | U_DM | Org=O001, Dept=D-TECH, 1M | FAIL (Dept) | 🟢 `DeptMgr_OtherDepartment_Denies` |

### 8.6 T9 (giữ) — validator tổng

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T9 | Policy/PolicyRule validator reject operand/operator ngoài whitelist | Reject on save; cover T34–T36 | `Authorization/OperandRegistryTests` · `Authorization/OperandValidatorTests` · `Authorization/OperandResolverTests.D10_*` | 🟢 tầng validator + resolver lúc evaluate (chưa có luồng save gọi validator) |

### 8.7 Done khi

- Smoke T1–T1b + Regression T2–T3 🟢.
- Runtime T4–T6, T10 🟢.
- Config Policy/Rule: **T20–T25, T30, T33–T36, T41, T45–T46, T48, T52, T60–T80** tối thiểu 🟢 trước khi coi AuthZ seed+config xong.
- T26–T32, T37–T44, T47, T49–T51, T53–T54 🟢 khi mở API cấu hình custom Policy (không chỉ seed).
- IdentityAdmin/Account T7–T8, T11–T13 🟢 theo phase module.
- Không 🟢 Implemented khi thiếu smoke/regression.

**Tình trạng so với Done khi (2026-10-02, T10 cập nhật 2026-10-03):** chưa đạt — còn 🔴: T1; T20–T21 (seed thiếu); T30, T33, T41, T52; T64–T65, T71–T73; T8, T11, T13. T2 cần xử lý `CFG_01` (ngoài phạm vi ADR) để suite Authentication xanh hoàn toàn.

### 8.8 Test bổ sung ngoài danh mục (2026-10-02)

Viết khi sửa lỗi phát hiện lúc rà soát; giữ xanh như regression.

| Chủ đề | Expect | Nơi | Trạng thái |
|--------|--------|-----|------------|
| Wildcard theo quyền **đang có** (D8, Q2) | User không có `*` hỏi `*`/`System.All` → DENY; user có `*` → mọi permission | `Authorization/PermissionProviderTests` · `AuthorizationEnginePerRolePathTests.HeldWildcard_*`, `RequestedWildcard_*` | 🟢 |
| Grant `*` không leo thang (C6) | Actor không có `*` không gán được `*`/`System.All` | `IdentityAdmin/AssignPermissionEscalationTests` | 🟢 |
| Multi-role per-role path (Q8) | 1 role path đạt là đủ; role không cấp permission không đóng góp path | `AuthorizationEnginePerRolePathTests.MultiRole_*`, `RoleWithoutPermission_*` | 🟢 |
| Mã policy không tồn tại | Provider ném `UnresolvedPolicyException`; path fail-closed; role hợp lệ khác vẫn đạt | `AuthorizationEnginePerRolePathTests.PolicyProvider_UnknownCode_*`, `UnknownPolicyCode_*` | 🟢 |
| Thuộc tính null | User thiếu OrgId vs resource thiếu OrgId → DENY | `AuthorizationEnginePerRolePathTests.NullAttributes_DoNotMatch` | 🟢 |
| Nguồn `User.*` (Q3) | `User.Id` = subject; TenantId/OrgId từ cột; claim cũ bị bỏ qua | `Authorization/OperandResolverTests.S7_*` · `OrgIdClaim_IsIgnored` | 🟢 |
| Login chặn lockout | User bị khoá không login được dù đúng mật khẩu | `Account/LoginCommandHandlerTests` | 🟢 (chỉ khoá tay; lockout tự động khi sai mật khẩu chưa có — V6, T90–T91) |
| Tenant claim trong JWT | Login phát claim theo `TenantClaimName`; JwtBearer đọc lại đúng | `Account/LoginCommandHandlerTests.HandleAsync_emits_tenant_claim_*` | 🟢 |
| Governance cho revoke / lock / delete / create role (C5) | Actor cấp thấp hơn/bằng → Forbidden | `IdentityAdmin/GovernanceEnforcementTests` | 🟢 |
| Operand registry (C7, D10) | Platform chỉ có operand generic; module đóng góp operand nghiệp vụ; trùng khác định nghĩa → lỗi lúc start; DI gom mọi đóng góp | `Authorization/OperandRegistryTests` | 🟢 |
| Whitelist cưỡng chế lúc evaluate (D10) | Operand chưa đăng ký bị từ chối dù resource có property; policy nghiệp vụ fail-closed khi host chưa đăng ký operand | `Authorization/OperandResolverTests.D10_*` · `AuthorizationEnginePerRolePathTests.BusinessPolicy_FailsClosedWithoutOperandRegistration` | 🟢 |
| Collection operand | CSV claim parse thành Guid → `IN` khớp resource Guid; `IN` chỉ nhận operand đăng ký là collection | `OperandResolverTests.CollectionClaim_ParsedAsGuids` · `OperandValidatorTests.T38c_*` · `ManagedProject_InCollection` | 🟢 |

### 8.9 Test bổ sung theo Amendment 2026-10-03 *(🟡 Proposed — chờ duyệt)*

Viết cùng lúc sửa V6–V14 (§7.3). Cột "Hiện tại" = kết quả dự kiến nếu chạy trên code 2026-10-03: **đỏ** nghĩa là test sẽ làm lộ lỗi.

| # | Case | Expect | Nơi đề xuất | V | Hiện tại |
|---|------|--------|-------------|---|----------|
| T90 | Sai mật khẩu N lần (theo `LockoutOptions.MaxFailedAccessAttempts`) | Lần N+1 bị khoá dù đúng mật khẩu; `AccessFailedCount` tăng từng lần | `Account/LoginCommandHandlerTests` | V6 | đỏ |
| T91 | Login đúng sau vài lần sai | `AccessFailedCount` reset về 0 | `Account/LoginCommandHandlerTests` | V6 | đỏ |
| T92 | TenantAdmin T001 `CreateUser` với `TenantId = T002` | Forbidden | `IdentityAdmin/TenantScopeTests` | V7 | đỏ |
| T93 | TenantAdmin T001 khoá / xoá user T002 (level thấp hơn) | Forbidden | `IdentityAdmin/TenantScopeTests` | V7 | đỏ |
| T94 | OrgAdmin O001 thao tác user O002 cùng tenant | Forbidden | `IdentityAdmin/TenantScopeTests` | V7 | đỏ |
| T95 | `GetUsers` bởi TenantAdmin | Chỉ trả user cùng tenant | `IdentityAdmin/TenantScopeTests` | V7 | đỏ |
| T96 | Actor không có `*` gỡ `P_SAME_ORG` khỏi role CFO (role còn `P_APPROVE_LIMIT`) | Forbidden (mở rộng phạm vi) | `IdentityAdmin/PolicyScopeTests` | V8 | đỏ |
| T97 | Admin (`*`, không policy) gán `P_SAME_ORG` cho role business | OK (gán = thu hẹp) | `IdentityAdmin/PolicyScopeTests` | V8, V3 | đỏ |
| T98 | Actor level cao nhưng thiếu `User.Manage` tạo / khoá / xoá user | Forbidden | `IdentityAdmin/PermissionGateTests` | V9 | đỏ |
| T99 | Actor thiếu `Role.Manage` / `Role.AssignPermission` / `Role.AssignPolicy` | Forbidden | `IdentityAdmin/PermissionGateTests` | V9 | đỏ |
| T100 | Actor chỉ có `Expense.Approve` + `P_SAME_DEPARTMENT` tạo role `Expense.Approve` + `P_SAME_TENANT` | Theo quyết định V10 (Forbidden nếu chọn kiểm theo path) | `IdentityAdmin/AssignPermissionEscalationTests` | V10 | đỏ nếu chọn kiểm theo path |
| T101 | Sửa rule / gỡ policy rồi đọc lại qua provider có cache | Trả definition mới (gộp với T52–T53) | `Authorization/CachedPolicyProviderTests` | V11 | đỏ |
| T102 | `AuthorizeAsync(resource: null)` khi role cấp permission có policy | Theo quyết định V2/V12 (chốt DENY hoặc tách API) | `Authorization/AuthorizationEngineTests` | V12 | hiện ALLOW — cần chốt |
| T103 | So sánh `<=` trên kiểu không `IComparable` / lệch kiểu | Rule không đạt | `Authorization/PolicyEvaluatorTests` | V12 | đỏ (`Compare` trả 0) |
| T104 | Resource `Amount` kiểu `int`/`long` với claim `ApproveLimit` decimal; `DepartmentId` string với claim | Đạt theo kiểu khai báo của operand (sau khi có typed operand) | `Authorization/OperandResolverTests` | V12, T37 | deny âm thầm |
| T105 | V1: TenantAdmin `*` + `P_SAME_TENANT`, resource tenant khác | Theo quyết định V1 (ghi nhận hành vi hiện tại: ALLOW) | `Authorization/AuthorizationEnginePerRolePathTests` | V1 | ALLOW — cần chốt |
| T106 | Seed: role `ADMIN` có RoleClaim `*`; bootstrap admin chạy 1 lần, idempotent | Có đúng 1 admin; chạy lại không tạo thêm | `Authorization/AuthorizationSeederTests` | V13 | đỏ |
| T107 | Token Login có `iss`/`aud` khớp cấu hình; JwtBearer bật validate đọc được | Validate pass; sai `aud` → reject | `Account/LoginCommandHandlerTests` | V14 | đỏ |
| T108 | Data Scope chạy trên PostgreSQL thật (Testcontainers) — đối chiếu engine từng bản ghi, có null | Khớp 100% | `UnitTest` integration (hoặc project integration riêng) | T10 | chưa có (Sample kiểm tay 21/21) |
| T109 | Lexora: seed `AddLexoraAuthorizationSeed` hợp lệ với `AddLexoraAuthorizationOperands` | `AuthorizationDataSeeder.SeedAsync` không throw | `lexora/tests/Lexora.Application.Tests` (hoặc Infrastructure test) | V5 | chưa có |
| T110 | Lexora Host compose: resolve `IAuthorizationEngine`, `IDataScopeFilter`, handler Account/IdentityAdmin | Không throw (smoke T1 đầy đủ) | `lexora/tests` (WebApplicationFactory) | T1 | chưa có |

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 (Q1–Q10) | Status → Accepted | — |
| 1 | `Platform.Authorization` (+ `Platform.Authentication.Identity`): model `Role`, `User`, `Policy`, `PolicyRule` + EF mapping | Build xanh | 0 |
| 2 | Seed System Roles + Policies/Rules chuẩn | T20–T25, T1b 🟢 | 1 |
| 3 | Permission check + Policy provider/evaluator + operand whitelist | T4–T5, T9, T33–T36, T60–T71 🟢 | 1–2 |
| 4 | Policy Group OR (Employee) + Data Scope query helpers | T6, T10, T74–T76 🟢 | 3 |
| 4b | API cấu hình Policy/Rule (nếu cho custom) + system immutable | T26–T32, T41–T43, T52 🟢 | 2–3 |
| 5 | Module **Account**: Login, Logout, Profile, Forgot/Change password | T11, T13 | Auth hiện có |
| 6 | Module **IdentityAdmin**: CRUD User/Role, gán Permission/Policy + governance | T7–T8, T12, T45–T51, T77–T80 🟢 | 2–3 |
| 7 | Authorization snapshot cache + invalidation | T52–T54 🟢 | 3 |
| 8 | Sample Host compose đủ 4 khối + docs + ADR index | Demo end-to-end + Phụ lục D | 3–7 |
| 9 | (Later) Expression tree / DSL; wire `UserManager`/`SignInManager` trong satellite `Authentication.Identity` (entity đã có — Q10) | ADR riêng nếu cần | — |

---

## 10. Checklist Done

*Cập nhật 2026-10-02 theo code thực tế (chi tiết từng file: §11).*

| # | Việc | Trạng thái | Ghi chú |
|---|------|------------|---------|
| 1 | Confirm §7 | 🟢 | Accepted 2026-09-29 |
| 2 | `Platform.Authorization` + Identity extension | 🟢 | Engine `Platform.Authorization` (không EF) + store EF `Platform.Authorization.EntityFramework` + `Platform.Authentication.Identity` (`User`, `Role`); migration `InitialIdentitySchema` đã tạo, **chưa áp DB** |
| 3 | Seed System Role/Policy (+ T20–T25) | 🟡 | Seeder + T20–T25 + 6 test V5 xanh; seed tách platform / sản phẩm (V5); Sample gọi seed lúc start; thiếu `P_MANAGED_DEPARTMENT`, chưa seed RoleClaims, Lexora chưa gọi lúc start |
| 4 | RBAC + ABAC runtime + Data Scope | 🟡 | Engine 🟢 (per-role path AND/OR, wildcard, fail-closed khi thiếu policy/thuộc tính); Data Scope 🟢 (SQL WHERE, 2026-10-03); endpoint nghiệp vụ chưa gọi engine / data scope |
| 5 | Cấu hình/validate Policy & PolicyRule (§8.5) | 🟡 | Validator whitelist có; chưa có API CRUD custom Policy (Phase 4b) |
| 6 | Module Account (chức năng 4–8) | 🟡 | Handler đủ 4–8 (lockout, tenant claim); 🔴 API `/api/account/*`, email thật, revoke token |
| 7 | Module IdentityAdmin (chức năng 1–3 gán) | 🟡 | Handler đủ + governance; 🔴 API `/api/identity-admin/*`, catalog permission (D19), sửa/xoá Role |
| 8 | Role Governance | 🟡 | C5 (RoleLevel) + C6 (grant-scope) 🟢 trên mọi lệnh; tenant/org scope (§5.4 mục 4–5) 🔴 |
| 9 | §8 Smoke + Regression (+ test mới khi implement) | 🟡 | 178 test Authorization/Account/IdentityAdmin xanh. Chạy full `UnitTest` 2026-10-03: 435/446 — T2/T3 (Authentication, Multitenancy, CurrentUser) xanh trừ `CFG_01` (plaintext API key trong appsettings Sample); 10 fail còn lại thuộc Workflow, SignalR, ORM/Dapper (cần PostgreSQL) — ngoài phạm vi ADR này |
| 10 | Sample demo + [README index](./README.md) | 🟡 | Sample có demo phân quyền `api/_authz-demo` (engine + EF store + operand/seed đóng góp, Postgres thật: 13/13 kịch bản, Data Scope 21/21) và code mẫu endpoint `api/_authz-sample/expenses` (§5.11.2, §11.8); chưa compose Account/IdentityAdmin; README platform đã đồng bộ 2026-10-02 |

---

## 11. Project Structure — Cấu trúc triển khai

> **Cập nhật 2026-10-02** theo code thực tế (thay cho cấu trúc dự kiến ban đầu). Icon: 🟢 xong · 🟡 có nhưng chưa đủ · 🔴 chưa làm.
> Khác với bản dự kiến: (1) module đặt trong `erp_core/platform/modules/identity/` và đặt tên `Platform.Modules.*` (không dùng `Lexora.*` — module platform không mang tên sản phẩm); (2) `ApplicationRole` đổi thành `Role` và cùng `User` chuyển sang satellite `Platform.Authentication.Identity` (Q10) để Account không phải reference engine phân quyền (C12/T13); (3) mỗi module hiện là **một project** (chưa tách Domain / Application / Infrastructure / Api); (4) migration nằm ở `Lexora.Infrastructure`, không ở framework; (5) engine phân quyền tên `Platform.Authorization` (Q4 amendment 2026-10-02 — trước đó `Platform.IdentityAccessManagement`); (6) phần EF của phân quyền tách thành `Platform.Authorization.EntityFramework` để core, Account, IdentityAdmin không phụ thuộc EF Core; (7) store danh tính đổi tên `Platform.Authentication.AspNetIdentity` → `Platform.Authentication.Identity` (D20, Q10).

### 11.0 Bố cục & phụ thuộc

| Khối (ADR) | Package / project | Đường dẫn | Trạng thái |
|---|---|---|---|
| Store danh tính dùng chung (C14, Q10) | `Platform.Authentication.Identity` | `erp_core/platform/frameworks/Platform.Authentication.Identity/` | 🟢 entity `User`, `Role` · 🔴 wire `UserManager`/`SignInManager` |
| Phân quyền — core (D14, Q4) | `Platform.Authorization` | `erp_core/platform/frameworks/Platform.Authorization/` | 🟡 · không phụ thuộc EF Core |
| Phân quyền — store EF (D14) | `Platform.Authorization.EntityFramework` | `erp_core/platform/frameworks/Platform.Authorization.EntityFramework/` | 🟢 tách 2026-10-02 |
| Account (D17) | `Platform.Modules.Account` | `erp_core/platform/modules/identity/Platform.Modules.Account/` | 🟡 handler xong · 🔴 API |
| IdentityAdmin (D18) | `Platform.Modules.IdentityAdmin` | `erp_core/platform/modules/identity/Platform.Modules.IdentityAdmin/` | 🟡 handler xong · 🔴 API |
| Persistence + migration | `Lexora.Infrastructure` | `lexora/src/Lexora.Infrastructure/Persistence/` | 🟡 migration đã tạo, chưa áp DB |
| Composition root (Q9) | `Lexora.Host` | `lexora/src/Lexora.Host/` | 🟡 |
| Test | `UnitTest` (platform) | `erp_core/platform/UnitTest/{Authorization,Account,IdentityAdmin}/` | 🟢 178 test xanh |
| Demo & code mẫu phân quyền | `Sample` (platform) | `erp_core/platform/Sample/{AuthorizationDemo,Controllers}/` | 🟢 chạy trên PostgreSQL thật — xem §11.8 |

```text
Platform.Authentication.Identity        → Platform.Authentication                                   (không EF)
Platform.Authorization                  → (không project nào) — chỉ Microsoft.Extensions.{DependencyInjection,Caching,Hosting}.Abstractions
Platform.Authorization.EntityFramework  → Platform.Authorization + Platform.Authentication.Identity + Platform.ORM.EntityFramework, EF Core
Platform.Modules.Account                → Platform.Authentication(.Jwt) + Platform.Authentication.Identity   (KHÔNG → Platform.Authorization; không EF)
Platform.Modules.IdentityAdmin          → Platform.Authentication + Platform.Authentication.Identity + Platform.Authorization   (không EF)
Lexora.Infrastructure                   → Platform.Authorization.EntityFramework (AppIdentityDbContext, LexoraIdentityStoreContext)
Lexora.Host                             → các khối trên; gọi AddPlatformAuthorization() + AddPlatformAuthorizationEntityFramework()
```

Kiểm chứng 2026-10-02 bằng `dotnet list package --include-transitive`: chỉ `Platform.Authorization.EntityFramework` có `Microsoft.EntityFrameworkCore*`; `Platform.Authorization`, `Platform.Authentication.Identity`, Account, IdentityAdmin đều không (kể cả bắc cầu).

### 11.1 Framework: Platform.Authentication.Identity

```text
erp_core/platform/frameworks/Platform.Authentication.Identity/
├── Platform.Authentication.Identity.csproj      [PackageId Platform.Authentications.Identity; FrameworkReference AspNetCore.App — tên cũ Platform.Authentication.AspNetIdentity]
└── Domain/
    ├── Role.cs    🟢 [Role : IdentityRole<Guid>; RoleLevel, IsSystemRole, DisplayName, CreatedAt — trước đây ApplicationRole]
    └── User.cs    🟢 [User : IdentityUser<Guid>; TenantId?, OrgId? là CỘT AspNetUsers (Q3)]
```

`Role.RoleLevel` / `IsSystemRole` là thuộc tính phân quyền nhưng nằm ở store danh tính vì một bảng `AspNetRoles` chỉ map một entity — có chủ đích, không phải đặt nhầm.

### 11.2 Framework: Platform.Authorization (core) + Platform.Authorization.EntityFramework

```text
erp_core/platform/frameworks/Platform.Authorization/                 ← core, KHÔNG EF
├── Platform.Authorization.csproj                [InternalsVisibleTo Platform.Authorization.EntityFramework, UnitTest]
├── HostApplicationBuilderExtension.cs          🟢 AddPlatformAuthorization(): engine, evaluator, data scope, validator · 🔴 AddPlatformAuthorizationCaching() (TODO)
├── Abstractions/
│   ├── IAuthorizationEngine.cs                 🟢 HasPermissionAsync / EvaluatePoliciesAsync / AuthorizeAsync
│   ├── IPermissionProvider.cs                  🟢 + GetPermissionsByRoleAsync (per-role path, Q8)
│   ├── IPolicyProvider.cs                      🟢 GetPoliciesByRoleAsync ném UnresolvedPolicyException khi mã policy không tồn tại
│   ├── IPolicyEvaluator.cs                     🟢
│   ├── IOperandResolver.cs                     🟢 chỉ còn ResolveAsync (whitelist chuyển sang IOperandRegistry)
│   ├── IRoleGovernanceValidator.cs             🟢 C5 — IdentityAdmin inject interface này (mới 2026-10-02)
│   ├── IDataScopeFilter.cs                     🟢 ApplyFilterAsync(query, userId, permission)
│   ├── AccessDecision.cs                       🟢 Allow/Deny + Reason + PolicyCode
│   └── UnresolvedPolicyException.cs            🟢 role tham chiếu mã policy không có trong store → path fail-closed
├── Domain/
│   ├── Policy.cs                               🟢
│   └── PolicyRule.cs                           🟢
├── Operands/                                   🟢 mới 2026-10-02 — operand registry (C7, D10)
│   ├── OperandDefinition.cs                    🟢 Path, Source (UserSubject | UserColumn | UserClaim | Resource), Kind (Scalar | Collection), ClaimType
│   ├── OperandCatalog.cs                       🟢 AddUserClaim / AddResource; mặc định platform: User.Id/TenantId/OrgId, Resource.TenantId/OrgId/CreatedBy
│   ├── IOperandRegistry.cs                     🟢 TryGet / All — nguồn duy nhất cho validator và resolver
│   └── OperandRegistry.cs                      🟢 Build(defaults + mọi OperandContribution); trùng khác định nghĩa → lỗi lúc start
└── Services/
    ├── PermissionMatcher.cs                    🟢 wildcard theo quyền user ĐANG CÓ ("*" / "System.All") — internal
    ├── PolicyEvaluator.cs                      🟢 rule trong 1 policy OR theo Order (Q5); thiếu thuộc tính → rule không đạt
    ├── AuthorizationEngine.cs                  🟢 RBAC fail-fast → wildcard bypass (Q7) → per-role path: AND trong role, OR giữa role (D11, Q8)
    ├── OperandValidator.cs                     🟢 operand theo registry, operator cố định, IN cần operand Collection (T9) · chưa có API save gọi tới
    ├── CachedPolicyProvider.cs                 🟡 chỉ cache GetPolicyByCode, chưa đăng ký DI, chưa có snapshot (D13)
    └── DataScopeFilter.cs                      🟢 policy → Expression → SQL WHERE (§5.11), cùng ngữ nghĩa engine

erp_core/platform/frameworks/Platform.Authorization.EntityFramework/  ← store EF (mới 2026-10-02)
├── Platform.Authorization.EntityFramework.csproj   [→ Platform.Authorization, Platform.Authentication.Identity, Platform.ORM.EntityFramework; EF Core 9]
├── HostApplicationBuilderExtension.cs          🟢 AddPlatformAuthorizationEntityFramework(): IPermissionProvider, IPolicyProvider, IOperandResolver, IRoleGovernanceValidator
├── Persistence/
│   ├── IdentityDbContextBase.cs                🟢 DbSet Roles/Users/UserRoles/RoleClaims/UserClaims/Policies/PolicyRules; map đúng bảng AspNet*
│   ├── AuthorizationSeeder.cs                  🟡 mặc định platform: 3 role quản trị + 3 policy chung; insert-missing; validate operand · chưa seed RoleClaims
│   ├── AuthorizationSeedCatalog.cs             🟢 catalog role/policy + AuthorizationSeedContribution (DI marker)
│   ├── AuthorizationDataSeeder.cs              🟢 gộp mặc định + đóng góp AddPlatformAuthorizationSeed(...), validate bằng OperandValidator
│   └── EntityConfigurations/
│       ├── RoleConfiguration.cs                🟢 AspNetRoles + RoleLevel/IsSystemRole/DisplayName/CreatedAt, index RoleLevel
│       ├── PolicyConfiguration.cs              🟢 lexora.policies, unique Code
│       └── PolicyRuleConfiguration.cs          🟢 lexora.policy_rules, unique (PolicyId, Order)
└── Services/
    ├── PermissionProvider.cs                   🟢
    ├── PolicyProvider.cs                       🟢
    ├── OperandResolver.cs                      🟢 chỉ resolve operand đã đăng ký (D10); nguồn theo OperandDefinition: subject / cột AspNetUsers / UserClaim / property resource
    │                                           🔴 ManagedProjectIds resolve từ domain (Q6) — đang đọc claim CSV
    └── RoleGovernanceValidator.cs              🟢 : IRoleGovernanceValidator — C5: level actor > level đích · 🔴 tenant/org scope (§5.4 mục 4–5)
```

Chưa tạo so với dự kiến cũ (🔴): `Domain/ValueObjects/*` (OperandPath, ComparisonOperator), `Application/Contracts/*`, tách `Validators/` riêng, `AuthorizationSnapshot` (§5.12), tích hợp `IAuthorizationHandler` / attribute để endpoint nghiệp vụ gọi engine, catalog / whitelist permission (D19), hằng số `SystemRoles` (D7). Đối chiếu đầy đủ từng mục C/D/Q: §7.2.

### 11.3 Module: Platform.Modules.Account (self-service)

```text
erp_core/platform/modules/identity/Platform.Modules.Account/
├── Platform.Modules.Account.csproj             [không reference Platform.Authorization — C12/T13]
├── Abstractions/IEmailSender.cs
├── Services/LoggingEmailSender.cs              🟡 chỉ log, chưa gửi email thật
├── Commands/
│   ├── LoginCommand(.Handler).cs               🟢 kiểm lockout trước mật khẩu; JWT có sub/jti/name + tenant claim (TenantClaimName, mặc định GroupSid)
│   │                                           🔴 RefreshToken (đang rỗng); OrgId/permission/policy code trong token (§5.9)
│   ├── LogoutCommand(.Handler).cs              🟡 chưa revoke token/session
│   ├── ChangePasswordCommand(.Handler).cs      🟢
│   ├── ForgotPasswordCommand(.Handler).cs      🟢 (phụ thuộc email thật)
│   ├── ResetPasswordCommand(.Handler).cs       🟢
│   └── UpdateProfileCommand(.Handler).cs       🟢
├── Queries/GetMyProfileQuery(.Handler).cs      🟢
└── DependencyInjection/AccountDependencyInjection.cs   🟢 AddAccountApplication()
```

🔴 Chưa có: API `/api/account/*` (login, logout, profile GET/PUT, change/forgot/reset password), email service thật.

### 11.4 Module: Platform.Modules.IdentityAdmin (admin)

```text
erp_core/platform/modules/identity/Platform.Modules.IdentityAdmin/
├── Platform.Modules.IdentityAdmin.csproj       [→ Authentication, Authentication.Identity, Authorization (core) — không EF Core]
├── Commands/   (mọi lệnh ghi đều qua IRoleGovernanceValidator — C5)
│   ├── CreateUserCommand(.Handler).cs                    🟢 ghi TenantId/OrgId vào cột User; không còn ghi claim GroupSid/OrgId
│   ├── UpdateUserCommand(.Handler).cs                    🟢 lock/unlock
│   ├── DeleteUserCommand(.Handler).cs                    🟢
│   ├── CreateRoleCommand(.Handler).cs                    🟢 role business, level < level actor
│   ├── AssignPermissionToRoleCommand(.Handler).cs        🟢 C6 grant-scope + C5 + chặn system role
│   ├── AssignPolicyToRoleCommand(.Handler).cs            🟢 C6 + C5 + chặn system role
│   ├── RemovePermissionFromRoleCommand(.Handler).cs      🟢 C5 (C6 không áp khi revoke)
│   └── RemovePolicyFromRoleCommand(.Handler).cs          🟢 C5
├── Queries/
│   ├── GetUsersQuery(.Handler).cs                        🟡 chưa lọc theo tenant/org
│   ├── GetRolesQuery(.Handler).cs                        🟢
│   ├── GetPermissionsQuery(.Handler).cs                  🟢
│   ├── GetPoliciesQuery(.Handler).cs                     🟢
│   └── QueryableExtensions.cs                            🟢 MaterializeAsync — chạy IQueryable async không cần EF (IAsyncEnumerable, fallback ToList)
└── DependencyInjection/IdentityAdminDependencyInjection.cs   🟢 AddIdentityAdminApplication()
```

🔴 Chưa có: API `/api/identity-admin/*`; kiểm permission kiểu `User.Manage` ở tầng endpoint (hiện chỉ có governance theo RoleLevel); CRUD custom Policy / PolicyRule (Phase 4b, T26–T44); Admin (không có policy) chưa gán được policy theo C6.

### 11.5 Test (platform UnitTest)

```text
erp_core/platform/UnitTest/
├── Authorization/              134 test   [+ OperandRegistryTests: registry C7/D10, DataScopeFilterTests: T10]
│   ├── AuthorizationEngineTests.cs               [mock — fail-fast, bypass, no-policy; DI core không cần store]
│   ├── AuthorizationEnginePerRolePathTests.cs    [store thật, seed thật — Phụ lục D: T60–T61, T66–T69, T74–T80, Q8, unknown policy code, null]
│   ├── PermissionProviderTests.cs                [wildcard D8/Q2]
│   ├── RoleGovernanceValidatorTests.cs           [C5, Phụ lục B]
│   ├── OperandResolverTests.cs                   [T9 + nguồn User.* theo Q3]
│   ├── OperandValidatorTests.cs / PolicyEvaluatorTests.cs / AuthorizationSeederTests.cs
│   └── Helpers/InMemoryTestDbContext.cs
├── Account/                    9 test   [LoginCommandHandlerTests: lockout, tenant claim đọc lại như JwtBearer]
└── IdentityAdmin/              35 test  [C6 escalation, C5 governance cho mọi lệnh, CreateUser ghi cột]
```

```bash
dotnet test erp_core/platform/UnitTest/UnitTest.csproj -p:SkipFrontendBuild=true --filter "FullyQualifiedName~UnitTest.Authorization|FullyQualifiedName~UnitTest.Account|FullyQualifiedName~UnitTest.IdentityAdmin"
```

Lưu ý: tên test trong `AuthorizationEngineTests` / `PolicyEvaluatorTests` (T2, T5, T77…) có từ trước và **không trùng nghĩa** với mã test §8; test mới ghi mã §8 trong `DisplayName`. 🔴 T13 (arch test chặn Account reference Authorization) chưa có — hiện chỉ bảo đảm bằng csproj.

### 11.6 Lexora.Host — Composition

```csharp
// lexora/src/Lexora.Host/DependencyInjection/HostLayerExtension.cs (AddHostLayer)
builder.Services.AddIdentity<User, Role>()
  .AddEntityFrameworkStores<AppIdentityDbContext>()
  .AddDefaultTokenProviders();
builder.Services.AddPlatformAuthentication(builder.Configuration, auth => auth.AddCoreJwtBearer(builder.Configuration));
builder.AddPlatformAuthorization();                 // Platform.Authorization (engine)
builder.AddPlatformAuthorizationEntityFramework();  // Platform.Authorization.EntityFramework (provider EF)
builder.AddLexoraAuthorizationOperands();           // operand nghiệp vụ Lexora (Lexora.Infrastructure/DependencyInjection/AuthorizationOperandsExtension.cs)
builder.AddLexoraAuthorizationSeed();               // role/policy nghiệp vụ Lexora (AuthorizationSeedExtension.cs, V5)
builder.Services.AddAccountApplication();       // Platform.Modules.Account
builder.Services.AddIdentityAdminApplication(); // Platform.Modules.IdentityAdmin

// UseHostLayer
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

| Mục | Trạng thái |
|---|---|
| DI 4 khối | 🟢 |
| `TenantClaimName` (mặc định `GroupSid`) — Login phát, `HttpCurrentUserStore` / `UserTenantIdResolver` đọc | 🟢 |
| Map API Account / IdentityAdmin | 🔴 |
| Endpoint nghiệp vụ gọi engine (RBAC + ABAC) | 🔴 |
| Gọi seeder lúc start | 🔴 Lexora đã đăng ký đóng góp seed nhưng chưa gọi `AuthorizationDataSeeder.SeedAsync()` (chờ áp migration); Sample đã gọi |
| Tự áp migration Identity lúc start | 🔴 cố ý chưa gắn — `appsettings.json` đang bật `AutoMigrate` |
| Cấu hình `PlatformAuthorization:CacheDuration` / `AllowCustomPolicies` | 🔴 |

### 11.7 Persistence & Migration (Lexora.Infrastructure)

```text
lexora/src/Lexora.Infrastructure/DependencyInjection/
├── AuthorizationOperandsExtension.cs           🟢 AddLexoraAuthorizationOperands(): User.DepartmentId/ApproveLimit/ManagedProjectIds[]/ManagedDepartmentIds[], Resource.DepartmentId/Amount/ProjectId/EmployeeId
└── AuthorizationSeedExtension.cs               🟢 AddLexoraAuthorizationSeed(): role DIRECTOR/CFO/DEPARTMENT_MANAGER/EMPLOYEE, policy P_SAME_DEPARTMENT/P_APPROVE_LIMIT/P_MANAGED_PROJECT/P_OWNER_OR_SAME_DEPT (V5)

lexora/src/Lexora.Infrastructure/Persistence/
├── AppIdentityDbContext.cs                     🟢 IdentityDbContext<User, Role, Guid> + Policies/PolicyRules — SỞ HỮU schema & migration
├── AppIdentityDbContextDesignTimeFactory.cs    🟢 cho dotnet ef; không nhúng credential
├── LexoraIdentityStoreContext.cs               🟢 : IdentityDbContextBase (Platform.Authorization.EntityFramework) — provider đọc cùng bảng; KHÔNG sinh migration
└── IdentityMigrations/
    ├── 20261002135313_InitialIdentitySchema.cs 🟢 tạo toàn bộ AspNet* + lexora.policies/policy_rules
    ├── 20261002135313_InitialIdentitySchema.Designer.cs
    └── AppIdentityDbContextModelSnapshot.cs
```

- Bảng lịch sử riêng: `__EFMigrationsHistory_Identity` (cùng database với ErpCore / Lexora `AppDbContext`).
- Chưa áp vào DB nào. Áp thủ công (chạy từ `lexora/src`):

```bash
dotnet ef database update --project Lexora.Infrastructure --startup-project Lexora.Host --context AppIdentityDbContext
```

Schema thực tế (PostgreSQL; bảng Identity ở schema mặc định, tên PascalCase theo ASP.NET Identity — khác mẫu snake_case dự kiến cũ):

```sql
-- AspNetUsers: cột Identity chuẩn +
"TenantId" uuid NULL,      -- index IX_AspNetUsers_TenantId
"OrgId"    uuid NULL,      -- index IX_AspNetUsers_OrgId

-- AspNetRoles: cột Identity chuẩn +
"RoleLevel"    integer NOT NULL DEFAULT 100,   -- index IX_AspNetRoles_RoleLevel
"IsSystemRole" boolean NOT NULL DEFAULT FALSE,
"DisplayName"  character varying(255) NULL,
"CreatedAt"    timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,

-- AspNetUserRoles, AspNetUserClaims, AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens: chuẩn ASP.NET Identity

-- lexora.policies:     Id, Code (unique), Name, ResourceType, Action, IsSystemPolicy, CreatedAt, UpdatedAt
-- lexora.policy_rules: Id, PolicyId (FK cascade), LeftOperand, Operator, RightOperand, Order, CreatedAt — unique (PolicyId, Order)
```

🔴 Còn lại: chuyển dữ liệu cũ nếu DB đã có claim `GroupSid` / `OrgId` trên user; gộp hai DbContext Identity nếu không cần tách đọc/ghi.

### 11.8 Sample — demo & code mẫu phân quyền (2026-10-03)

```text
erp_core/platform/Sample/
├── AuthorizationDemo/
│   ├── SampleAuthorizationDbContext.cs     🟢 : IdentityDbContextBase + bảng SampleExpenses; database riêng SampleAuthorization (EnsureCreated — chỉ demo)
│   ├── SampleAuthorizationDemoData.cs      🟢 operand + seed đóng góp (role SAMPLE_*, policy SAMPLE_*), RoleClaims, 7 user, 7 chứng từ, kịch bản AZ-01..13
│   └── SampleAuthorizationExtensions.cs    🟢 AddSampleAuthorization() + SeedSampleAuthorizationAsync() (lỗi DB → cảnh báo, không chặn khởi động)
├── Controllers/
│   ├── AuthorizationDemoController.cs      🟢 api/_authz-demo — bộ kiểm thử trực tiếp
│   └── ExpenseSampleController.cs          🟢 api/_authz-sample/expenses — code mẫu endpoint nghiệp vụ (§5.11.2)
├── Program.cs                              🟢 builder.AddSampleAuthorization(); await app.SeedSampleAuthorizationAsync();
└── Sample.http                             🟢 request mẫu cho cả hai controller
```

```csharp
// Sample/AuthorizationDemo/SampleAuthorizationExtensions.cs — composition tối thiểu cho một host
builder.AddPlatformAuthorization();
builder.AddPlatformAuthorizationEntityFramework();
builder.AddPlatformAuthorizationOperands(SampleAuthorizationDemoData.ConfigureOperands); // operand nghiệp vụ (C7)
builder.AddPlatformAuthorizationSeed(SampleAuthorizationDemoData.ConfigureSeed);         // role/policy nghiệp vụ (V5)
builder.Services.AddDbContext<SampleAuthorizationDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddScoped<IdentityDbContextBase>(sp => sp.GetRequiredService<SampleAuthorizationDbContext>());
// lúc start: EnsureCreated → AuthorizationDataSeeder.SeedAsync() → dữ liệu demo
```

| Endpoint | Mục đích | Kết quả 2026-10-03 (PostgreSQL thật) |
|---|---|---|
| `GET api/_authz-demo/scenarios` | 13 kịch bản engine AZ-01..13 (wildcard, hạn mức, khác phòng/tenant, IN, owner, policy chưa seed) | 13/13 pass |
| `GET api/_authz-demo/scope-check` | Data Scope (SQL) so với engine từng bản ghi, 7 user × 3 permission | 21/21 khớp |
| `GET api/_authz-demo/expenses?user=&permission=` | Danh sách trong phạm vi + câu SQL sinh ra | `WHERE … = @p`, `= ANY (@p)`, `WHERE FALSE`, wildcard không lọc |
| `GET api/_authz-demo/operands` · `store` · `users` | Xem registry, role/policy/role claims, user demo | — |
| `POST api/_authz-demo/check` · `rules/validate` · `reset` | Kiểm tra tùy ý, validate rule, tạo lại DB demo | — |
| `GET/POST api/_authz-sample/expenses[...]` | Code mẫu (§5.11.2): list + lọc + phân trang, summary, chi tiết, approve | 200 / 403 / 404 đúng quy ước |

- Chạy: Sample cần PostgreSQL + Redis. Connection string `ConnectionStrings:AuthorizationDbContext`; không có thì dùng server của `MasterDbContext` với database `SampleAuthorization`. DB tạo trước khi có bảng `SampleExpenses` → gọi `POST api/_authz-demo/reset`.
- User demo chọn theo tên (query/body hoặc header `X-Demo-User`), **không qua xác thực** — chỉ dùng cho demo; ứng dụng thật lấy userId từ `ICurrentUser` / claims.
- 🔴 `Sample/appsettings.json` hiện không khớp mật khẩu PostgreSQL local (lỗi môi trường có sẵn); kiểm thử 2026-10-03 dùng container tạm qua biến môi trường `ConnectionStrings__AuthorizationDbContext`.
- 🟡 `ApiResponseWrapperMiddleware` của Sample nhét body ProblemDetails (401/403/404) vào trường `code` của response — có sẵn, ngoài phạm vi ADR này.

---

## 12. Implementation Phases (Chi tiết)

| Phase | Target | Nội dung | Duration | Done khi | Trạng thái (2026-10-02) |
|-------|--------|---------|----------|----------|-------------------------|
| **0** | ADR | ✅ Confirm Q1–Q10; Status = Accepted; Project Structure written | 1 ngày | ADR ngay | 🟢 §11 viết lại theo code thực tế 2026-10-02 |
| **1** | Framework | ✅ `Platform.Authorization` package (lúc đó tên `Platform.IdentityAccessManagement`, đổi 2026-10-02 — Q4): Domain (Policy, PolicyRule; `Role` — trước đây ApplicationRole — chuyển sang `Platform.Authentication.Identity` cùng `User`), EF mappings, base DbContext; Abstractions (IAuthorizationEngine, IOperandResolver, etc.); Services (PermissionProvider, PolicyProvider, PolicyEvaluator, OperandResolver, AuthorizationEngine); HostExtension for DI | **22m 40s** (2026-09-29 21:46–22:08) | ✅ Build xanh; 39 unit tests pass (thời điểm đó) | 🟢 Migration `InitialIdentitySchema` đã tạo (chưa áp DB) |
| **2** | Seed | ✅ System Roles (R_ADMIN, R_TENANT_ADMIN, R_ORG_ADMIN, R_DIRECTOR, R_CFO, R_DEPARTMENT_MANAGER, R_EMPLOYEE) + Policies (P_SAME_TENANT, P_SAME_ORG, P_SAME_DEPARTMENT, P_OWNER_ONLY, P_APPROVE_LIMIT, P_MANAGED_PROJECT, P_OWNER_OR_SAME_DEPT) + PolicyRules (seed extension method AuthorizationSeeder) | **~3m** (2026-09-29 22:10–22:13) | ✅ T20–T25, T1b pass (7 tests); total 46 tests 🟢 (thời điểm đó) | 🟡 Thiếu `P_MANAGED_DEPARTMENT`; chưa seed RoleClaims; chưa gọi lúc start |
| **3** | Engine | `PermissionProvider`, `PolicyProvider`, `PolicyEvaluator`, `OperandResolver`, `AuthorizationEngine` (Permission + ABAC check + Admin bypass); Operand whitelist (T9); validation on save | 5–7 ngày | T4–T6, T9, T33–T36, T60–T71 🟢; unit tests; benchmark | 🟡 Engine đúng D11/Q8 (AND trong role, OR giữa role), wildcard theo quyền đang có, `User.*` theo Q3, fail-closed khi thiếu policy/thuộc tính — 94 test Authorization xanh; chưa benchmark; resolver vẫn reflection + 1 query/operand |
| **4** | Data Scope | Policy group OR (Employee: P_OWNER_OR_SAME_DEPT = Owner ∨ SameDept); `IDataScopeFilter` impl; query filtering helpers | 3–4 ngày | T6, T10, T74–T76 🟢 | 🟡 OR trong policy 🟢 (T74–T76); `DataScopeFilter` 🟢 (SQL WHERE); Sample có endpoint mẫu (§11.8); Lexora chưa có endpoint list dùng |
| **4b** | Policy Config API | (Optional v1) API endpoints: Create/Update/Delete custom Policy + PolicyRule; immutability guard (system policies reject); validate operand/operator on save | 5–7 ngày | T26–T32, T41–T43, T52 🟢 (nếu enabled) | 🔴 |
| **5** | Account | Module: `Platform.Modules.Account` (`erp_core/platform/modules/identity/`) + Handlers (Login, Logout, ChangePassword, ForgotPassword, ResetPassword, GetProfile, UpdateProfile); Email service; Controller `/api/account/*` | 4–5 ngày | T11, T13 🟢; E2E login + logout OK | 🟡 Handler xong (lockout, tenant claim — 9 test); 🔴 API, email thật, revoke token, T13 tự động |
| **6** | IdentityAdmin | Module: `Platform.Modules.IdentityAdmin` (`erp_core/platform/modules/identity/`) + Handlers (CreateUser, AssignPermission, AssignPolicy, CreatePolicyRule); scope validation (C6); Controller `/api/identity-admin/*` with [Authorize(Roles="Admin")] | 5–7 ngày | T7–T8, T12, T45–T51, T77–T80 🟢 | 🟡 Handler + C5/C6 trên mọi lệnh (35 test); 🔴 API, tenant/org scope, CreatePolicyRule |
| **7** | Caching | `CachedPolicyProvider` decorator; TTL-based + event invalidation on PolicyRule change; `IMemoryCache` or distributed | 2–3 ngày | T52–T54 🟢 | 🟡 Decorator có nhưng chưa đăng ký DI; chưa có snapshot/invalidation |
| **8** | Demo + Docs | Sample compose (Lexora.Host with all 4 khối); end-to-end smoke test (login → create expense → cffo approves); README + ADR index; Phụ lục D data seeded | 2–3 ngày | E2E demo passes; docs reviewed | 🟡 Lexora.Host đăng ký DI đủ 4 khối; README platform đồng bộ; Sample: demo + code mẫu phân quyền chạy trên PostgreSQL (§11.8); 🔴 E2E, seed Phụ lục D |
| **9+** | (Later) | DSL expression tree for OR (Policy Group); wire UserManager/SignInManager trong `Platform.Authentication.Identity` (package + entity đã có — Q10); ADR riêng nếu scope mở rộng | TBD | Separate ADR; scope/timeline chốt sau | 🔴 |

---

## 13. Tham chiếu thêm

- [Refactor Authentication](./refactor-authentication.md) — AuthN foundation; **không** chứa Account use-case  
- [refactoring-rules.md](./refactoring-rules.md) — Module Atomic; không nhét business vào `Platform.*` core  
- ASP.NET Core Authorization / Identity docs  
- Nygard ADR / MADR (template repo)  
- Commit / PR implement (điền khi xong)

---

## Phụ lục A — Ví dụ RoleClaim

**CFO**

| ClaimType | ClaimValue |
|-----------|------------|
| Permission | Accounting.View |
| Permission | Accounting.Edit |
| Permission | Expense.View |
| Permission | Expense.Approve |
| Policy | P_SAME_ORG |
| Policy | P_APPROVE_LIMIT |

**TechnicalManager**

| ClaimType | ClaimValue |
|-----------|------------|
| Permission | Project.View |
| Permission | Project.Edit |
| Policy | P_MANAGED_PROJECT |

**SalesEmployee**

| ClaimType | ClaimValue |
|-----------|------------|
| Permission | SalesResult.View |
| Policy | P_OWNER_ONLY |

---

## Phụ lục B — Ví dụ evaluate nhanh

| Scenario | Kết quả |
|----------|---------|
| CFO duyệt 8M, limit 10M, cùng Org | ALLOW |
| CFO duyệt 15M, limit 10M | DENY (ApproveLimit) |
| Director View cùng Org | ALLOW; Edit → DENY (RBAC) |
| TechnicalManager project cùng Dept | ALLOW; khác Dept → DENY |
| Employee owner khác Dept | ALLOW (OwnerOnly) |
| Employee không owner, khác Dept | DENY |
| OrgAdmin tạo Role Level 500 | ALLOW; Level 900 → DENY |

---

## Phụ lục C — Query audit mẫu

```sql
-- Role có Permission Expense.Approve
SELECT r.Name
FROM AspNetRoles r
JOIN AspNetRoleClaims rc ON rc.RoleId = r.Id
WHERE rc.ClaimType = 'Permission' AND rc.ClaimValue = 'Expense.Approve';

-- Role dùng Policy P_SAME_ORG
SELECT r.Name
FROM AspNetRoles r
JOIN AspNetRoleClaims rc ON rc.RoleId = r.Id
WHERE rc.ClaimType = 'Policy' AND rc.ClaimValue = 'P_SAME_ORG';

-- PolicyRule tham chiếu Resource.OrgId
SELECT *
FROM PolicyRules
WHERE LeftOperand = 'Resource.OrgId' OR RightOperand = 'Resource.OrgId';
```

---

## Phụ lục D — Dữ liệu mẫu theo cấu hình

Seed minh họa cho Tenant `T001`, hai Org `O001` / `O002`, và các role nghiệp vụ đã nêu ở §5.7–§5.8. ID dùng mã ngắn (production = `Guid`).

> **So với seed thực tế (`AuthorizationSeeder`, 2026-10-02):**
> - D.1/D.2: seed **7 policy / 8 rule** — thiếu `P_MANAGED_DEPARTMENT` (P007 / PR07); `P_MANAGED_PROJECT` seed là `Resource.DepartmentId IN User.ManagedDepartmentIds` thay vì PR06 `==`.
> - Nguồn seed (V5): platform seed `ADMIN`/`TENANT_ADMIN`/`ORG_ADMIN` + `P_SAME_TENANT`/`P_SAME_ORG`/`P_OWNER_ONLY`; phần còn lại do Lexora đóng góp (`AuthorizationSeedExtension`).
> - D.3: seed **7 role** (`ADMIN`, `TENANT_ADMIN`, `ORG_ADMIN`, `DIRECTOR`, `CFO`, `DEPARTMENT_MANAGER`, `EMPLOYEE`); chưa có `IT_CORP`, `CHIEF_ACCOUNTANT`, `CTO`, `TECHNICAL_MANAGER`, `SALES_MANAGER`, `SALES_EMPLOYEE`.
> - D.4–D.7: **chưa seed** RoleClaims, user mẫu, UserClaims, UserRoles. Seeder chưa được gọi lúc Lexora.Host start.
> - Sample (`erp_core/platform/Sample/AuthorizationDemo`) có bộ dữ liệu demo riêng (role `SAMPLE_*`, 7 user, RoleClaims/UserClaims) trên database `SampleAuthorization` và 13 kịch bản AZ-01..AZ-13 chạy qua `GET api/_authz-demo/scenarios` — kiểm thử trực tiếp 2026-10-02: 13/13 pass trên Postgres.
> - Test dùng dữ liệu tương đương Phụ lục D nhưng tự dựng trong `Authorization/AuthorizationEnginePerRolePathTests` (§8.5.6).

### D.0 Bối cảnh tổ chức

| Id | Loại | Mô tả |
|----|------|--------|
| T001 | Tenant | Tập đoàn demo |
| O001 | Org | Công ty thành viên A |
| O002 | Org | Công ty thành viên B |
| D-ACC | Department | Phòng Kế toán (O001) |
| D-TECH | Department | Phòng Kỹ thuật (O001) |
| D-SALES | Department | Phòng Kinh doanh (O001) |

### D.1 `Policies` (seed hệ thống)

| Id | Code | Name | ResourceType | Action | IsSystemPolicy |
|----|------|------|--------------|--------|----------------|
| P001 | P_SAME_TENANT | SameTenant | Any | * | true |
| P002 | P_SAME_ORG | SameOrg | Any | * | true |
| P003 | P_SAME_DEPARTMENT | SameDepartment | Any | * | true |
| P004 | P_OWNER_ONLY | OwnerOnly | Any | * | true |
| P005 | P_APPROVE_LIMIT | ApproveLimit | Expense | Approve | true |
| P006 | P_MANAGED_PROJECT | ManagedProject | Project | View/Edit | true |
| P007 | P_MANAGED_DEPARTMENT | ManagedDepartment | Any | * | true |
| P008 | P_OWNER_OR_SAME_DEPT | OwnerOrSameDepartment | Any | View/Edit | true |

> `P008` là policy tổng hợp (OR) cho Employee v1 nếu chưa dùng Policy Group expression tree — rules bên dưới dùng `Logic` minh họa; nếu schema chỉ có AND thì seed thành **một** Policy với evaluator OR theo `Order`/`Group`, hoặc dùng AuthorizationRuleSet OR ở runtime (D11).

### D.2 `PolicyRules`

| Id | PolicyId | Order | LeftOperand | Operator | RightOperand | Ghi chú |
|----|----------|------:|-------------|----------|--------------|---------|
| PR01 | P001 | 1 | Resource.TenantId | == | User.TenantId | SameTenant |
| PR02 | P002 | 1 | Resource.OrgId | == | User.OrgId | SameOrg |
| PR03 | P003 | 1 | Resource.DepartmentId | == | User.DepartmentId | SameDepartment |
| PR04 | P004 | 1 | Resource.CreatedBy | == | User.Id | OwnerOnly |
| PR05 | P005 | 1 | Resource.Amount | <= | User.ApproveLimit | ApproveLimit |
| PR06 | P006 | 1 | Resource.DepartmentId | == | User.DepartmentId | ManagedProject (biến thể Dept) |
| PR07 | P007 | 1 | Resource.DepartmentId | IN | User.ManagedDepartmentIds | ManagedDepartment |
| PR08 | P008 | 1 | Resource.CreatedBy | == | User.Id | OR nhánh 1 |
| PR09 | P008 | 2 | Resource.DepartmentId | == | User.DepartmentId | OR nhánh 2 |

Sales Employee dùng `P_OWNER_ONLY` với `Resource.EmployeeId == User.Id` nếu domain không map `CreatedBy` — có thể thêm rule riêng hoặc alias operand `CreatedBy` ↔ `EmployeeId`.

### D.3 `AspNetRoles`

| Id | Name (normalized) | Display | IsSystemRole | RoleLevel | Cấu hình nghiệp vụ |
|----|-------------------|---------|-------------:|----------:|--------------------|
| R_ADMIN | ADMIN | Admin | true | 1000 | Toàn hệ thống |
| R_TENANT_ADMIN | TENANT_ADMIN | TenantAdmin | true | 900 | Toàn Tenant |
| R_ORG_ADMIN | ORG_ADMIN | OrgAdmin | true | 800 | Toàn Org |
| R_DIRECTOR | DIRECTOR | Director | false | 700 | Xem Org, không sửa |
| R_IT_CORP | IT_CORP | IT tập đoàn | false | 720 | Config công ty thành viên |
| R_CHIEF_ACCT | CHIEF_ACCOUNTANT | Kế toán trưởng | false | 600 | Kế toán trong Org |
| R_CFO | CFO | CFO | false | 650 | Duyệt chi ≤ ApproveLimit + Org |
| R_CTO | CTO | CTO | false | 650 | Giống CFO (limit riêng trên UserClaim) |
| R_TECH_MGR | TECHNICAL_MANAGER | Trưởng phòng KT | false | 500 | Project theo Dept |
| R_DEPT_MGR | DEPARTMENT_MANAGER | Trưởng phòng | false | 500 | Duyệt chi ≤ 2M + Dept |
| R_SALES_MGR | SALES_MANAGER | Sales Manager | false | 450 | SalesResult theo Dept |
| R_EMPLOYEE | EMPLOYEE | Employee | false | 100 | Owner OR SameDept |
| R_SALES_EMP | SALES_EMPLOYEE | Sales Employee | false | 100 | Chỉ doanh số của mình |

### D.4 `AspNetRoleClaims` — theo từng cấu hình

#### R_ADMIN — xem toàn hệ thống

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_ADMIN | Permission | System.All |

*(không gán Policy — global bypass ABAC)*

#### R_TENANT_ADMIN — toàn Tenant

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_TENANT_ADMIN | Permission | System.All |
| R_TENANT_ADMIN | Policy | P_SAME_TENANT |

#### R_ORG_ADMIN — toàn Org

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_ORG_ADMIN | Permission | System.All |
| R_ORG_ADMIN | Policy | P_SAME_ORG |

#### R_DIRECTOR — xem Org, không sửa

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_DIRECTOR | Permission | Customer.View |
| R_DIRECTOR | Permission | Project.View |
| R_DIRECTOR | Permission | Accounting.View |
| R_DIRECTOR | Permission | Expense.View |
| R_DIRECTOR | Permission | SalesResult.View |
| R_DIRECTOR | Policy | P_SAME_ORG |

#### R_IT_CORP — cấu hình công ty thành viên trong Tenant

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_IT_CORP | Permission | Company.Config.View |
| R_IT_CORP | Permission | Company.Config.Edit |
| R_IT_CORP | Policy | P_SAME_TENANT |

#### R_CHIEF_ACCT — kế toán trong Org

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_CHIEF_ACCT | Permission | Accounting.View |
| R_CHIEF_ACCT | Permission | Accounting.Edit |
| R_CHIEF_ACCT | Policy | P_SAME_ORG |

#### R_CFO / R_CTO — duyệt chi + hạn mức + Org

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_CFO | Permission | Accounting.View |
| R_CFO | Permission | Accounting.Edit |
| R_CFO | Permission | Expense.View |
| R_CFO | Permission | Expense.Approve |
| R_CFO | Policy | P_SAME_ORG |
| R_CFO | Policy | P_APPROVE_LIMIT |
| R_CTO | Permission | Expense.View |
| R_CTO | Permission | Expense.Approve |
| R_CTO | Permission | Project.View |
| R_CTO | Policy | P_SAME_ORG |
| R_CTO | Policy | P_APPROVE_LIMIT |

#### R_TECH_MGR — project theo phòng

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_TECH_MGR | Permission | Project.View |
| R_TECH_MGR | Permission | Project.Edit |
| R_TECH_MGR | Policy | P_MANAGED_PROJECT |

#### R_DEPT_MGR — duyệt ≤ 2M trong phòng

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_DEPT_MGR | Permission | Expense.View |
| R_DEPT_MGR | Permission | Expense.Approve |
| R_DEPT_MGR | Policy | P_SAME_ORG |
| R_DEPT_MGR | Policy | P_SAME_DEPARTMENT |
| R_DEPT_MGR | Policy | P_APPROVE_LIMIT |

#### R_SALES_MGR — doanh số phòng

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_SALES_MGR | Permission | SalesResult.View |
| R_SALES_MGR | Permission | SalesResult.Edit |
| R_SALES_MGR | Policy | P_SAME_DEPARTMENT |

*(hoặc `P_MANAGED_DEPARTMENT` nếu quản lý nhiều phòng)*

#### R_EMPLOYEE — Owner OR SameDepartment

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_EMPLOYEE | Permission | Customer.View |
| R_EMPLOYEE | Permission | Customer.Edit |
| R_EMPLOYEE | Policy | P_OWNER_OR_SAME_DEPT |

#### R_SALES_EMP — chỉ doanh số của mình

| RoleId | ClaimType | ClaimValue |
|--------|-----------|------------|
| R_SALES_EMP | Permission | SalesResult.View |
| R_SALES_EMP | Policy | P_OWNER_ONLY |

### D.5 `AspNetUsers` (mẫu)

| Id | UserName | Email | LockoutEnabled | Mô tả |
|----|----------|-------|----------------|--------|
| U_SYS | sys.admin | admin@demo.local | true | System Admin |
| U_TA | tenant.admin | ta@demo.local | true | TenantAdmin T001 |
| U_OA1 | org.admin.a | oa.a@demo.local | true | OrgAdmin O001 |
| U_OA2 | org.admin.b | oa.b@demo.local | true | OrgAdmin O002 |
| U_DIR | director.a | director@demo.local | true | Director O001 |
| U_IT | it.corp | it@demo.local | true | IT tập đoàn |
| U_CA | chief.acct | ca@demo.local | true | Kế toán trưởng O001 |
| U_CFO | cfo.a | cfo@demo.local | true | CFO O001 — limit 10M |
| U_CTO | cto.a | cto@demo.local | true | CTO O001 — limit 10M |
| U_TM | tech.mgr | tm@demo.local | true | Trưởng phòng KT |
| U_DM | dept.mgr.acc | dm@demo.local | true | Trưởng phòng KT toán — limit 2M |
| U_SM | sales.mgr | sm@demo.local | true | Sales Manager |
| U_EMP | employee.1 | emp1@demo.local | true | Employee thường |
| U_SE | sales.emp | se@demo.local | true | Sales Employee |
| U_LOCKED | locked.user | locked@demo.local | true | User bị khóa (demo lock) |

`U_LOCKED`: `LockoutEnd` = thời điểm tương lai (Identity lockout) — minh họa chức năng lock/unlock của IdentityAdmin.

### D.6 `AspNetUserClaims` — subject attributes theo user

> **Theo Q3 (đã implement):** `TenantId`, `OrgId` là **cột `AspNetUsers`**, không phải UserClaim — các dòng `TenantId` / `OrgId` dưới đây áp vào cột của user tương ứng; resolver bỏ qua claim cùng tên. Chỉ `DepartmentId`, `ApproveLimit` (và Managed*Ids tạm thời) nằm ở UserClaims.

| UserId | ClaimType | ClaimValue |
|--------|-----------|------------|
| U_SYS | TenantId | *(empty / omit — global)* |
| U_TA | TenantId | T001 |
| U_OA1 | TenantId | T001 |
| U_OA1 | OrgId | O001 |
| U_OA2 | TenantId | T001 |
| U_OA2 | OrgId | O002 |
| U_DIR | TenantId | T001 |
| U_DIR | OrgId | O001 |
| U_IT | TenantId | T001 |
| U_CA | TenantId | T001 |
| U_CA | OrgId | O001 |
| U_CA | DepartmentId | D-ACC |
| U_CFO | TenantId | T001 |
| U_CFO | OrgId | O001 |
| U_CFO | DepartmentId | D-ACC |
| U_CFO | ApproveLimit | 10000000 |
| U_CTO | TenantId | T001 |
| U_CTO | OrgId | O001 |
| U_CTO | DepartmentId | D-TECH |
| U_CTO | ApproveLimit | 10000000 |
| U_TM | TenantId | T001 |
| U_TM | OrgId | O001 |
| U_TM | DepartmentId | D-TECH |
| U_DM | TenantId | T001 |
| U_DM | OrgId | O001 |
| U_DM | DepartmentId | D-ACC |
| U_DM | ApproveLimit | 2000000 |
| U_SM | TenantId | T001 |
| U_SM | OrgId | O001 |
| U_SM | DepartmentId | D-SALES |
| U_EMP | TenantId | T001 |
| U_EMP | OrgId | O001 |
| U_EMP | DepartmentId | D-TECH |
| U_SE | TenantId | T001 |
| U_SE | OrgId | O001 |
| U_SE | DepartmentId | D-SALES |

Không có hàng `Permission` trên UserClaims.

### D.7 `AspNetUserRoles`

| UserId | RoleId |
|--------|--------|
| U_SYS | R_ADMIN |
| U_TA | R_TENANT_ADMIN |
| U_OA1 | R_ORG_ADMIN |
| U_OA2 | R_ORG_ADMIN |
| U_DIR | R_DIRECTOR |
| U_IT | R_IT_CORP |
| U_CA | R_CHIEF_ACCT |
| U_CFO | R_CFO |
| U_CTO | R_CTO |
| U_TM | R_TECH_MGR |
| U_DM | R_DEPT_MGR |
| U_SM | R_SALES_MGR |
| U_EMP | R_EMPLOYEE |
| U_SE | R_SALES_EMP |

Ví dụ multi-role (optional): `U_CFO` có thể thêm `R_DIRECTOR` nếu business cho phép — Authorization **union** Permission/Policy từ mọi Role (Q8).

### D.8 Ánh xạ cấu hình ↔ dữ liệu

| Cấu hình / requirement | User mẫu | Role | UserClaims chính | RoleClaims (Permission) | RoleClaims (Policy) |
|------------------------|----------|------|------------------|-------------------------|---------------------|
| Admin toàn hệ thống | U_SYS | R_ADMIN | — | System.All | *(none)* |
| TenantAdmin toàn Tenant | U_TA | R_TENANT_ADMIN | TenantId=T001 | System.All | P_SAME_TENANT |
| OrgAdmin toàn Org | U_OA1 | R_ORG_ADMIN | TenantId, OrgId=O001 | System.All | P_SAME_ORG |
| Director xem Org | U_DIR | R_DIRECTOR | OrgId=O001 | *.View | P_SAME_ORG |
| IT sửa config thành viên | U_IT | R_IT_CORP | TenantId=T001 | Company.Config.* | P_SAME_TENANT |
| Kế toán trưởng | U_CA | R_CHIEF_ACCT | OrgId=O001 | Accounting.View/Edit | P_SAME_ORG |
| CFO duyệt ≤ 10M | U_CFO | R_CFO | OrgId + ApproveLimit=10M | Expense.Approve, … | P_SAME_ORG + P_APPROVE_LIMIT |
| CTO duyệt ≤ 10M | U_CTO | R_CTO | OrgId + ApproveLimit=10M | Expense.Approve, … | P_SAME_ORG + P_APPROVE_LIMIT |
| Trưởng phòng KT / project | U_TM | R_TECH_MGR | DepartmentId=D-TECH | Project.View/Edit | P_MANAGED_PROJECT |
| Trưởng phòng duyệt ≤ 2M | U_DM | R_DEPT_MGR | Dept + ApproveLimit=2M | Expense.Approve | P_SAME_ORG + P_SAME_DEPARTMENT + P_APPROVE_LIMIT |
| Sales Manager | U_SM | R_SALES_MGR | DepartmentId=D-SALES | SalesResult.View/Edit | P_SAME_DEPARTMENT |
| Employee Owner∨Dept | U_EMP | R_EMPLOYEE | Org + Dept | Customer.View/Edit | P_OWNER_OR_SAME_DEPT |
| Sales Employee only mine | U_SE | R_SALES_EMP | Dept=D-SALES | SalesResult.View | P_OWNER_ONLY |
| Lock user | U_LOCKED | *(any)* | — | — | `LockoutEnd` set |

### D.9 Resource mẫu để evaluate (không lưu Identity)

Dùng khi viết test T5/T6/T10 — không phải bảng Identity:

| Resource | OrgId | DepartmentId | CreatedBy / EmployeeId | Amount | Kỳ vọng với actor |
|----------|-------|--------------|------------------------|-------:|-------------------|
| EXP-001 | O001 | D-ACC | — | 8_000_000 | U_CFO → ALLOW; U_DM → DENY (vượt 2M) |
| EXP-002 | O001 | D-ACC | — | 1_500_000 | U_DM → ALLOW; U_CFO → ALLOW |
| EXP-003 | O002 | D-ACC | — | 1_000_000 | U_CFO (O001) → DENY (SameOrg) |
| PRJ-TECH-1 | O001 | D-TECH | — | — | U_TM → ALLOW |
| PRJ-SALES-1 | O001 | D-SALES | — | — | U_TM → DENY |
| CUS-OWN | O001 | D-SALES | U_EMP | — | U_EMP → ALLOW (Owner) |
| CUS-DEPT | O001 | D-TECH | U_OTHER | — | U_EMP → ALLOW (SameDept) |
| CUS-OUT | O001 | D-SALES | U_OTHER | — | U_EMP → DENY |
| SR-SE-1 | O001 | D-SALES | U_SE | — | U_SE → ALLOW; U_SM → ALLOW (SameDept) |
| SR-OTHER | O001 | D-SALES | U_OTHER | — | U_SE → DENY; U_SM → ALLOW |

### D.10 Ghi chú seed / IdentityAdmin

1. **System roles / policies** (`IsSystemRole` / `IsSystemPolicy` = true): seed migration; API IdentityAdmin **reject** sửa Permission/Policy/xóa.  
2. **Business roles** (`R_DIRECTOR` …): OrgAdmin (Level 800) được tạo/sửa role Level &lt; 800 và chỉ gán Permission/Policy mà actor đang có.  
3. **ApproveLimit** khác nhau cùng RoleClaim Policy `P_APPROVE_LIMIT` — phân biệt CFO 10M vs DeptMgr 2M bằng **UserClaim**, không nhân bản Role.  
4. **Permission** không có bảng riêng ở v1 — danh sách whitelist trong code/seed docs; chỉ tồn tại khi xuất hiện trên `AspNetRoleClaims`.  
5. Khi đổi RoleClaim / UserClaim / UserRole → invalidate Authorization snapshot (§5.12).
