# Workflow: Khởi tạo Jarvis Caching

Áp dụng khi project **chưa** có `AddJarvisCaching()`.

## Checklist

```text
- [ ] 1. Xác định layer (Infrastructure / Host)
- [ ] 2. Package Jarvis.Caching
- [ ] 3. AddJarvisCaching() trước AddEntityFramework (nếu có EF)
- [ ] 4. appsettings Cache:Items tối thiểu
- [ ] 5. Inject ICacheService nơi cần
- [ ] 6. Validate GetOrSetAsync / TryGetAsync
```

## Bước 1 — Package

```xml
<PackageReference Include="Jarvis.Caching" Version="1.1.0" />
```

Layer: **Infrastructure** (cùng EF) hoặc Host nếu chỉ API cache.

## Bước 2 — Registration

[templates/program-setup.cs](../templates/program-setup.cs):

```csharp
using Jarvis.Caching.Extensions;

builder.AddJarvisCaching();
```

**Trước** `AddEntityFramework()` khi dùng EF multitenancy.

## Bước 3 — appsettings

[templates/appsettings-cache.json](../templates/appsettings-cache.json) — tối thiểu item `ConnectionString` nếu dùng EF:

```json
{
  "Cache": {
    "Items": {
      "ConnectionString": {
        "Key": "conn:{dbid}",
        "MemSeconds": 14400
      }
    }
  }
}
```

## Bước 4 — Sử dụng

```csharp
var param = CacheParam.Create("MyItem").WithParam("id", id);
var dto = await cache.GetOrSetAsync(param, async ct => await LoadAsync(ct), ct);
```

Khai báo `MyItem` trong `Cache:Items` với `Key`, `MemSeconds`, …

## Bước 5 — Validate

- `ICacheService` resolve được
- Miss → loader chạy một lần; hit không gọi loader
- EF: `AddEntityFramework` không throw thiếu `ICacheService`

## Sau init

Redis: [workflows/add.md](add.md) + [providers/redis-distributed/SKILL.md](../providers/redis-distributed/SKILL.md).
