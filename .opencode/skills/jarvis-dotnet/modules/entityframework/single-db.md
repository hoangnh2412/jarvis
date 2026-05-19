# EF — Single DB (shared database)

→ Setup: [setup.md](setup.md) | Index: [SKILL.md](SKILL.md)

Một database; phân tách bằng `TenantId` + global query filter (`ITenantEntity`).

## Đăng ký DI

Overload **một** generic, connection cố định, **không** interceptor:

```csharp
public static IHostApplicationBuilder AddAppDbContext(this IHostApplicationBuilder builder)
{
    builder.Services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();

    builder.Services.AddCoreDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("AppDbContext")!));

    return builder;
}
```

## Hành vi

- Cùng connection mọi request.
- `X-Tenant-Id` (hoặc claim/query/host) → `SetTenantId` → filter `ITenantEntity`.
- Không tenant → `TenantId` null trên context → thường không thấy dữ liệu tenant.

## Khi nào dùng

SaaS nhỏ/vừa, ops đơn giản, không isolate DB per tenant.

## Checklist

- [ ] `AddCoreDbContext<T>` 1 generic
- [ ] Entity business: `ITenantEntity`
- [ ] `ConnectionStrings:AppDbContext`
- [ ] Không Master / `DbTenantConnectionStringResolver` (trừ khi nâng cấp hybrid sau)
