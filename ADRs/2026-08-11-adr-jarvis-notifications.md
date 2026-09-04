# ADR — In-app Notification Core (`Jarvis.Notifications.*`)

> **Trạng thái:** 🟢 **Accepted + Implemented** (MVP + D12) · **🔄 Package boundary D1/D4/D8/D12 superseded** bởi [2026-08-12-adr-jarvis-realtime-inbox-boundary](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md) (framework → **Realtime**; inbox store → **module**).  
> **Ngày:** 2026-08-11 · Accept: 2026-08-11 · Implement MVP: 2026-08 · Implement D12: 2026-08-11 · Boundary amend: 2026-08-12  
> **Loại:** Module Atomic / package boundary / realtime delivery  
> **Liên quan:** [current-user-tenant](./2026-08-01-adr-current-user-tenant.md) (🟢 D5), [Multitenancy](./2026-08-06-adr-jarvis-multitenancy-package.md) (🟢), [architecture-software.md](./architecture-software.md) (§0.1–0.3, §3.4 fluent builder), [Realtime vs Inbox boundary](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md)  
> **Phạm vi:** quyết định kiến trúc **in-app notification** dùng chung (MVP): persist-first + SignalR + REST/FE; ranh giới Host. **Layout package sau 2026-08-12:** xem ADR Realtime/Inbox — không dùng D12 làm target mới.  
> **Ngoài phạm vi:** email/SMS/push mobile; Event Bus inbound ERP; SQL store production; Authorization/RBAC chi tiết; definition/template registry; toast pipeline đầy đủ.  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm

---

## 1. Bối cảnh

Jarvis cần notification dùng chung cho business module và host app: khi sự kiện nghiệp vụ xảy ra, tạo thông báo, lưu để xem lại (kể cả offline trong retention), và đẩy realtime khi user online.

| Thành phần | Đường dẫn / package | Vai trò hiện tại |
|------------|---------------------|------------------|
| Framework core | `frameworks/Jarvis.Notifications/` · `PackageId` `Jarvis.Notifications` · namespace `Jarvis.Notifications.*` | Contracts, `NotificationAppService`, options, `AddCoreNotifications()` |
| Framework SignalR | `frameworks/Jarvis.Notifications.SignalR/` · namespace `Jarvis.Notifications.SignalR.*` | Hub, `HubSignalRNotifier`, `UseSignalR()`, `MapCoreNotificationHub()` |
| Framework Redis | `frameworks/Jarvis.Notifications.Redis/` · namespace `Jarvis.Notifications.Redis.*` | `RedisNotificationStore`, `UseRedisStore()` (opt-in) |
| Module API | `modules/notifications/Jarvis.Modules.Notifications.Api/` | REST `api/notifications`, `AddNotificationModule()` — reference **core only** |
| Frontend | `modules/notifications/frontend/` · `@jarvis/notifications` | Bell, center, hook, SignalR client |
| Host | Sample `Program.cs` | `AddCoreNotifications().UseSignalR().UseRedisStore().AddNotificationAppServiceWithRealtime<…>()` + `MapCoreNotificationHub` |

**Giả định vận hành:** Host đã có auth + `AddCurrentUser` / `AddCurrentTenant`; Redis connection qua `SignalR:Store:Configuration` (fallback `SignalR:Redis:Configuration`); peak concurrent ~hàng trăm; fan-out `NotifyUsersAsync` tối đa 16 concurrent.

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Không có notification dùng chung → mỗi module tự invent | Lệch UX, trùng Redis/SignalR wiring |
| P2 | Cần persist + realtime + REST/FE trong một mô hình | Không chốt ranh giới → god-package hoặc copy-paste Host |
| P3 | Scale-out multi-host SignalR | Thiếu backplane / sticky → mất message cross-node |
| P4 | Identity scope lệch pattern Jarvis | Duplicate claim parsing (legacy `NotificationClaims`) |
| P5 | ~~Atomic debt~~ **Đã xử lý (D12):** gộp store + SignalR + AppService; auto-`UseRedisStore()`; namespace lệch PackageId | Tách 3 package; Host gọi rõ satellite; namespace `Jarvis.Notifications.*` |
| P6 | Chưa skill `*-dotnet` cho module | Lệch §0.3 — agent/Host thiếu workflow chuẩn |
| P7 | Chưa idempotency / archive / sync-after-reconnect / toast | Duplicate khi retry; UX thiếu sau reconnect |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Dependency một chiều; Domain **không** reference Notifications; module API → framework **core** contracts |
| C2 | Scope tenant/user từ **`ICurrentUser<TUser>` / `ICurrentTenant<TTenant>`** (không `IWorkContext` trên đường notification) — khớp [ADR current-user-tenant](./2026-08-01-adr-current-user-tenant.md) |
| C3 | REST **không** nhận tenant/user trên query — lấy từ ambient context |
| C4 | Persistence-first: `INotificationStore.SaveAsync` **trước** `ISignalRNotifier`; lỗi push chỉ log — REST vẫn đọc được |
| C5 | Redis key scoped `{tenantId:userId}` (hash tag Cluster); retention cấu hình được |
| C6 | Host là composition root: gọi `Add*` / `Use*` / `Map*`; không nhét business rule nguồn vào framework |
| C7 | Extension seam giữ được: `INotificationStore`, `ISignalRNotifier`, `INotificationAppService` thay impl sau (SQL / Event Bus) mà không đổi REST contract |
| C8 | DI library: `TryAdd*` cho AppService/Notifier; Host override bằng `Add*` sau |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | Redis store (chọn) | Sorted set + string JSON + TTL | Nhanh, feed/unread native, retention đơn giản | Khó báo cáo / full-text; phụ thuộc Redis HA |
| O2 | EF / SQL store ngay | Relational history | Query mạnh | Chậm MVP; lệch inbox tạm thời |
| O3 | SignalR **full payload** (chọn MVP) | Hub gửi cả message | FE cập nhật không round-trip | Payload lớn; duplicate enrich BE/FE |
| O4 | SignalR **signal-only** (`created` + id) | REST authoritative | Hub nhẹ; một nguồn sự thật UI | Thêm GET mỗi lần nhận |
| O5 | Direct `INotificationAppService` (chọn) | In-process từ business code | Đơn giản, debug tuần tự | Coupling assembly contracts (core only) |
| O6 | Event Bus inbound first | Decouple producer | Scale / multi-host producer | Phức tạp sớm; ngoài phạm vi MVP |
| O7 | `IWorkContext` / NotificationClaims | Self-contained module | Ít Host setup | Duplicate identity; lệch D5 |
| O8 | Monolith FE hook (chọn) | `useNotifications` + optional controller | Nhẹ, embed bell/center | Toast cross-route cần callback |
| O9 | **Atomic split (chọn D12):** core + `*.SignalR` + `*.Redis` satellite | Khớp §0.2 | Rõ opt-in; module API chỉ kéo core | Churn PackageId / DI (đã làm) |

**Đã ship:** O1 + O3 + O5 + O8 + **O9**; identity = C2.

---

## 5. Quyết định

Chúng ta cung cấp **in-app notification** theo modular monolith: framework core + satellite SignalR/Redis, module REST, FE package; Host compose opt-in; persist Redis trước, push realtime sau.

| # | Quyết định | Chi tiết | Status |
|---|------------|----------|--------|
| D1 | Package layout | `Jarvis.Notifications` (core) + `Jarvis.Notifications.SignalR` + `Jarvis.Notifications.Redis` + `Jarvis.Modules.Notifications.Api` + `@jarvis/notifications` | 🟢 |
| D2 | Producer API | `INotificationAppService.NotifyUserAsync` / `NotifyUsersAsync` — direct invocation | 🟢 |
| D3 | Persistence-first | Save store → Publish SignalR; push fail ≠ mất data | 🟢 |
| D4 | Store | Redis (`RedisNotificationStore`) qua `INotificationStore` satellite; key prefix `signalr:notification` | 🟢 |
| D5 | Realtime | ASP.NET Core SignalR hub `/hubs/notifications`; method `notification`; groups `user:{id}`, `tenant:{id}` | 🟢 |
| D6 | Hub payload MVP | **Full** `SignalRNotificationMessage` (O3); revisit O4 khi payload/bandwidth thành vấn đề | 🟢 |
| D7 | Identity | `AddNotificationAppServiceWithRealtime<TUser,TTenant>` inject `ICurrentUser` / `ICurrentTenant` | 🟢 |
| D8 | DI Host | `AddCoreNotifications()` → `NotificationsBuilder`; `UseSignalR()` / `UseRedisStore()` opt-in; `AddNotificationModule()`; `MapCoreNotificationHub<TUser,TTenant>()` | 🟢 |
| D9 | Scale-out | Sticky session + optional Redis backplane (`SignalR:UseRedisBackplane`) | 🟢 config |
| D10 | FE | React hook local state + optimistic mark read; auth qua `configureNotificationAuth` | 🟢 |
| D11 | JSON `data` | Embedded object trên REST (`JsonElementNewtonsoftConverter`) — không `valueKind` wrapper | 🟢 |
| D12 | Atomic split | Core + SignalR + Redis satellite; namespace `Jarvis.Notifications.*`; không auto-`UseRedisStore` trong core | 🟢 code · **🔄 superseded** (layout) bởi ADR 2026-08-12 |
| D13 | Skill | Skill `notifications-dotnet` — workflows init/add | 🔴 · xem thêm `realtime-dotnet` trên ADR 2026-08-12 |


### 5.1 Sơ đồ (runtime)

```text
Business / Job ──► INotificationAppService          (Jarvis.Notifications)
                      ├─► INotificationStore      (Jarvis.Notifications.Redis — opt-in)
                      └─► ISignalRNotifier        (Jarvis.Notifications.SignalR — opt-in)
                              └─► Hub → Client (@jarvis/notifications)
REST Controller ──► INotificationAppService       (Jarvis.Modules.Notifications.Api → core)
Host: AddCurrentUser + AddCurrentTenant + AddNotificationModule
    + AddCoreNotifications().UseSignalR().UseRedisStore()
      .AddNotificationAppServiceWithRealtime<TUser,TTenant>()
    + MapCoreNotificationHub<TUser,TTenant>()
```

```mermaid
sequenceDiagram
    autonumber
    participant Biz as Business
    participant App as NotificationAppService
    participant Store as RedisNotificationStore
    participant RT as HubSignalRNotifier
    participant Client as Web Client

    Biz->>App: NotifyUserAsync(message)
    App->>App: Resolve ICurrentUser / ICurrentTenant
    App->>Store: SaveAsync
    App->>RT: PublishToUserAsync
    RT-->>Client: notification (full payload)
    Note over App,RT: Push failure → warning; REST vẫn có data
```

### 5.2 Package layout (D12 — đã triển khai)

| Package | Namespace | Nội dung chính |
|---------|-----------|----------------|
| `Jarvis.Notifications` | `Jarvis.Notifications.*` | `INotificationStore`, `INotificationAppService`, `ISignalRNotifier`, DTOs, `NotificationAppService`, `JarvisNotificationsOptions`, `AddCoreNotifications()`, `AddNotificationAppService()` |
| `Jarvis.Notifications.SignalR` | `Jarvis.Notifications.SignalR.*` | `NotificationHub`, `HubSignalRNotifier`, `UseSignalR()`, `UseRedisBackplane()`, `AddNotificationAppServiceWithRealtime()`, `MapCoreNotificationHub()` |
| `Jarvis.Notifications.Redis` | `Jarvis.Notifications.Redis.*` | `RedisNotificationStore`, `UseRedisStore()` |

**Đánh giá vs [architecture-software.md](./architecture-software.md) sau D12:**

| Rule | Trạng thái |
|------|------------|
| §0.2 Core + satellite | 🟢 Store/SignalR là satellite; Host gọi `UseRedisStore()` / `UseSignalR()` rõ ràng |
| §0.2 Một concern / PackageId | 🟢 Namespace khớp family `Jarvis.Notifications.*` |
| §3.4 Fluent builder | 🟢 `NotificationsBuilder` + `Use*` opt-in; fail-fast khi thiếu config satellite |
| §3.2 TryAdd* | 🟢 AppService / Notifier |
| §3.3 Options + snapshot | 🟢 `JarvisNotificationsOptions` (section `SignalR` — backward compat); core validator chỉ `HubPath`; store/backplane validate tại `Use*` |
| §0.3 Skill | 🔴 D13 |
| §5.3 Observability | 🔴 tech debt |

**Breaking changes (migration từ MVP monolith):**

| Cũ | Mới |
|----|-----|
| `Jarvis.SignalR.*` | `Jarvis.Notifications.*` / `Jarvis.Notifications.SignalR.*` |
| `AddCoreSignalR()` | `AddCoreNotifications().UseSignalR().UseRedisStore()` |
| `AddCoreSignalRNotificationAppService<T>()` | `AddNotificationAppServiceWithRealtime<T>()` |
| `JarvisSignalROptions` | `JarvisNotificationsOptions` |
| Keyed Redis `"SignalR.Store"` | `"Notifications.Redis.Store"` |

### 5.3 Contract tạo notification (tóm tắt)

| Field | Bắt buộc | Ghi chú |
|-------|----------|---------|
| `Type`, `Title` | ✓ | FE map icon / hiển thị |
| `Body`, `Data` | | `Data` = metadata JSON (`actionUrl`, …) |
| `NotificationId` | | GUID v7 nếu empty |
| `TenantId` / `UserId` | | Enrich nội bộ; producer hiếm khi set |

REST base: `/api/notifications` — list / get / unread-count / read / unread / read-all. Chi tiết query/response giữ trong code + Sample.

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | MVP gọn; Atomic khớp architecture-software; module API chỉ reference core; store/realtime opt-in; offline vẫn đọc trong retention; identity thống nhất |
| Xấu / chi phí | Host wiring dài hơn (3 extension calls); breaking change namespace/DI cho consumer cũ; chưa idempotency → duplicate khi retry |
| Trung lập | Full payload O3; có thể chuyển O4 sau mà không đổi store |
| Đã giải quyết | P5 atomic debt; business/module chỉ cần `Jarvis.Notifications` assembly |

| Rủi ro | Mitigation |
|--------|------------|
| Quên `UseRedisStore()` / `UseSignalR()` / Map hub | Sample + (Later) skill D13 |
| Crash giữa save và push | Data còn; user mở center / REST reload |
| Background job thiếu ambient user/tenant | Dùng `NotifyUsersAsync` + establish tenant context Host |
| Redis HA / mất store | Ops Redis; Later SQL store qua `INotificationStore` |

---

## 7. Confirm

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D11 (MVP) như quyết định chính thức? | **Yes** — 2026-08-11 |
| Q2 | Hub payload: giữ **O3 full** hay chuyển **O4 signal-only**? | **Giữ O3** |
| Q3 | Accept **D12** Atomic split (core / SignalR / Redis)? | **Yes** — implemented 2026-08-11 |
| Q4 | Align naming: namespace `Jarvis.Notifications.*` hay đổi PackageId? | **Namespace → `Jarvis.Notifications.*`** (MVP/D12) · **🔄 2026-08-12:** framework đổi **`Jarvis.Realtime.*`**; module giữ Notifications |
| Q5 | Auto-`UseRedisStore()` hay Host gọi rõ? | **Host gọi rõ** — `UseRedisStore()` opt-in · **🔄 store chuyển module** (ADR 2026-08-12) |
| Q6 | Skill name: `notifications-dotnet` vs `signalr-dotnet`? | Khuyến nghị cũ: **`notifications-dotnet`** · **🔄 thêm `realtime-dotnet`** (ADR 2026-08-12) |
| Q7 | Tech debt ưu tiên: idempotency / archive / sync / toast / metrics? | _chờ thứ tự_ |
| Q8 | Boundary framework realtime / module inbox? | Xem **[ADR 2026-08-12](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md)** — Accepted |


---

## 8. Kế hoạch triển khai

| Phase | Việc | Done khi | Status |
|-------|------|----------|--------|
| 0 | Confirm §7 (Q1–Q7) | Metadata ADR cập nhật | 🟢 (Q6–Q7 còn mở) |
| 1 | MVP runtime: Redis store, hub, REST, FE, Sample | Build + UnitTest notifications | 🟢 |
| 2 | Docs: ADR + README module đồng bộ | File này + `modules/notifications/README.md` | 🟢 |
| 3 | Tách satellite Redis + SignalR; opt-in `UseRedisStore`; align namespace | Host compose rõ; 24 UnitTest xanh | 🟢 |
| 4 | Skill D13 + Sample template appsettings | Workflow init/add | 🔴 |
| 5 | Tech debt: idempotency → archive → sync cursor → toast → metrics | Theo Q7 | 🔴 |

---

## 9. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | Framework core + SignalR + Redis + AppService | 🟢 |
| 2 | Module REST + FE package + Sample wire | 🟢 |
| 3 | Identity qua `ICurrentUser` / `ICurrentTenant` | 🟢 |
| 4 | ADR theo template + đánh giá architecture-software | 🟢 |
| 5 | Confirm §7 Q1–Q5 | 🟢 |
| 6 | Atomic D12 | 🟢 |
| 7 | Skill D13 | 🔴 |
| 8 | Idempotency / archive / sync / toast / E2E / metrics | 🔴 |

---

## 10. Tham chiếu thêm

- **Target layout mới:** [2026-08-12-adr-jarvis-realtime-inbox-boundary.md](./2026-08-12-adr-jarvis-realtime-inbox-boundary.md)
- [architecture-software.md](./architecture-software.md) — §0.2 Core + satellite, §3.4 fluent builder, §0.3 skill
- Code (hiện tại, pre-refactor): `frameworks/Jarvis.Notifications/`, `frameworks/Jarvis.Notifications.SignalR/`, `frameworks/Jarvis.Notifications.Redis/`, `modules/notifications/`
- Sample: `Sample/Program.cs` — `AddNotificationModule` + `AddCoreNotifications().UseSignalR().UseRedisStore().AddNotificationAppServiceWithRealtime<…>()`
- Redis keys: `{prefix}:{tenantId:userId}:item|index|unread|read`
- Config section: `SignalR:*` (giữ backward compat tới khi migrate `Realtime:*` / `Notifications:Store:*`)


---
