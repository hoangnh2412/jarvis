# Workflow: Khởi tạo Jarvis Blob Storing

Áp dụng khi project **chưa** đăng ký `IBlobStoringService`.

## Checklist

```text
- [ ] 1. Chọn provider(s): filesystem, minio, hoặc cả hai
- [ ] 2. Package Jarvis.BlobStoring + provider packages
- [ ] 3. Configure options từ appsettings
- [ ] 4. AddKeyedSingleton IBlobStoringService
- [ ] 5. Inject trong service/handler với FromKeyedServices
- [ ] 6. Validate upload/download
```

## Bước 1 — Packages

```xml
<PackageReference Include="Jarvis.BlobStoring" Version="1.0.0" />
<!-- Chọn một hoặc cả hai -->
<PackageReference Include="Jarvis.BlobStoring.FileSystem" Version="1.0.0" />
<PackageReference Include="Jarvis.BlobStoring.MinIO" Version="1.0.0" />
```

## Bước 2 — Extension

Tạo `{App}BlobStoringExtension.cs` từ [templates/program-setup.cs](../templates/program-setup.cs):

```csharp
public static IHostApplicationBuilder AddBlobStoring(this IHostApplicationBuilder builder)
{
    builder.Services.Configure<FileSystemOption>(builder.Configuration.GetSection("FileSystem"));
    builder.Services.Configure<MinIOOption>(builder.Configuration.GetSection("MinIO"));

    builder.Services.AddKeyedSingleton<IBlobStoringService, FileSystemService>("FileSystem");
    builder.Services.AddKeyedSingleton<IBlobStoringService, MinioService>("MinIO");

    return builder;
}
```

Gọi trong `InfrastructureLayerExtension` hoặc `HostLayerExtension`:

```csharp
builder.AddBlobStoring();
```

Chỉ cần một provider → bỏ registration và package provider kia; đọc [providers/](../providers/).

## Bước 3 — appsettings

[templates/appsettings-blob.json](../templates/appsettings-blob.json)

## Bước 4 — Sử dụng

[templates/blob-service-usage.cs](../templates/blob-service-usage.cs)

## Bước 5 — Validate

- `dotnet build`
- Upload test file → `DownloadAsync` trả đúng bytes
- MinIO: bucket tồn tại trên server (tạo bucket nếu policy yêu cầu)

## Sau init

Thêm provider thứ hai → [workflows/add.md](add.md).
