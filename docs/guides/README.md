# Hướng dẫn kỹ thuật (guides)

How-to, luồng runtime, diagram và tích hợp host — dành cho **engineer**.

## Mục lục

| Chủ đề | Tài liệu |
|--------|----------|
| **Authentication — tổng quan** | [authentication/README.md](./authentication/README.md) |
| Authentication — appsettings (Schemes / Profile) | [authentication/appsettings.md](./authentication/appsettings.md) |
| Authentication — storage / `Source` | [authentication/storage-backed-providers.md](./authentication/storage-backed-providers.md) |
| Authentication — Current user | [authentication/current-user.md](./authentication/current-user.md) |
| **Multi-tenant, multi-org** | [multitenancy/multi-tenant-multi-org.md](./multitenancy/multi-tenant-multi-org.md) |
| Request scope — header Tenant / Org | [multitenancy/request-scope.md](./multitenancy/request-scope.md) |

## Quy ước

- Doc **gắn một package** có thể đặt thêm tại `frameworks/<Package>/doc/`; bản trong `guides/` là entry chính hoặc tóm tắt + link.
- Thay đổi kiến trúc → ADR trong [`04-platform/DOC-09-adr/`](../04-platform/DOC-09-adr/); guide chỉ mô tả *cách dùng as-built* và link ADR.

## Tham chiếu nhanh

- ADR Current user / tenant: [`2026-08-01_platform_current-user-tenant.md`](../04-platform/DOC-09-adr/2026-08-01_platform_current-user-tenant.md)
- Sample host: [`Sample/Program.cs`](../../Sample/Program.cs)
