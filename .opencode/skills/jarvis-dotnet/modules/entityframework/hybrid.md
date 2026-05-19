# EF — Hybrid multitenancy

→ Setup: [setup.md](setup.md) | Separate DB: [separate-tenant-db.md](separate-tenant-db.md) | Index: [SKILL.md](SKILL.md)

Master quản lý connection string: một số tenant **chung DB** (cùng `ConnectionString`), một số **DB riêng**.

## Đăng ký DI

**Giống separate tenant DB** — Master + `DbTenantConnectionStringResolver`. Không có API “hybrid” riêng.

## Dữ liệu Master (ví dụ)

| TenantId | ConnectionString |
|---|---|
| `1111-…` | `…Database=pool_shared` |
| `2222-…` | `…Database=pool_shared` |
| `3333-…` | `…Database=enterprise_acme` |

## Quy tắc entity

| Tenant trên | Entity app |
|---|---|
| DB chung (pool) | **Bắt buộc** `ITenantEntity` + `TenantId` khi ghi |
| DB riêng | `ITenantEntity` tùy chọn |

```csharp
await repo.InsertAsync(new Order { Id = id, TenantId = tenantId, ... }, ct);
```

`SetTenantId` vẫn chạy qua UoW dù connection string trùng.

## Migrate

- Master: một lần.
- Pool shared: migrate **một** DB pool.
- Dedicated: migrate **từng** connection riêng.

Resolver: kế thừa `DbTenantConnectionStringResolver` (`virtual`) trong app nếu cần fallback — [custom-di.md](custom-di.md).

## Checklist

- [ ] Giống separate + nhiều tenant cùng `ConnectionString` trên Master
- [ ] Pool: `ITenantEntity` bắt buộc
- [ ] Migrate pool một lần, dedicated từng DB
