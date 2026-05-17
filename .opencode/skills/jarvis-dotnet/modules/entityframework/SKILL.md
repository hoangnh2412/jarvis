---
name: jarvis-dotnet-entityframework
description: Cài Jarvis.EntityFramework — repository, multitenancy, AddCoreDbContext factory. Dùng khi project dùng EF Core với tenant và UoW Jarvis.
dependencies:
  - Jarvis.EntityFramework
  - Npgsql.EntityFrameworkCore.PostgreSQL
---

# Entity Framework

## Packages

| Project | PackageId | Ghi chú |
|---|---|---|
| Jarvis.EntityFramework | `Jarvis.EntityFramework` | Core |
| Jarvis.EntityFramework.PostgreSql | — | ProjectReference; marker provider |
| Jarvis.EntityFramework.MySql | — | ProjectReference; placeholder |

**NuGet host (chọn một):** `Npgsql.EntityFrameworkCore.PostgreSQL`, `Pomelo.EntityFrameworkCore.MySql`, …

## Program.cs

```csharp
using Jarvis.EntityFramework;
using Jarvis.Domain.DataStorages;

builder.AddEntityFramework();

builder.Services.AddKeyedConfigConnectionStringResolver();

builder.Services.AddCoreDbContext<MyDbContext, HeaderTenantIdResolver, ConfigConnectionStringResolver>(
    (options, tenantIdResolver, connectionResolver) =>
    {
        var tenantId = tenantIdResolver.GetTenantId() ?? nameof(MyDbContext);
        var connectionString = connectionResolver.GetConnectionString(tenantId);
        options.UseNpgsql(connectionString); // hoặc UseMySql
    });
```

## Host extension mẫu

```csharp
public static IHostApplicationBuilder AddMyDbContext(this IHostApplicationBuilder builder)
{
    builder.Services.AddScoped<IMyUnitOfWork, MyUnitOfWork>();
    builder.Services.AddKeyedConfigConnectionStringResolver();
    builder.Services.AddCoreDbContext<MyDbContext, HeaderTenantIdResolver, ConfigConnectionStringResolver>(
        (options, tenantResolver, connResolver) =>
        {
            var tenantId = tenantResolver.GetTenantId() ?? nameof(MyDbContext);
            options.UseNpgsql(connResolver.GetConnectionString(tenantId));
        });
    return builder;
}
```

## appsettings.json

```json
{
  "ConnectionStrings": {
    "AutoMigrate": "true",
    "MyDbContext": "Host=localhost;..."
  }
}
```

## Extension

| Extension | Mục đích |
|---|---|
| `AddEntityFramework` | Repository + multitenancy resolvers |
| `AddCoreDbContext<T,TTenant,TConn>` | DbContext factory + UoW initializer |
| `AddKeyedConfigConnectionStringResolver` | Connection string từ config |

Migrate: `app.EnsureMigrateDb<IUnitOfWork>()` (extension host).
