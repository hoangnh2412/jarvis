# IBlobStoringService — API

```csharp
Task UploadAsync(string bucket, string fileName, byte[] bytes);
Task<byte[]> DownloadAsync(string bucket, string fileName);
Task DeleteAsync(string bucket, string fileName);
Task DeletesAsync(string bucket, IEnumerable<string> fileNames);
Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800);
IEnumerable<string> GetFileNames(string bucket, string? prefix = null);
```

| Method | FileSystem | MinIO |
|--------|------------|-------|
| Upload / Download / Delete | ✅ | ✅ |
| ViewAsync (presigned) | Trả `""` | ✅ |
| GetFileNames | Rỗng (mặc định) | Có implementation |

**bucket** — thư mục logic (FS) hoặc tên bucket MinIO.  
**fileName** — path tương đối trong bucket (vd. `tenantId/doc.pdf`).
