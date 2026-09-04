# ADR — `IStorageContext.TenantId` private; app đọc tenant qua `ICurrentTenant`

> **Trạng thái:** 🟢 **Accepted + Implemented** (confirm §7 + code 2026-08-12)  
> **Ngày:** 2026-08-12 · Accept: 2026-08-12 · Implement: 2026-08-12  
> **Loại:** API / boundary (Domain contract + ORM.EF)  
> **Liên quan:** [current-user-tenant](./2026-08-01-adr-current-user-tenant.md) (D5 home vs current), [generic current tenant](./2026-08-06-adr-generic-current-tenant.md), [Multitenancy.EF](./2026-08-06-adr-jarvis-multitenancy-entityframework.md), [ORM packages](./2026-08-07-adr-jarvis-orm-packages.md), [architecture-software.md](./architecture-software.md)  
> **Phạm vi:** Ẩn `TenantId` khỏi surface công khai của `IStorageContext` / `BaseStorageContext`; chuẩn hóa nguồn tenant cho app = `ICurrentTenant<TTenant>`; giữ snapshot nội bộ cho EF global query filter.  
> **Ngoài phạm vi:** Đổi semantic R2 / D5; inject `ICurrentTenant` vào DbContext; redesign `ITenantIdResolverFactory`; Tenants CRUD.  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm

---

## 1. Bối cảnh

Hiện có **hai chỗ** có thể đọc working tenant id:

| Nguồn | API | Vai trò đúng |
|-------|-----|--------------|
| App / service | `ICurrentTenant<TTenant>.GetIdAsync()` / `Change` | Ambient + R2 + profile — [G4](./2026-08-06-adr-generic-current-tenant.md) |
| Persistence | `IStorageContext.TenantId` + `SetTenantId` | Snapshot trên DbContext cho EF query filter |

`BaseStorageContext` expose public:

```csharp
public Guid? TenantId { get; set; }
public virtual void SetTenantId(Guid? tenantId) => TenantId = tenantId;
```

Filter:

```csharp
modelBuilder.Entity(clrType).AddQueryFilter<ITenantEntity>(x => x.TenantId == TenantId);
```

UoW (`BaseUnitOfWork`) resolve tenant → `SetTenantId` trước query/save. Validation save đọc `storage.TenantId` qua `IStorageContext`.

App dễ nhầm: lấy tenant từ `dbContext.TenantId` thay vì `ICurrentTenant` — lệch ISP và D5 (working tenant = `ICurrentTenant*`).

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | `TenantId` public trên `IStorageContext` / `BaseStorageContext` | Hai API đọc tenant; app có thể bypass `ICurrentTenant` |
| P2 | Setter public trên property | Có thể gán tenant ngoài UoW → filter/save lệch ambient |
| P3 | Muốn “chỉ dùng `ICurrentTenant`” nhưng EF filter cần giá trị sync trên context | Không thể bỏ hẳn field trên DbContext |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | **D5 giữ nguyên:** working tenant = `ICurrentTenant*` / accessor; không đổi home vs current |
| C2 | **R2 giữ nguyên:** `GetIdAsync` = ambient ?? `ITenantIdResolverFactory` |
| C3 | EF global query filter **bắt buộc** có snapshot sync trên `DbContext` instance (không dùng `GetIdAsync` async trong expression) |
| C4 | `Jarvis.ORM.EntityFramework` **không** reference `Jarvis.Multitenancy`; UoW chỉ consume port Domain (`ICurrentTenantAccessor`, `ITenantIdResolverFactory`) |
| C5 | **Không** inject `ICurrentTenant` / `ICurrentTenantAccessor` vào `BaseStorageContext` chỉ để filter đọc ambient |
| C6 | Breaking change API: bỏ getter public `TenantId` trên `IStorageContext` — Sample/tests/consumers phải cắt đọc trực tiếp |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | Giữ public `TenantId` | Status quo | Không churn | Hai nguồn đọc; dễ lệch `ICurrentTenant` |
| O2 | Private snapshot + bỏ getter khỏi `IStorageContext`; app = `ICurrentTenant` | Ẩn field; UoW vẫn `SetTenantId` | Một nguồn sự thật cho app; filter vẫn hoạt động | Breaking; validation cần đọc nội bộ |
| O3 | Inject `ICurrentTenant` vào DbContext / filter đọc accessor | Bỏ `SetTenantId` | Ít state trên context | Async không vào filter; ORM.EF phụ thuộc Multitenancy semantic sâu hơn; khó test/filter timing |

**Chọn O2.**

---

## 5. Quyết định

Chúng ta **ẩn** tenant id trên storage surface công khai; app **chỉ** đọc/đổi working tenant qua `ICurrentTenant`. DbContext giữ snapshot **private** cho EF.

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Nguồn sự thật (app) | Working tenant id/profile **chỉ** qua `ICurrentTenant<TTenant>` (`GetIdAsync` / `GetAsync` / `Change`) |
| D2 | `IStorageContext` | **Bỏ** `Guid? TenantId { get; }`; **giữ** `void SetTenantId(Guid? tenantId)` (UoW / infra) |
| D3 | `BaseStorageContext` | `TenantId` → **private**. Thêm `internal bool HasTenantId` (Q3 = A). Không public get/set |
| D4 | Ai gọi `SetTenantId` | Chỉ `BaseUnitOfWork` (và test infra tương đương) sau khi resolve `_switchedTenantId` / R2 |
| D5 | EF filter | Tiếp tục `x => x.TenantId == TenantId` trên private member của cùng class — **không** đọc `ICurrentTenant` trong `OnModelCreating` |
| D6 | Save validation | `TenantScopedContextValidation`: `context is BaseStorageContext b && b.HasTenantId` → không bắt buộc tenant |
| D7 | Cấm | App/service **không** đọc tenant từ DbContext; **không** inject `ICurrentTenant` vào `BaseStorageContext` cho filter |

### 5.1 Flow sau quyết định

```text
HTTP / Job
  └─ ICurrentTenant.Change / ambient / R2
        │
        ▼
BaseUnitOfWork.EnsureDbContextAsync
  ├─ tenantId = switched ?? ITenantIdResolverFactory
  └─ SetTenantId(tenantId)     ← ghi snapshot private
        │
        ▼
BaseStorageContext (private TenantId + internal HasTenantId)
  └─ Global query filter: entity.TenantId == context.TenantId

App / Application service
  └─ ICurrentTenant.GetIdAsync() / GetAsync()   ← đọc tenant ở đây
  └─ (không) dbContext.TenantId / IStorageContext.TenantId
```

### 5.2 Mapping API

| Trước | Sau |
|-------|-----|
| `IStorageContext.TenantId` (get) | **Xóa** — app dùng `ICurrentTenant.GetIdAsync()` |
| `IStorageContext.SetTenantId` | **Giữ** — chỉ infra UoW |
| `BaseStorageContext.TenantId` public | **Private** snapshot + `internal bool HasTenantId` |
| `dbContext.TenantId` trong Sample/job | Xóa / thay bằng `ICurrentTenant` |

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Một cổng đọc tenant cho app (`ICurrentTenant`); ISP rõ; giảm gán tenant lệch ngoài UoW |
| Xấu / chi phí | Breaking `IStorageContext`; sửa validation + mọi chỗ đọc `storage.TenantId` / `dbContext.TenantId` |
| Trung lập | Hành vi filter + UoW `SwitchDbContextAsync` / R2 **không** đổi nếu UoW vẫn `SetTenantId` đúng lúc |

---

## 7. Confirm

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept O2 + D1–D7? | **OK** (2026-08-12) |
| Q2 | `SetTenantId` giữ trên `IStorageContext` hay chuyển internal-only? | **Giữ trên `IStorageContext`** (2026-08-12) |
| Q3 | Validation: cast `BaseStorageContext` / `HasTenantId`? | **Phương án A** — `internal bool HasTenantId` (2026-08-12) |

### 7.1 Q3 — Phương án A (đã chọn)

```csharp
// BaseStorageContext
private Guid? TenantId { get; set; }
internal bool HasTenantId => TenantId.HasValue;
public virtual void SetTenantId(Guid? tenantId) => TenantId = tenantId;
```

```csharp
// TenantScopedContextValidation.RequiresTenant
if (context is BaseStorageContext baseCtx && baseCtx.HasTenantId)
    return false;
```

---

## 8. Test cases *(regression EF / Multitenancy)*

Không thêm suite contract riêng (`StorageContextTenantIdTests` đã bỏ). **Done khi** `MultitenancyEfTests` (+ CurrentTenant liên quan) xanh sau breaking `IStorageContext`.

| # | Case | Nơi | Trạng thái |
|---|------|-----|------------|
| T5 | Save `ITenantEntity` không `SetTenantId` → throw | `MultitenancyEfTests.TenantDb_SaveChanges_WithoutTenantId_Throws` | 🟢 |
| T10 | Query / persist qua UoW + R2 / master | `MultitenancyEfTests` HTTP/master | 🟢 |
| T11 | `SwitchDbContextAsync` isolate tenant A/B | `MultitenancyEfTests.TenantDb_IsolatedPerTenant_*` | 🟢 |
| T13 | Job / HTTP insert-update student tenant DB | `JobMasterTenantWithTenantId_*`, `HttpMasterTenant_*` | 🟢 |
| T15 | Suite Multitenancy EF xanh | `UnitTest/Multitenancy/MultitenancyEfTests.cs` | 🟢 |

Compile Sample + ORM.EF sau bỏ getter `TenantId` = đủ cho surface API (không cần reflection T1–T4).

---

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 | Status → Accepted | — 🟢 |
| 1 | `IStorageContext`: bỏ getter `TenantId` | Build Domain | 0 🟢 |
| 2 | `BaseStorageContext`: private `TenantId` + `internal HasTenantId` | Build ORM.EF | 1 🟢 |
| 3 | `TenantScopedContextValidation` dùng `HasTenantId` | T5 xanh | 2 🟢 |
| 4 | Sample cut-over + `MultitenancyEfTests` | T5/T10/T11/T13/T15 xanh | 3 🟢 |
| 5 | ADR README + status 🟢 | Index cập nhật | 4 🟢 |

---

## 10. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | Confirm §7 (Q1–Q3) | 🟢 |
| 2 | Contract + `BaseStorageContext` private + `HasTenantId` | 🟢 |
| 3 | Validation + UoW | 🟢 |
| 4 | `MultitenancyEfTests` regression | 🟢 |
| 5 | ADR + [README index](./README.md) → 🟢 | 🟢 |

---

## 11. Tham chiếu thêm

- [D5 home vs current](./2026-08-01-adr-current-user-tenant.md)
- [G4 `GetIdAsync` / `GetAsync`](./2026-08-06-adr-generic-current-tenant.md)
- Code neo: `BaseStorageContext`, `IStorageContext`, `BaseUnitOfWork.ApplyTenantId`, `TenantScopedContextValidation`
- Tests neo: `UnitTest/Multitenancy/MultitenancyEfTests.cs`
