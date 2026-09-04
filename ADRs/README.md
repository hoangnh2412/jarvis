# ADR Index — Jarvis

Chỉ mục Architecture Decision Records và tài liệu kế hoạch trong `ADRs/`.

**Chú thích:** 🟢 xong / accepted+implemented · 🟡 accepted hoặc đang làm, còn việc · 🔴 proposed / chưa làm · 📄 tham chiếu (rules / template / lịch sử)

**Viết ADR mới:** copy [`adr-template.md`](./adr-template.md) → `YYYY-MM-DD-adr-<slug>.md`. Mã mục chỉ dùng **P / C / O / D / Q / T** (theo vai trò); không dùng prefix theo chủ đề (E, G, M…). ADR đổi code: §8 bắt buộc **smoke + regression** (test mới chỉ khi cần). Chi tiết trong template.

---

## ADR (quyết định kiến trúc)

| Trạng thái | Ngày | ADR |
|------------|------|-----|
| 🟢 Accepted + Implemented | 2026-08-01 · D5 code 2026-08-06 | [Tách `ICurrentUser` / `ICurrentTenant`, giữ `IWorkContext`](./2026-08-01-adr-current-user-tenant.md) — D1–D5 Done. Packages: [Multitenancy](./2026-08-06-adr-jarvis-multitenancy-package.md) + [split Phase 2–4](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) 🟢. |
| 🟢 Accepted + Implemented | 2026-08-01 · implement 2026-08-06 | [Tech debt — Tách Enrich Log/Trace khỏi Domain](./2026-08-01-adr-techdebt-otel-enrichment-out-of-ddd.md) — `Jarvis.OpenTelemetry` + bridge `Jarvis.OpenTelemetry.DDD` |
| 🟢 Accepted + Implemented (Phase 2–5) | 2026-08-01 · Phase 3–4 2026-08-06 · Phase 5 2026-08-07 | [Tech debt — Tách Current User / Tenant khỏi Domain (packages)](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) — Auth + Multitenancy; Phase 5 resolvers 🟢 |
| 🟢 Accepted + Implemented | 2026-08-06 · Accept §7 + implement 2026-08-06 | [Tạo `Jarvis.Multitenancy`](./2026-08-06-adr-jarvis-multitenancy-package.md) — `CurrentTenant*` + `AddCurrentTenant`; EF bỏ accessor TryAdd |
| 🟢 Accepted + Implemented | 2026-08-06 · Accept §8 + implement 2026-08-07 | [Generic `ICurrentTenant<TTenant>`](./2026-08-06-adr-generic-current-tenant.md) — mirror `ICurrentUser<TUser>`; **không** `IWorkContext` |
| 🟢 Accepted + Implemented | 2026-08-06 · Accept §7 + Phase A–E 2026-08-07 | [`Jarvis.Multitenancy.EntityFramework`](./2026-08-06-adr-jarvis-multitenancy-entityframework.md) — satellite Persistence; opt-in `AddMultitenancyEntityFramework` |
| 🔴 Proposed | 2026-08-07 | [Module quản lý Tenant (`Jarvis.Tenants`)](./2026-08-07-adr-jarvis-tenants-module.md) — catalog/CRUD; **không** gộp vào Multitenancy (pattern Setting) |
| 🟢 Accepted + Implemented | 2026-08-07 · Accept §7 + Phase B–D 2026-08-10 | [Family `Jarvis.ORM.*`](./2026-08-07-adr-jarvis-orm-packages.md) — `Jarvis.ORM.EntityFramework` (giữ `AddEntityFramework`) + `Jarvis.ORM.Dapper` |
| 🔴 Proposed | 2026-08-07 | [Base CRUD Application Service](./2026-08-07-adr-jarvis-crud-app-service.md) — `ICrudAppService` / `CrudAppService` trong `Jarvis.DDD.Application`; bổ sung CQRS |
| 🔴 Proposed | 2026-08-07 | [FluentValidation cho Application input](./2026-08-07-adr-jarvis-fluentvalidation.md) — chuẩn validate CQRS/CRUD; map 400 `BaseResponse` |
| 🟢 MVP + D12 implemented · 🔄 boundary superseded | 2026-08-11 · amend 2026-08-12 | [In-app Notification Core](./2026-08-11-adr-jarvis-notifications.md) — MVP persist-first + SignalR + REST/FE; **layout D12 superseded** bởi [Realtime vs Inbox](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md) |
| 🟡 Accepted (chưa implement code) | 2026-08-12 | [Framework Realtime vs Module Inbox](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md) — framework = `Jarvis.Realtime*`; inbox store + AppService → `modules/notifications` |
| 🟢 Accepted + Implemented | 2026-08-12 · Accept §7 + implement | [`IStorageContext.TenantId` private](./2026-08-12-adr-storage-context-tenant-id-private.md) — app = `ICurrentTenant`; `HasTenantId` internal; regression `MultitenancyEfTests` 🟢 |
| 🟡 Accepted (chưa implement code) | 2026-08-13 | [Tách `Jarvis.Modules.Setting.Abstractions`](./2026-08-13-adr-jarvis-setting-abstractions.md) — SPI Library mỏng; **không** đưa vào DDD.Domain |

### Ưu tiên đang mở

1. 🔴 [Base CRUD AppService](./2026-08-07-adr-jarvis-crud-app-service.md) — foundation Application cho entity CRUD đơn giản  
2. 🔴 [FluentValidation](./2026-08-07-adr-jarvis-fluentvalidation.md) — validate input Application (nên làm cùng / trước hook CRUD)  
3. 🔴 Split Phase 6 — Skill / README Host (`AddCurrentUser` + `AddCurrentTenant`)  
4. 🔴 [`Jarvis.Tenants`](./2026-08-07-adr-jarvis-tenants-module.md) — module catalog/CRUD (độc lập lịch; làm khi có nhu cầu sản phẩm)  
5. 🟡 [Realtime vs Inbox boundary](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md) — Accepted; refactor code (move store + rename Realtime)  
6. 🟡 [Setting.Abstractions](./2026-08-13-adr-jarvis-setting-abstractions.md) — Accepted; tách SPI trước/cùng module Notifications Define  
7. 🟢 [`TenantId` private trên storage](./2026-08-12-adr-storage-context-tenant-id-private.md) — Implemented 2026-08-12  
8. 🟢 [Notification MVP](./2026-08-11-adr-jarvis-notifications.md) — runtime 🟢; layout xem ADR Realtime/Inbox

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
| 📄 Architecture software | [architecture-software.md](./architecture-software.md) — Clean Architecture + Module Atomic khi sửa `Jarvis.*`; ranh giới `frameworks/` · `modules/` · `autotest/` (§0.2) |
| 📄 Autotest SAD | [Architecture autotest](./architecture-autotest.md) — core vs project, UI/API, layout engine-agnostic; NPM `@jarvis/autotest*` |
| 📄 Template skill | [Template skill Jarvis .NET](./template-skill.md) |
| 📄 UI kit skill | [Tabler UI kit](./tabler-uikit-skill.md) |
| 📄 Lịch sử | [Tutorial index](./tutorial-index.md) — hướng dẫn AI agent (đã chuyển trọng tâm sang `.opencode/`) |

---

## Liên quan

- Minipower / AI skills ADR: [`../ai-skills/ADRs/README.md`](../ai-skills/ADRs/README.md)
