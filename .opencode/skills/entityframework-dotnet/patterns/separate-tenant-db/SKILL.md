---
name: entityframework-dotnet-separate-tenant-db
description: Đăng ký Jarvis EF separate tenant database — Master DbContext + DbTenantConnectionStringResolver per tenant. Dùng khi mỗi tenant một database riêng.
dependencies:
  - Jarvis.EntityFramework
  - Npgsql.EntityFrameworkCore.PostgreSQL
---

# Separate tenant DB

→ Setup: [reference/setup.md](../../reference/setup.md) | Hybrid: [hybrid/SKILL.md](../hybrid/SKILL.md)

Mỗi tenant một DB. **Master DB** lưu `Tenant.Id` + `ConnectionString`.

## Đăng ký DI

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

## Background job

```csharp
await using var scope = scopeFactory.CreateAsyncScope();
var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
await uow.SwitchDbContextAsync(tenantId, ct);
var repo = await uow.GetRepositoryAsync<IRepository<Order>>(ct);
```

**Batch:** mỗi tenant một scope + UoW; không trộn Master UoW với tenant UoW.

## Checklist

- [ ] Master + `DbTenantConnectionStringResolver<MasterDbContext, Tenant>`
- [ ] `ITenantManagementEntity` trên Master
- [ ] Migrate Master + từng DB tenant
- [ ] Job: `SwitchDbContextAsync` hoặc scope mới
