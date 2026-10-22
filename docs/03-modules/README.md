# Modules

Mỗi **module** (bounded context, epic domain, subsystem) = một folder con.

## Tạo module mới

```bash
cp -r _template/ {module-id}/
```

Điền `{module-id}` (lowercase, kebab-case) và **MOD prefix** (uppercase, 3–6 ký tự) trong README module.

## Cấu trúc chuẩn mỗi module

| File | DOC |
|------|-----|
| `DOC-04-business-rules.md` | 04 |
| `DOC-05-use-cases.md` | 05 |
| `DOC-06-srs.md` | 06 |
| `DOC-07-acceptance-criteria.md` | 07 |
| `DOC-16-test-strategy.md` | 16 |

Template: [`../../templates/`](../../templates/README.md)

## Module đã tạo

| Module ID | MOD prefix | Folder |
|-----------|------------|--------|
| *(chưa có — copy từ `_template/`)* | | |
