# IBlobStoringService — API

```csharp
Task UploadAsync(string bucket, string fileName, byte[] bytes, CancellationToken cancellationToken = default);
Task<byte[]> DownloadAsync(string bucket, string fileName, CancellationToken cancellationToken = default);
Task DeleteAsync(string bucket, string fileName, CancellationToken cancellationToken = default);
Task DeletesAsync(string bucket, IEnumerable<string> fileNames, CancellationToken cancellationToken = default);
Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800, CancellationToken cancellationToken = default);
Task<IReadOnlyList<string>> GetFileNamesAsync(string bucket, string? prefix = null, CancellationToken cancellationToken = default);
```

| Method | FileSystem | MinIO | AwsS3 |
|--------|------------|-------|-------|
| Upload / Download / Delete | ✅ | ✅ | ✅ |
| ViewAsync (presigned) | `""` | ✅ | ✅ |
| GetFileNamesAsync | ✅ | ✅ | ✅ |

**bucket** — thư mục logic (FS) hoặc tên bucket object storage.  
**fileName** — path tương đối trong bucket (vd. `tenantId/doc.pdf`).

Default `IBlobStoringService` resolve qua `BlobStoringProviderRegistry` (`DefaultProvider` hoặc `AutoSelectPriority`).

Keyed: `[FromKeyedServices("FileSystem"|"MinIO"|"AwsS3")]`.
