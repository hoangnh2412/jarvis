# EF — Setup chung

Đọc sau [SKILL.md](SKILL.md). Chọn mô hình: [single-db](single-db.md) | [separate-tenant-db](separate-tenant-db.md) | [hybrid](hybrid.md).

## Thành phần Jarvis

| Thành phần | Layer | Vai trò |
|---|---|---|
| `BaseStorageContext<T>` | EF | Global query filter `TenantId == context.TenantId` (`ITenantEntity`) |
| `BaseUnitOfWork<T>` | EF | `IDbContextFactory`, `SetTenantId`, `SwitchDbContextAsync` |
| `ITenantIdResolver` / `ITenantIdResolverFactory` | Domain | Tenant id (keyed: Header, User, Query, Host) |
| `ITenantConnectionStringResolver` | Domain | `GetConnectionStringAsync(name)` |
| `TenantConnectionStringResolverFactory` | Domain | Tenant id + keyed resolver |
| `ConfigConnectionStringResolver` | Domain | `IConfiguration.GetConnectionString` |
| `DbTenantConnectionStringResolver<TMaster, TTenant>` | EF | Lookup `ITenantManagementEntity` trên Master |
| `TenantDbConnectionInterceptor` | EF | Ghi connection lúc mở (overload 2 generic) |
| `ITenantManagementEntity` | Domain | Registry Master: `Id`, `ConnectionString` |

`AddEntityFramework()` → repository + keyed `ITenantIdResolver` + `ICurrentTenantAccessor`. Host gọi `AddCoreDbContext` theo mô hình.

## Luồng resolve tenant

1. **UoW / filter:** `_switchedTenantId` → `ITenantIdResolverFactory` (không đọc `ICurrentTenantAccessor`).
2. **Connection:** `ICurrentTenantAccessor` → `ITenantIdResolverFactory` → `TenantConnectionStringResolverFactory` → keyed resolver.
3. Overload 2 generic: interceptor gán connection khi mở.
4. `SetTenantId` cho `ITenantEntity`.

**Không có tenant:** `ConnectionStrings:{DbContextName}` (migrate, Master-only job).

## DbContext & UoW

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : BaseStorageContext<AppDbContext>(options)
{
    public DbSet<Order> Orders => Set<Order>();
}

public class AppUnitOfWork(
    IServiceProvider services,
    IDbContextFactory<AppDbContext> factory,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ICurrentTenantAccessor currentTenantAccessor)
    : BaseUnitOfWork<AppDbContext>(services, factory, tenantIdResolverFactory, currentTenantAccessor),
      IAppUnitOfWork;
```

## Entity

**`ITenantEntity`** (single DB, hybrid pool):

```csharp
public class Order : BaseEntity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
}
```

**`ITenantManagementEntity`** (Master, mô hình 2 & 3):

```csharp
public class Tenant : BaseEntity<Guid>, ITenantManagementEntity
{
    public required string ConnectionString { get; set; }
}
```

## appsettings.json

```json
{
  "ConnectionStrings": {
    "AutoMigrate": "true",
    "AppDbContext": "Host=localhost;...",
    "MasterDbContext": "Host=localhost;...;Database=master"
  },
  "TenantHeaderKey": "X-Tenant-Id",
  "TenantQueryName": "tenantId"
}
```

Placeholder `AppDbContext` khi dùng interceptor (connection thật từ Master / resolver).

## Migrate

```csharp
app.EnsureMigrateDb<IMasterUnitOfWork>();
app.EnsureMigrateDb<IAppUnitOfWork>();
```

`ConnectionStrings:AutoMigrate` = `true`. Dedicated/hybrid: migrate từng DB tenant ngoài placeholder.

## API tham chiếu

| API | Mục đích |
|---|---|
| `AddEntityFramework()` | Repository + multitenancy |
| `AddCoreDbContext<TDb>(configure)` | Connection cố định |
| `AddCoreDbContext<TDb, TResolver>(configure)` | Per-tenant connection |
| `EnsureMigrateDb<TUnitOfWork>(app)` | Auto migrate |
| `SwitchDbContextAsync(tenantId)` | Job: pin tenant + connection scope |

**Không** truyền `HeaderTenantIdResolver` vào `AddCoreDbContext` — đó là `ITenantIdResolver`.
