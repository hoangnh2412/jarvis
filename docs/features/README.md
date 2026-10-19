# Giới thiệu tính năng (capabilities)

Tài liệu **overview** cho PM, BA, lead và dev cần bản đồ platform trước khi đọc code. Ít snippet; chi tiết kỹ thuật → [`guides/`](../guides/README.md).

## Bản đồ capability (khung)

| Capability | Mô tả ngắn | Package / module chính | Hướng dẫn kỹ thuật |
|------------|------------|-------------------------|---------------------|
| Authentication | Xác thực (JWT, ApiKey, Basic, Cognito…) | `Platform.Authentication.*` | [authentication.md](./authentication.md) → [guides/authentication/README.md](../guides/authentication/README.md) |
| Current user / tenant | Profile user & tenant trong request / job | `Platform.Authentication`, `Platform.Multitenancy` | [current-user](../guides/authentication/current-user.md) |
| Authorization | RBAC + ABAC, policy, data scope | `Platform.Authorization.*` | ADR [RBAC+ABAC](../../ADRs/2026-08-14_platform_rbac-abac-authorization.md) |
| Multitenancy | Tenant hiện tại, connection DB theo tenant | `Platform.Multitenancy.*` | ADR Multitenancy |
| Caching | Memory + Redis, invalidation | `Platform.Caching.*` | *(bổ sung)* |
| Realtime / Notifications | SignalR, SSE, inbox | `Platform.Realtime.*`, `modules/notifications` | ADR Notifications / Realtime |
| Import / Export | Template Excel, pipeline import | `modules/importexport` | `modules/.../doc/` |
| Workflow | Elsa embedded / client | `Platform.Workflow`, `modules/workflows` | Sample `Program.cs` |
| Observability | OTEL trace/log/metric | `Platform.OpenTelemetry.*` | Sample telemetry |

## Cách viết bài trong `features/`

- **Một file một capability** (vd. `authentication.md`, `authorization.md`).
- Nêu: giá trị, phạm vi, phụ thuộc hạ tầng, giới hạn đã biết, link Sample demo.
- Link ADR khi cần giải thích *quyết định*, không copy ADR vào đây.

## Trạng thái module (tham chiếu)

Bảng chi tiết theo module: [README.md](../../README.md) (mục modules).
