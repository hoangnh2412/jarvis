# CR-000 — Example

| Mục | Giá trị |
|-----|---------|
| **Status** | Example — xóa hoặc đổi tên khi dùng thật |
| **Type** | modify |
| **Module(s)** | `{module-id}` |

> Khung CR: [template DOC-18](../../../../analyst/templates/DOC-18-change-request-register.md)

## Mô tả

Ví dụ: thay đổi chức năng trong **một module** — không đụng module khác trừ khi ghi impact.

## Affected documents

| DOC | Path | Change |
|-----|------|--------|
| 06 | `03-modules/{module-id}/DOC-06-srs.md` | +{MOD}-FR-xxx |
| 07 | `03-modules/{module-id}/DOC-07-acceptance-criteria.md` | +AC-xxx |

## Impact

| Dimension | Impact |
|-----------|--------|
| Scope | Module `{module-id}` only |
| Regression | TC gắn {MOD}-FR bị ảnh hưởng |

Xem `impact.yaml` và `deltas/`.
