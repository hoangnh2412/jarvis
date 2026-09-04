---
name: entityframework-dotnet-custom-di
description: Tùy biến Jarvis EF tenant resolution qua ITenantIdResolverFactory và ITenantConnectionStringResolver — không sửa thư viện Jarvis.
dependencies:
  - Jarvis.EntityFramework
---

# Custom DI (không sửa Jarvis)

→ Setup: [reference/setup.md](../../reference/setup.md)

App `AddScoped` **sau** `AddEntityFramework()` / `AddCoreDbContext()`.

| Muốn thay | Ghi đè |
|---|---|
| **TenantId** (vd. MST → Guid) | `ITenantIdResolverFactory` |
| **Connection** (vd. MinIO) | Keyed `ITenantConnectionStringResolver` |

## TenantId — keyed resolver + factory chain

```csharp
builder.Services.AddKeyedScoped<ITenantIdResolver, TaxCodeTenantIdResolver>(nameof(TaxCodeTenantIdResolver));
builder.Services.AddScoped<ITenantIdResolverFactory, AppTenantIdResolverFactory>();
```

Tra Master trong resolver: dùng `IDbContextFactory<MasterDbContext>`, không `IMasterUnitOfWork`.

## ConnectionString — custom resolver

```csharp
builder.Services.AddCoreDbContext<AppDbContext, MinioTenantConnectionStringResolver>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AppDbContext")!));
```

Cần overload 2 generic + interceptor khi connection động theo tenant.

## Checklist

- [ ] `AddScoped` sau `AddEntityFramework`
- [ ] Job: `SwitchDbContextAsync` khi đã biết `TenantId`
