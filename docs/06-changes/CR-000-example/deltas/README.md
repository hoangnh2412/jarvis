# Deltas

Chỉ chứa **phần thay đổi** — không copy cả file DOC.

**Naming:** `{module-id}-DOC-06-srs.patch.md`

```markdown
## Delta — {MOD}-FR-012 (add)
**CR:** CR-000
**Target:** ../03-modules/{module-id}/DOC-06-srs.md

### Added
| FR ID | Description |
|-------|-------------|
| {MOD}-FR-012 | Hệ thống shall … |
```

Sau approve: merge vào file working + bump version.
