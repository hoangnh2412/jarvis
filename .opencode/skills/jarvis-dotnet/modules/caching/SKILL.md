---
name: jarvis-dotnet-caching
description: Cài Jarvis.Caching — memory + Redis distributed cache qua AddJarvisCaching. Dùng khi project cần cache phân tầng mem/Redis theo section Cache.
dependencies:
  - Jarvis.Caching
  - Jarvis.Caching.Redis
  - StackExchange.Redis
---

# Caching

## Packages

| Project | PackageId |
|---|---|
| Jarvis.Caching | `Jarvis.Caching` (memory qua `AddMemoryCache` + `MsMemoryCacheAdapter`) |
| Jarvis.Caching.Redis | `Jarvis.Caching.Redis` |

## appsettings.json

```json
{
  "Cache": {
    "DefaultDistributedGroup": "Default",
    "DefaultDistributedType": "Redis",
    "MemoryInvalidation": {
      "Redis": {
        "Configuration": "127.0.0.1:6379"
      }
    },
    "DistributedGroups": {
      "Redis": {
        "Default": {
          "Configuration": "127.0.0.1:6379",
          "InstanceName": "app_"
        },
        "Auth": {
          "Configuration": "127.0.0.1:6379",
          "InstanceName": "auth_"
        }
      }
    },
    "Items": {
      "ContextData": {
        "Key": "context:{sid}",
        "MemSeconds": 180,
        "DistributedSeconds": 3600,
        "DistributedGroup": "Auth"
      },
      "ConnectionString": {
        "Key": "conn:{dbid}",
        "MemSeconds": 14400
      }
    }
  }
}
```

- `MemSeconds > 0` — ghi/đọc memory (tầng đầu tiên).
- `DistributedSeconds > 0` — bật Redis; cần `DistributedGroup` hoặc `DefaultDistributedGroup` + `DefaultDistributedType`.
- `MemoryInvalidation:Redis:Configuration` — Redis **riêng** cho pub/sub xóa memory giữa các node (không dùng chung multiplexer với `DistributedGroups`).

## Wiring (host)

```csharp
using Jarvis.Caching.Extensions;
using Jarvis.Caching.Redis;

builder.AddJarvisCaching()
    .UseRedisDistributedCache()
    .UseRedisMemoryCacheInvalidation();
```

Inject `ICacheService`:

```csharp
var param = CacheParam.Create("ContextData").WithParam("sid", sessionId);

// Cache-aside (preferred)
var data = await cacheService.GetOrSetAsync(param, async ct => await LoadFromDbAsync(ct), ct);

// Hit/miss without calling the data source
var hit = await cacheService.TryGetAsync<MyDto>(param, ct);
if (hit.HasValue)
    return hit.Value;
```

## `GetOrSetAsync` — delegate `query` (data source on miss)

Chữ ký:

```csharp
Func<CancellationToken, Task<T>> query
```

**Đủ** cho mọi nguồn dữ liệu async khi cache miss: SQL/EF, HTTP API, MinIO/FileSystem (`IBlobStoringService`), tính toán trong app, v.v. `CacheService` **không** biết delegate gọi gì — chỉ:

1. `TryGetAsync` → memory → Redis  
2. Nếu miss → `await query(cancellationToken)`  
3. Nếu kết quả không phải `null` (reference) → `SetAsync` theo TTL trong config  

Host inject service vào closure (primary constructor / field) rồi gọi trong lambda.

### Luồng

```
GetOrSetAsync
  → TryGet (memory, Redis)
  → [hit] return Value
  → [miss] query(ct)  ← DB / MinIO / API của bạn
  → SetAsync (MemSeconds / DistributedSeconds)
```

### Ví dụ: đọc DB (EF / repository)

Khai báo item trong `Cache:Items` (vd. `ProductDemo` với `Key`: `Product:{tenantId}:{id}`).

```csharp
public sealed class ProductService(ICacheService cache, AppDbContext db)
{
    public Task<ProductDto?> GetProductAsync(string tenantId, Guid id, CancellationToken ct)
    {
        var param = CacheParam.Create("ProductDemo")
            .WithParam("tenantId", tenantId)
            .WithParam("id", id.ToString());

        return cache.GetOrSetAsync(param, async cancellationToken =>
        {
            return await db.Products
                .AsNoTracking()
                .Where(p => p.TenantId == tenantId && p.Id == id)
                .Select(p => new ProductDto(p.Id, p.Name))
                .FirstOrDefaultAsync(cancellationToken);
        }, ct);
    }
}
```

- Truyền `cancellationToken` xuống EF/API — hủy được khi client disconnect.  
- `FirstOrDefaultAsync` trả `null` → **không** ghi cache (theo logic hiện tại của `CacheService`).  
- Sau `Update`/`Delete` trong DB: `await cache.RemoveAsync(param, ct)` để xóa memory + Redis + notify peer.

### Ví dụ: đọc MinIO / blob (`IBlobStoringService`)

Cache **metadata hoặc nội dung đã deserialize**, không thay thế stream lớn trừ khi cố ý (byte[], DTO nhỏ).

```csharp
public sealed class DocumentCacheService(
    ICacheService cache,
    [FromKeyedServices("MinIO")] IBlobStoringService blobStorage)
{
    public Task<byte[]?> GetDocumentBytesAsync(string tenantId, string docId, CancellationToken ct)
    {
        var param = CacheParam.Create("TenantDocument") // khai báo trong appsettings
            .WithParam("tenantId", tenantId)
            .WithParam("docId", docId);

        return cache.GetOrSetAsync(param, async cancellationToken =>
        {
            // IBlobStoringService hiện chưa nhận CancellationToken — vẫn truyền ct vào GetOrSetAsync;
            // có thể dùng cancellationToken.ThrowIfCancellationRequested() trước/sau gọi blob.
            cancellationToken.ThrowIfCancellationRequested();

            var path = $"{tenantId}/{docId}.pdf";
            return await blobStorage.DownloadAsync(bucket: "documents", fileName: path);
        }, ct);
    }
}
```

- `T` = `byte[]`: value type reference trong heap — cache được; miss khi chưa có key.  
- Upload/Delete blob: gọi `RemoveAsync` cùng `CacheParam` (cùng placeholder) sau khi đổi file.  
- Presigned URL (`ViewAsync`): có thể cache `string` URL với TTL ngắn trong `MemSeconds` / `DistributedSeconds`.

### Ví dụ: API ngoài (HTTP)

```csharp
return await cache.GetOrSetAsync(param, async ct =>
{
    using var response = await httpClient.GetAsync(url, ct);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<ExternalDto>(ct);
}, ct);
```

### Lưu ý khi viết `query`

| Chủ đề | Gợi ý |
|--------|--------|
| **CancellationToken** | Luôn dùng `ct` từ delegate trong I/O hỗ trợ hủy (EF, HttpClient). |
| **`null`** | Reference `null` từ loader → không `SetAsync`; lần sau vẫn miss. |
| **Kích thước** | Tránh cache object quá lớn; blob lớn nên cache DTO/link, không cache full file nếu không cần. |
| **Invalidation** | Sửa nguồn (DB/blob) → `RemoveAsync(param)`; không rely vào TTL khi cần nhất quán. |
| **Transaction** | Ghi DB xong mới invalidate cache; hoặc invalidate trước khi đọc lại tùy chiến lược. |
| **Lỗi** | Exception từ `query` không được cache — bubble ra host (try/catch ở handler). |

`GetOrSetAsync` **không** thay repository/UoW — chỉ bọc read path cache-aside. Command/write vẫn do host điều phối + `RemoveAsync`.

Invalidate (local memory + Redis + pub/sub peers):

```csharp
await cacheService.RemoveAsync(param, ct);
```

## Extension (Redis package)

| Extension | Mục đích |
|---|---|
| `AddJarvisCaching` | Bind options, memory, `ICacheService` |
| `UseRedisDistributedCache` | Đăng ký Redis theo `Cache:DistributedGroups:Redis` |
| `UseRedisMemoryCacheInvalidation` | Pub/sub xóa memory; keyed `MemoryCacheInvalidationDefaults.ConnectionServiceKey` |
| `AddJarvisCachingMemoryInvalidationRedisInstrumentation` | OTEL trace cho connection invalidation (không đụng Redis khác) |

### OpenTelemetry (nhiều Redis connection)

Invalidation dùng `MemoryCacheInvalidationDefaults` (`ConnectionServiceKey`, `RedisChannel`). Instrument **từng** connection — không gộp với distributed cache:

```csharp
using Jarvis.Caching.Redis.Extensions;

.ConfigureTrace(options =>
{
    options.AddRedisInstrumentation("Default"); // app/demo Redis
    options.AddJarvisCachingMemoryInvalidationRedisInstrumentation();
    // hoặc: options.AddJarvisCachingRedisInstrumentation("MyOtherRedisKey");
})
```

`UseRedisMemoryCacheInvalidation()` phải chạy **trước** host build để keyed multiplexer có trong DI khi OTEL resolve.

## Sample

- `GET /api/cache-demo/product/{tenantId}/{id}` — demo `ProductDemo` item
- `DELETE /api/cache-demo/product/{tenantId}/{id}` — invalidate

Chi tiết refactor: `docs/refactor-cache-plan.md`.
