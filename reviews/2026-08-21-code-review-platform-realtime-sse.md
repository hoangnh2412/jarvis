# Code review: `Platform.Realtime` multi-transport + SSE satellite

| Mục | Giá trị |
|-----|---------|
| Ngày review | 2026-08-21 |
| Tiêu chuẩn | [ADRs/architecture-rules.md](../ADRs/architecture-rules.md) |
| ADR liên quan | [2026-08-17-adr-platform-realtime-sse](../ADRs/2026-08-17-adr-platform-realtime-sse.md), [2026-08-12-adr-platform-realtime-inbox-boundary](../ADRs/2026-08-12-adr-platform-realtime-inbox-boundary.md) |
| Phạm vi | `Platform.Realtime`, `Platform.Realtime.SignalR`, `Platform.Realtime.Sse`, wiring `Platform.Modules.Notifications`, `Sample/Program.cs`, `UnitTest/Realtime` |
| Trọng tâm | Bug, regression hành vi, bảo mật, thiếu test |
| Tests | `UnitTest/Realtime` — **20/20 passed** tại thời điểm review |

---

## Verdict

Refactor **đúng hướng ADR**: tách core/satellite, `CompositeRealtimeNotifier`, shared `RealtimeGroupNames` + `RealtimeConnectionIdentityResolver`, SSE registry + Redis fan-out, shared Redis multiplexer. Happy path có test tốt.

**Chưa sẵn sàng production** nếu chưa xử lý P1 (desync fan-out registration, API `Use*()` không generic) và P2 (composite rỗng, breaking config, validator).

---

## Findings (theo mức độ nghiêm trọng)

### [P1] `UseRedisFanOut` bật qua `IOptions` nhưng subscriber không được đăng ký

**File:** `frameworks/Platform.Realtime.Sse/Extensions/RealtimeSseExtensions.cs:23-24`, `frameworks/Platform.Realtime.Sse/Services/SseRealtimeNotifier.cs:51-52`

`UseSse()` quyết định gọi `UseSseRedisFanOut()` dựa trên **`builder.Options` snapshot** lúc `AddCoreRealtime()`:

```csharp
if (builder.Options.Sse.UseRedisFanOut)
    builder.UseSseRedisFanOut();
```

Trong khi `SseRealtimeNotifier` đọc **`IOptions<PlatformRealtimeOptions>` lúc publish**:

```csharp
if (options.Value.Sse.UseRedisFanOut)
    return PublishViaRedisFanOutAsync(groupName, eventName, jsonData);
```

**Kịch bản lỗi:** Host gọi `AddCoreRealtime().UseSse<TUser, TTenant>()` khi snapshot `UseRedisFanOut = false`, sau đó bật fan-out bằng `services.Configure<PlatformRealtimeOptions>(o => o.Sse.UseRedisFanOut = true)` (hoặc config source bind sau snapshot). Kết quả:

- Publish **chỉ** `PUBLISH` Redis, **không** ghi local
- `SseRedisFanOutSubscriber` **không** được đăng ký → **không instance nào deliver**
- Inbox vẫn save (persist-first OK), nhưng **push SSE mất hoàn toàn**, không exception

**Đề xuất:** Đồng bộ quyết định đăng ký subscriber với `IOptions` lúc runtime (hoặc validate lúc startup: `UseRedisFanOut == true` ⇒ bắt buộc có `SseRedisFanOutSubscriber` + `IRealtimeRedisMultiplexer`).

---

### [P1] `UseSignalR()` / `UseSse()` không generic — thiếu DI bắt buộc

**File:** `frameworks/Platform.Realtime.SignalR/Extensions/RealtimeSignalRExtensions.cs`, `frameworks/Platform.Realtime.Sse/Extensions/RealtimeSseExtensions.cs`

Hai overload không generic **chỉ** đăng ký infrastructure, **không** đăng ký:

- `RealtimeConnectionIdentityResolver<TUser, TTenant>` (bắt buộc cho hub + SSE endpoint)
- `HubRealtimeNotifier<TUser, TTenant>` (SignalR transport)

Host theo README cũ (`UseSignalR()` không type) + `MapRealtimeHub<TUser, TTenant>()` sẽ:

- Hub connect: fail DI khi resolve `NotificationHub` (thiếu resolver), **hoặc**
- Push: composite không có transport SignalR → **notify save OK nhưng không push**

Sample đã dùng `UseSignalR<TUser, TTenant>()` — OK — nhưng public API vẫn là footgun.

**Đề xuất:** Obsolete/remove overload không generic, hoặc throw rõ lúc `Map*` nếu resolver/transport chưa đăng ký.

---

### [P2] Composite rỗng — push im lặng thành no-op

**File:** `frameworks/Platform.Realtime/Services/CompositeRealtimeNotifier.cs:42-43`, `modules/notifications/Platform.Modules.Notifications/Extensions/NotificationsHostBuilderExtensions.cs:37-38`

```csharp
if (transports.Count == 0)
    return Task.CompletedTask;
```

Host có thể:

```csharp
AddCoreRealtime().AddNotificationAppServiceWithRealtime<TUser, TTenant>()
// quên UseSignalR / UseSse
```

Build thành công, `NotifyUserAsync` save inbox OK, push **không làm gì**, không log/warning (chỉ catch nếu transport throw — ở đây không throw).

**Đề xuất:** Fail-fast lúc startup hoặc log warning khi `IEnumerable<IRealtimeTransport>` rỗng sau `EnsureCompositeRealtimeNotifier()`.

---

### [P2] Breaking change config Redis — không có migration path

**File:** `frameworks/Platform.Realtime.SignalR/Extensions/RealtimeSignalRBackplaneExtensions.cs`

Config cũ `Realtime:SignalR:Redis:Configuration` đã bỏ; bắt buộc `Realtime:Redis:Configuration`. Validator/test đã cập nhật, nhưng **không có** legacy bind / migration helper.

Deploy cũ không đổi config → startup fail hoặc backplane/fan-out không hoạt động. ADR §3 C9 cho phép breaking **nếu có migration path** — hiện chưa có.

**Đề xuất:** Document cut-over rõ trong README module + Sample; hoặc thêm bind fallback 1 release cho key cũ.

---

### [P2] `PlatformRealtimeOptionsValidator` luôn validate cả hai path

**File:** `frameworks/Platform.Realtime/Configuration/PlatformRealtimeOptionsValidator.cs:9-13`

Validator yêu cầu cả `SignalRPath` và `SsePath` **không xét** `Enabled`. Host chỉ dùng SSE (`SignalR.Enabled=false`) vẫn phải có `SignalRPath` hợp lệ (default cứu được). Nếu Host xóa path của transport tắt → **`ValidateOnStart` fail** dù không dùng transport đó.

ADR P2 đã nêu vấn đề tương tự với `HubPath`; refactor chưa giải quyết triệt để.

**Đề xuất:** Validate path chỉ khi transport tương ứng `Enabled == true`.

---

### [P2] Documentation drift — README module chưa cập nhật

**File:** `modules/notifications/README.md:106`

README vẫn hướng dẫn `.UseSignalR()` (không generic) và config shape cũ. Sample đã migrate sang nested `Realtime:SignalR` / `Realtime:Sse` / `Realtime:Redis` — doc chưa → regression vận hành cho team khác.

---

### [P3] `SseConnectionRegistry.Register` overwrite `connectionId` — leak group index

**File:** `frameworks/Platform.Realtime.Sse/Services/SseConnectionRegistry.cs:73-83`

Nếu `Register` cùng `connectionId` lần 2 (bug caller), entry cũ bị overwrite trong `_connections` nhưng **group index cũ không được dọn**. Publish tới group cũ vẫn deliver tới connection đã đổi group.

`connectionId = Guid.NewGuid()` trong endpoint → khó xảy ra production, nhưng registry API không an toàn khi reuse id.

---

### [P3] `Enabled` flags không được framework enforce

**File:** `Sample/Program.cs`

`SignalR.Enabled` / `Sse.Enabled` chỉ được Sample check thủ công. Gọi `UseSse()` + `MapRealtimeSse()` khi `Enabled=false` vẫn wire đầy đủ service/endpoint. Dễ cấu hình sai, khó debug.

**Đề xuất:** `UseSignalR` / `UseSse` / `Map*` tôn trọng `Enabled` trong framework, hoặc document bắt buộc guard ở Host.

---

### [P3] `EnsureRealtimeRedis` connect đồng bộ lúc DI

**File:** `frameworks/Platform.Realtime/Extensions/RealtimeRedisExtensions.cs:28-30`

```csharp
Multiplexer = ConnectionMultiplexer.Connect(redisConfig)
```

Redis unreachable → **block/fail lúc registration**, không lazy. Pattern phổ biến nhưng làm startup brittle khi Redis chậm/down (đặc biệt test/local dev).

---

### Residual risk (không phải regression mới)

| Chủ đề | Ghi chú |
|--------|---------|
| `AllowAccessTokenQuery` | Default `false`. Khi bật: token lộ qua log/Referer — ADR D16 đã document |
| `MapRealtimeSse` không `.RequireAuthorization()` | Handler tự 401 nếu thiếu identity — phù hợp SSE + query token; anonymous vẫn mở TCP trước 401 |
| Tenant header fallback | `X-Tenant-Id` trong `RealtimeConnectionIdentityResolver` — cùng semantic hub cũ |

---

## Những phần làm tốt

| Area | Đánh giá |
|------|----------|
| Composite + `IRealtimeTransport` | Đúng ADR O4/O12; fan-out SignalR + SSE song song |
| Shared `RealtimeGroupNames` + `RealtimeConnectionIdentityResolver` | Loại duplicate logic hub/SSE |
| SSE fan-out protocol | Publish-only-Redis + subscriber deliver local — tránh double delivery (`SseRedisFanOutTests`) |
| Shared Redis multiplexer | `EnsureRealtimeRedis` idempotent; backplane dùng `ConnectionFactory` |
| Persist-first | `NotificationAppService` vẫn save trước, catch push fail |
| `RealtimeBuilder` | Bỏ `ISignalRServerBuilder` khỏi core — đúng D5 |
| Tests | Endpoint stream + heartbeat, fan-out plumbing, composite registration, options binding |

---

## Test gaps

| # | Scenario | Ghi chú |
|---|----------|---------|
| 1 | `AddNotificationAppServiceWithRealtime` không `UseSignalR`/`UseSse` | Assert fail-fast hoặc warning |
| 2 | `UseRedisFanOut` bật sau snapshot (`Configure` sau `UseSse`) | Regression cho P1 |
| 3 | `UseSignalR()` không generic + `MapRealtimeHub<T,T>` | Expect fail rõ ràng |
| 4 | Composite: một transport throw, transport kia OK | Partial delivery behavior |
| 5 | E2E SSE nhận `event: notification` sau `PublishToUserAsync` | Hiện chỉ test heartbeat + fan-out plumbing |
| 6 | Validator khi `SignalR.Enabled=false` / `Sse.Enabled=false` | Path rỗng không fail |

---

## Phạm vi thay đổi (tóm tắt)

```text
frameworks/
├── Platform.Realtime/           # Options root, composite, groups, identity, shared Redis
├── Platform.Realtime.SignalR/   # Hub transport, backplane, builder holder
└── Platform.Realtime.Sse/       # Registry, notifier, fan-out subscriber, endpoint

modules/notifications/
└── Platform.Modules.Notifications/  # AddNotificationAppServiceWithRealtime → composite

Sample/Program.cs                # Toggle SignalR/SSE theo Enabled flags
UnitTest/Realtime/               # 20 tests (composite, SSE, fan-out, registration)
```

---

## Action items (ưu tiên)

1. **P1** — Đồng bộ đăng ký `SseRedisFanOutSubscriber` với `IOptions.Sse.UseRedisFanOut` lúc runtime / validate startup
2. **P1** — Deprecate hoặc guard `UseSignalR()` / `UseSse()` không generic
3. **P2** — Guard composite rỗng lúc startup
4. **P2** — Cập nhật `modules/notifications/README.md` + document config migration
5. **P2** — Validator chỉ require path khi transport enabled
6. **P3** — Bổ sung test gaps ở bảng trên
