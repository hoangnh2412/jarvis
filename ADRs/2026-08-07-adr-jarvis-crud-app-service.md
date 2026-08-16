# ADR — Base CRUD Application Service (`Jarvis.DDD.Application`)

> **Trạng thái:** 🔴 **Proposed** (chờ confirm §7).  
> **Ngày:** 2026-08-07  
> **Loại:** API / Application layer foundation  
> **Liên quan:** [architecture-software.md](./architecture-software.md) (§0.1 Clean Architecture), [Setting ADR](../modules/settings/Jarvis.Setting/doc/2026-07-30-adr-setting.md) (facade domain-specific), [Tenants module](./2026-08-07-adr-jarvis-tenants-module.md) (🔴 — dùng pattern Manager riêng, **không** thay bằng base CRUD), [ORM packages](./2026-08-07-adr-jarvis-orm-packages.md) (🟢 Done — Persistence foundation), [FluentValidation](./2026-08-07-adr-jarvis-fluentvalidation.md) (🔴 — validate Create/Update input).  
> **Phạm vi:** quyết định **base service CRUD** trên tầng Application: contract + class nền trong `Jarvis.DDD.Application` / `.Contracts`; map Entity ↔ DTO; Get / GetList (pagination) / Create / Update / Delete; quan hệ với CQRS hiện có.  
> **Ngoài phạm vi:** Authorization / RBAC policy; AutoMapper bắt buộc; HTTP controller base / OpenAPI; soft-delete policy toàn jarvis; module Tenants / Setting / Identity CRUD cụ thể; đổi semantic `IRepository` / `PagedListRequest`; chi tiết FluentValidation (xem ADR riêng).  
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm

---

## 1. Bối cảnh

Jarvis đã có đủ **Persistence ports** và **CQRS wiring**, nhưng **chưa** có lớp Application chuẩn cho CRUD entity đơn giản:

| Thành phần | Hiện tại |
|------------|----------|
| `Jarvis.DDD.Domain` | `IRepository` / `IQueryRepository` / `ICommandRepository`, `PagedListRequest`, `PagedQueryOptions` |
| `Jarvis.EntityFramework` | Repo EF + `PagedListExecutor` |
| `Jarvis.DDD.Application` | Chỉ `CommandDispatcher` / `QueryDispatcher` (+ async) — **không** CRUD base |
| `Jarvis.DDD.Application.Contracts` | Handler/dispatcher contracts + `IPagingDto` / `IPagedDto<T>` |
| Module Setting | Facade domain-specific `ISettingManager` (không generic entity CRUD) |
| Sample / Host | Một số controller gọi thẳng `DbContext` / executor (vd. `CompanyController`) |

Clean Architecture ([architecture-software](./architecture-software.md) §0.1): use case nằm **Application**; Host/API không query `DbSet` trực tiếp. Host đang lặp boilerplate Get/Create/Update/Delete + map DTO cho mỗi aggregate đơn giản, trong khi CQRS phù hợp use case phức tạp hơn là CRUD phẳng.

Cần chốt **một** base service CRUD dùng lại được, không thay CQRS và không biến Jarvis thành “business app”.

---

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Không có `CrudAppService` / tương đương | Mỗi host tự viết vòng CRUD; lệch convention giữa sản phẩm |
| P2 | Controller / Host dễ gọi `DbContext` trực tiếp | Phá ranh giới Application; khó test / khó gắn UoW thống nhất |
| P3 | CQRS hiện tại không giảm boilerplate CRUD phẳng | 4–8 handler files / entity cho Create/Update/Delete/Get/List |
| P4 | `ISettingManager` / (đề xuất) `ITenantManager` là facade **nghiệp vụ** | Không thể dùng làm mẫu generic CRUD cho mọi `IEntity<TKey>` |
| P5 | Mapping Entity ↔ DTO chưa có chỗ chuẩn ở Application | Copy-paste mapping hoặc kéo AutoMapper vào Domain |

---

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | Base CRUD sống ở **Application** (`Jarvis.DDD.Application` + contracts) — **không** Domain, **không** EF package |
| C2 | Domain **không** reference Application; Application chỉ ports Domain (`IRepository`, `IUnitOfWork`, `IEntity<TKey>`, …) |
| C3 | **Không** chứa Authorization / policy / HTTP — Host gắn filter / `[Authorize]` trước khi gọi service |
| C4 | **Không** thay CQRS: dispatcher giữ nguyên; CRUD base **bổ sung** cho entity CRUD đơn giản |
| C5 | Module nghiệp vụ có facade riêng (Setting, Tenants, …) **không bắt buộc** inherit base CRUD; được dùng khi phù hợp |
| C6 | Semantic repo / pagination hiện tại **không** đổi chỉ vì ADR này — CRUD gọi `PaginationAsync` / `InsertAsync` / … |
| C7 | Business rule đặc thù (unique domain, ACL row-level, side-effect) → override ở **host** service, không nhồi vào Jarvis core |
| C8 | Entity concrete + DTO + migration = **host-owned** |

---

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | **Base `CrudAppService` generic** trong `Jarvis.DDD.Application` | Một class nền + interface; host inherit / đăng ký | Ít file; khớp ABP-style quen thuộc; tái dùng repo + paging sẵn có | Generic nhiều type param; cần hook override rõ |
| O2 | Chỉ generate CQRS handlers (Create/Update/…) | Mỗi thao tác = command/query | Đồng nhất MediatR/CQRS | Boilerplate cao cho CRUD phẳng; chậm onboard |
| O3 | Convention-only (doc + Sample), không base class | Host copy pattern Setting | Zero API surface Jarvis | Không chuẩn hóa; lặp mã |
| O4 | Package riêng `Jarvis.DDD.Application.Crud` | Tách CRUD khỏi CQRS package | Opt-in package | Thêm PackageId sớm khi Application còn mỏng |

**Khuyến nghị:** **O1** — base trong package Application hiện có; tách package riêng chỉ khi Application phình (Later).

---

## 5. Quyết định

Chúng ta sẽ cung cấp **base CRUD Application Service** generic trên `Jarvis.DDD.Application`, contract trên `.Contracts`, để Host inherit cho entity CRUD đơn giản; CQRS giữ cho use case phức tạp.

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Vị trí | Contract: `Jarvis.DDD.Application.Contracts`; impl base: `Jarvis.DDD.Application` |
| D2 | Tên API | **`ICrudAppService`** + **`CrudAppService`** (chốt §7 nếu đổi `*ApplicationService`) |
| D3 | Shape generic (MVP) | `CrudAppService<TEntity, TKey, TGetOutput, TGetListOutput, TCreateInput, TUpdateInput>` — rút gọn overload khi Get = GetList / Create = Update (chốt §7) |
| D4 | Thao tác MVP | `GetAsync(TKey)` · `GetListAsync(PagedListRequest)` · `CreateAsync(TCreateInput)` · `UpdateAsync(TKey, TUpdateInput)` · `DeleteAsync(TKey)` |
| D5 | Persistence | Qua `IUnitOfWork` → `GetRepositoryAsync<IRepository<TEntity>>` (hoặc `IRepository<TEntity>` inject + bind context) + `SaveChangesAsync` sau Create/Update/Delete |
| D6 | List / filter | Ủy quyền `IQueryRepository.PaginationAsync` + `PagedQueryOptions<TEntity>`; host override `CreatePagedQueryOptions` / `CreateFilteredQuery` cho whitelist / RLS |
| D7 | Mapping | **Virtual methods** `MapToGetOutput` / `MapToEntity` / `MapToEntity(update)` — **không** bắt buộc AutoMapper trong Jarvis; Host có thể gắn mapper trong override |
| D8 | Not found | Throw exception Domain.Shared hiện có (vd. kiểu “not found” / bad request) — **không** trả `null` im lặng ở `Get`/`Update`/`Delete` (chốt exact type §7) |
| D9 | Soft delete | Nếu entity `ILogDeletedEntity` (hoặc convention host): hook `DeleteAsync` virtual — mặc định hard delete qua repo; soft-delete = override Later / opt-in |
| D10 | DI | Host đăng ký `AddScoped<IXxxAppService, XxxAppService>()`; Jarvis **không** auto-scan mọi entity |
| D11 | Quan hệ CQRS | Dùng CRUD base **hoặc** command/query handler — không bắt buộc bọc CRUD trong dispatcher; Sample minh họa cả hai |
| D12 | Skill / docs | Cập nhật skill Application (hoặc mục CRUD trong `application-dotnet`) khi implement |

### 5.1 Sơ đồ phụ thuộc

```text
  Host Controller / Minimal API
           │  (authz ở đây)
           ▼
  IXxxAppService  ──inherit──►  ICrudAppService<…>
  XxxAppService   ──inherit──►  CrudAppService<TEntity, TKey, …>
           │
           │  Map* (virtual)     CreatePagedQueryOptions (virtual)
           ▼
  IUnitOfWork  →  IRepository<TEntity>
           │
           ▼
  Jarvis.EntityFramework / ORM.*   (infra — không reference ngược Application)
```

### 5.2 API sketch (đích)

```csharp
// Contracts
public interface ICrudAppService<TGetOutput, TGetListOutput, TKey, TCreateInput, TUpdateInput>
{
    Task<TGetOutput> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IPagedDto<TGetListOutput>> GetListAsync(PagedListRequest input, CancellationToken cancellationToken = default);
    Task<TGetOutput> CreateAsync(TCreateInput input, CancellationToken cancellationToken = default);
    Task<TGetOutput> UpdateAsync(TKey id, TUpdateInput input, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}

// Application
public abstract class CrudAppService<TEntity, TKey, TGetOutput, TGetListOutput, TCreateInput, TUpdateInput>
    : ICrudAppService<TGetOutput, TGetListOutput, TKey, TCreateInput, TUpdateInput>
    where TEntity : class, IEntity<TKey>
{
    // GetAsync / GetListAsync / CreateAsync / UpdateAsync / DeleteAsync
    // protected virtual Map*, CreatePagedQueryOptions, GetEntityByIdAsync, …
}
```

### 5.3 Khi nào **không** dùng base CRUD

| Tình huống | Dùng thay |
|------------|-----------|
| Setting theo Key/Group, encrypt, cache | `ISettingManager` |
| Catalog tenant + bridge store | `ITenantManager` ([ADR Tenants](./2026-08-07-adr-jarvis-tenants-module.md)) |
| Use case nhiều aggregate / saga / outbox | CQRS command/query handlers |
| Read model báo cáo nặng | Query handler + (sau này) ORM.Dapper |

---

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Chuẩn hóa CRUD Application; giảm boilerplate Host; giữ Clean Architecture (không `DbSet` ở controller) |
| Tốt | Tái dùng pagination / filter / sort đã có ở Domain + EF |
| Xấu / chi phí | Thêm surface API generic; cần Sample + test + skill |
| Xấu / chi phí | Nguy cơ “God service” nếu nhồi business vào base — mitigation: virtual hooks + C7 |
| Trung lập | CQRS và CRUD base sống song song; team chọn theo độ phức tạp use case |
| Trung lập | Tenants/Setting có thể không inherit — không phá module Atomic |

---

## 7. Confirm *(bắt buộc trước khi Accepted)*

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D12, C1–C8? | _chờ_ |
| Q2 | Chọn phương án? | **O1** (khuyến nghị) / O2 / O3 / O4 |
| Q3 | Tên: `ICrudAppService` + `CrudAppService` — OK? | _chờ_ |
| Q4 | Đủ 6 type param hay thêm overload rút gọn (Get=GetList, Create=Update)? | MVP đủ 6 + 1–2 alias / Later |
| Q5 | Not-found: exception type nào (existing Domain.Shared)? | _chờ_ |
| Q6 | Soft-delete mặc định? | Hard delete MVP; soft-delete override Later — OK? |
| Q7 | AutoMapper trong Jarvis? | **Không** (D7) — OK? |
| Q8 | `PagedListRequest` giữ ở Domain hay mirror DTO ở Application.Contracts? | Giữ Domain request ở service input MVP / wrap Later |

Sau khi chốt: cập nhật **Trạng thái** → 🟡 Accepted, ghi ngày Accept.

---

## 8. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 | Status → Accepted | — |
| 1 | Contracts `ICrudAppService` + `IPagedDto` response helper (nếu thiếu impl) | Build | 0 |
| 2 | `CrudAppService` base + unit test (in-memory / test factory) | Test xanh | 1 |
| 3 | Sample: một entity CRUD qua AppService (không `DbContext` ở controller) | Sample API chạy | 2 |
| 4 | Skill / README Application cập nhật | Docs đồng bộ | 3 |

---

## 9. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | Confirm §7 → Accepted | 🔴 |
| 2 | `ICrudAppService` + `CrudAppService` | 🔴 |
| 3 | Unit test CRUD MVP | 🔴 |
| 4 | Sample AppService + controller mỏng | 🔴 |
| 5 | Skill / docs Application | 🔴 |
| 6 | ADR + [README index](./README.md) cập nhật | 🟡 (Proposed đã index) |

---

## 10. Tham chiếu thêm

- [README Jarvis — Clean Architecture](../README.md) — Application = command/query/handler/DTO  
- ABP `CrudAppService` — tham chiếu shape (không copy license/code)  
- Pagination hiện tại: `PagedListRequest`, `PagedQueryOptions`, `IQueryRepository.PaginationAsync`  
- Không supersede: [Tenants](./2026-08-07-adr-jarvis-tenants-module.md), Setting ADR, CQRS dispatcher hiện có
