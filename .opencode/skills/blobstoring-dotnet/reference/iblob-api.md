# IBlobStoringService — API

```csharp
Task UploadAsync(string bucket, string fileName, byte[] bytes);
Task<byte[]> DownloadAsync(string bucket, string fileName);
Task DeleteAsync(string bucket, string fileName);
Task DeletesAsync(string bucket, IEnumerable<string> fileNames);
Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800);
IEnumerable<string> GetFileNames(string bucket, string? prefix = null);
```

| Method | FileSystem | MinIO | AwsS3 |
|--------|------------|-------|-------|
| Upload / Download / Delete | ✅ | ✅ | ✅ |
| ViewAsync (presigned) | `""` | ✅ | ✅ |
| GetFileNames | ✅ | ✅ | ✅ |

**bucket** — thư mục logic (FS) hoặc tên bucket object storage.  
**fileName** — path tương đối trong bucket (vd. `tenantId/doc.pdf`).

Default `IBlobStoringService` resolve qua `BlobStoringProviderRegistry` (`DefaultProvider` hoặc `AutoSelectPriority`).

Keyed: `[FromKeyedServices("FileSystem"|"MinIO"|"AwsS3")]`.
