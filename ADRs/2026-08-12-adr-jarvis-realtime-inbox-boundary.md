# ADR — Framework Realtime vs Module Inbox

> **Trạng thái:** 🟢 **Implemented** (Phase 0–5; T17–T20 Redis IT deferred).  
> **Ngày:** 2026-08-12 · Accept boundary: 2026-08-12 · Revise store/config/retention: 2026-08-13  
> **Loại:** Package boundary / naming / Module Atomic / inbox Redis store  
> **Supersedes (phần):** [2026-08-11-adr-jarvis-notifications](./2026-08-11-adr-jarvis-notifications.md) — D1, D4, D8, D12 (layout); hành vi MVP persist-first + SignalR + REST/FE giữ.  
> **Liên quan:** [architecture-software.md](./architecture-software.md), [current-user-tenant](./2026-08-01-adr-current-user-tenant.md), [adr-template.md](./adr-template.md)  
> **Phạm vi:** (1) Framework = realtime transport; Module = inbox; (2) Rename `Jarvis.Realtime*`; (3) Config + Redis inbox model (Lua save, pipeline mark, retention = TTL item + heal orphan lúc List); (4) DTO bỏ `IsRead` / `ReadAtUtc` / `ExpiresAtUtc`.  
> **Ngoài phạm vi:** email/SMS/push; sticky session (LB); retention job định kỳ; implement code trước khi §7 chốt hết.  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm / chờ confirm · 🔴 chưa làm

---

## 1. Bối cảnh

MVP + D12 đặt inbox (AppService, Redis store) trong `frameworks/Jarvis.Notifications*`. Product intent mới:

| Tầng | Trách nhiệm |
|------|-------------|
| **Framework Realtime** | Hub, connection, group user/tenant, publish, optional Redis **backplane** |
| **Module Notifications** | Inbox: store, AppService, REST, FE, Setting definitions |

Sticky session = LB/Host — **không** phải Redis. Redis framework chỉ cho **backplane**. Redis inbox = module + `Cache:DistributedGroups:Redis:Notifications`.

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Inbox trong framework | Host dùng store mà không adopt module |
| P2 | Tên `Jarvis.Notifications.*` trên framework | Nhầm transport với product inbox |
| P3 | Store options (`RetentionDays`, connection, KeyPrefix…) trong appsettings `SignalR:Store` | Lẫn backplane / cache / product policy |
| P4 | Mark dùng MULTI/EXEC; prune lazy + `PruneThreshold` / job quét mailbox | Phức tạp hơn mức cần cho inbox |
| P5 | `SignalRNotificationItemDto` lưu `IsRead` / `ReadAtUtc` / `ExpiresAtUtc` trong JSON | Trùng nguồn sự thật với ZSET `unread`/`read` + TTL |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Dependency một chiều; Host = composition root |
| C2 | Identity: `ICurrentUser` / `ICurrentTenant` |
| C3 | Persist-first: Save store → Publish; push fail ≠ mất data |
| C4 | Framework **không** chứa inbox semantics (list/mark/retention product) |
| C5 | Host muốn inbox → **bắt buộc** module notifications |
| C6 | Backplane Redis ≠ inbox Redis (config/package tách) |
| C7 | Trạng thái đọc = membership ZSET `unread` / `read` — **không** lưu `IsRead` trong JSON item |

---

## 4. Phương án đã cân nhắc (tóm tắt)

| # | Chủ đề | Chọn | Từ chối |
|---|--------|------|---------|
| O2 | Boundary | Framework realtime + Module inbox | Store trong framework (O1); Hub trong module (O3) |
| N3 | Naming | `Jarvis.Realtime` + `.SignalR` | Giữ Notifications trên framework; gộp 1 package |
| S1 | Redis inbox | **Giữ 1 STRING + 3 ZSET** (`item` / `index` / `unread` / `read`) | Hash-only; 1 ZSET + IsRead JSON |
| S2 | Save | **Lua** (1 RTT, atomic 4 key) | 4 lệnh rời không atomic |
| S3 | Mark | **Pipeline**, không MULTI/EXEC | `CreateTransaction` |
| S4 | Retention | **TTL item** (`RetentionDays`) + **heal orphan lúc List** (`MGET` miss → `ZREM` 3 ZSET) | Job định kỳ `ZREMRANGEBYSCORE`; lazy `MaybePrune` + `PruneThreshold` |

---

## 5. Quyết định

### 5.0 Bảng quyết định

| # | Quyết định | Chi tiết | Status |
|---|------------|----------|--------|
| D1 | Boundary | Framework = transport; Module = inbox product | 🟡 Accepted |
| D2 | Packages framework | `Jarvis.Realtime` + `Jarvis.Realtime.SignalR` (tách riêng) | 🟡 Accepted |
| D3 | Packages module | Giống Setting: `Jarvis.Modules.Notifications` + `.Redis` + `.Api` + `@jarvis/notifications` | 🟡 Accepted |
| D4 | Move inbox | AppService, store contracts, Redis store → module | 🟡 Accepted |
| D5 | DI API | `AddCoreRealtime` / `UseSignalR` / `UseRedisBackplane` / `MapRealtimeHub` / `IRealtimeNotifier`; module: `AddNotificationModule` + `UseRedisInboxStore` | 🟡 Accepted |
| D6 | Hub path | Inbox **`/hubs/notifications`**; chat sau **`/hubs/chat`**; client method **`notification`** | 🟡 Accepted |
| D7 | PR order | **Rename Realtime trước** → **move inbox sau** | 🟡 Accepted |
| D8 | Inbox Redis connection | Chỉ `Cache:DistributedGroups:Redis:Notifications` (`Configuration` + **`InstanceName`**) — không `Notifications:Store` appsettings | 🟡 Accepted (Q12/Q14) |
| D9 | Backplane config | `Realtime:SignalR:UseRedisBackplane` + `Realtime:SignalR:Redis` | 🟡 Accepted (Q15) |
| D10 | Redis key model | 1 STRING `item` + 3 ZSET `index` / `unread` / `read` | 🟡 Accepted |
| D11 | Save | **Lua** atomic SET + ZADD index + ZADD unread + ZREM read | 🟡 Accepted |
| D12 | Mark | **Pipeline** SET không còn (JSON không IsRead); chỉ **ZADD dest + ZREM src** (+ heal) — **không** MULTI/EXEC | 🟡 Accepted |
| D13 | DTO item | `SignalRNotificationItemDto` **bỏ** `IsRead`, `ReadAtUtc`, `ExpiresAtUtc` | 🟡 Accepted (2026-08-13) |
| D14 | Retention | Setting `RetentionDays` → **TTL `item` lúc Save**; ZSET dọn bằng **heal orphan lúc List** (không job, không `MaybePrune` / `PruneThreshold`) | 🟡 Accepted (2026-08-13) |
| D15 | Skill | `realtime-dotnet` + `notifications-dotnet` | 🟢 |

### 5.1 Target package layout

```text
frameworks/
  Jarvis.Realtime/              # options, RealtimeBuilder, IRealtimeNotifier, AddCoreRealtime
  Jarvis.Realtime.SignalR/      # Hub, groups, UseSignalR, UseRedisBackplane, MapRealtimeHub

modules/notifications/
  Jarvis.Modules.Notifications/       # contracts, AppService, SettingDefinition inbox
  Jarvis.Modules.Notifications.Redis/ # Redis store, UseRedisInboxStore, Lua save script
  Jarvis.Modules.Notifications.Api/   # Controllers, AddNotificationModule
  frontend/                             # @jarvis/notifications
```

### 5.2 Runtime

```text
Business ──► INotificationAppService          (module library)
               ├─► INotificationStore         (module Redis)
               └─► IRealtimeNotifier          (framework)
                       └─► Hub /hubs/notifications

Host inbox:
  AddCoreRealtime().UseSignalR()[+ UseRedisBackplane]
  + UseRedisInboxStore() + AppService
  + AddNotificationModule()
  + MapRealtimeHub<…>()   // path default /hubs/notifications
```

### 5.3 Config (chốt)

**Bỏ** toàn bộ block Sample `SignalR` (HubPath / Store / Redis lẫn).

```json
"Cache": {
  "DistributedGroups": {
    "Redis": {
      "Default": { "Configuration": "127.0.0.1:6379", "InstanceName": "Instance1_" },
      "Auth": { "Configuration": "127.0.0.1:6379", "InstanceName": "SharedAuth1_" },
      "Notifications": {
        "Configuration": "127.0.0.1:6379",
        "InstanceName": "Notifications1_"
      }
    }
  }
},
"Realtime": {
  "SignalR": {
    "UseRedisBackplane": false,
    "Redis": {
      "Configuration": "127.0.0.1:6379",
      "ChannelPrefix": "jarvis-realtime"
    }
  }
}
```

| Key | Owner | Dùng cho |
|-----|-------|----------|
| `Cache:DistributedGroups:Redis:Notifications:Configuration` | Caching + module store | Connection multiplexer inbox |
| `…:InstanceName` | Module store | Prefix key Redis inbox (thay `KeyPrefix`) |
| `Realtime:SignalR:UseRedisBackplane` | Framework Realtime | Opt-in backplane khi `UseSignalR` |
| `Realtime:SignalR:Redis:*` | Framework SignalR | Backplane only |

**SettingDefinition** (module library, group `NotificationInbox` — tránh đụng Sample group `Notification`):

| Key | Default | Dùng |
|-----|---------|------|
| `NotificationInbox.RetentionDays` | `20` | TTL lúc Save (Lua `PX` = còn lại tới `CreatedAtUtc + RDays`) |
| `NotificationInbox.RedisRetryCount` | `2` | Retry transient Save (và Mark nếu cần) |

**Không** còn Setting/appsettings: `PruneThreshold`, `PruneIntervalMinutes`, `KeyPrefix`, `Notifications:Store:*`. Không retention job.

`UseRedisInboxStore()` fail-fast nếu thiếu group `Notifications`.

### 5.4 Redis inbox model (chi tiết)

**Connection:** `Cache:DistributedGroups:Redis:Notifications`  
**Prefix instance:** `InstanceName` (ví dụ `Notifications1_`) — **không** dùng `KeyPrefix` riêng.  
**Scope (hash-tag Cluster):** `{tenantId:D:userId:D:notifications}` → mọi key cùng mailbox cùng slot.

Ký hiệu dưới đây:

```text
Inst   = Cache:DistributedGroups:Redis:Notifications:InstanceName   // vd Notifications1_
Scope  = {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx:yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy:notifications}
Id     = messageId dạng D (Guid)
Score  = CreatedAtUtc Unix milliseconds (số nguyên)
RDays  = Setting NotificationInbox.RetentionDays (default 20)
```

---

#### 5.4.0 Catalog key Redis

##### A. Bốn key **bắt buộc** — mỗi cặp (tenant, user)

| # | Tên key (pattern) | Kiểu Redis | Giá trị / member | Score | Expire |
|---|-------------------|------------|------------------|-------|--------|
| 1 | `{Inst}inbox:{Scope}:item:{Id}` | **STRING** | JSON payload (xem bảng JSON) | — | **Có TTL:** `PX` lúc Save = thời gian còn lại tới `CreatedAtUtc + RDays`. Redis **tự DEL** khi hết hạn. |
| 2 | `{Inst}inbox:{Scope}:index` | **ZSET** | member = `{Id}` | `Score` = `CreatedAtUtc` ms | **Không TTL key.** Member orphan (item đã mất TTL) → `ZREM` khi **List** hydrate miss. |
| 3 | `{Inst}inbox:{Scope}:unread` | **ZSET** | member = `{Id}` **chưa đọc** | cùng `Score` | **Không TTL key.** Cùng heal orphan lúc List. |
| 4 | `{Inst}inbox:{Scope}:read` | **ZSET** | member = `{Id}` **đã đọc** | cùng `Score` | **Không TTL key.** Cùng heal orphan lúc List. |

**Ví dụ cụ thể** (`Inst=Notifications1_`, tenant `aaaaaaaa-…`, user `bbbbbbbb-…`, message `cccccccc-…`):

```text
Notifications1_inbox:{aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb:notifications}:item:cccccccc-cccc-cccc-cccc-cccccccccccc
Notifications1_inbox:{aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb:notifications}:index
Notifications1_inbox:{aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb:notifications}:unread
Notifications1_inbox:{aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb:notifications}:read
```

##### JSON trong key `item` (STRING)

```json
{
  "notificationId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "type": "OrderCompleted",
  "title": "…",
  "body": "…",
  "data": { "actionUrl": "…" },
  "createdAtUtc": "2026-08-12T10:00:00+00:00"
}
```

| Field JSON | Có? | Ghi chú |
|------------|-----|---------|
| `notificationId`, `type`, `title`, `body`, `data`, `createdAtUtc` | Có | Khớp `SignalRNotificationItemDto` sau D13 |
| `isRead`, `readAtUtc`, `expiresAtUtc` | **Không** | Đọc = ZSET; hết hạn payload = TTL `item`; ZSET dọn = heal orphan lúc List |

##### Quan hệ 4 key

```text
Save:     item (TTL) + ZADD index + ZADD unread + ZREM read
MarkRead: ZADD read + ZREM unread     (không sửa JSON)
List:     ZREVRANGE (index|unread|read) → MGET item:* → orphan miss → ZREM 3 ZSET
Count:    ZCARD unread
```

| Trạng thái đọc | `unread` | `read` | JSON |
|----------------|----------|--------|------|
| Mới tạo | có id | không | không field isRead |
| Đã đọc | không | có id | không đổi |
| List All + HTTP | — | — | `isRead` **computed** (`SISMEMBER unread` → false nếu đang unread) |

##### Expire — tóm tắt

| Key | Expire thế nào | Ai thực hiện |
|-----|----------------|--------------|
| `item:{Id}` | TTL từng key (`PX` / `EX`) = còn lại của retention từ `createdAtUtc` | Redis passive expire. Mark **không** đụng `item` → không reset TTL. |
| `index` / `unread` / `read` | **Không** EXPIRE cả key; **không** job `ZREMRANGEBYSCORE` | **Heal orphan lúc List:** `MGET` miss → `ZREM` id khỏi cả 3 ZSET (trang đang lấy) |
| Toàn bộ mailbox trống | 3 ZSET có thể còn key rỗng | Chấp nhận; optional `DEL` nếu `ZCARD==0` (không bắt buộc ADR) |

**Trade-off chấp nhận:** `ZCARD` / unread count có thể **cao hơn thật** cho đến khi orphan nằm trên trang được List (heal opportunistic, không quét cả mailbox).

---

**Không** dùng key registry `mailboxes` / SCAN discovery — không còn retention job (Q18 N/A).

#### Save — Lua (giữ)

```text
SET item json PX <ttlMs>
ZADD index  score id
ZADD unread score id
ZREM read   id
```

- `ttlMs` từ `CreatedAtUtc + RDays - now` (floor ≥ 1s).
- `score` = `CreatedAtUtc` unix ms.
- 1 round-trip, atomic.

#### Mark read / unread — pipeline, không MULTI/EXEC

JSON **không** cập nhật → chỉ ZSET:

```text
ZADD read   score id
ZREM unread id
```

Thứ tự: **ZADD đích trước ZREM nguồn**. Nhiều id: pipeline/`CreateBatch`. Heal khi list như bảng dưới.

**Heal (List):**

| Tình huống | Xử lý |
|------------|--------|
| ZSET có id, `MGET`/`GET` item miss (TTL đã DEL) | `ZREM` id khỏi `index` + `unread` + `read` (orphan heal — **cơ chế retention ZSET duy nhất**) |
| Id vừa trong `unread` vừa `read` | `ZREM` phía thừa (ưu tiên khớp filter; list Unread mà đã ở read → `ZREM unread`) |
| List All | `isRead` computed cho HTTP (`SISMEMBER unread` / batch tương đương) |

#### List / count

| API | Redis |
|-----|--------|
| List All + page | `ZREVRANGE index` → `MGET` items → heal orphan → optional `SISMEMBER unread` cho `isRead` |
| List Unread + page | `ZREVRANGE unread` → `MGET` → heal orphan |
| List Read + page | `ZREVRANGE read` → `MGET` → heal orphan |
| Unread count | `ZCARD unread` (có thể lệch cao tạm nếu còn orphan chưa list tới) |

#### Retention — TTL item + heal orphan lúc List (D14)

```text
# Save
PX item = remaining(CreatedAtUtc + RDays)

# List (mỗi trang)
ids = ZREVRANGE source start stop
values = MGET item:ids
for each id where value miss:
  ZREM index/unread/read id
return hydrated items only
```

- **Không** job định kỳ, **không** `ZREMRANGEBYSCORE`, **không** `MaybePrune` / `PruneThreshold`, **không** mailbox registry/SCAN.
- Payload hết hạn = Redis TTL trên `item`.
- Index ZSET sạch dần khi user List các trang chứa orphan.

### 5.5 DTO & API surface (D13)

```csharp
// Storage + contract item — không IsRead / ReadAtUtc / ExpiresAtUtc
public sealed class SignalRNotificationItemDto
{
    public Guid NotificationId { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public string? Body { get; init; }
    public JsonElement? Data { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
```

- Tab Unread/Read: không bắt buộc `isRead` trên item.
- Tab All: HTTP trả **`isRead` computed** (Q16 = Yes), không persist Redis.
- `NotificationListResult.UnreadCount` = `ZCARD unread`.

### 5.6 Mapping từ ADR 2026-08-11

| Cũ | Mới |
|----|-----|
| Framework Notifications + Redis + SignalR | Realtime + Realtime.SignalR; inbox → module |
| `SignalR:Store:*` | Cache group Notifications + Setting Retention |
| `KeyPrefix` | `InstanceName` |
| Transaction mark | Pipeline ZSET only |
| Lazy prune / Job định kỳ | **Heal orphan lúc List** (TTL `item` + `ZREM` khi MGET miss) |
| DTO có IsRead/Expires | Bỏ; đọc = ZSET |

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Boundary rõ; config tách backplane/inbox; Save atomic; Mark đơn giản; Retention không job / không prune hot-path; một nguồn sự thật cho read state (ZSET) |
| Chi phí | Breaking PackageId/DI/config/DTO/FE; churn sau D12 |
| Rủi ro Mark | Lệch ZSET ngắn → mitigate heal + thứ tự ZADD→ZREM |
| Rủi ro Retention | Orphan / `ZCARD` lệch cao đến khi List heal trang đó; mailbox idle có thể giữ member ZSET lâu (payload `item` đã mất) — **chấp nhận** theo D14 |

---

## 7. Confirm *(chốt trước code)*

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1–Q9, Q11 | Boundary, Realtime split, DI, hub path, library Setting-like, rename-first | **Yes** (đã chốt) |
| Q12 | Inbox Redis = `Cache:DistributedGroups:Redis:Notifications`? | **Yes** |
| Q14 | Bỏ KeyPrefix; dùng `InstanceName`? | **Yes** |
| Q15 | Backplane = `Realtime:SignalR:Redis` riêng? | **Yes** |
| Q13a | Save giữ **Lua**? | **Yes** |
| Q13b | Mark = **pipeline**, không MULTI/EXEC; JSON không IsRead? | **Yes** |
| Q13c | Retention = `RetentionDays` (TTL `item`) + **heal orphan lúc List**; bỏ job / `PruneThreshold` / lazy `MaybePrune`? | **Yes** (2026-08-13) |
| Q13d | DTO bỏ `IsRead`, `ReadAtUtc`, `ExpiresAtUtc`? | **Yes** |
| Q16 | REST/FE tab All: còn trả **`isRead` computed** (không persist) trên response không? | **Yes** — List All map `isRead` qua `SISMEMBER unread` (hoặc tương đương); không lưu JSON |
| Q17 | Chu kỳ job mặc định? | **N/A** — không retention job |
| Q18 | Discovery mailbox cho job: SCAN vs registry? | **N/A** — không job → không registry/`mailboxes` |

Sau khi §7 đủ → bắt đầu Phase 2 (rename) / Phase 3 (move + store).

---

## 8. Test cases

### 8.1 Smoke

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T1 | Sample compose Realtime + inbox module | Build + resolve AppService + notifier | 🟢 |
| T2 | Realtime-only Host | Không cần `INotificationStore` | 🟢 |
| T3 | Backplane thiếu `Realtime:SignalR:Redis:Configuration` | Fail-fast | 🟢 |
| T4 | `UseRedisInboxStore` thiếu Cache group `Notifications` | Fail-fast | 🟢 |

### 8.2 Regression

| # | Case | Nơi (hiện tại → sau move) | Trạng thái |
|---|------|---------------------------|------------|
| T5 | Persist-first Save→Publish | `NotificationAppServiceTests` | 🟢 |
| T6 | AppService validation / List / Mark forward | cùng suite | 🟢 |
| T7 | Notifier group + method `notification` | `HubRealtimeNotifierTests` | 🟢 |
| T8 | Group names | `RealtimeGroupNamesTests` | 🟢 |
| T9 | `AddNotificationModule` ApplicationPart | `NotificationModuleRegistrationTests` | 🟢 |
| T14 | Đăng ký `IRealtimeNotifier` | `RealtimeHostRegistrationTests` | 🟢 |

### 8.3 Test mới (store)

| # | Case | Expect | Trạng thái |
|---|------|--------|------------|
| T10 | Realtime DI không đăng ký store | null/`GetService` miss | 🟢 |
| T17 | Save Lua: item + index + unread; không có trong read | Assert 4 key | 🟡 Redis IT deferred |
| T18 | Mark read pipeline: member chuyển unread→read; JSON không có isRead | 🟡 Redis IT deferred |
| T19 | List Unread/Read/All page + `ZCARD` unread | 🟡 Redis IT deferred |
| T20 | List heal orphan: ZSET còn id, `item` miss → `ZREM` 3 ZSET; response không trả orphan | 🟡 Redis IT deferred |
| T21 | DTO/JSON serialize không còn IsRead/ReadAtUtc/ExpiresAtUtc | 🟢 |
| T15 | Không còn `frameworks/Jarvis.Notifications.Redis` sau move | 🟢 |

### 8.4 Done khi

Smoke T1–T4 + regression T5–T9/T14 + T15/T21 🟢; T17–T20 Redis integration có thể follow-up.

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Status |
|-------|------|----------|--------|
| 0 | Confirm §7 | Q13c/Q17/Q18 = heal-on-List / N/A | 🟢 |
| 1 | Docs SAD/README trỏ ADR này | Đồng bộ | 🟢 |
| 2 | Rename → `Jarvis.Realtime` + `.SignalR`; Sample `Realtime:SignalR`; T7–T9 | Grep frameworks hết Notifications (trừ inbox tạm) | 🟢 |
| 3 | Module library + Redis; move store; Lua save; pipeline mark; heal-orphan List; DTO; Cache group; Setting; T1–T6, T17–T21 | Không còn framework Notifications Redis/AppService | 🟢 |
| 4 | Skills | Workflow Host | 🟢 |
| 5 | §8 xanh hết | 🟢 Implemented (T17–T20 Redis IT deferred) | 🟢 |

---

## 10. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | ADR viết lại đủ boundary + store catalog + config + DTO + retention heal-on-List | 🟢 (doc) |
| 2 | Supersede ghi ADR 2026-08-11 | 🟢 |
| 3 | Code rename Realtime | 🟢 |
| 4 | Code move inbox + store semantics mới | 🟢 |
| 5 | §8 tests xanh | 🟢 |

---

## 11. Tham chiếu

- [2026-08-11-adr-jarvis-notifications.md](./2026-08-11-adr-jarvis-notifications.md)
- [architecture-software.md](./architecture-software.md)
- [adr-template.md](./adr-template.md)
- Review: [reviews/2026-08-12-architecture-review-notifications.md](../reviews/2026-08-12-architecture-review-notifications.md)
- Code hiện tại (pre-refactor): `frameworks/Jarvis.Notifications*`, `modules/notifications/Jarvis.Modules.Notifications.Api`
