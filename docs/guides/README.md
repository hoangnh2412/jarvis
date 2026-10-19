# Hướng dẫn kỹ thuật (guides)

How-to, luồng runtime, diagram và tích hợp host — dành cho **engineer**.

## Mục lục

| Chủ đề | Tài liệu |
|--------|----------|
| **Authentication — tổng quan** | [authentication/README.md](./authentication/README.md) |
| Authentication — appsettings (Schemes / Profile) | [authentication/appsettings.md](./authentication/appsettings.md) |
| Authentication — storage / `Source` | [authentication/storage-backed-providers.md](./authentication/storage-backed-providers.md) |
| Authentication — Current user | [authentication/current-user.md](./authentication/current-user.md) |

## Quy ước

- Doc **gắn một package** có thể đặt thêm tại `frameworks/<Package>/doc/`; bản trong `guides/` là entry chính hoặc tóm tắt + link.
- Thay đổi kiến trúc → ADR trong [`ADRs/`](../../ADRs/); guide chỉ mô tả *cách dùng as-built* và link ADR.

## Tham chiếu nhanh

- ADR Current user / tenant: [`2026-08-01_platform_current-user-tenant.md`](../../ADRs/2026-08-01_platform_current-user-tenant.md)
- Sample host: [`Sample/Program.cs`](../../Sample/Program.cs)
