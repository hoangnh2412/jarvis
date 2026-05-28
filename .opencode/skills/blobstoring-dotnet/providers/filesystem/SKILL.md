---
name: blobstoring-dotnet-filesystem
description: Đăng ký Jarvis BlobStoring FileSystem — keyed IBlobStoringService FileSystemBlobStoringService với BlobStoring:FileSystem. Dùng khi lưu file trên disk local hoặc volume mount.
dependencies:
  - Jarvis.BlobStoring
---

# FileSystem provider

Path vật lý: `{RootPath}/{SubPath}/{bucket}/{fileName}`.  
`RootPath` rỗng → `{ContentRoot}/wwwroot/blobs` (PostConfigure trong `UseFileSystem`).

`AddCoreBlobStoring()` gọi `UseFileSystem()` tự động.

## appsettings

```json
{
  "BlobStoring": {
    "DefaultProvider": "FileSystem",
    "FileSystem": {
      "RootPath": "",
      "SubPath": "",
      "AutoSelectPriority": 10
    }
  }
}
```

## Registration

```csharp
using Jarvis.BlobStoring.Extensions;

builder.AddCoreBlobStoring();
// hoặc tùy chỉnh:
builder.AddCoreBlobStoring(o =>
{
    o.DefaultProvider = nameof(BlobStoringType.FileSystem);
    o.FileSystem.RootPath = @"D:\uploads";
    o.FileSystem.SubPath = "tenant-1";
});
```

Keyed resolve:

```csharp
[FromKeyedServices("FileSystem")] IBlobStoringService storage
```

## Lưu ý

- `ViewAsync` trả chuỗi rỗng — không presigned URL.
- `GetFileNames` liệt kê file dưới bucket (recursive).
- Process cần quyền ghi `RootPath`.

## Validate

- Upload + download cùng `bucket` + `fileName`
- Không cho `fileName` chứa `..` ở tầng application
