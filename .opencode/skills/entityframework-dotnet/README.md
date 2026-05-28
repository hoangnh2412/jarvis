# entityframework-dotnet

Skill tích hợp **Jarvis.EntityFramework** — repository, multitenancy, UoW. Agent đọc [SKILL.md](./SKILL.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Chưa có EF Jarvis | [workflows/init.md](./workflows/init.md) |
| Đổi mô hình DB / custom resolver | [workflows/add.md](./workflows/add.md) + [patterns/](./patterns/) |

Scaffold `jarvis-dotnet` đã wire Caching + EF trong Infrastructure — dùng skill khi đổi pattern hoặc project chưa Jarvis.

## Cách gọi

```text
@.opencode/skills/entityframework-dotnet/patterns/single-db/SKILL.md

Cấu hình EF single DB multitenancy cho MyApp.
```

---

## Multitenancy & DbContext (tham chiếu)

## Hai mô hình cơ sở dữ liệu

### Shared database (nhiều tenant trên một DB)

- Một (hoặc vài) connection string chung; phân tách bằng cột `TenantId` (`Guid`) trên entity `ITenantEntity`.
- `BaseStorageContext` áp global query filter `TenantId == context.TenantId`.
- `BaseUnitOfWork` gọi `SetTenantId(Guid?)` từ `ITenantIdResolverFactory` sau mỗi lần factory tạo context.
- Resolver mặc định: header (`X-Tenant-Id`) → claim (`GroupSid`) → query (`tenantId`) → host.

### Dedicated database (một tenant — một DB)

- Connection string khác theo tenant (`ITenantConnectionStringResolver`, Master DB, config).
- Mỗi tenant: **DbContext instance mới** — không đổi connection trên context đã query.

## Đăng ký DI

```csharp
builder.AddJarvisCaching();   // bắt buộc trước EF
builder.AddEntityFramework();
builder.Services.AddCoreDbContext<AppDbContext, ConfigConnectionStringResolver>(options =>
    options.UseNpgsql("Host=localhost;Database=placeholder"));
```

- `AddMultitenancy` chỉ đăng ký keyed `ITenantIdResolver` — **không** đăng ký `ITenantConnectionStringResolver` (app tự chọn).
- Mọi resolver đăng ký qua `AddCoreDbContext` / `AddEntityFramework` được bọc **`CachingTenantConnectionStringResolver`** (`Cache:Items:ConnectionString`).
- `AddCoreDbContext<TDb,TConn>`: `AddDbContextFactory` + `TenantDbConnectionInterceptor`.

## Unit of work

- Resolve tenant trên UoW: `_switchedTenantId` → `ITenantIdResolverFactory` — **không** đọc `ICurrentTenantAccessor` trong UoW khi `SetTenantId`.
- `ICurrentTenantAccessor` chỉ cho **mở connection** (interceptor).

### `SwitchDbContextAsync`

1. `SwitchDbContextAsync(tenantId)`
2. **`GetRepositoryAsync` lại** (repository cũ giữ context sai)
3. Query / `SaveChangesAsync`

Job/background không có HTTP tenant: `CreateAsyncScope` → `SwitchDbContextAsync` → `GetRepositoryAsync` lại.

## Master DB + batch nhiều tenant

Tách Master và tenant DbContext — không một UoW cho cả hai.

```csharp
var tenants = await masterDb.TenantConnections.ToListAsync(ct);

foreach (var tenant in tenants)
{
    await using var scope = scopeFactory.CreateAsyncScope();
    var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
    var repo = await uow.GetRepositoryAsync<IOrderRepository>(ct);
    await repo.InsertManyAsync(orders, ct);
    await uow.CommitAsync(ct);
}
```

## Chọn mô hình

| Mô hình | Doc |
|---------|-----|
| Single DB + `TenantId` | [patterns/single-db/SKILL.md](./patterns/single-db/SKILL.md) |
| Mỗi tenant một DB | [patterns/separate-tenant-db/SKILL.md](./patterns/separate-tenant-db/SKILL.md) |
| Master + pool + DB riêng | [patterns/hybrid/SKILL.md](./patterns/hybrid/SKILL.md) |
| Resolver tùy biến | [patterns/custom-di/SKILL.md](./patterns/custom-di/SKILL.md) |
| Setup chung | [reference/setup.md](./reference/setup.md) |

## Sample repo

| File | Nội dung |
|------|----------|
| `Sample/HostApplicationBuilderExtension.cs` | Master + tenant DbContext |
| `Sample/Controllers/MultitenancyEfTestController.cs` | HTTP + background job |

## Liên quan

- [caching-dotnet/README.md](../caching-dotnet/README.md) — cache connection string
- [telemetry-dotnet/README.md](../telemetry-dotnet/README.md) — `BaseWorker` + EF job
- [jarvis-dotnet/README.md](../jarvis-dotnet/README.md) — scaffold
