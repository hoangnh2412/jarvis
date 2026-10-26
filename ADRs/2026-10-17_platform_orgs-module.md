# ADR — Module quản lý Organization (`Platform.Orgs`)

> **Trạng thái:** 🟢 **Accepted** (2026-10-25). Catalog, không seed, không xóa cứng, list theo cây tenant đã code. Còn `OrgApiTests` và skill `orgs-dotnet` (§10).  
> **Ngày:** 2026-10-17 · Cập nhật phân cấp và §7: 2026-10-25  
> **Loại:** Business module / Module Atomic (Library + Persistence + optional Abstractions)  
> **Liên quan:** [Tenants module](./2026-08-07_platform_tenants-module.md) (🟡 catalog + UI vận hành), [Multitenancy package](./2026-08-06_platform_multitenancy-package.md) (🟢 ambient), [generic `ICurrentTenant<TTenant>`](./2026-08-06_platform_generic-current-tenant.md) (🟢), [Current User / Tenant](./2026-08-01_platform_current-user-tenant.md) (🟢 — D5 home vs working), [RBAC + ABAC](./2026-08-14_platform_rbac-abac-authorization.md) (🟡 — `User.OrgId`, `SameOrg`, `OrgAdmin`), [OrgStructure module](./2026-10-17_platform_orgstructure-module.md) (🟡 — cây nội bộ **trong** một Org; chưa gán Position), [Catalog Org events](./2026-10-21_platform_orgs-catalog-events.md) (🟡 publish sau `Success*`, T1/T2 chưa khép), [architecture-rules.md](./architecture-rules.md) (§0.2).  
> **Phạm vi:** Catalog **Org** — người bán đứng trên hóa đơn, do một Tenant (kể cả Tenant con) tạo. Facade `IOrgManager`; marker **`IOrgEntity`**; optional **`ICurrentOrg`**. **Không** chứa Tenant cha-con (ADR Tenants) và **không** chứa cây phòng ban (ADR OrgStructure).  
> **Change log:** 2026-10-25 — Mục 10 bỏ đỏ ở seed, list theo cây và không hard delete. `Org.View` / `Org.Manage` có trên Host. 2026-10-25 — `DeleteAsync` từ chối xóa cứng (98306), bỏ HTTP DELETE. `ListAsync` gộp org của tenant hiện tại và tenant con. Host không gọi seed `VNA_DEFAULT`. Publish event sau `Success*`, update là diff. 2026-10-25 — Org không có cha-con. Kênh tự tạo Org khách (chi nhánh, đại lý Minvoice) là Tenant con, không phải Org và không phải `OrganizationalUnit`. Chi nhánh VNA không tạo Org khách nên không thuộc ADR này. 2026-10-25 — Accept §7: D1–D18, C1–C10, O3. Vô hiệu hóa, không hard delete. Không seed Org. Danh sách nhìn xuống tenant con. API sản phẩm giữ resolver host. API vận hành theo luật root ở ADR Tenants.
> **Ngoài phạm vi:** Cơ cấu tổ chức nội bộ tập đoàn; CRUD Tenant; ambient tenant R2 / `Change`; Authorization engine (rule evaluator); billing / subscription; dedicated-DB provisioning; UI portal production (host-owned). **Trong phạm vi platform:** catalog + ambient org + SPI cho module khác + HTTP API opt-in + filter EF + tích hợp IdentityAdmin/Authorization — Sample chỉ là host reference.  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

### 1.1 Mô hình phân cấp (2026-10-25)

Sơ đồ đầy đủ: [multi-tenant, multi-org](../docs/guides/multitenancy/multi-tenant-multi-org.md).

```text
Root
  └── Tenant (có thể có Tenant con)
        └── Org          ← người bán trên hóa đơn; ADR này
              └── OrgStructure
```

| Lớp | Ai giữ | Ví dụ |
|-----|--------|--------|
| **Root** | Ngoài module này | Minipower |
| **Tenant** | [ADR Tenants](./2026-08-07_platform_tenants-module.md) | Minvoice; Tenant con HCM, HN, đại lý |
| **Org** | ADR này | Org Minvoice, Org khách, Org EPAY, Org VNA, Org Chi nhánh HCM (treo phòng ban của chính chi nhánh) |
| **OrgStructure** | [ADR OrgStructure](./2026-10-17_platform_orgstructure-module.md) | Phòng ban; chi nhánh / phòng vé / đại lý của VNA vì chúng không tạo Org khách |

Org **không** có `ParentOrgId`. Nhiều Org trong một Tenant là anh em. Khách của chi nhánh HCM là Org trong **Tenant HCM**, không phải con của Org Minvoice.

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
| P5 | Nghiệp vụ thiếu contract scope | Không có `IOrgEntity` — filter/list cross-Org trong cùng tenant |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | **`Platform.Multitenancy` không CRUD Org** — ambient tenant giữ nguyên semantic R2 / D5 |
| C2 | **Org luôn thuộc một Tenant** — `ITenantEntity` (hoặc tương đương) trên entity Org |
| C3 | **Một Org = một người bán trên hóa đơn** trong đúng một Tenant. Cây nội bộ của Org đó là OrgStructure. Kênh tự tạo Org khách là Tenant con, không phải Org con |
| C4 | Module Orgs **không** reference OrgStructure runtime nặng; OrgStructure chỉ cần `Platform.Orgs.Abstractions` |
| C5 | Entity concrete + migration = **host-owned** (pattern Setting / Tenants) |
| C6 | Authorization rules (`SameOrg`, Role Governance) = `Platform.Authorization` — Orgs chỉ cung cấp **dữ liệu tham chiếu** |
| C7 | Pattern Atomic: `Platform.Orgs` + `Platform.Orgs.EntityFramework` + `Platform.Orgs.Abstractions` (SPI mỏng) |
| C8 | `User.OrgId` (Identity) **không** thay thế catalog — IdentityAdmin gán user vào Org qua **`IOrgCatalog`** (Abstractions), không query DbSet |
| C9 | Module khác (OrgStructure, ImportExport, …) **chỉ** đọc/validate catalog qua **`Platform.Orgs.Abstractions`** — CRUD admin qua **`IOrgManager`** |
| C10 | **`Platform.MultiOrg`** là package platform (cùng “tầng” Multitenancy), không coi là tùy chọn demo — sản phẩm không cần working-org vẫn có thể không gọi `AddCurrentOrg`, nhưng package và contract Domain **phải** tồn tại khi ship Orgs |

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
| D2 | Satellite | **`Platform.Orgs.Abstractions`** (SPI đọc catalog), **`Platform.Orgs.EntityFramework`**, **`Platform.Orgs.Api`** (HTTP opt-in, pattern Setting.API) |
| D3 | Vai trò | **Catalog + lifecycle** Org cho mọi sản phẩm: onboarding khách cuối, vô hiệu hóa, tra cứu theo tenant — nguồn sự thật cho `User.OrgId` / `Resource.OrgId` |
| D4 | Facade ghi / admin | **`IOrgManager`** — create, update, set active, list có phân quyền; host **không** query `DbSet` trực tiếp |
| D4b | Port đọc / tích hợp | **`IOrgCatalog`** (Abstractions) — `GetAsync(orgId, tenantId)`, `GetByCodeAsync`, `ListAsync`, **`ValidateForAssignmentAsync(orgId, tenantId, …)`**; **`OrgDescriptor`** DTO; **không** type `IOrgReference` riêng |
| D5 | Bản ghi catalog | **Không** class `Org` trong package. Contract **`IOrgCatalogEntity`**: `Id`, `TenantId`, `Code`, `Name`, `IsActive`. Host class implement interface và **thêm cột** (`TaxCode`, …). `ApplyPlatformOrgCatalog<TOrg>()` chỉ cấu hình cột chuẩn; EF convention map cột host. Ghi cột host: subclass `CreateOrgInput` / `UpdateOrgInput` + **`IOrgCatalogHooks<TOrg>`** (Validate / Prepare / Success / Error / Finally). Đọc cột host: **`IOrgCatalogReadMapper<TOrg,TReadModel>`** + **`IOrgCatalogReader<TReadModel>`** (`UseReadModel` — ambient + explicit tenant). **`OrgDescriptor`** + **`IOrgCatalog`** giữ SPI cột chuẩn — không kế thừa descriptor |
| D14 | Catalog events (đồng bộ) | **`OrgCreatedEvent` / `OrgUpdatedEvent` / `OrgDeletedEvent`** + port publish — chi tiết [ADR events](./2026-10-21_platform_orgs-catalog-events.md); **🔴 chưa code** |
| D6 | Scope nghiệp vụ | **`IOrgEntity`** (marker, cùng vai `ITenantEntity`) — 🟢 Domain. **Filter EF** theo working org: extension **`Platform.ORM.EntityFramework`** (hoặc satellite gắn ORM) bật qua host, song song filter `ITenantEntity` — không để “ADR ORM sau” |
| D7 | Working Org | **`Platform.MultiOrg`** + contract Domain **`ICurrentOrg*` / `IOrgIdResolver*`** — mirror Multitenancy (accessor, R2). **`ICurrentOrgStore`** bridge từ **`IOrgCatalog`**. **Home** = `User.OrgId` (claim/resolver legacy). **Working HTTP** = **`IRequestScopeResolver`** + header **`X-Org-Id`** (bắt buộc) và **`X-Tenant-Id`** (tùy rule D16) — §5.1.1 |
| D16 | HTTP working scope (org-first) | **`OrgFirstRequestScopeResolver`** (`IRequestScopeResolver`) + **`UseOrgFirstScope()`** middleware → **403** khi thiếu/sai scope. Org working **chỉ** từ **`X-Org-Id`** (GUID hợp lệ). Tenant: có header thì parse header; không header thì domain (`ITenantDomainLookup` nếu đăng ký) rồi **`ITenantIdResolverFactory.GetTenantIdForOrgScopeAsync`** (user → query → host, **không** đọc `X-Tenant-Id`). Luôn **`IOrgTenantScopeValidator`** (host: **`OrgCatalogTenantScopeValidator`**). Chi tiết §5.1.1 |
| D11 | Permission platform | Permission keys chuẩn: `Org.View`, `Org.Manage` (+ mở rộng host); seed mẫu qua `AuthorizationSeedContribution` — không hard-code trong module |
| D12 | IdentityAdmin | Gán / đổi `User.OrgId` **bắt buộc** gọi **`IOrgCatalog.ValidateForAssignmentAsync`** (module IdentityAdmin reference Abstractions only) |
| D13 | Fluent DI | `AddPlatformOrgs<TOrg, TTenant>()` → **`PlatformOrgsBuilder<TOrg, TTenant>`**: `.UseEntityFramework<…>()`, `.UseHooks<…>()`, `.UseReadModel<…>()`, **`.UseAsCurrentOrgStore()`** (+ host `AddCurrentOrg<OrgCatalogOrgProfile>()`), (Later) `.UseHttpApi()` |
| D15 | Persistence nội bộ | **`IOrgStore<TOrg>`** (core) + **`OrgStore<TOrg,TUnitOfWork>`** (EF satellite) — **không** đặt tên `*Service` / public repository; Manager + Catalog + Read dùng chung store |
| D8 | **Không** chứa OrgStructure và **không** chứa Tenant con | Phòng ban và node không tạo Org khách → [OrgStructure ADR](./2026-10-17_platform_orgstructure-module.md). Kênh tạo Org khách → Tenant con trong [ADR Tenants](./2026-08-07_platform_tenants-module.md) |
| D9 | DI Host | `AddPlatformOrgs<TOrg, TTenant>()` opt-in; **không** thay `AddCurrentTenant` |
| D10 | DB placement | Org catalog **nằm trong DB của tenant** (bảng có `TenantId`). Không đưa bảng Org lên Master |
| D17 | Ai được thấy Org nào | Tenant thấy Org của chính nó và Org của các tenant thấp hơn trong cây. Không thấy Org của tenant cùng cấp hoặc tenant cao hơn. Ví dụ Tenant HCM chỉ thấy Org trong HCM. Tenant Minvoice thấy Org của Minvoice, của HCM và của HN. Vì mỗi tenant một DB, danh sách của tenant cha đọc catalog của các DB con theo `ParentTenantId` |
| D18 | Không seed Org | Platform và Host không tạo Org mặc định (không `VNA_DEFAULT`, không aligner seed). Người dùng tạo Org trên UI |
| D19 | Hai cửa HTTP scope | API vận hành (`/platform/tenants` và API platform admin) theo luật root ở [ADR Tenants](./2026-08-07_platform_tenants-module.md) T12. API sản phẩm giữ resolver host hiện tại: người đã đăng nhập, không gửi header, dùng org và tenant gắn trên user. Thiếu header không được thành root |

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
          │    Platform.MultiOrg (ambient, optional)
          └───────────────────────┘
```

| Concern | Thuộc |
|---------|--------|
| Header / claim → tenant đang làm việc | **Platform.Multitenancy** + contract Domain; **HTTP pipeline** (khi bật D16): tenant từ resolver scope hoặc **`X-Tenant-Id`** |
| Header → org đang làm việc (working) | **`X-Org-Id`** qua **D16** (`IRequestScopeResolver`); chain **`IOrgIdResolver*`** (domain/query) **không** thay middleware scope |
| Home org trên user (`User.OrgId`) | Identity — claim / resolver; khác working org trên request |
| Validate org ∈ tenant (HTTP scope) | **`IOrgTenantScopeValidator`** — **`Platform.Orgs`**: `OrgCatalogTenantScopeValidator` |
| Danh sách Org của một Tenant và của tenant thấp hơn; tạo Org | **Orgs** (D17) — không từ OrgStructure |
| `User.OrgId` gán user vào Org home | IdentityAdmin + validate qua **Orgs** |
| Cây phòng ban trong một Org; chi nhánh VNA | **OrgStructure** |
| Chi nhánh hoặc đại lý tự tạo Org khách | **Tenants** — Tenant con (`ParentTenantId`) |
| Policy `SameOrg` | Authorization (operand `User.OrgId` / `Resource.OrgId`) |

#### 5.1.1 HTTP — resolve working Tenant + Org (D16)

Áp dụng khi host gọi **`AddCurrentOrg`** (đăng ký **`IRequestScopeResolver`**) và **`UseOrgFirstScope()`** sau authentication (Sample). Mục tiêu: **không** phục vụ API nghiệp vụ khi chưa chốt org cụ thể — kể cả host/domain đã biết tenant.

| # | `X-Tenant-Id` | `X-Org-Id` | Hành vi |
|---|---------------|------------|---------|
| 1 | Không | Không | **403** — không resolve tenant từ domain để “xem cả tenant” |
| 2 | Không | Có | Tenant theo §5.1.1 (domain lookup nếu có → else `GetTenantIdForOrgScopeAsync`); org từ header; **validate** org ∈ tenant; fail → **403** |
| 3 | Có | Không | **403** — bắt buộc org |
| 4 | Có | Có | Parse cả hai GUID; **validate** org ∈ tenant; fail → **403** |

**Ghi chú triển khai**

- Header org: **`X-Org-Id`** (`HeaderOrgIdResolver.HeaderName`). Giá trị rỗng / GUID sai → **403**.
- **`GetTenantIdForOrgScopeAsync`**: `UserTenantIdResolver` → `QueryTenantIdResolver` → `HostTenantIdResolver` (không header tenant).
- Accessor đã có **cả** org + tenant (sau `BeginScope` trong cùng request): resolver cache **Allowed** — không đọc lại header.
- Route không cần scope (health, swagger, …): host **exclude** middleware hoặc không gắn `UseOrgFirstScope` — **TBD** Sample (hiện middleware toàn app).

### 5.2 Model dữ liệu (sketch)

**Bảng Org (host entity `Org` : `IOrgCatalogEntity` + `ITenantEntity` / `TenantId`)**

| Cột | Ghi chú |
|-----|---------|
| `Id` | Guid PK |
| `TenantId` | FK logic tới tenant catalog |
| `Code` | Unique trong tenant (index host-owned) |
| `Name` | Tên hiển thị |
| `IsActive` | Soft-disable |
| `…` | Logo, locale, timezone — host extension |

**Không** có `ParentOrgId` trên Org platform — quan hệ cha-con tập đoàn nằm ở OrgStructure (`OrganizationalUnit` / `LegalEntity`).

### 5.3 Phạm vi platform — catalog & tích hợp

**`IOrgManager` (ghi + admin đọc)**

| Nhóm | Capability |
|------|------------|
| Lifecycle | Create, Update, SetActive (soft-disable); **không** hard-delete mặc định nếu còn user/FK |
| Tra cứu admin | GetById, GetByCode, List → **`OrgDescriptor`** (cột chuẩn); cột host → **`IOrgCatalogReader<TReadModel>`** (D5) |
| Quy tắc | Unique `Code` trong tenant; `TenantId` từ ambient / command; audit hook (host) |

**`IOrgCatalog` (Abstractions — đọc cho module khác, C9)**

| Nhóm | Capability |
|------|------------|
| Read | `GetAsync`, `GetByCodeAsync`, `ListAsync` (read-only, không bypass tenant) |
| Validate | `ValidateForAssignmentAsync(orgId, tenantId, …)` — Org tồn tại, thuộc tenant, `IsActive` (IdentityAdmin, OrgStructure); caller truyền **hai Guid**, không qua marker riêng |
| DTO | **`OrgDescriptor`**: Id, TenantId, Code, Name, IsActive — **chỉ** SPI; **không** kế thừa để thêm cột host |

#### 5.3.1 Command / Query — nằm ở đâu?

| Lớp | Command / Query | Ghi chú |
|-----|-----------------|--------|
| **Sản phẩm** (`{Product}.Application`) | `Commands/`, `Queries/`, handler MediatR (hoặc dispatcher host) | Use case nghiệp vụ **của app** — gọi **`IOrgManager`** / **`IOrgCatalog`**, không query `DbSet` |
| **Module `Platform.Orgs`** | **Không** bắt buộc tách folder Command/Query như IdentityAdmin | Pattern **Setting**: *command* = `CreateOrgInput` / `UpdateOrgInput` → `IOrgManager.CreateAsync` / `UpdateAsync`; *query admin* = `ListOrgsQuery` + `GetByIdAsync` trên **`IOrgManager`**; *query tích hợp* = **`IOrgCatalog`** (Abstractions) |
| **HTTP** | `Platform.Orgs.Api` map request → input trên Manager | Controller mỏng; không nhét handler Application vào package Api |

Module Orgs là **facade platform** (manager + catalog port), không thay **Application layer** của từng sản phẩm. Nếu sau này Orgs phình (workflow, event), có thể tách `Commands/` nội bộ module — **không** là mặc định ADR này.

#### 5.3.2 Đọc: `OrgManager` vs `IOrgCatalog` vs read model host

| API | Đối tượng gọi | Trả về | Mục đích |
|-----|---------------|--------|----------|
| `IOrgManager.GetByIdAsync` / `ListAsync` | Admin sau CRUD, API mỏng | **`OrgDescriptor`** | Snapshot cột chuẩn |
| **`IOrgCatalogReader<TReadModel>`** | Đọc cột host (UI/API ambient hoặc job với `tenantId`) | **`TReadModel`** | **`IOrgCatalogReadMapper`** + `UseReadModel`; overload ambient / explicit tenant |
| `IOrgCatalog.GetAsync` / `ValidateForAssignmentAsync` | IdentityAdmin, OrgStructure | **`OrgDescriptor`** / void | SPI tích hợp (C9) |

**Persistence chung:** `OrgManager`, `OrgCatalog`, `OrgCatalogReader` dùng **`IOrgStore<TOrg>`** — không `IOrgCatalog` trong pipeline ghi của Manager.

#### 5.3.3 Lifecycle hooks (`IOrgCatalogHooks<TOrg>`) — 🟢 catalog slice

Thứ tự **Create / Update / SetActive** (Delete hooks đã khai báo; chờ `DeleteAsync`):

```text
Validate → invariant platform (unique Code, cột chuẩn) → Prepare → SaveChanges
  → Success (nếu OK)
  → (lỗi) gán context.Exception → Error → rethrow
  → Finally (luôn)
```

| Hook | Vai trò |
|------|---------|
| **Validate** | Ném exception để hủy thao tác |
| **Prepare** | Map input / cột host **trước** lưu (sau khi platform gán cột chuẩn lần 1) |
| *(platform)* | Gán lại cột chuẩn lần 2 trước `SaveChanges` — host không “sở hữu” `Code`/`Name`/`TenantId`/`IsActive` |
| **Success** | Side effect sau commit thành công |
| **Error** | Telemetry / compensation khi lỗi (`context.Exception` đã set) |
| **Finally** | Luôn chạy; kiểm tra `Succeeded` / `Exception` |

Mặc định: **`DefaultOrgCatalogHooks<TOrg>`** (no-op, method `virtual`). Host: `.UseHooks<THooks>()` — có thể kế thừa default và override vài hook.

**Không** dùng `IOrgCatalogEntityFactory` / customizer — entity tạo trong `OrgManager` (`Activator` + `Id`; host: ctor public không tham số).

#### 5.3.4 Read model host (`UseReadModel`) — 🟢

| Thành phần | Package |
|------------|---------|
| `IOrgCatalogReadMapper<TOrg, TReadModel>` | `Platform.Orgs` / `Mapping/` |
| `IOrgCatalogReader<TReadModel>` | Read model host — ambient tenant hoặc `tenantId` caller truyền |

Sample: `SampleOrgDetail` + `SampleOrgReadMapper` — **không** subclass `OrgDescriptor`.

**IdentityAdmin** validate `User.OrgId` qua **`IOrgCatalog.ValidateForAssignmentAsync`** — không qua `IOrgManager.GetById`.

**Ambient & persistence**

| Thành phần | Platform deliverable |
|------------|---------------------|
| `Platform.MultiOrg` | Resolvers + `AddCurrentOrg` + unit test parity Multitenancy |
| `ICurrentOrgStore` | **`OrgCatalogCurrentOrgStore`** (module Orgs) + `.UseAsCurrentOrgStore()`; profile `OrgCatalogOrgProfile`; host có thể thay store riêng |
| `IOrgEntity` + EF | Global query filter / scope theo working `OrgId` khi host bật (cùng pipeline `ITenantEntity`) |
| `Platform.Orgs.Api` | REST admin catalog + `[Authorize]` permission keys — opt-in `.UseHttpApi()` |

**Tích hợp bắt buộc khi ship module (không “phase sau”)**

| Consumer | Cách dùng |
|----------|-----------|
| IdentityAdmin | `IOrgCatalog` khi Create/Update user (`OrgId`) |
| Authorization | Operand `Resource.OrgId` / `User.OrgId` — catalog là nguồn validate ID (seed Sample) |
| OrgStructure | `IOrgCatalog.GetAsync` trước khi gắn `OrgId` lên OU |
| OTEL / menu / combobox | `ListAsync` / `GetAsync` qua Abstractions hoặc API |

**Later (product-specific, không chặn platform):** org-user membership table riêng; billing/license per Org; auto-provision OrgStructure template khi Create Org (host orchestration).

### 5.4 Cấu trúc thư mục (đích)

Song song **tenant** (đã có) vs **org** (đích):

| Lớp | Tenant (tham chiếu) | Org (ADR này) |
|-----|---------------------|---------------|
| Marker cột scope | `ITenantEntity` (Domain) | `IOrgEntity` (Domain) — 🟢 |
| Contract ambient + resolver | `ICurrentTenant`, … (Domain) — 🟢 | `ICurrentOrg*`, `IOrgIdResolver*`, **`IRequestScopeResolver`** — 🟢 |
| HTTP scope org-first | — | **D16** — `OrgFirstRequestScopeResolver`, `OrgFirstScopeMiddleware` — 🟢 |
| Package ambient (không CRUD catalog) | `Platform.Multitenancy` — 🟢 | **`Platform.MultiOrg`** — 🟢 |
| Module catalog nghiệp vụ | `Platform.Tenants` (🔴 Proposed) | **`Platform.Orgs`** (module) — 🔴 |
| Query filter EF | `Platform.ORM` + `ITenantEntity` | Extension filter `IOrgEntity` (platform, bật tại host) |

**Không** nhét ambient org vào `Platform.Multitenancy` (C1: multitenancy không ôm lifecycle Org) và **không** đặt `ICurrentOrg*` trong module catalog — cùng ranh giới tenant vs `Platform.Tenants`.

```text
frameworks/Platform.DDD.Domain/
├── Entities/
│   ├── ITenantEntity.cs                 # 🟢 TenantId
│   └── IOrgEntity.cs                    # 🟢 OrgId trên bảng nghiệp vụ (D6)
├── Services/
│   ├── ICurrentTenant.cs                # 🟢 GetIdAsync / GetAsync / Change
│   ├── ICurrentTenantAccessor.cs        # 🟢 AsyncLocal TenantId
│   ├── ICurrentTenantIdentity.cs        # 🟢 profile tenant tối thiểu
│   ├── ICurrentTenantStore.cs           # 🟢 port load profile
│   ├── ICurrentOrg.cs                   # 🟢 mirror ICurrentTenant
│   ├── ICurrentOrgAccessor.cs           # 🟢 mirror ICurrentTenantAccessor
│   ├── ICurrentOrgIdentity.cs           # 🟢 mirror ICurrentTenantIdentity
│   ├── ICurrentOrgStore.cs              # 🟢 mirror ICurrentTenantStore
│   ├── IRequestScopeResolver.cs         # 🟢 RequestScopeResolution (D16)
│   └── IOrgTenantScopeValidator.cs    # 🟢 validate org ∈ tenant
└── DataStorages/
    ├── ITenantIdResolver.cs             # 🟢 resolve TenantId từ HTTP/claim/…
    ├── ITenantIdResolverFactory.cs      # 🟢 + GetTenantIdForOrgScopeAsync
    ├── ITenantDomainLookup.cs           # 🟢 optional — host domain → tenant
    ├── IOrgIdResolver.`cs                # 🟢 mirror ITenantIdResolver
    └── IOrgIdResolverFactory.cs         # 🟢 mirror ITenantIdResolverFactory

frameworks/Platform.Multitenancy/          # 🟢 đã có — pattern tham chiếu, không sửa cho Org
├── Platform.Multitenancy.csproj
├── CurrentTenant.cs                       # ICurrentTenant<TTenant>
├── CurrentTenantAccessor.cs               # ICurrentTenantAccessor
├── CurrentTenantInfo.cs                   # ICurrentTenantIdentity mặc định
├── HostApplicationBuilderExtension.cs     # AddCurrentTenant<TTenant>(), AddTenantIdResolvers()
└── DataStorages/
    ├── HeaderTenantIdResolver.cs
    ├── QueryTenantIdResolver.cs
    ├── UserTenantIdResolver.cs
    ├── HostTenantIdResolver.cs
    ├── TenantIdResolverFactory.cs         # ITenantIdResolverFactory + GetTenantIdForOrgScopeAsync
    └── TenantIdGuidParser.cs

frameworks/Platform.MultiOrg/              # 🟢 mirror Multitenancy, không CRUD catalog
├── Platform.MultiOrg.csproj
├── CurrentOrg.cs                            # ICurrentOrg<TOrg> — ưu tiên IRequestScopeResolver
├── CurrentOrgAccessor.cs                    # ICurrentOrgAccessor
├── CurrentOrgInfo.cs                        # ICurrentOrgIdentity mặc định
├── HostApplicationBuilderExtension.cs       # AddCurrentOrg, AddOrgIdResolvers, AddOrgFirstScopeResolution
├── HostApplicationBuilderWebExtensions.cs # UseOrgFirstScope()
├── Middleware/
│   └── OrgFirstScopeMiddleware.cs           # 403 + BeginScope org/tenant
├── Scope/
│   ├── OrgFirstRequestScopeResolver.cs      # D16 — bảng §5.1.1
│   └── NullOrgTenantScopeValidator.cs       # default; host override qua Orgs
└── DataStorages/
    ├── HeaderOrgIdResolver.cs               # X-Org-Id (working scope D16)
    ├── QueryOrgIdResolver.cs                # chain UoW khi không D16
    ├── OrgDomainIdResolver.cs
    └── OrgIdResolverFactory.cs

modules/orgs/
├── README.md                              # 🔴
├── Platform.Orgs.Abstractions/
│   ├── Platform.Orgs.Abstractions.csproj
│   ├── IOrgCatalog.cs                     # port đọc + validate (D4b); tham số orgId + tenantId
│   └── OrgDescriptor.cs                   # SPI snapshot cột chuẩn (events D14 payload)
├── Platform.Orgs/
│   ├── Platform.Orgs.csproj
│   ├── IOrgCatalogEntity.cs
│   ├── OrgErrorCode.cs
│   ├── Persistence/
│   │   └── IOrgStore.cs                   # 🟢 load/save nội bộ; Manager + Catalog + Read
│   ├── Hooks/
│   │   ├── IOrgCatalogHooks.cs            # 🟢 Validate / Prepare / Success / Error / Finally
│   │   ├── DefaultOrgCatalogHooks.cs
│   │   ├── OrgCatalogHookContextBase.cs
│   │   ├── OrgCatalogCreateHookContext.cs
│   │   ├── OrgCatalogUpdateHookContext.cs
│   │   └── OrgCatalogDeleteHookContext.cs
│   ├── Mapping/
│   │   └── IOrgCatalogReadMapper.cs       # 🟢 entity → TReadModel host
│   ├── Services/
│   │   ├── IOrgManager.cs / OrgManager.cs # 🟢 ghi + Get/List → OrgDescriptor
│   │   ├── IOrgCatalogReader.cs / OrgCatalogReader.cs   # 🟢 Get/List → TReadModel
│   │   ├── OrgCatalog.cs                  # 🟢 IOrgCatalog
│   │   ├── OrgDescriptorMapper.cs
│   │   ├── OrgCatalogOrgProfile.cs        # 🟢 ICurrentOrgIdentity từ catalog
│   │   ├── OrgCatalogCurrentOrgStore.cs   # 🟢 ICurrentOrgStore — .UseAsCurrentOrgStore()
│   │   └── OrgCatalogTenantScopeValidator.cs # 🟢 IOrgTenantScopeValidator (D16)
│   ├── Models/
│   │   ├── CreateOrgInput.cs / UpdateOrgInput.cs / ListOrgsQuery.cs
│   └── Extensions/
│       ├── PlatformOrgsExtensions.cs      # 🟢 AddPlatformOrgs<TOrg, TTenant>()
│       └── PlatformOrgsBuilder.cs         # 🟢 .UseHooks / .UseReadModel / (EF từ satellite)
├── Platform.Orgs.EntityFramework/           # satellite ORM — không ship entity catalog, không migration (C5)
│   ├── Platform.Orgs.EntityFramework.csproj # → Platform.Orgs, Platform.ORM.EntityFramework, EF Core
│   ├── Configuration/
│   │   └── OrgModelBuilderConfigurationOptions.cs   # schema / table prefix / tên bảng mặc định "Org"
│   ├── EntityConfigurations/
│   │   └── OrgCatalogEntityConfiguration.cs         # IEntityTypeConfiguration<TOrg> — index (TenantId, Code), cột chuẩn
│   ├── Extensions/
│   │   ├── OrgModelBuilderExtensions.cs             # ApplyPlatformOrgCatalog<TOrg>(ModelBuilder)
│   │   └── PlatformOrgsEntityFrameworkExtensions.cs # 🟢 UseEntityFramework → IOrgStore
│   └── Persistence/
│       └── OrgStore.cs                              # 🟢 IOrgStore qua IUnitOfWork + IRepository<TOrg>
└── Platform.Orgs.Api/
    ├── Platform.Orgs.Api.csproj
    ├── Controllers/OrgController.cs
    ├── Models/OrgHttpRequests.cs
    └── Extensions/PlatformOrgsApiExtensions.cs   # UseHttpApi()

Platform.sln                                 # 🟡 thêm 4 project (Orgs, Abstractions, EF, Api) + MultiOrg

Sample/                                      # 🔴 mới · 🟡 sửa file có sẵn
├── Entities/
│   └── Org.cs                               # 🟢 IOrgCatalogEntity + TaxCode; OrgEntityConfiguration
├── Organization/
│   ├── SampleOrgInputs.cs                   # 🟢 Create/Update input + SampleOrgCatalogHooks
│   └── SampleOrgReadModel.cs                # 🟢 SampleOrgDetail + SampleOrgReadMapper
├── Persistence/
│   └── TenantDbContext.cs                   # 🟡 DbSet<Org>; ApplyPlatformOrgCatalog<Org>()
├── Extensions/
│   └── SampleOrgsExtensions.cs              # 🟡 AddPlatformOrgs + EF + Hooks + ReadModel
├── Services/
│   └── SampleCurrentOrgStore.cs             # 🔴 stub demo; production dùng CurrentOrgStore + .UseAsCurrentOrgStore()
├── Controllers/
│   └── OrgController.cs                     # 🔴 hoặc dùng Platform.Orgs.Api + map route Sample
├── Program.cs                               # 🟡 gọi SampleOrgsExtensions
├── Sample.csproj                            # 🟡 ProjectReference Platform.Orgs*
└── Migrations/Tenant/
    ├── 20261021172200_AddOrgCatalog.cs      # 🟢
    ├── 20261021172200_AddOrgCatalog.Designer.cs
    └── TenantDbContextModelSnapshot.cs      # 🟢

UnitTest/
├── UnitTest.csproj                          # 🟡 ProjectReference module + Platform.MultiOrg
├── MultiOrg/
│   └── CurrentOrgTests.cs                   # 🟢 MO_01–MO_06
└── Org/
    ├── OrgManagerTests.cs                   # 🟢 T1, T2, T5, T9
    ├── OrgCatalogModelTests.cs              # 🟢 model / limits
    └── OrgApiTests.cs                       # 🔴 HTTP + permission (nếu bật Api)
```

**Host compose (sketch):**

```text
AddCurrentTenant<TTenant>()                    → Platform.Multitenancy
AddCurrentOrg<TOrgProfile>()                   → Platform.MultiOrg
AddPlatformOrgs<Org, CurrentTenantInfo>()    → .UseEntityFramework<Org, CurrentTenantInfo, ITenantUnitOfWork>()
                                               .UseHooks<SampleOrgCatalogHooks>()
                                               .UseReadModel<SampleOrgDetail, SampleOrgReadMapper>()
                                               .UseAsCurrentOrgStore()
                                               # host: AddCurrentOrg<OrgCatalogOrgProfile>() trước chain
                                               # Later: .UseHttpApi()
```

**Không có:** `ParentOrgId` trên catalog; OrgStructure runtime trong Orgs; migration trong package EF; accessor/resolver trong module catalog (thuộc `Platform.MultiOrg`).

#### 5.4.1 `Platform.Orgs.EntityFramework` — file và ý nghĩa

| File | Ý nghĩa |
|------|---------|
| **`Platform.Orgs.EntityFramework.csproj`** | Package satellite: reference `Platform.Orgs` + ORM/EF; **không** reference Api, MultiOrg, Abstractions consumer. |
| **`Configuration/OrgModelBuilderConfigurationOptions.cs`** | Options bind section (prefix bảng, schema) — host gọi `ConfigurePlatformOrgs` trong `OnModelCreating` hoặc extension đọc options. |
| **`EntityConfigurations/OrgCatalogEntityConfiguration.cs`** | Cấu hình EF **generic** cho `TOrg : ITenantEntity` (host entity): PK `Id`, `TenantId`, `Code`, `Name`, `IsActive`, unique index `(TenantId, Code)`. **Không** chứa class entity concrete. |
| **`Extensions/OrgModelBuilderExtensions.cs`** | `modelBuilder.ApplyPlatformOrgCatalog<TOrg>(options?)` — một dòng trong `DbContext` host thay vì copy fluent API. |
| **`Extensions/PlatformOrgsEntityFrameworkExtensions.cs`** | `UseEntityFramework<TOrg, TTenant, TUnitOfWork>()`: đăng ký **`OrgStore<TOrg,TUnitOfWork>`** làm **`IOrgStore<TOrg>`**. |
| **`Persistence/OrgStore.cs`** | Implement **`IOrgStore`**: Find/List/CodeExists/Add/SaveChanges qua **`IUnitOfWork.GetRepositoryAsync`**. |

**Host-owned (không nằm trong package EF):** class `Org : BaseEntity<Guid>, ITenantEntity` + migration + `DbSet<Org>` trong `TenantDbContext` (Sample). Host có thể map bằng `ApplyPlatformOrgCatalog<Org>()` **hoặc** `IEntityTypeConfiguration<Org>` riêng trong `Sample/Entities/Org.cs` (pattern Student).

**Không có trong package EF:** `DbContext`, migration, `DbSet`, entity `Org` mặc định (khác Setting có `Entities/Setting.cs` quick-start — Orgs ADR chọn **chỉ** host entity).

| Package | Được reference | Không reference |
|---------|----------------|-----------------|
| `Platform.MultiOrg` | Domain | `Platform.Orgs`, EF catalog |
| `Platform.Orgs.Abstractions` | BCL | `Platform.Orgs`, EF, ASP.NET |
| `Platform.Orgs` | Domain, Abstractions | `Platform.MultiOrg` (bridge store đăng ký Host) |
| `Platform.Orgs.EntityFramework` | `Platform.Orgs`, ORM EF | OrgStructure, migration host |
| `Platform.Orgs.Api` | `Platform.Orgs`, ASP.NET | — |
| `Platform.OrgStructure` | **Abstractions** (`IOrgCatalog`) | `Platform.Orgs` runtime |
| `Platform.Modules.IdentityAdmin` | **Abstractions** | `Platform.Orgs` runtime |

---

## 6. Hệ quả

### Tích cực

- Ranh giới rõ: **Tenant (đại lý)** vs **Org (khách cuối)** vs **OrgStructure (nội bộ)**.
- Catalog chuẩn cho `User.OrgId` và `Resource.OrgId` trong ABAC.
- Cùng pattern Tenants/Setting: Atomic, host-owned schema.

### Chi phí

- Thêm module + Sample; Host compose thêm `AddPlatformOrgs`.
- Host bật **D16** phải gửi **`X-Org-Id`** (và tenant theo rule §5.1.1); client/portal cần đồng bộ header.

### Rủi ro

| Rủi ro | Mitigation |
|--------|------------|
| Nhầm Org vs LegalEntity | Doc + OrgStructure ADR; không `ParentOrgId` trên Org |
| Cross-Org leak trong tenant | `IOrgEntity` + Data Scope; test T* |
| User `OrgId` trỏ Org inactive | `IOrgManager` validate khi gán; login policy |

---

## 7. Confirm

Đã chốt 2026-10-25.

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D10, C1–C8, phương án O3? | Có, kể cả D11–D19 và C9–C10 |
| Q2 | Tên package | `Platform.Orgs`, `Platform.Orgs.Abstractions`, `Platform.Orgs.EntityFramework`, `Platform.Orgs.Api` |
| Q3 | Facade | `IOrgManager`. Đọc tích hợp qua `IOrgCatalog` |
| Q4 | Marker `IOrgEntity` | Giữ trong Domain, cạnh `ITenantEntity` |
| Q5 | Ai thấy Org nào | Org của tenant hiện tại và của tenant thấp hơn. Không thấy ngang, không thấy lên |
| Q6 | `Platform.MultiOrg` | Ship cùng module Orgs |
| Q8 | Xóa Org | Vô hiệu hóa. Không hard delete |
| Q12 | Seed Org | Không. Bỏ `VNA_DEFAULT` |
| Q13 | Header | API vận hành theo luật root. API sản phẩm giữ resolver host (D19) |
| Q10 | Cột host | Giữ hook Prepare và read mapper |
| Q11 | Catalog events | Làm trong lượt platform. Chi tiết [ADR events](./2026-10-21_platform_orgs-catalog-events.md) |

- [x] Accept D1–D19, C1–C10, O3
- [x] Tên package và `IOrgManager` / `IOrgCatalog`
- [x] `IOrgEntity` ở Domain
- [x] Org trong DB tenant; danh sách nhìn xuống cây tenant
- [x] Vô hiệu hóa, không seed Org
- [x] Hai cửa HTTP scope

---

## 8. Test cases *(khi implement)*

### 8.1 Smoke

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T1 | Host `AddPlatformOrgs` + Create Org under tenant X | Persist; `TenantId` = X; cột host (`TaxCode`) qua Prepare hook | 🟢 `OrgManagerTests.Create_PersistsTenantFromAmbient_AndHostColumnViaPrepareHook` |
| T2 | List Org khi ambient tenant X | Chỉ A,B; không C,D (tenant Y) | 🟢 `OrgManagerTests.List_ReturnsOnlyAmbientTenant` |

### 8.2 Regression

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T3 | Authorization seed / `User.OrgId` Sample | Không gãy khi thêm bảng Org | 🟡 |
| T4 | `ITenantEntity` query filter hiện có | Không đổi semantic tenant | 🟢 |

### 8.3 Mới

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T5 | SetActive=false → gán user mới | Reject | 🟢 `Validate_RejectsInactive_AndOrgOutsideTenant` |
| T6 | `IOrgCatalog.ValidateForAssignmentAsync` | Org sai tenant / inactive → fail | 🟢 cùng test T5 |
| T7 | `ICurrentOrg` R2 + resolver | Mirror test Multitenancy | 🟢 `CurrentOrgTests` MO_01–MO_06 |
| T8 | EF filter `IOrgEntity` | List không lộ Org khác khi bật scope | 🟢 `OrgEntityFilterTests` |
| T9 | `IOrgCatalogReader` + mapper | `TaxCode` (hoặc cột host) trên read model sau Create | 🟢 `OrgManagerTests.CatalogReader_ReturnsHostFieldsViaMapper` |
| T10 | D16 rule 1 — không header, có domain tenant | **403** | 🟢 `OrgFirstScopeResolverTests.Rule1_No_Headers_Forbidden_Even_With_Tenant_Domain` |
| T11 | D16 rule 2 — chỉ `X-Org-Id`, tenant từ domain | Allowed + validate | 🟢 `Rule2_Org_Header_Tenant_From_Domain` |
| T12 | D16 rule 3 — `X-Tenant-Id` không org | **403** | 🟢 `Rule3_Tenant_Header_Without_Org_Forbidden` |
| T13 | D16 rule 4 — cả hai header + validate fail | Allowed / **403** | 🟢 `Rule4_Both_Headers`, `Validation_Failure_Forbidden` |

---

## 9. Kế hoạch triển khai (platform)

Giao **một vertical slice platform** — sản phẩm compose một lần, không “CRUD trước, MultiOrg sau”.

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 (D1–D13, C1–C10) | Accepted | — |
| 1 | **Domain** `ICurrentOrg*` + `IOrgIdResolver*` | Build + contract doc | 0 — 🟢 |
| 2 | **`Platform.MultiOrg`** + D16 scope + `UnitTest/MultiOrg` | T7, T10–T13 | 1 — 🟢 |
| 3 | **Abstractions** `IOrgCatalog` + **Orgs** Manager/Catalog/Store/Hooks/ReadMapper + EF + DI builder | T1–T2, T6, T9 | 0 — 🟡 slice catalog 🟢 |
| 3b | **Catalog events** (Abstractions + publisher + Manager) — [ADR](./2026-10-21_platform_orgs-catalog-events.md) | E2 tests | 3; không chặn 4–7 |
| 4 | **ORM** filter `IOrgEntity` + test T8 | T8 | 3 |
| 5 | **Api** package + permission seed pattern + Sample compose đầy đủ | T3, HTTP | 3; Authorization 🟡 |
| 6 | **IdentityAdmin** → `IOrgCatalog` (csproj Abstractions only) | Create/Update user OrgId | 3 |
| 7 | Skill `orgs-dotnet` + README module | Docs | 5 |

**Thứ tự vs OrgStructure:** Orgs platform **trước** OrgStructure (phụ thuộc `IOrgCatalog`). Song song Tenants module được.

---

## 10. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** 2026-10-25 (D1–D19, C1–C10, O3) |
| Implementation | 🟡 Catalog + MultiOrg + EF org filter + Api + events + IdentityAdmin + Sample. Host có `Org.View` / `Org.Manage`. Không seed org. `ListAsync` gộp tenant hiện tại và tenant con (D17). Không hard delete (`98306`, không có HTTP DELETE). **🔴** `OrgApiTests`, skill `orgs-dotnet` |
| Blocked by | Không. Phần đỏ: test HTTP và skill |
| Unblocks | Catalog Org cho RBAC; module OrgStructure |
| Owner | Platform |
