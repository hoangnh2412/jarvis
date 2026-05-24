# Workflow: Khởi tạo Jarvis Entity Framework

Áp dụng khi Infrastructure **chưa** có `AddEntityFramework()` / DbContext.

## Checklist

```text
- [ ] 1. Caching-dotnet init (AddJarvisCaching) — bắt buộc
- [ ] 2. Package Jarvis.EntityFramework + DB provider
- [ ] 3. AddEntityFramework()
- [ ] 4. Chọn pattern (single-db | separate-tenant-db | hybrid)
- [ ] 5. AppDbContext, UoW, entities
- [ ] 6. appsettings ConnectionStrings + Cache:ConnectionString
- [ ] 7. Migrate / EnsureMigrateDb
- [ ] 8. dotnet build
```

## Bước 1 — Caching trước

[caching-dotnet/workflows/init.md](../../caching-dotnet/workflows/init.md):

```csharp
builder.AddJarvisCaching();
```

## Bước 2 — Packages

```xml
<PackageReference Include="Jarvis.EntityFramework" Version="1.0.0" />
<PackageReference Include="Jarvis.Caching" Version="1.1.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.*" />
```

## Bước 3 — Bootstrap

[templates/infrastructure-extension.cs](../templates/infrastructure-extension.cs):

```csharp
builder.AddJarvisCaching();
builder.AddEntityFramework();
builder.AddAppDbContext(); // theo pattern đã chọn
```

## Bước 4 — Chọn pattern

| Mô hình | SKILL |
|---------|-------|
| Single DB | [patterns/single-db/SKILL.md](../patterns/single-db/SKILL.md) |
| Separate tenant DB | [patterns/separate-tenant-db/SKILL.md](../patterns/separate-tenant-db/SKILL.md) |
| Hybrid | [patterns/hybrid/SKILL.md](../patterns/hybrid/SKILL.md) |

Đọc [reference/setup.md](../reference/setup.md) trước khi code.

## Bước 5 — appsettings

```json
{
  "ConnectionStrings": {
    "AutoMigrate": "true",
    "AppDbContext": "Host=localhost;..."
  },
  "Cache": {
    "Items": {
      "ConnectionString": { "Key": "conn:{dbid}", "MemSeconds": 14400 }
    }
  },
  "TenantHeaderKey": "X-Tenant-Id"
}
```

## Bước 6 — Validate

- `dotnet build`
- HTTP có tenant header → query filter hoạt động (single/hybrid pool)
- Job: `SwitchDbContextAsync` + `GetRepositoryAsync` lại
