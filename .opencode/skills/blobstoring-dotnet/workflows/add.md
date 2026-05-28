# Workflow: Thêm blob provider

Áp dụng khi **đã có** `AddCoreBlobStoring()` và cần thêm MinIO hoặc AwsS3.

## Checklist

```text
- [ ] 1. Chọn provider: minio | awss3
- [ ] 2. Đọc providers/<name>/SKILL.md — chỉ một file
- [ ] 3. PackageReference package vệ tinh
- [ ] 4. .UseMinIO() hoặc .UseAwsS3() trên builder (hoặc chain sau AddCoreBlobStoring)
- [ ] 5. appsettings BlobStoring:MinIO / BlobStoring:AwsS3
- [ ] 6. Validate
```

## Bước 1 — Chọn provider

Chỉ đọc **một** file:

- [providers/minio/SKILL.md](../providers/minio/SKILL.md)
- [providers/awss3/SKILL.md](../providers/awss3/SKILL.md)

## Bước 2 — Registration

```csharp
builder.AddCoreBlobStoring()
    .UseMinIO(); // hoặc .UseAwsS3() hoặc cả hai
```

Không gọi `UseMinIO` hai lần; keyed service dùng `TryAddKeyedSingleton`.

## Bước 3 — Consumer

```csharp
public class DocsService(
    IBlobStoringService blobs, // default theo registry / DefaultProvider
    [FromKeyedServices("MinIO")] IBlobStoringService minio)
```

## Anti-patterns

- Cấu hình `BlobStoring:MinIO` đầy đủ nhưng **không** gọi `.UseMinIO()` — priority trong JSON không có tác dụng
- Hardcode `SecretKey` trong appsettings commit
- Hai registration cùng key `"MinIO"`
- `UseMinIO()` khi `Endpoint` rỗng — throw `InvalidOperationException`
