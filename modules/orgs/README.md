# Platform.Orgs

Module catalog **Org** (khách cuối trong tenant): CRUD chuẩn, hooks host, read model, SPI `IOrgCatalog`, tích hợp `Platform.MultiOrg`.

## Packages

| Package | Vai trò |
|---------|---------|
| `Platform.Orgs.Abstractions` | `IOrgCatalog`, `OrgDescriptor`, integration events |
| `Platform.Orgs` | `IOrgManager`, hooks, `OrgPermissions` |
| `Platform.Orgs.EntityFramework` | `IOrgStore`, `ApplyPlatformOrgCatalog<TOrg>()` |
| `Platform.Orgs.Api` | REST opt-in `.UseHttpApi()` |

Ambient org (`ICurrentOrg`, resolvers) nằm ở **`Platform.MultiOrg`**, không trong module catalog.

## Host compose (minimal)

```csharp
builder.AddCurrentOrg<OrgCatalogOrgProfile>();

builder.AddPlatformOrgs<Org, CurrentTenantInfo>()
    .UseEntityFramework<Org, CurrentTenantInfo, ITenantUnitOfWork>()
    .UseHooks<MyOrgHooks>()
    .UseReadModel<MyOrgDetail, MyOrgReadMapper>()  // IOrgCatalogReader<MyOrgDetail>
    .UseAsCurrentOrgStore()
    .UseHttpApi(); // optional
```

DbContext: `modelBuilder.ApplyPlatformOrgCatalog<Org>();` + migration host-owned.

## EF org scope

Entity nghiệp vụ implement `IOrgEntity`. `BaseStorageContext` áp filter khi UoW gọi `SetOrgId` (ambient `ICurrentOrgAccessor` hoặc `IOrgIdResolverFactory`).

## Permissions

- `Org.View` — đọc catalog (API gợi ý)
- `Org.Manage` — ghi catalog

Gán claim `Permission` trên role (vd. `ORG_ADMIN`) tại host seed.

## Events

Sau `SaveChanges`, `OrgManager` gọi `IOrgCatalogEventPublisher` (`OrgCreatedEvent` / `OrgUpdatedEvent`). Mặc định `NullOrgCatalogEventPublisher`; host thay bằng bus/outbox.

ADR: [2026-10-17_platform_orgs-module.md](../../ADRs/2026-10-17_platform_orgs-module.md), [2026-10-21_platform_orgs-catalog-events.md](../../ADRs/2026-10-21_platform_orgs-catalog-events.md).
