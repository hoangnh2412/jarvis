# EF — Separate tenant DB

→ Setup: [setup.md](setup.md) | Hybrid: [hybrid.md](hybrid.md) | Index: [SKILL.md](SKILL.md)

Mỗi tenant một DB. **Master DB** lưu `Tenant.Id` + `ConnectionString`.

## Đăng ký DI

Hai DbContext, hai UoW:

```csharp
public static IHostApplicationBuilder AddAppDbContext(this IHostApplicationBuilder builder)
{
    builder.Services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
    builder.Services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();

    builder.Services.AddCoreDbContext<MasterDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbContext")!));

    builder.Services.AddCoreDbContext<AppDbContext,
        DbTenantConnectionStringResolver<MasterDbContext, Tenant>>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("AppDbContext")!));

    return builder;
}
```

- `MasterDbContext`: connection cố định.
- `AppDbContext`: placeholder + interceptor + lookup Master.

## Hành vi

1. `X-Tenant-Id` → interceptor → `DbTenantConnectionStringResolver` load `ConnectionString`.
2. `name` = `tenantId.ToString()` (Guid).
3. Thiếu tenant / connection rỗng → exception khi mở connection.
4. `ITenantEntity` không bắt buộc (cả DB thuộc một tenant).

## Provision tenant

```csharp
await masterRepo.InsertAsync(new Tenant
{
    Id = tenantGuid,
    ConnectionString = "Host=...;Database=tenant_acme;..."
}, ct);
await masterUow.SaveChangesAsync(ct);
// Migrate DB mới
```

## Background job

```csharp
await using var scope = scopeFactory.CreateAsyncScope();
var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
await uow.SwitchDbContextAsync(tenantId, ct);
var repo = await uow.GetRepositoryAsync<IRepository<Order>>(ct);
```

Sample: `Sample/HostApplicationBuilderExtension.cs`, `MultitenancyEfTestController`, `MultitenancyEfJobRunner`.

**Batch:** mỗi tenant một scope + UoW; đọc Master trước vòng lặp; không trộn Master UoW với tenant UoW.

## Checklist

- [ ] Master + `DbTenantConnectionStringResolver<MasterDbContext, Tenant>`
- [ ] `ITenantManagementEntity` trên Master
- [ ] Migrate Master + từng DB tenant
- [ ] Job: `SwitchDbContextAsync` hoặc scope mới
