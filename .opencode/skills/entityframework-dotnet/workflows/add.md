# Workflow: Thêm / đổi mô hình EF

Áp dụng khi **đã có** EF Jarvis và cần đổi pattern hoặc bổ sung Master / resolver tùy biến.

## Checklist

```text
- [ ] 1. Xác định pattern mới (single-db | separate-tenant-db | hybrid | custom-di)
- [ ] 2. Đọc patterns/<name>/SKILL.md
- [ ] 3. Cập nhật AddAppDbContext / entities / Master
- [ ] 4. Migration plan (pool vs dedicated)
- [ ] 5. Validate HTTP + background job
```

## Bước 1 — Chọn pattern

Chỉ đọc **một** file trong `patterns/`.

## Bước 2 — Custom resolver

Nếu MST, MinIO, API connection → [patterns/custom-di/SKILL.md](../patterns/custom-di/SKILL.md).

## Bước 3 — Đổi từ single → separate

- Thêm `MasterDbContext`, `Tenant` entity `ITenantManagementEntity`
- Đổi `AddCoreDbContext` sang overload 2 generic
- Migrate dữ liệu tenant sang DB riêng (ngoài Jarvis)

## Anti-patterns

- `AddEntityFramework` trước `AddJarvisCaching`
- Tái dùng repository sau `SwitchDbContextAsync` không gọi `GetRepositoryAsync` lại
- Một UoW cho Master và tenant trong batch job
