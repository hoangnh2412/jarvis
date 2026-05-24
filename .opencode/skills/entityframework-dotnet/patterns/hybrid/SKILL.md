---
name: entityframework-dotnet-hybrid
description: Jarvis EF hybrid multitenancy — Master registry với pool DB chung và dedicated DB per tenant. Dùng khi tenant chia sẻ hoặc tách database linh hoạt.
dependencies:
  - Jarvis.EntityFramework
  - Npgsql.EntityFrameworkCore.PostgreSQL
---

# Hybrid multitenancy

→ Setup: [reference/setup.md](../../reference/setup.md) | Separate: [separate-tenant-db/SKILL.md](../separate-tenant-db/SKILL.md)

Đăng ký DI **giống separate tenant DB** — Master + `DbTenantConnectionStringResolver`.

## Quy tắc entity

| Tenant trên | Entity app |
|---|---|
| DB chung (pool) | **Bắt buộc** `ITenantEntity` + `TenantId` khi ghi |
| DB riêng | `ITenantEntity` tùy chọn |

## Migrate

- Master: một lần.
- Pool shared: migrate **một** DB pool.
- Dedicated: migrate **từng** connection riêng.

Resolver tùy biến: [custom-di/SKILL.md](../custom-di/SKILL.md).

## Checklist

- [ ] Nhiều tenant cùng `ConnectionString` trên Master
- [ ] Pool: `ITenantEntity` bắt buộc
- [ ] Migrate pool một lần, dedicated từng DB
