# GetOrSetAsync — cache-aside

Ưu tiên `GetOrSetAsync(param, async ct => ..., ct)` thay vì `GetAsync` overload cũ.

## EF / repository

```csharp
var param = CacheParam.Create("ProductDemo")
    .WithParam("tenantId", tenantId)
    .WithParam("id", id.ToString());

return cache.GetOrSetAsync(param, async ct =>
    await db.Products.AsNoTracking()
        .Where(p => p.TenantId == tenantId && p.Id == id)
        .FirstOrDefaultAsync(ct), ct);
```

Sau update/delete: `await cache.RemoveAsync(param, ct)`.

## HTTP API

```csharp
return await cache.GetOrSetAsync(param, async ct =>
{
    using var response = await httpClient.GetAsync(url, ct);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<ExternalDto>(ct);
}, ct);
```

## Lưu ý

| Chủ đề | Gợi ý |
|--------|--------|
| `CancellationToken` | Truyền xuống EF/HttpClient |
| `null` từ loader | Không ghi cache |
| Invalidation | `RemoveAsync` sau write — không chỉ TTL |
