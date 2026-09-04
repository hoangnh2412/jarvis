# ADR — Tạo `Jarvis.Multitenancy` (slice trước package-split)

> **Trạng thái:** 🟢 **Accepted + Implemented** (2026-08-06). Prerequisite done cho [split packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) Phase 3.  
> **Ngày:** 2026-08-06 · Accept §7: 2026-08-06 · Implement: 2026-08-06  
> **Loại:** Package boundary / foundation  
> **Liên quan:** [adr-techdebt-split-current-user-tenant-packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) (🟡 Accepted — Phase 2 tách thành ADR này), [adr-current-user-tenant](./2026-08-01-adr-current-user-tenant.md) (API + D5 🟢), [architecture-software.md](./architecture-software.md).  
> **Phạm vi:** tạo package **`Jarvis.Multitenancy`**; move **`CurrentTenant`** + **`CurrentTenantAccessor`** + DI **`AddCurrentTenant()`**; cut-over Sample/EF/tests cho nửa Tenant; Domain giữ contract `ICurrentTenant` / `ICurrentTenantAccessor`.  
> **Ngoài phạm vi:** move `CurrentUser*` → Authentication (ADR split Phase 3); xóa hẳn `AddCoreCurrentContext` (làm khi Auth cũng tách); move resolvers `DataStorages` (Phase 5 split); [`Jarvis.Multitenancy.EntityFramework`](./2026-08-06-adr-jarvis-multitenancy-entityframework.md) (🟡 Accepted); đổi semantic R2; Tenants CRUD → [ADR `Jarvis.Tenants`](./2026-08-07-adr-jarvis-tenants-module.md).  
> **Chú thích icon:** 🟢 xong 100% · 🟡 đã làm, còn việc · 🔴 chưa làm

---

## 1. Bối cảnh

ADR [split packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) đã **Accepted**: User → Authentication, Tenant → Multitenancy.

`Jarvis.Authentication` **đã có**. Package tenant **chưa có** → không chỗ đặt `CurrentTenant` / `CurrentTenantAccessor`.

Làm cả User + Tenant trong một PR lớn = rủi ro cut-over DI. Slice này tạo **Multitenancy trước**, chứng minh pattern Host compose, rồi mới tách User.

Hiện tại:

| Thành phần | Chỗ |
|------------|-----|
| `ICurrentTenant`, `ICurrentTenantAccessor` | Domain (giữ) |
| `CurrentTenant`, `CurrentTenantAccessor` | Domain (move) |
| Đăng ký tenant | `AddCoreCurrentContext` (Domain) + EF `TryAdd` accessor |
| Resolvers | Domain `DataStorages` (chưa move) |

---

## 2. Quyết định (Accepted 2026-08-06)

| # | Quyết định | Chi tiết |
|---|------------|----------|
| M1 | Tên package | **`Jarvis.Multitenancy`** (`frameworks/Jarvis.Multitenancy`) |
| M2 | Nội dung đợt này | `CurrentTenant`, `CurrentTenantAccessor`, `AddCurrentTenant()` |
| M3 | Domain | Chỉ còn `ICurrentTenant`, `ICurrentTenantAccessor`; xóa class đã move |
| M4 | DI Host | Sample (và host dùng tenant): gọi **`AddCurrentTenant()`** bên cạnh `AddCoreCurrentContext` (user vẫn Domain tạm) |
| M5 | EF | **Bỏ** `TryAddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>()` — impl chỉ đăng ký qua Multitenancy. **Không** tạo `Jarvis.Multitenancy.EntityFramework` trong ADR này (sau) |
| M6 | `AddCoreCurrentContext` | **Bỏ** đăng ký tenant/accessor tenant; vẫn đăng ký User + `WorkContext` đến khi ADR split Phase 3–4 |
| M7 | Resolvers | **Không** move trong ADR này (để Domain / Phase 5) |

### Target (sau ADR này)

```text
Jarvis.DDD.Domain
  ├─ ICurrentTenant, ICurrentTenantAccessor     (contract)
  ├─ CurrentUser*, AddCoreCurrentContext        (tạm — chờ split Phase 3–4)
  └─ DataStorages resolvers                     (tạm — Phase 5)

Jarvis.Multitenancy
  ├─ CurrentTenant
  ├─ CurrentTenantAccessor
  └─ AddCurrentTenant()

Host
  └─ AddCoreCurrentContext<T>() + AddCurrentTenant()
```

```text
  Host
   ├── AddCoreCurrentContext<T>   → Domain (User + WorkContext)
   └── AddCurrentTenant()         → Multitenancy (Tenant + accessor)
         └── ref Domain contracts only
```

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Multitenancy → Domain only; Domain **không** reference Multitenancy |
| C2 | Không reference `Jarvis.Authentication` |
| C3 | Semantic R2 / `Change` / ambient không đổi |
| C4 | Một `ICurrentTenantAccessor` ambient — chỉ Multitenancy đăng ký impl |
| C5 | OTEL.DDD không đổi (vẫn contract Domain) |
| C6 | Dọn bản trùng `DataStorages/CurrentTenantAccessor` (nếu còn) khi move |

---

## 4. Kế hoạch triển khai

| Phase | Việc | Done khi |
|-------|------|----------|
| 0 | Accept ADR (M1–M7) | 🟢 Status → Accepted |
| 1 | Tạo `Jarvis.Multitenancy.csproj` + thêm `Jarvis.sln` | 🟢 Project build |
| 2 | Move `CurrentTenant` + `CurrentTenantAccessor`; `AddCurrentTenant()` | 🟢 Domain không còn hai class |
| 3 | Domain: `AddCoreCurrentContext` bỏ tenant DI; EF bỏ `TryAdd` accessor | 🟢 Không đăng ký đôi |
| 4 | Sample: `AddCurrentTenant()` + ProjectReference; UnitTest cập nhật | 🟢 Build + `CurrentUserTenantTests` xanh |
| 5 | Cập nhật ADR split (Phase 2 🟢) + README | 🟢 Docs đồng bộ |

### `AddCurrentTenant` (sketch)

```csharp
public static IHostApplicationBuilder AddCurrentTenant(this IHostApplicationBuilder builder)
{
    builder.Services.TryAddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
    builder.Services.TryAddScoped<ICurrentTenant, CurrentTenant>();
    return builder;
}
```

Yêu cầu Host đã có `ITenantIdResolverFactory` (`AddCurrentTenant` / `AddTenantIdResolvers`) khi resolve `GetIdAsync` R2.

### Unit tests (`UnitTest/Multitenancy/CurrentTenantTests.cs`)

| Id | Test | Cover | Mô tả (VI) |
|----|------|-------|------------|
| MT_01 | `MT_01_Accessor_BeginScope_Sets_And_Restores` | Accessor ambient set/restore | `BeginScope` gán tenant ambient và khôi phục khi dispose |
| MT_02 | `MT_02_Accessor_Nested_BeginScope_Restores_Outer` | Nested scope | Scope lồng nhau: dispose inner → trả lại ambient outer |
| MT_03 | `MT_03_GetIdAsync_Prefers_Ambient_Over_Resolver` | R2: ambient > resolver | Có ambient thì `GetIdAsync` không gọi resolver |
| MT_04 | `MT_04_GetIdAsync_Falls_Back_To_Resolver_When_Ambient_Empty` | R2 fallback | Không ambient → lấy tenant từ `ITenantIdResolverFactory` |
| MT_05 | `MT_05_GetIdAsync_Caches_Resolver_Result_On_Scoped_Instance` | Cache scoped | Kết quả resolver được cache trên instance scoped (chỉ gọi 1 lần) |
| MT_06 | `MT_06_Change_Sets_And_Restores_Ambient` | `Change` / dispose | `Change` gán ambient làm việc và khôi phục khi dispose |
| MT_07 | `MT_07_Change_Empty_Guid_Throws` | Validation | `Change(Guid.Empty)` ném `ArgumentException` |
| MT_08 | `MT_08_AddCurrentTenant_Registers_Accessor_And_CurrentTenant` | DI lifetimes | `AddCurrentTenant` đăng ký accessor singleton + `ICurrentTenant` scoped |
| MT_09 | `MT_09_AddCurrentTenant_Is_Idempotent` | `TryAdd` idempotent | Gọi `AddCurrentTenant` hai lần không đăng ký trùng |

Cross-cutting (User + Tenant) vẫn ở `UnitTest/Services/CurrentUserTenantTests.cs` (`WorkContext_*`, `D5_*`).

---

## 5. Hệ quả

### Tích cực

- Có nhà cho tenant impl; mở đường ADR split Phase 3 (User → Auth).
- Host đã compose tenant độc lập với user.
- EF không kéo class accessor từ Domain.

### Chi phí / tạm thời

- Sample gọi **hai** extension (`AddCoreCurrentContext` + `AddCurrentTenant`) đến khi xóa `AddCoreCurrentContext`.
- Resolvers vẫn Domain.
- EF multitenancy wiring vẫn trong `Jarvis.EntityFramework` (chưa có Multitenancy.EF).

### Rủi ro

- Quên `AddCurrentTenant` → thiếu `ICurrentTenant` / accessor lúc runtime. Mitigation: Sample + test cut-over cùng PR; doc Host.

---

## 6. Ngoài phạm vi → ADR tiếp

| Việc | ADR / phase |
|------|-------------|
| Move `CurrentUser*` → Authentication; xóa `AddCoreCurrentContext` | [split packages](./2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) Phase 3–4 |
| Move resolvers `DataStorages` | split Phase 5 |
| `Jarvis.Multitenancy.EntityFramework` | [ADR Multitenancy.EF](./2026-08-06-adr-jarvis-multitenancy-entityframework.md) (🟢 Done) |

---

## 7. Checklist confirm

### Confirm §7 (2026-08-06)

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| 1 | Accept M1–M7? | **OK** |
| 2 | Tên package | **`Jarvis.Multitenancy`** |
| 3 | Đợt này | Chỉ Tenant impl + accessor + `AddCurrentTenant` (không User, không resolvers) |
| 4 | `AddCoreCurrentContext` | Tạm giữ cho User; bỏ phần tenant DI |
| 5 | EF accessor | Bỏ `TryAdd` trong `Jarvis.EntityFramework`; **không** tách `Jarvis.Multitenancy.EntityFramework` (sau) |
| 6 | Resolvers (M7) | **OK** — không move đợt này |

- [x] Accept M1–M7
- [x] Tên package: `Jarvis.Multitenancy`
- [x] Đợt này: chỉ Tenant impl + accessor + `AddCurrentTenant` (không User, không resolvers)
- [x] `AddCoreCurrentContext` tạm giữ cho User; bỏ phần tenant DI
- [x] EF bỏ đăng ký accessor impl; không tạo Multitenancy.EF đợt này

---

## 8. Trạng thái

| Mục | Giá trị |
|-----|---------|
| Decision | 🟢 **Accepted** (2026-08-06) |
| Implementation | 🟢 **Done** (2026-08-06) |
| Blocked by | — |
| Unblocks | ADR split Phase 2 🟢 → tiếp Phase 3–4 (Auth) |
| Owner | Jarvis |
