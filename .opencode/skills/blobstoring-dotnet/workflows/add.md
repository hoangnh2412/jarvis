# Workflow: Thêm blob provider

Áp dụng khi **đã có** ít nhất một `IBlobStoringService` keyed và cần thêm FileSystem hoặc MinIO.

## Checklist

```text
- [ ] 1. Chọn provider: filesystem | minio
- [ ] 2. Đọc providers/<name>/SKILL.md
- [ ] 3. Thêm PackageReference
- [ ] 4. Configure<TOption> + AddKeyedSingleton
- [ ] 5. appsettings section
- [ ] 6. Validate
```

## Bước 1 — Chọn provider

Chỉ đọc **một** file:

- [providers/filesystem/SKILL.md](../providers/filesystem/SKILL.md)
- [providers/minio/SKILL.md](../providers/minio/SKILL.md)

## Bước 2 — Registration

Thêm vào extension blob storing hiện có — không duplicate key.

## Bước 3 — Consumer

Service mới chọn key phù hợp:

```csharp
[FromKeyedServices("MinIO")] IBlobStoringService blobStorage
```

## Anti-patterns

- Hai implementation cùng key `"MinIO"`
- Hardcode đường dẫn thay vì `FileSystem:RootPath`
- Commit `MinIO:SecretKey` vào repo
- Dùng `MinIO` keyed service khi chưa cài package `Jarvis.BlobStoring.MinIO`
