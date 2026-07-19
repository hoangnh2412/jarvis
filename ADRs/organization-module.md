# ADR — Tách module `Jarvis.Organization`

> **Trạng thái:** 📝 Draft — phân tích sơ bộ, chờ trao đổi chi tiết.
> **Vấn đề:** Có nên tách `Organization` (cây đơn vị, phòng ban) thành module riêng, độc lập với `Tenant`, và quan hệ với ABAC như thế nào?
> **Liên quan:** [multi-tenant.md](./multi-tenant.md), [refactor-authentication.md](./refactor-authentication.md) (story Authorization — chưa thiết kế), [platform-architecture.md](./platform-architecture.md).

---

## 1. Khuyến nghị

**Tách `Organization` thành module riêng — thành 3 concern độc lập, không phải 2.** ABAC **không** nằm trong `Organization`: `Organization` cung cấp *cấu trúc cây + truy vấn scope*, còn ABAC là *policy engine* thuộc tầng Authorization.

```text
Jarvis.Authorization (ABAC)  ─┐
                              ├─→  Jarvis.Organization  ──→  Jarvis.MultiTenancy
   (đọc org-scope + org-tree) ─┘        (cây đơn vị)          (Tenant, TenantId, isolation)
```

---

## 2. Vì sao tách `Organization` khỏi `Tenant`

Hai concern khác bản chất — đúng tinh thần Atomic module của Jarvis:

| | `Jarvis.MultiTenancy` | `Jarvis.Organization` |
|---|---|---|
| Bản chất | Hạ tầng cô lập dữ liệu (infra) | Cấu trúc tổ chức nghiệp vụ (domain-ish) |
| Vai trò | Ranh giới `TenantId`, global query filter, tenant hierarchy (SaaS reseller) | Cây đơn vị/phòng ban, traversal, scope resolution |
| Ai cần | Gần như mọi module (cross-cutting) | Chỉ app cần phân cấp tổ chức + ABAC theo đơn vị |
| Opt-in độc lập? | Có — app 1 tenant vẫn có thể bật | Có — app không cần org tree thì không tham chiếu |

Phụ thuộc **một chiều, sạch**: `Organization` có `TenantId` → phụ thuộc `MultiTenancy`; `MultiTenancy` không biết gì về `Organization`. Gộp chung sẽ kéo logic cây tổ chức vào module nền của mọi thứ → nặng và khó tái dùng.

---

## 3. Vì sao ABAC không thuộc `Organization`

ABAC cần 3 nguồn:

1. Thuộc tính chủ thể — `scope`, `organization_id`.
2. Thuộc tính resource — `TenantId`, `OrganizationId`.
3. **Cây tổ chức** — để bung "scope = org X" thành toàn bộ subtree.

Chỉ (3) là của `Organization`. Gộp ABAC vào `Organization` sẽ khiến module này dính authentication/authorization, options policy, claim… → phình, mất tính nguyên tử.

Ranh giới đúng:

- **`Organization`** expose contract truy vấn cây:
  - `IOrganizationScopeResolver.GetDescendants(orgId)` / `IsUnder(node, ancestor)`
  - `IOrganizationTree` — ancestors, subtree, membership của user
- **`Jarvis.Authorization` (ABAC)** *tiêu thụ* contract đó để đánh giá policy — khớp story Authorization đang "chưa thiết kế" trong [refactor-authentication.md](./refactor-authentication.md).

---

## 4. Ranh giới module đề xuất

Dùng gói **Abstractions** để cắt phụ thuộc cứng:

```text
Jarvis.MultiTenancy.Abstractions   ← ITenantContext, TenantId resolution
Jarvis.MultiTenancy                ← Tenant entity, EF global filter, tenant hierarchy
Jarvis.Organization.Abstractions   ← IOrganizationTree, IOrganizationScopeResolver, OrganizationNode
Jarvis.Organization                ← entity (Id, TenantId, ParentId, Code, Name) + traversal
Jarvis.Authorization(.Abac)        ← policy engine, dùng 2 abstractions trên
```

`Jarvis.Organization` phụ thuộc `Jarvis.MultiTenancy.Abstractions` (đọc tenant hiện tại + gắn `TenantId`), **không** phụ thuộc bản impl.

---

## 5. Cảnh báo (Simplicity First)

1. **Giữ `Organization` generic** — cây `Id/TenantId/ParentId/Code/Name`; "đơn vị, phòng ban" chỉ là cách gọi, không hard-code loại node vào core (đúng quyết định trong [multi-tenant.md](./multi-tenant.md)).
2. **YAGNI cho gói Abstractions** — chỉ tách `*.Abstractions` khi thực sự có module thứ hai (`Authorization`) phụ thuộc contract. Trước đó, giữ contract trong chính `Jarvis.Organization`.
3. **Không tách quá sớm nếu chưa có ABAC** — nếu ABAC còn xa, để `Organization` đứng riêng nhưng chưa cần Abstractions; nâng cấp khi Authorization được thiết kế.

---

## 6. Lộ trình

1. `Jarvis.MultiTenancy` — nền (Tenant, isolation, tenant hierarchy).
2. `Jarvis.Organization` — cây tổ chức, kèm contract scope resolution.
3. `Jarvis.Authorization` (ABAC) — tiêu thụ contract của `Organization` + `MultiTenancy`.

> **Còn mở (chờ trao đổi):** mô hình membership user↔organization, cách nhiều đơn vị của một user tương tác với scope, ranh giới chính xác giữa `Organization` core và mở rộng nghiệp vụ của dự án, thiết kế policy ABAC cụ thể.
