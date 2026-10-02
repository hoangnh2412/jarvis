# ADR Index — Platform

Chỉ mục Architecture Decision Records và tài liệu kế hoạch trong `ADRs/`.

**Chú thích:** 🟢 xong / accepted+implemented · 🟡 accepted hoặc đang làm, còn việc · 🔴 proposed / chưa làm · 📄 tham chiếu (rules / template / lịch sử)

**Viết ADR mới:** copy [`adr-template.md`](./adr-template.md) → `yyyy-MM-dd_{code}_{name}.md`. Mã mục chỉ dùng **P / C / O / D / Q / T** (theo vai trò); không dùng prefix theo chủ đề (E, G, M…). ADR đổi code: §8 bắt buộc **smoke + regression** (test mới chỉ khi cần). Chi tiết trong template.

---

## ADR (quyết định kiến trúc)

| Trạng thái | Ngày | ADR |
|------------|------|-----|
| 🟢 Accepted + Implemented | 2026-08-01 · D5 code 2026-08-06 | [Tách `ICurrentUser` / `ICurrentTenant`, giữ `IWorkContext`](./2026-08-01-adr-current-user-tenant.md) — D1–D5 Done. Packages: [Multitenancy](./2026-08-06-adr-platform-multitenancy-package.md) + [split Phase 2–4](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) 🟢. |
| 🟢 Accepted + Implemented | 2026-08-01 · implement 2026-08-06 | [Tech debt — Tách Enrich Log/Trace khỏi Domain](./2026-08-01-adr-techdebt-otel-enrichment-out-of-ddd.md) — `Platform.OpenTelemetry` + bridge `Platform.OpenTelemetry.DDD` |
| 🟢 Accepted + Implemented (Phase 2–5) | 2026-08-01 · Phase 3–4 2026-08-06 · Phase 5 2026-08-07 | [Tech debt — Tách Current User / Tenant khỏi Domain (packages)](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) — Auth + Multitenancy; Phase 5 resolvers 🟢 |
| 🟢 Accepted + Implemented | 2026-08-06 · Accept §7 + implement 2026-08-06 | [Tạo `Platform.Multitenancy`](./2026-08-06-adr-platform-multitenancy-package.md) — `CurrentTenant*` + `AddCurrentTenant`; EF bỏ accessor TryAdd |
| 🟢 Accepted + Implemented | 2026-08-06 · Accept §8 + implement 2026-08-07 | [Generic `ICurrentTenant<TTenant>`](./2026-08-06-adr-generic-current-tenant.md) — mirror `ICurrentUser<TUser>`; **không** `IWorkContext` |
| 🟢 Accepted + Implemented | 2026-08-06 · Accept §7 + Phase A–E 2026-08-07 | [`Platform.Multitenancy.EntityFramework`](./2026-08-06-adr-platform-multitenancy-entityframework.md) — satellite Persistence; opt-in `AddMultitenancyEntityFramework` |
| 🔴 Proposed | 2026-08-07 | [Module quản lý Tenant (`Platform.Tenants`)](./2026-08-07-adr-platform-tenants-module.md) — catalog/CRUD; **không** gộp vào Multitenancy (pattern Setting) |
| 🟢 Accepted + Implemented | 2026-08-07 · Accept §7 + Phase B–D 2026-08-10 | [Family `Platform.ORM.*`](./2026-08-07-adr-platform-orm-packages.md) — `Platform.ORM.EntityFramework` (giữ `AddEntityFramework`) + `Platform.ORM.Dapper` |
| 🔴 Proposed | 2026-08-07 | [Base CRUD Application Service](./2026-08-07-adr-platform-crud-app-service.md) — `ICrudAppService` / `CrudAppService` trong `Platform.DDD.Application`; bổ sung CQRS |
| 🔴 Proposed | 2026-08-07 | [FluentValidation cho Application input](./2026-08-07-adr-platform-fluentvalidation.md) — chuẩn validate CQRS/CRUD; map 400 `BaseResponse` |
| 🟢 MVP + D12 implemented · 🔄 boundary superseded | 2026-08-11 · amend 2026-08-12 | [In-app Notification Core](./2026-08-11-adr-platform-notifications.md) — MVP persist-first + SignalR + REST/FE; **layout D12 superseded** bởi [Realtime vs Inbox](./2026-08-12-adr-platform-realtime-inbox-boundary.md) |
| 🟡 Accepted (chưa implement code) | 2026-08-12 | [Framework Realtime vs Module Inbox](./2026-08-12-adr-platform-realtime-inbox-boundary.md) — framework = `Platform.Realtime*`; inbox store + AppService → `modules/notifications` |
| 🟢 Accepted + Implemented | 2026-08-12 · Accept §7 + implement | [`IStorageContext.TenantId` private](./2026-08-12-adr-storage-context-tenant-id-private.md) — app = `ICurrentTenant`; `HasTenantId` internal; regression `MultitenancyEfTests` 🟢 |
| 🟡 Accepted (chưa implement code) | 2026-08-13 | [Tách `Platform.Modules.Setting.Abstractions`](./2026-08-13-adr-platform-setting-abstractions.md) — SPI Library mỏng; **không** đưa vào DDD.Domain |
| 🟡 Accepted · đang implement | 2026-08-14 · Accept §7 2026-09-29 · cập nhật 2026-10-03 | [RBAC + ABAC Authorization + ranh giới Account / IdentityAdmin](./2026-08-14_platform_rbac-abac-authorization.md) — phân quyền `Platform.Authorization` (core không EF) + `Platform.Authorization.EntityFramework`, store danh tính `Platform.Authentication.Identity`, `modules/identity` (Account, IdentityAdmin). Engine RBAC+ABAC + Role Governance 🟢; operand registry + seed nghiệp vụ do sản phẩm đóng góp (C7, V5) 🟢; Data Scope → SQL `WHERE` (§5.11) 🟢; Sample demo + code mẫu endpoint chạy trên PostgreSQL (§11.8) 🟢; API, cache, endpoint Lexora, seed lúc start Lexora 🔴 (xem §10) |

### Ưu tiên đang mở

1. 🔴 [Base CRUD AppService](./2026-08-07_platform_crud-app-service.md) — foundation Application cho entity CRUD đơn giản  
2. 🔴 [FluentValidation](./2026-08-07_platform_fluentvalidation.md) — validate input Application (nên làm cùng / trước hook CRUD)  
3. 🔴 Split Phase 6 — Skill / README Host (`AddCurrentUser` + `AddCurrentTenant`)  
4. 🔴 [`Platform.Tenants`](./2026-08-07-adr-platform-tenants-module.md) — module catalog/CRUD (độc lập lịch; làm khi có nhu cầu sản phẩm)  
5. 🟡 [Realtime vs Inbox boundary](./2026-08-12-adr-platform-realtime-inbox-boundary.md) — Accepted; refactor code (move store + rename Realtime)  
6. 🟡 [Setting.Abstractions](./2026-08-13-adr-platform-setting-abstractions.md) — Accepted; tách SPI trước/cùng module Notifications Define  
7. 🟢 [`TenantId` private trên storage](./2026-08-12-adr-storage-context-tenant-id-private.md) — Implemented 2026-08-12  
8. 🟢 [Notification MVP](./2026-08-11-adr-platform-notifications.md) — runtime 🟢; layout xem ADR Realtime/Inbox
9. 🟡 [RBAC + ABAC Authorization](./2026-08-14_platform_rbac-abac-authorization.md) — còn: API Account / IdentityAdmin, endpoint Lexora gọi engine + `DataScopeFilter` (mẫu: Sample §11.8), áp migration Identity + gọi seed lúc start (Lexora), cache/snapshot, chốt vấn đề mở V1–V4 (§7.2) và role/policy nghiệp vụ Lexora (Phụ lục D)

---

## Refactor / kế hoạch module

| Trạng thái | Tài liệu |
|------------|----------|
| 🟡 Auth — Phase 0–3 + Basic xong; OpenIddict chưa; Cognito stub; 29 tests pass | [Refactor Authentication](./refactor-authentication.md) |
| 🟢 Cache — hoàn tất (merge-ready) | [Refactor Cache plan](./refactor-cache-plan.md) |
| 🟡 Blob — review / kế hoạch (branch `refactor-blob-storing`) | [Refactor Blob Storing](./refactor-blob-storing.md) |

---

## Tham chiếu (không phải ADR quyết định)

| Loại | Tài liệu |
|------|----------|
| 📄 **Template ADR** | [adr-template.md](./adr-template.md) — Nygard/MADR + quy ước mã P/C/O/D/Q/T + §8 smoke/regression |
| 📄 Rules | [Architecture rules](./architecture-rules.md) — Clean Architecture + Module Atomic khi sửa `Platform.*` |
| 📄 Phân tích | [Thiết kế phân quyền RBAC + ABAC trên ASP.NET Identity](./identity-requirement.md) — tài liệu nền cho [ADR RBAC + ABAC](./2026-08-14_platform_rbac-abac-authorization.md) |
| 📄 Template skill | [Template skill Platform .NET](./template-skill.md) |
| 📄 UI kit skill | [Tabler UI kit](./tabler-uikit-skill.md) |
| 📄 Lịch sử | [Tutorial index](./tutorial-index.md) — hướng dẫn AI agent (đã chuyển trọng tâm sang `.opencode/`) |

---

## Liên quan

- Minipower / AI skills ADR: [`../ai-skills/ADRs/README.md`](../ai-skills/ADRs/README.md)
