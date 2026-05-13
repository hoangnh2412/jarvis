# Entity Framework: multi-tenant và DbContext factory

## Hai mô hình cơ sở dữ liệu

### Shared database (nhiều tenant trên một DB)

- Một (hoặc vài) connection string chung; phân tách dữ liệu bằng cột `TenantId` (`Guid`) trên các entity implement `ITenantEntity`.
- `BaseStorageContext` áp global query filter `TenantId == context.TenantId` cho các entity đó.
- Sau mỗi lần `IDbContextFactory` tạo context, `IStorageContextTenantInitializer` (scoped) gọi `SetTenantId` từ `ITenantIdResolver` — chuỗi tenant phải là GUID hợp lệ (không parse được sẽ gây `ArgumentException`).

### Dedicated database (một tenant — một DB)

- Connection string khác nhau theo tenant (lookup qua `ITenantConnectionStringResolver`, ví dụ key = tenant id).
- `AddDbContextFactory` cấu hình options theo connection string tại thời điểm tạo factory options (callback có thể dùng scope để resolve tenant/CS).

## Đăng ký DI

- `AddMultitenancy` chỉ đăng ký keyed **`ITenantIdResolver`** (Header, Query, User, Host). **Không** đăng ký `ITenantConnectionStringResolver` — app tự chọn nguồn connection string (config, header, custom).
- Tuỳ chọn Jarvis:
  - `AddKeyedConfigConnectionStringResolver` — `ConfigConnectionStringResolver` từ `IConfiguration`.
  - `AddKeyedCachingConfigConnectionStringResolver` — bọc config resolver bằng `CachingTenantConnectionStringResolver` (gọi `AddMemoryCache`; **không** dùng chung với `AddKeyedConfigConnectionStringResolver` cùng một key).
- `AddMultitenancyWithMemoryCache` — `AddMultitenancy` + `AddMemoryCache` khi resolver của bạn cần cache.
- `AddCoreDbContext<...>` dùng **`AddDbContextFactory`**; đăng ký **`IStorageContextTenantInitializer`** scoped để `BaseUnitOfWork` gọi `SetTenantId` sau khi tạo context. Generic `TConnectionStringResolver` phải khớp keyed service bạn đã đăng ký (ví dụ `ConfigConnectionStringResolver`).

## Filter động `field:op:value`

- `IQueryRepository<TEntity>.PageAsync` dùng `PagedListRequest` (page index/size và tùy chọn `Columns` để projection server-side). Filter/sort động có thể bổ sung sau qua `GetQuery()` + `Expression` hoặc lớp application riêng.
- Không thay đổi grammar trong thư viện Jarvis — parser nằm ở implementation của bạn.

## Unit of work

- `BaseUnitOfWork` dùng `IDbContextFactory.CreateDbContext()` / `CreateDbContextAsync()` (không `Task.Run`), rồi gọi `IStorageContextTenantInitializer` khi có.
- Implement `IDisposable` / `IAsyncDisposable`: dispose context khi scope UoW kết thúc.
