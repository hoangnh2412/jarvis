---
name: blobstoring-dotnet-awss3
description: Đăng ký Jarvis BlobStoring AWS S3 — keyed AwsS3BlobStoringService với BlobStoring:AwsS3 Region và BucketName. Dùng khi lưu file trên Amazon S3.
dependencies:
  - Jarvis.BlobStoring
  - Jarvis.BlobStoring.AwsS3
---

# AWS S3 provider

`bucket` tham số API map tới S3 bucket (hoặc `BucketName` mặc định trong options nếu bucket rỗng).  
Client tạo lazy (`Lazy<IAmazonS3>`), `IDisposable`, có `ILogger`.

## appsettings

```json
{
  "BlobStoring": {
    "DefaultProvider": "AwsS3",
    "AwsS3": {
      "Region": "ap-southeast-1",
      "BucketName": "my-app-uploads",
      "AccessKey": "",
      "SecretKey": "",
      "AutoSelectPriority": 20
    }
  }
}
```

`AccessKey` / `SecretKey` rỗng → SDK dùng instance profile / env credentials.

## Registration

```csharp
using Jarvis.BlobStoring.Extensions;
using Jarvis.BlobStoring.AwsS3.Extensions;

builder.AddCoreBlobStoring()
    .UseAwsS3();

builder.AddCoreBlobStoring()
    .UseAwsS3(s3 =>
    {
        s3.Region = "ap-southeast-1";
        s3.BucketName = "my-app-uploads";
    });
```

## Inject

```csharp
[FromKeyedServices("AwsS3")] IBlobStoringService s3
```

## Lưu ý

- `UseAwsS3()` yêu cầu `Region` và `BucketName`.
- `ViewAsync` — presigned URL (giây).

## Validate

- Upload / download / delete trên bucket thật hoặc LocalStack
- IAM / credentials đủ quyền `s3:PutObject`, `s3:GetObject`
