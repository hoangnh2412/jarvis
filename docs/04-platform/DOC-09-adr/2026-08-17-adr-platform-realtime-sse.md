# ADR — `Platform.Realtime.Sse` 

> **Trạng thái:** 🟡 **Accepted** (chưa implement code)  
> **Ngày:** 2026-08-17 · **Accept:** 2026-08-17  
> **Loại:** Package boundary / Module Atomic (core + satellite) / transport  
> **Liên quan:** [Realtime vs Inbox](./2026-08-12-adr-platform-realtime-inbox-boundary.md) (D1–D6, D9), [architecture-rules.md](./architecture-rules.md) §0.2 Core + satellite, [current-user-tenant](./2026-08-01-adr-current-user-tenant.md), [adr-template.md](./adr-template.md)  
> **Phạm vi:** (1) Refactor `Platform.Realtime` core (options, group naming, composite notifier, builder); (2) Satellite `Platform.Realtime.Sse` (endpoint, connection registry, `IRealtimeNotifier`); (3) Redis **fan-out** scale-out cho SSE (opt-in, pattern tương tự SignalR backplane); (4) DI Host + Sample config; (5) Test smoke/regression.  
> **Ngoài phạm vi:** FE client EventSource (package `@platform/notifications` — ADR/PR riêng); chat hub `/hubs/chat`; thay thế SignalR; sticky session LB; inbox Redis store (module); gộp SSE vào `Platform.Realtime.SignalR`.  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm / chờ confirm · 🔴 chưa làm

---



## 1. Bối cảnh

[ADR Realtime vs Inbox](./2026-08-12-adr-platform-realtime-inbox-boundary.md) đã chốt boundary: **framework = transport**, **module = inbox**. Hiện chỉ có một satellite transport:


| Thành phần                  | Hiện tại                                                                                              |
| --------------------------- | ----------------------------------------------------------------------------------------------------- |
| `Platform.Realtime`         | `IRealtimeNotifier`, `RealtimeBuilder`, `AddCoreRealtime`, options bind `Realtime:SignalR`            |
| `Platform.Realtime.SignalR` | Hub `/hubs/notifications`, `HubRealtimeNotifier`, `UseSignalR`, `UseRedisBackplane`, `MapRealtimeHub` |
| Module Notifications        | `INotificationAppService` → `IRealtimeNotifier` (không biết transport)                                |


Một số Host / client cần **push một chiều** (server → browser) qua **HTTP dài** (SSE), ví dụ: proxy/firewall chặn WebSocket, client nhẹ không cần hub bidirectional, hoặc tích hợp với stack chỉ hỗ trợ `text/event-stream`.

SignalR đã có **Redis backplane** (`UseRedisBackplane`) để fan-out message giữa nhiều instance. SSE **không** dùng được `IHubContext` / group built-in của SignalR — cần registry connection cục bộ + cơ chế pub/sub riêng khi scale-out.

---



## 2. Vấn đề


| #   | Vấn đề                                                                                      | Hệ quả                                                                                              |
| --- | ------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| P1  | Chỉ có SignalR làm transport                                                                | Host không opt-in SSE; client bắt buộc WebSocket/long-polling                                       |
| P2  | Options + validator gắn chặt SignalR                                                        | `PlatformRealtimeOptions` bind `Realtime:SignalR` trực tiếp; `HubPath` bắt buộc dù Host chỉ dùng SSE |
| P3  | `RealtimeGroupNames` nằm trong package SignalR                                              | Satellite SSE phải duplicate convention `user:` / `tenant:` hoặc reference ngược SignalR            |
| P4  | `AddNotificationAppServiceWithRealtime` hard-code `HubRealtimeNotifier` + `TryAddSingleton` | Bật cả SignalR + SSE → transport sau **không** được đăng ký; không fan-out đa transport             |
| P5  | `RealtimeBuilder` chứa `ISignalRServerBuilder?`                                             | Core builder lệ thuộc SignalR; thêm SSE làm builder “ô nhiễm” transport khác                        |
| P6  | SSE multi-instance không có backplane                                                       | Publish trên instance A không tới connection mở trên instance B                                     |
| P7  | EventSource không set header tùy ý như `fetch`                                              | Auth/tenant cần quy ước rõ (cookie, query `access_token`, v.v.) — tránh mỗi Host tự làm             |


---



## 3. Ràng buộc


| #   | Ràng buộc                                                                                                                                                                  |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C1  | Dependency một chiều; Host = composition root ([architecture-rules](./architecture-rules.md))                                                                              |
| C2  | `Platform.Realtime` **không** reference `Platform.Realtime.Sse` hay `Platform.Realtime.SignalR`                                                                            |
| C3  | Module Notifications **không** đổi contract `INotificationAppService`; vẫn inject `IRealtimeNotifier`                                                                      |
| C4  | Persist-first inbox giữ nguyên: store Save → Publish; push fail ≠ mất data ([Realtime/Inbox D1, C3](./2026-08-12-adr-platform-realtime-inbox-boundary.md))                 |
| C5  | Identity: `ICurrentUser` / `ICurrentTenant` + claim/header giống hub ([D6](./2026-08-12-adr-platform-realtime-inbox-boundary.md))                                          |
| C6  | **`Realtime:Redis`** = connection chung framework; **khác** `Cache:…:Notifications` (inbox). Channel prefix **theo transport**, không gộp protocol ([D8, D9](./2026-08-12-adr-platform-realtime-inbox-boundary.md)) |
| C7  | Sticky session LB **không** thay fan-out Redis ([Realtime/Inbox §1](./2026-08-12-adr-platform-realtime-inbox-boundary.md))                                                 |
| C8  | Satellite SSE opt-in: `UseSse()` / `MapRealtimeSse()` — không auto-wire trong core                                                                                         |
| C9  | Breaking change options được phép **có migration path** (bind cũ + obsolete 1 release) hoặc document rõ cut-over Sample                                                    |


---



## 4. Phương án đã chọn

Các phương án khác (nhét SSE vào SignalR, sticky-only scale-out, gộp Redis channel, một notifier duy nhất, v.v.) đã được cân nhắc và **loại** trong quá trình thảo luận — không liệt kê tại đây.


| #   | Chủ đề            | Quyết định |
| --- | ----------------- | ---------- |
| O2  | Vị trí SSE        | Satellite **`Platform.Realtime.Sse`** — core + satellite như Blob/Auth |
| O4  | Đa transport      | **`CompositeRealtimeNotifier`** — một `IRealtimeNotifier` fan-out N transport |
| O6  | Group naming      | **`RealtimeGroupNames` lên core** — convention `user:` / `tenant:` dùng chung |
| O8  | Scale-out SSE     | **Redis pub/sub fan-out** opt-in (`UseSseRedisFanOut`); share `ConnectionMultiplexer` với backplane |
| O11 | Options           | Root **`Realtime`** + nested **`SignalR`** / **`Sse`** / **`Redis`**; `SignalRPath` / `SsePath` |
| O12 | Notifier contract | **`IRealtimeTransport` internal** + **`IRealtimeNotifier` public** (composite) |
| O14 | Redis config      | **`Realtime:Redis:Configuration`** chung; **`ChannelPrefix` theo transport** |

---



## 5. Quyết định

Chúng ta sẽ refactor core Realtime để hỗ trợ nhiều transport, thêm satellite `Platform.Realtime.Sse`, và Redis fan-out opt-in cho SSE scale-out — song song với SignalR backplane, không gộp protocol.

### 5.0 Bảng quyết định


| #   | Quyết định                | Chi tiết                                                                                                                                                                                                                                                                                                                                    | Status      |
| --- | ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| D1  | Satellite package         | `frameworks/Platform.Realtime.Sse` — reference `Platform.Realtime` + `Platform.DDD.Domain` (identity)                                                                                                                                                                                                                                       | 🟡 Accepted |
| D2  | Core refactor — options   | **Rename:** type hiện `PlatformRealtimeOptions` → **`RealtimeSignalROptions`** (`SignalRPath`, `UseRedisBackplane`, `ChannelPrefix`). Type mới **`PlatformRealtimeOptions`** = root `Realtime` với `ClientMethodName` + nested **`Redis`**, `SignalR`, `Sse` | 🟡 Accepted |
| D3  | Core refactor — groups    | `RealtimeGroupNames` chuyển từ SignalR → **core** `Platform.Realtime.Groups`                                                                                                                                                                                                                                                                | 🟡 Accepted |
| D4  | Core refactor — composite | **`IRealtimeTransport`** (`internal`, core) + **`CompositeRealtimeNotifier`** là **duy nhất** `IRealtimeNotifier` public; `EnsureCompositeRealtimeNotifier()` trên `RealtimeBuilder`                                                                                                                                                         | 🟡 Accepted |
| D5  | Core refactor — builder   | `RealtimeBuilder` **bỏ** `ISignalRServerBuilder`; state SignalR giữ trong extension SignalR (closure / marker service)                                                                                                                                                                                                                      | 🟡 Accepted |
| D6  | SSE endpoint              | Default **`SsePath`** `/sse/notifications`; content-type `text/event-stream`; event name = `ClientMethodName` (default `notification`)                                                                                                                                                                                                        | 🟡 Accepted |
| D7  | SSE connection model      | Mỗi GET SSE = 1 connection; registry cục bộ map `connectionId` → groups (`user:`, `tenant:`); identity qua **`RealtimeConnectionIdentityResolver`** (core) — cùng semantic `NotificationHub`                                                                                                                                                  | 🟡 Accepted |
| D8  | SSE publish               | `SseRealtimeNotifier` implement `IRealtimeTransport`: `PublishToGroupAsync` → ghi SSE tới mọi connection trong group (local registry)                                                                                                                                                                                                       | 🟡 Accepted |
| D9  | DI API SSE                | `UseSse(this RealtimeBuilder)`; `MapRealtimeSse<TUser,TTenant>(this IEndpointRouteBuilder)`; gọi sau `UseAuthentication` / `UseAuthorization`                                                                                                                                                                                               | 🟡 Accepted |
| D10 | Inbox + realtime wiring   | **`AddNotificationAppServiceWithRealtime`** chuyển sang **`Platform.Modules.Notifications.Extensions`** → `AddNotificationAppService` + `builder.EnsureCompositeRealtimeNotifier()`; **bỏ** khỏi `Platform.Realtime.SignalR`                                                                                                                | 🟡 Accepted |
| D11 | Redis fan-out SSE         | `UseSseRedisFanOut()` opt-in; `Realtime:Sse:UseRedisFanOut`; **`Configuration` từ `Realtime:Redis`** (D14)                                                                                                                                                                                                                                   | 🟡 Accepted |
| D12 | Fan-out protocol          | Publish → `PUBLISH` channel `{SseChannelPrefix}:sse:{groupName}`; **chỉ Redis path** khi fan-out on — mọi instance subscribe và deliver local                                                                                                                                                                                                | 🟡 Accepted |
| D13 | Redis shared multiplexer  | **Một** `IConnectionMultiplexer` từ `Realtime:Redis:Configuration` — SignalR backplane + SSE fan-out **cùng** instance (core `IRealtimeRedisMultiplexer` / `AddRealtimeRedis`)                                                                                                                                                                 | 🟡 Accepted |
| D14 | Redis config chung        | Section **`Realtime:Redis`** chỉ `Configuration`; **`ChannelPrefix` theo transport** (`Realtime:SignalR:ChannelPrefix`, `Realtime:Sse:ChannelPrefix`) — **không** lồng `Redis` trong từng transport                                                                                                                                          | 🟡 Accepted |
| D15 | Heartbeat                 | SSE comment heartbeat mỗi `HeartbeatSeconds` (default 15) để proxy không cắt idle connection                                                                                                                                                                                                                                                | 🟡 Accepted |
| D16 | Auth SSE                  | Ưu tiên cookie auth (same-site); hỗ trợ query `access_token` (tùy chọn config) cho dev — **document** rủi ro log/Referer                                                                                                                                                                                                                    | 🟡 Accepted |
| D17 | PR order                  | **Phase 1** core refactor (backward-compat bind) → **Phase 2** SSE single-instance → **Phase 3** Redis fan-out → **Phase 4** Sample + tests                                                                                                                                                                                                 | 🟡 Accepted |
| D18 | Sample dev toggles        | **`Realtime:SignalR:Enabled`** + **`Realtime:Sse:Enabled`** (default `true` dev); Sample chỉ `Use*` / `Map*` khi flag bật — test riêng từng transport (Q2)                                                                                                                                                                                  | 🟡 Accepted |
| D19 | Supersedes (phần)         | Bổ sung [Realtime/Inbox D5, D9](./2026-08-12-adr-platform-realtime-inbox-boundary.md) — thêm API SSE; **không** đổi inbox boundary                                                                                                                                                                                                          | 🟡 Accepted |




### 5.1 Target package layout

```text
frameworks/
  Platform.Realtime/
    Contracts/IRealtimeNotifier.cs              # public — module inject
    Contracts/IRealtimeTransport.cs             # internal — transport đăng ký composite
    Groups/RealtimeGroupNames.cs                # move từ SignalR
    Identity/RealtimeConnectionIdentityResolver.cs  # shared hub + SSE
    Configuration/
      PlatformRealtimeOptions.cs                # root: ClientMethodName + Redis + SignalR + Sse
      RealtimeRedisOptions.cs                   # Configuration (Realtime:Redis) — rename từ RealtimeSignalRRedisOptions
      RealtimeSignalROptions.cs                 # SignalRPath, Enabled, UseRedisBackplane, ChannelPrefix
      RealtimeSseOptions.cs                     # SsePath, Enabled, HeartbeatSeconds, UseRedisFanOut, ChannelPrefix, AllowAccessTokenQuery
    Hosting/RealtimeBuilder.cs
    Services/CompositeRealtimeNotifier.cs
    Services/IRealtimeRedisMultiplexer.cs       # shared ConnectionMultiplexer (optional abstraction)
    Extensions/RealtimeHostBuilderExtensions.cs
    Extensions/RealtimeBuilderCompositeExtensions.cs  # EnsureCompositeRealtimeNotifier()

  Platform.Realtime.SignalR/
    Hubs/NotificationHub.cs                     # OnConnected → groups (dùng shared identity resolver)
    Services/HubRealtimeNotifier.cs             # IRealtimeTransport
    Extensions/RealtimeSignalRExtensions.cs     # UseSignalR only (bỏ AddNotificationAppServiceWithRealtime)

  Platform.Realtime.Sse/
    Endpoints/SseNotificationEndpoint.cs        # entry HTTP SSE (tương đương OnConnected hub)
    Services/SseConnectionRegistry.cs
    Services/SseRealtimeNotifier.cs
    Services/SseRedisFanOutSubscriber.cs
    Extensions/RealtimeSseExtensions.cs
    Extensions/RealtimeSseApplicationBuilderExtensions.cs
    Extensions/RealtimeSseRedisFanOutExtensions.cs

modules/notifications/Platform.Modules.Notifications/
  Extensions/NotificationsHostBuilderExtensions.cs  # + AddNotificationAppServiceWithRealtime(RealtimeBuilder)
```



### 5.2 Runtime (single Host, SignalR + SSE + inbox)

```text
Business ──► INotificationAppService              (module)
               ├─► INotificationStore             (module Redis)
               └─► IRealtimeNotifier              (framework core)
                       └─► CompositeRealtimeNotifier
                             ├─► HubRealtimeNotifier      → SignalR groups
                             └─► SseRealtimeNotifier       → SseConnectionRegistry
                                       ▲
                                       │ (optional)
                             SseRedisFanOutSubscriber ← Redis PUB/SUB (shared multiplexer)

Host:
  AddCoreRealtime()
    .UseSignalR()[.UseRedisBackplane()]
    .UseSse()[.UseSseRedisFanOut()]
    .UseRedisInboxStore()
    .AddNotificationAppServiceWithRealtime<TUser,TTenant>()   // module Notifications
  + AddNotificationModule()
  + MapRealtimeHub<TUser,TTenant>()
  + MapRealtimeSse<TUser,TTenant>()
```



### 5.3 Config (chốt)

**Migration:** Phase 1 bind `Realtime` (mới) **và** fallback:
- `Realtime:SignalR:HubPath` → `Realtime:SignalR:SignalRPath`
- `Realtime:SignalR:Redis:Configuration` → `Realtime:Redis:Configuration`
- `Realtime:SignalR:Redis:ChannelPrefix` → `Realtime:SignalR:ChannelPrefix`
- Root keys cũ `Realtime:SignalR:*` khi chưa có nested (1 release)

```json
"Realtime": {
  "ClientMethodName": "notification",
  "Redis": {
    "Configuration": "127.0.0.1:6379"
  },
  "SignalR": {
    "Enabled": true,
    "SignalRPath": "/hubs/notifications",
    "UseRedisBackplane": false,
    "ChannelPrefix": "platform-realtime"
  },
  "Sse": {
    "Enabled": true,
    "SsePath": "/sse/notifications",
    "HeartbeatSeconds": 15,
    "UseRedisFanOut": false,
    "ChannelPrefix": "platform-realtime-sse",
    "AllowAccessTokenQuery": false
  }
}
```

**Tại sao `Realtime:Redis` tách riêng (O14 / D14):**

| Thành phần | Lý do |
| ---------- | ----- |
| `Realtime:Redis:Configuration` | **Một** connection string → **một** `ConnectionMultiplexer` cho backplane + SSE fan-out |
| `SignalR:ChannelPrefix` | Protocol SignalR backplane — không dùng cho SSE |
| `Sse:ChannelPrefix` | Pub/sub app-defined — không dùng cho SignalR |
| `SignalR:Enabled` / `Sse:Enabled` | Sample dev bật/tắt từng transport độc lập (D18) |

**Không** gộp `ChannelPrefix` lên `Realtime:Redis` — hai transport **phải** khác prefix (tránh O9).


| Key | Owner | Dùng cho |
| --- | ----- | -------- |
| `Realtime:ClientMethodName` | Core | SignalR method + SSE `event:` |
| `Realtime:Redis:Configuration` | Core | Shared `ConnectionMultiplexer` (D13) |
| `Realtime:SignalR:Enabled` | SignalR / Sample | Skip `UseSignalR` / `MapRealtimeHub` khi `false` |
| `Realtime:SignalR:SignalRPath` | SignalR | Map hub |
| `Realtime:SignalR:UseRedisBackplane` | SignalR | `UseRedisBackplane()` |
| `Realtime:SignalR:ChannelPrefix` | SignalR | SignalR backplane channels |
| `Realtime:Sse:Enabled` | SSE / Sample | Skip `UseSse` / `MapRealtimeSse` khi `false` |
| `Realtime:Sse:SsePath` | SSE | Map SSE endpoint |
| `Realtime:Sse:HeartbeatSeconds` | SSE | Heartbeat interval |
| `Realtime:Sse:UseRedisFanOut` | SSE | `UseSseRedisFanOut()` |
| `Realtime:Sse:ChannelPrefix` | SSE | `{prefix}:sse:{group}` pub/sub |
| `Realtime:Sse:AllowAccessTokenQuery` | SSE | Query bearer cho EventSource |
| `Cache:DistributedGroups:Redis:Notifications:*` | Module inbox | **Không đổi** |

`UseRedisBackplane()` / `UseSseRedisFanOut()` fail-fast nếu thiếu `Realtime:Redis:Configuration` khi flag tương ứng bật.

### 5.3.1 Sample — bật/tắt transport (Q2 / D18)

Dev mặc định **cả hai** `Enabled: true`. Để test riêng:

```json
// Chỉ SignalR
"SignalR": { "Enabled": true, ... },
"Sse": { "Enabled": false, ... }

// Chỉ SSE
"SignalR": { "Enabled": false, ... },
"Sse": { "Enabled": true, ... }
```

`Sample/Program.cs` (pattern):

```csharp
var realtime = builder.AddCoreRealtime();
if (realtime.Options.SignalR.Enabled)
    realtime.UseSignalR();
if (realtime.Options.Sse.Enabled)
    realtime.UseSse();

// … sau Build …
if (realtimeOptions.SignalR.Enabled)
    app.MapRealtimeHub<TUser, TTenant>();
if (realtimeOptions.Sse.Enabled)
    app.MapRealtimeSse<TUser, TTenant>();
```

`AddNotificationAppServiceWithRealtime` vẫn gọi khi có inbox — composite chỉ chứa transport đã `Use*`.

### 5.4 SSE wire format


| Thành phần              | Giá trị                                                                                   |
| ----------------------- | ----------------------------------------------------------------------------------------- |
| Response `Content-Type` | `text/event-stream`                                                                       |
| Cache-Control           | `no-cache`                                                                                |
| Connection              | keep-alive                                                                                |
| Event                   | `event: {ClientMethodName}\n`                                                             |
| Data                    | `data: {json}\n\n` — serialize `message` object (System.Text.Json, camelCase đồng bộ API) |
| Heartbeat               | `: heartbeat {utc}\n\n` mỗi `HeartbeatSeconds`                                            |
| Client disconnect       | `CancellationToken` request aborted → remove registry                                     |


**Không** lưu lịch sử trên SSE stream (giống `IRealtimeNotifier` contract).

### 5.5 Redis fan-out SSE + shared multiplexer

Đối xứng **ý tưởng** SignalR backplane ([D9 Realtime/Inbox](./2026-08-12-adr-platform-realtime-inbox-boundary.md)), **không** dùng protocol SignalR backplane.


| Khía cạnh | SignalR backplane | SSE fan-out |
| --------- | ----------------- | ----------- |
| Kích hoạt | `UseRedisBackplane()` | `UseSseRedisFanOut()` |
| Connection | `Realtime:Redis:Configuration` | **Cùng** `Realtime:Redis:Configuration` |
| Channel prefix | `Realtime:SignalR:ChannelPrefix` | `Realtime:Sse:ChannelPrefix` |
| `ConnectionMultiplexer` | Singleton từ core `AddRealtimeRedis` | **Cùng instance** (D13) |
| Cơ chế | SignalR Redis protocol | App-defined JSON pub/sub |
| Channel pattern | SignalR internal | `{SseChannelPrefix}:sse:{groupName}` |

**Shared multiplexer (D13 / O8 / O14):**

```text
AddCoreRealtime()
  → bind Realtime:Redis:Configuration
  → đăng ký singleton IConnectionMultiplexer (khi có transport cần Redis)

UseRedisBackplane()   → dùng multiplexer + SignalR:ChannelPrefix
UseSseRedisFanOut()   → dùng CÙNG multiplexer + Sse:ChannelPrefix
```

| Tình huống Host | Multiplexer |
| --------------- | ----------- |
| Chỉ SignalR backplane | 1 multiplexer từ `Realtime:Redis` |
| Chỉ SSE fan-out | 1 multiplexer từ `Realtime:Redis` |
| Cả hai | **1 multiplexer** — không nhân đôi TCP |

**Luồng publish (fan-out bật):**

```text
SseRealtimeNotifier.PublishToGroupAsync(group, message)
  → Redis PUBLISH (shared multiplexer) platform-realtime-sse:sse:user:{guid}
  → (mọi instance, kể cả publisher) SseRedisFanOutSubscriber nhận
  → SseConnectionRegistry.GetByGroup(group) → ghi SSE local
```

**Luồng publish (fan-out tắt):** `SseRealtimeNotifier` → registry local → ghi SSE trực tiếp.

Connection SSE luôn **local** một instance; chỉ **message** đi qua Redis khi fan-out bật.

### 5.6 Identity — shared resolver (hub + SSE)

`RealtimeConnectionIdentityResolver` (core) thay logic trùng lặp trong hub/endpoint:

1. Resolve `userId` từ `ICurrentUser<TUser>` → fallback claim `NameIdentifier` / `sub`.
2. Resolve `tenantId` từ `ICurrentTenant<TTenant>` → claim `TenantClaimName` / `tenant_id` → header `X-Tenant-Id`.
3. Thiếu user hoặc tenant → **abort** (hub) hoặc **401/403** (SSE).
4. On success → join groups `RealtimeGroupNames.ForUser` + `ForTenant`.

`NotificationHub` gọi resolver trong `OnConnectedAsync`; `SseNotificationEndpoint` gọi **trước** khi mở stream.

### 5.7 `SseNotificationEndpoint` vs `NotificationHub` — giống và khác

**Không** copy class hub sang endpoint. Hai entry point **song song**, cùng inbox notifications, khác protocol.


| Khía cạnh | `NotificationHub` (SignalR) | `SseNotificationEndpoint` (SSE) |
| --------- | ------------------------- | ------------------------------- |
| Vai trò | Entry **SignalR** — WebSocket/long-poll | Entry **HTTP** — `GET` stream dài |
| Khi nào chạy | Client gọi `connection.start()` | Client `new EventSource(url)` |
| Lifecycle | `OnConnectedAsync` / `OnDisconnectedAsync` | Mở response → loop heartbeat → `CancellationToken` abort |
| Join group | `Groups.AddToGroupAsync(connectionId, …)` | `SseConnectionRegistry.Register(…, groups)` |
| Identity | `RealtimeConnectionIdentityResolver` | **Cùng resolver** |
| Publish ra | `HubRealtimeNotifier` → `IHubContext` | `SseRealtimeNotifier` → registry → `HttpResponse.WriteAsync` |
| Bidirectional | Có (hub methods) | **Không** — chỉ server → client |
| Auth header | SignalR negotiate + headers | Cookie / optional query token (P7) |

```text
                    RealtimeConnectionIdentityResolver
                              │
              ┌───────────────┴───────────────┐
              ▼                               ▼
    NotificationHub.OnConnected      SseNotificationEndpoint.Handle
              │                               │
              ▼                               ▼
    SignalR Groups                   SseConnectionRegistry
              │                               │
              ▼                               ▼
    HubRealtimeNotifier              SseRealtimeNotifier
```

**Tóm lại:** Endpoint SSE = **`OnConnectedAsync` của hub** cho world HTTP/SSE, không thay hub; publish vẫn qua `IRealtimeNotifier` → composite.

### 5.8 `IRealtimeTransport` vs `IRealtimeNotifier` — phân tích chọn O12

Hai interface có **cùng method signature** (`PublishToUser/Users/Group`) nhưng **vai trò DI khác nhau**.


| Tiêu chí | Tách — `IRealtimeNotifier` (public) + `IRealtimeTransport` (internal) **O12** | Không tách — chỉ `IRealtimeNotifier` **O13** |
| -------- | --------------------------------------------------------------------------- | --------------------------------------------- |
| Module inject | Một facade rõ: `IRealtimeNotifier` | Một facade nếu chỉ register composite |
| Đăng ký nhiều transport | `RealtimeBuilder` thêm `IRealtimeTransport`; composite inject `IEnumerable<IRealtimeTransport>` | Builder giữ `List<Delegate>` hoặc concrete types — **không** expose qua DI |
| Rủi ro DI | Thấp — không ai `GetService<HubRealtimeNotifier>()` nhầm notifier | Cao nếu ai đó `AddSingleton<IRealtimeNotifier, HubRealtimeNotifier>()` song song composite |
| Test unit | Mock `IRealtimeTransport` trong composite test | Mock concrete hoặc delegate list |
| Surface API package | `IRealtimeNotifier` public; transport internal | Chỉ một interface public |
| Số lượng type | +1 interface internal | Ít hơn |
| Khớp pattern repo | Giống tách orchestration vs provider (Auth composite, Blob `Use*`) | Đơn giản hơn cho team nhỏ |

**Quyết định: O12** — giữ **`IRealtimeNotifier`** là contract duy nhất cho **application/module**; **`IRealtimeTransport`** `internal` trong `Platform.Realtime` cho **satellite đăng ký**. Không public `IRealtimeTransport` ra ngoài assembly (trừ `InternalsVisibleTo` test).

Nếu sau này chỉ còn một transport trong production, composite vẫn hoạt động với list 1 phần tử — không đổi module.

### 5.9 Inventory / mapping (refactor)

```text
Platform.Realtime/Configuration/RealtimeSignalRRedisOptions.cs
  → đổi tên RealtimeRedisOptions.cs (bind Realtime:Redis)

Platform.Realtime/Configuration/PlatformRealtimeOptions.cs (hiện tại)
  → đổi tên class/file thành RealtimeSignalROptions.cs (bỏ nested Redis; thêm Enabled, ChannelPrefix)

Tạo mới Platform.Realtime/Configuration/PlatformRealtimeOptions.cs
  → SectionName = "Realtime"
  → ClientMethodName + Redis : RealtimeRedisOptions + SignalR : RealtimeSignalROptions + Sse : RealtimeSseOptions

Platform.Realtime.SignalR/Groups/RealtimeGroupNames.cs
  → Platform.Realtime/Groups/RealtimeGroupNames.cs

Platform.Realtime.SignalR/Extensions/RealtimeSignalRExtensions.cs
  → bỏ AddNotificationAppServiceWithRealtime

Platform.Modules.Notifications/Extensions/NotificationsHostBuilderExtensions.cs
  → thêm AddNotificationAppServiceWithRealtime(RealtimeBuilder)

Hub identity private methods
  → Platform.Realtime/Identity/RealtimeConnectionIdentityResolver.cs
```



### 5.10 So sánh transport (product)


|              | SignalR                         | SSE                         |
| ------------ | ------------------------------- | --------------------------- |
| Hướng        | Bidirectional (hub)             | Server → client             |
| Protocol     | WebSocket / long-poll           | HTTP `text/event-stream`    |
| Config path  | `Realtime:SignalR:SignalRPath`  | `Realtime:Sse:SsePath`      |
| Scale-out    | Redis backplane (built-in)      | Redis pub/sub fan-out (app) |
| Client       | `@microsoft/signalr`            | `EventSource`               |
| Default path | `/hubs/notifications`           | `/sse/notifications`        |


Host có thể bật **một hoặc cả hai**; module inbox không phân biệt.

### 5.11 `AddNotificationAppServiceWithRealtime` — vị trí tối ưu (Q3)

**Chốt:** extension nằm tại **`Platform.Modules.Notifications.Extensions`**, không ở core Realtime hay satellite SignalR.

```csharp
// Platform.Modules.Notifications.Extensions
public static RealtimeBuilder AddNotificationAppServiceWithRealtime<TUser, TTenant>(
    this RealtimeBuilder builder)
    where TUser : class, ICurrentUserIdentity
    where TTenant : class, ICurrentTenantIdentity
{
    builder.HostBuilder.AddNotificationAppService<TUser, TTenant>();
    builder.EnsureCompositeRealtimeNotifier();  // Platform.Realtime
    return builder;
}
```

**Vì sao đây là phương án tối ưu:**


| Phương án | Vấn đề |
| --------- | ------ |
| Để trong `Platform.Realtime` (core) | Core phải reference `Platform.Modules.Notifications` — vi phạm C2 (core không biết inbox product) |
| Để trong `Platform.Realtime.SignalR` | Host **chỉ SSE** vẫn phải reference SignalR package chỉ để đăng ký AppService — sai semantic |
| Để trong **module Notifications** | Module đã biết `INotificationAppService` + `IRealtimeNotifier`; chỉ **compose** hai framework; Host chỉ SSE gọi `UseSse()` + `AddNotificationAppServiceWithRealtime` **không** cần SignalR |

**Dependency graph sau refactor:**

```text
Platform.Modules.Notifications
  → Platform.Realtime (IRealtimeNotifier, RealtimeBuilder.EnsureCompositeRealtimeNotifier)
  → (không bắt buộc Platform.Realtime.SignalR hay .Sse)

Host
  → Platform.Modules.Notifications
  → Platform.Realtime.SignalR   (nếu dùng SignalR)
  → Platform.Realtime.Sse       (nếu dùng SSE)
```

`Platform.Realtime.SignalR` **bỏ** `ProjectReference` tới Notifications nếu không còn extension inbox (chỉ giữ Realtime + Domain).

---



## 6. Hệ quả


| Hướng         | Hệ quả                                                                                                 |
| ------------- | ------------------------------------------------------------------------------------------------------ |
| Tốt           | Opt-in SSE; đa transport qua composite; scale-out SSE đối xứng SignalR; module Notifications không đổi |
| Tốt           | Group naming một chỗ; options mở rộng transport sau (WebSocket raw, v.v.)                              |
| Xấu / chi phí | Breaking rename `HubPath` → `SignalRPath`; đổi shape options root `Realtime` |
| Xấu / chi phí | SSE fan-out phụ thuộc shared multiplexer — cần document khi chỉ SSE không SignalR |
| Rủi ro        | `AllowAccessTokenQuery` lộ token — default `false`                                                     |
| Rủi ro        | Double delivery nếu fan-out vừa local vừa Redis — bắt buộc test T8 (chỉ Redis path)                    |
| Trung lập     | Một `ConnectionMultiplexer` cho backplane + SSE fan-out khi cùng Redis server                             |


---



## 7. Confirm *(đã chốt 2026-08-17)*


| #   | Câu hỏi | Trả lời |
| --- | ------- | ------- |
| Q1  | Accept D1–D19? | **OK** — O2 + O4 + O6 + O8 + O11 + O12 + O14 |
| Q2  | Sample dev: SignalR / SSE? | **Cả hai** `Enabled: true` mặc định; toggle độc lập (§5.3.1 / D18) |
| Q3  | Vị trí `AddNotificationAppServiceWithRealtime`? | **`Platform.Modules.Notifications.Extensions`** (§5.11 / D10) |
| Q4  | Fan-out publish khi `UseRedisFanOut`? | **Chỉ Redis path** — mọi instance subscribe, deliver local |
| Q5  | Shared identity resolver? | **`RealtimeConnectionIdentityResolver`** trong core (D7) |
| Q6  | Obsolete bind config cũ? | **1 minor release** + comment Sample |

---



## 8. Test cases



### 8.1 Smoke *(bắt buộc — ≥ 1)*


| #   | Case                                                                                       | Expect                                                    | Nơi                                               | Trạng thái |
| --- | ------------------------------------------------------------------------------------------ | --------------------------------------------------------- | ------------------------------------------------- | ---------- |
| T1  | Host `AddCoreRealtime().UseSignalR().UseSse().AddNotificationAppServiceWithRealtime` build | Resolve `IRealtimeNotifier` = `CompositeRealtimeNotifier` | `UnitTest/Realtime/RealtimeHostRegistrationTests` | 🔴         |
| T2  | `MapRealtimeSse` + GET authenticated stream                                                | `200`, `text/event-stream`, heartbeat comment             | `UnitTest/Realtime/SseEndpointTests` (mới)        | 🔴         |




### 8.2 Regression — test cũ cần giữ xanh *(bắt buộc)*


| #   | Case                                         | Expect                                              | Nơi                                                                            | Trạng thái |
| --- | -------------------------------------------- | --------------------------------------------------- | ------------------------------------------------------------------------------ | ---------- |
| T3  | `UseRedisBackplane` missing config throws    | Message chứa `Realtime:Redis:Configuration` | `RealtimeHostRegistrationTests.UseRedisBackplane_Missing_Configuration_Throws` | 🟡         |
| T4  | `HubRealtimeNotifier` publish group          | `SendAsync` đúng method + group                     | `UnitTest/Realtime/HubRealtimeNotifierTests`                                   | 🟡         |
| T5  | `RealtimeGroupNames` format                  | `user:{D}`, `tenant:{D}`                            | `UnitTest/Realtime/RealtimeGroupNamesTests`                                    | 🟡         |
| T6  | Options bind nested `Realtime:SignalR`       | `SignalRPath`, backplane flags                      | `UnitTest/Realtime/RealtimeOptionsBindingTests`                                | 🟡         |
| T7  | `AddCoreRealtime` alone không register store | `INotificationStore` null                           | `RealtimeHostRegistrationTests`                                                | 🟡         |




### 8.3 Test mới *(semantic/API mới)*


| #   | Case                                                        | Expect                                                    | Nơi                                                | Trạng thái |
| --- | ----------------------------------------------------------- | --------------------------------------------------------- | -------------------------------------------------- | ---------- |
| T8  | SSE publish với fan-out: 2 instance logic (mock subscriber) | Mỗi connection local nhận đúng 1 lần                      | `UnitTest/Realtime/SseRedisFanOutTests`            | 🔴         |
| T9  | SSE connection thiếu tenant                                 | Không mở stream; 401/403                                  | `SseEndpointTests`                                 | 🔴         |
| T10 | `UseSseRedisFanOut` missing `Configuration`                 | `InvalidOperationException` fail-fast                     | `RealtimeHostRegistrationTests`                    | 🔴         |
| T11 | `CompositeRealtimeNotifier` fan-out 2 transports            | Cả `HubRealtimeNotifier` + `SseRealtimeNotifier` được gọi | `UnitTest/Realtime/CompositeRealtimeNotifierTests` | 🔴         |
| T12 | Config migration `HubPath` + `SignalR:Redis:*` → root `Redis` | Bind path/prefix cũ | `RealtimeOptionsBindingTests` | 🔴 |
| T13 | Shared multiplexer từ `Realtime:Redis` | Một instance cho backplane + fan-out | `RealtimeRedisMultiplexerTests` | 🔴 |
| T14 | Sample `SignalR.Enabled=false` | Không map hub; SSE vẫn chạy nếu enabled | `RealtimeHostRegistrationTests` hoặc integration | 🔴 |




### 8.4 Done khi

- Mọi hàng **Smoke** + **Regression** 🟢.
- Hàng **Test mới** T8–T14 🟢.
- Sample chạy được `MapRealtimeHub` + `MapRealtimeSse` (manual smoke).

---



## 9. Kế hoạch triển khai


| Phase | Việc                                                                                                                                          | Done khi           | Phụ thuộc |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------- | ------------------ | --------- |
| 0     | Confirm §7                                                                                                                                    | 🟢 Accepted 2026-08-17 | —         |
| 1     | Core refactor: rename options, `RealtimeGroupNames`, `IRealtimeTransport` internal, `CompositeRealtimeNotifier`, `EnsureCompositeRealtimeNotifier`, move inbox extension | T3–T7, T12–T13 🟢 | 0         |
| 2     | Package `Platform.Realtime.Sse`: registry, endpoint, `SseRealtimeNotifier`, `UseSse`, `MapRealtimeSse`                                        | T1, T2, T9, T11 🟢 | 1         |
| 3     | `UseSseRedisFanOut` + hosted subscriber + publish path                                                                                        | T8, T10 🟢         | 2         |
| 4     | Sample appsettings + `Program.cs` map SSE; README module notifications                                                                        | Manual smoke       | 2         |
| 5     | FE EventSource (optional, PR riêng)                                                                                                           | Product            | 4         |


**Thứ tự PR gợi ý:** Phase 1 (core) → Phase 2 (SSE) → Phase 3 (Redis fan-out) — tránh PR khổng lồ.

---



## 10. Checklist Done


| #   | Việc                                                      | Trạng thái |
| --- | --------------------------------------------------------- | ---------- |
| 1   | §7 Confirm team                                           | 🟢 |
| 2   | Core refactor merged                                      | 🔴 |
| 3   | `Platform.Realtime.Sse` package + DI                      | 🔴 |
| 4   | Redis fan-out SSE                                         | 🔴 |
| 5   | Sample + config migration                                 | 🔴 |
| 6   | §8 Smoke + Regression + Test mới 🟢                       | 🔴 |
| 7   | ADR status → 🟡 Accepted + [README index](./README.md)    | 🟢 |


---



## 11. Tham chiếu thêm

- [Realtime vs Inbox](./2026-08-12-adr-platform-realtime-inbox-boundary.md) — D1 boundary, D5 DI, D6 paths, D9 backplane
- [architecture-rules.md](./architecture-rules.md) §0.2 Core + satellite, §3.4 fluent builder
- [MDN: Server-sent events](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events)
- ASP.NET Core SignalR Redis backplane — pattern tham khảo, **không** reuse implementation
- Commit / PR implement (điền khi xong)

