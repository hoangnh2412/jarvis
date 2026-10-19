# Tài liệu Platform (Jarvis)

Mục lục tài liệu **chung** cho monorepo. Quyết định kiến trúc nằm ở [`ADRs/`](../ADRs/README.md); tổng quan repo ở [`README.md`](../README.md).

## Đọc theo vai trò

| Bạn là… | Đọc trước |
|--------|-----------|
| Dev mới vào team | [`onboarding/getting-started.md`](./onboarding/getting-started.md) → [`features/`](./features/README.md) → [`guides/`](./guides/README.md) (phần đang làm) |
| Dev tích hợp / maintain | [`guides/`](./guides/README.md) + ADR khi đụng quyết định thiết kế |
| PM / BA / lead (overview) | [`features/`](./features/README.md) |

## Cấu trúc

| Thư mục | Mục đích |
|---------|----------|
| [`onboarding/`](./onboarding/) | Setup, chạy Sample, quen solution |
| [`guides/`](./guides/) | Hướng dẫn kỹ thuật, luồng, tích hợp (cho engineer) |
| [`features/`](./features/) | Giới thiệu capability / tính năng platform (ít code) |

## Tài liệu ngoài `docs/`

| Nội dung | Vị trí |
|----------|--------|
| ADR (vì sao / quyết định) | [`ADRs/`](../ADRs/) |
| SAD / OpenAPI module | `modules/*/doc/` |
| Frontend kit `@platform/core` | [`frameworks/frontend/docs/`](../frameworks/frontend/docs/) |
| Chi tiết sát một package .NET | `frameworks/*/doc/` (khi có) — `guides/` tóm tắt + link |

## Quy ước ghi doc mới

- Onboarding / “ngày đầu” → `onboarding/`
- How-to, diagram, DI, Sample → `guides/` (có thể nhóm `guides/authentication/`, …)
- Overview tính năng cho stakeholder → `features/`
- Thay đổi kiến trúc có trade-off → ADR mới trong `ADRs/`, doc hướng dẫn chỉ link ADR
