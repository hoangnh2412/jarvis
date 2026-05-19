# Entity Framework: multi-tenant và DbContext factory

## Hai mô hình cơ sở dữ liệu

### Shared database (nhiều tenant trên một DB)

- Một (hoặc vài) connection string chung; phân tách dữ liệu bằng cột `TenantId` (`Guid`) trên các entity implement `ITenantEntity`.
- `BaseStorageContext` áp global query filter `TenantId == context.TenantId` cho các entity đó.
- Sau mỗi lần `IDbContextFactory` tạo context, `BaseUnitOfWork` gọi `SetTenantId(Guid?)` từ `ITenantIdResolverFactory` — resolver parse GUID từ header/claim/query/host; giá trị không parse được được bỏ qua.
- Mặc định dùng **`ITenantIdResolverFactory`**: header (`X-Tenant-Id`) → user claim (`GroupSid`) → query (`tenantId`) → host. Đăng ký: `AddMultitenancy` (trong `AddEntityFramework`) hoặc `AddCoreDbContext<TDb,TConn>` (overload 2 generic).

### Dedicated database (một tenant — một DB)

- Connection string khác nhau theo tenant (lookup qua `ITenantConnectionStringResolver`, Master DB, hoặc config).
- Mỗi tenant nên dùng **DbContext instance mới** với connection string tương ứng — không “nhảy” DB trên cùng một context đã query.

## Đăng ký DI

- `AddMultitenancy` chỉ đăng ký keyed **`ITenantIdResolver`** (Header, Query, User, Host). **Không** đăng ký `ITenantConnectionStringResolver` — app tự chọn nguồn connection string (config, header, custom).
- Tuỳ chọn Jarvis:
  - `AddKeyedConfigConnectionStringResolver` — `ConfigConnectionStringResolver` từ `IConfiguration`.
  - `AddKeyedCachingConfigConnectionStringResolver` — bọc config resolver bằng `CachingTenantConnectionStringResolver` (gọi `AddMemoryCache`; **không** dùng chung với `AddKeyedConfigConnectionStringResolver` cùng một key).
- `AddMultitenancyWithMemoryCache` — `AddMultitenancy` + `AddMemoryCache` khi resolver của bạn cần cache.
- `AddCoreDbContext<TDb,TConn>` dùng **`AddDbContextFactory`** + **`TenantDbConnectionInterceptor`**: `configure` chỉ cần provider với connection placeholder; khi mở connection, interceptor resolve tenant qua `ITenantIdResolverFactory` rồi lấy connection string từ keyed `ITenantConnectionStringResolver`.

## Filter động `field:op:value`

- `IQueryRepository<TEntity>.PaginationAsync` dùng `PagedListRequest` (page index/size và tùy chọn `Columns` để projection server-side). Filter/sort động có thể bổ sung sau qua `GetQuery()` + `Expression` hoặc lớp application riêng.
- Không thay đổi grammar trong thư viện Jarvis — parser nằm ở implementation của bạn.

## Unit of work

- `BaseUnitOfWork` dùng `IDbContextFactory.CreateDbContextAsync()`, rồi `SetTenantId` khi có tenant. Thứ tự resolve **trên từng UoW**: `_switchedTenantId` (sau `SwitchDbContextAsync`) → `ITenantIdResolverFactory` (header/claim/query/host). **Không** đọc `ICurrentTenantAccessor` trong UoW (tránh tenant UoW switch làm Master UoW nhận nhầm tenant trong cùng request).
- `ICurrentTenantAccessor` chỉ được set trong `SwitchDbContextAsync` và đọc khi **mở connection** (interceptor / `TenantConnectionStringResolverFactory`), không dùng khi UoW gọi `SetTenantId`.
- Implement `IDisposable` / `IAsyncDisposable`: dispose context và restore accessor scope khi UoW kết thúc.

### `SwitchDbContextAsync`

- Pin tenant trên UoW (`_switchedTenantId`) và `ICurrentTenantAccessor` (cho interceptor); dispose DbContext đã cache nếu tenant đổi.
- **Sau `SwitchDbContextAsync` phải gọi lại `GetRepositoryAsync`** — repository lấy trước đó vẫn giữ `IStorageContext` cũ, có thể đọc/ghi nhầm DB tenant.
- Thứ tự đúng: `SwitchDbContextAsync(tenantId)` → `GetRepositoryAsync` / `GetDbContextAsync` → query hoặc `SaveChangesAsync`.

## Master DB + batch cập nhật nhiều tenant (dedicated DB)

Tách **Master** và **tenant** DbContext (hai registration / hai UoW). Không dùng chung một UoW cho Master và tenant.

```csharp
// 1) Đọc danh sách connection string từ Master (context Master riêng)
var tenants = await masterDb.TenantConnections.ToListAsync(ct);

foreach (var tenant in tenants)
{
    // 2) Mỗi tenant: scope DI mới → UoW + context sạch
    await using var scope = scopeFactory.CreateAsyncScope();
    var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();

    // 3) UoW mới → factory tạo context với connection của tenant (configure trong AddCoreDbContext / resolver theo scope)
    var repo = await uow.GetRepositoryAsync<IOrderRepository>(ct);
    await repo.InsertManyAsync(orders, ct);
    await uow.CommitAsync(ct);
}
```

Đăng ký ví dụ:

```csharp
builder.Services.AddKeyedConfigConnectionStringResolver();
builder.Services.AddCoreDbContext<AppDbContext, ConfigConnectionStringResolver>(options =>
    options.UseNpgsql("Host=localhost;Database=placeholder"));

```

Job/background không có HTTP tenant: mỗi job dùng `CreateAsyncScope`, gọi `SwitchDbContextAsync(tenantId)`, rồi **`GetRepositoryAsync` lại** (không tái sử dụng repository từ trước switch).
