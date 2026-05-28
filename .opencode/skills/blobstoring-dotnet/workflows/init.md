# Workflow: Khởi tạo Jarvis Blob Storing

Áp dụng khi project **chưa** gọi `AddCoreBlobStoring()`.

## Checklist

```text
- [ ] 1. Chọn provider(s): filesystem (mặc định), minio, awss3
- [ ] 2. Package Jarvis.BlobStoring (+ MinIO / AwsS3 nếu cần)
- [ ] 3. AddCoreBlobStoring (+ UseMinIO / UseAwsS3)
- [ ] 4. appsettings section BlobStoring
- [ ] 5. Inject IBlobStoringService trong service/handler
- [ ] 6. Validate upload/download
```

## Bước 1 — Packages

```xml
<PackageReference Include="Jarvis.BlobStoring" Version="1.0.0" />
<!-- Tùy chọn -->
<PackageReference Include="Jarvis.BlobStoring.MinIO" Version="1.0.0" />
<PackageReference Include="Jarvis.BlobStoring.AwsS3" Version="1.0.0" />
```

## Bước 2 — Registration

Dùng [templates/program-setup.cs](../templates/program-setup.cs):

```csharp
using Jarvis.BlobStoring.Extensions;
// using Jarvis.BlobStoring.MinIO.Extensions;
// using Jarvis.BlobStoring.AwsS3.Extensions;

// FileSystem — AddCoreBlobStoring gọi UseFileSystem sẵn
builder.AddCoreBlobStoring();

// + MinIO (cần Endpoint trong config hoặc configure delegate)
// builder.AddCoreBlobStoring().UseMinIO();

// + AWS S3 (cần Region + BucketName)
// builder.AddCoreBlobStoring().UseAwsS3();
```

Gọi trong `Program.cs` hoặc `HostLayerExtension` / `InfrastructureLayerExtension`.

## Bước 3 — appsettings

[templates/appsettings-blob.json](../templates/appsettings-blob.json)

## Bước 4 — Sử dụng

[templates/blob-service-usage.cs](../templates/blob-service-usage.cs)

## Bước 5 — Validate

- `dotnet build`
- Upload → `DownloadAsync` bytes khớp
- `DefaultProvider` hoặc auto-select đúng provider đã đăng ký

## Sau init

Thêm provider khác → [workflows/add.md](add.md).
