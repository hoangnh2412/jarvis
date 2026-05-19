# EF — Tùy biến qua DI (không sửa Jarvis)

→ Setup: [setup.md](setup.md) | Index: [SKILL.md](SKILL.md)

Chỉ code trong **Host / Infrastructure**. Jarvis dùng `TryAddScoped` → app `AddScoped<TInterface, TImpl>()` **sau** `AddEntityFramework()` / `AddCoreDbContext()`.

```csharp
builder.AddEntityFramework();
builder.AddAppDbContext();

builder.Services.AddScoped<ITenantIdResolverFactory, AppTenantIdResolverFactory>();
builder.Services.AddScoped<ITenantConnectionStringResolverFactory, AppTenantConnectionStringResolverFactory>();
```

| Muốn thay | Ghi đè |
|---|---|
| **TenantId** (vd. MST → Guid) | `ITenantIdResolverFactory` |
| **Connection** theo tenant (vd. MinIO) | Keyed `ITenantConnectionStringResolver` hoặc `ITenantConnectionStringResolverFactory` |

## Phân vai factory

| Thành phần | `ICurrentTenantAccessor` | `ITenantIdResolverFactory` |
|---|---|---|
| `BaseUnitOfWork` / filter | Không | Có |
| Connection / interceptor | Trước, rồi factory | Có nếu accessor trống |
| `SwitchDbContextAsync` | `BeginScope` (connection) | `_switchedTenantId` (filter) |

---

## TenantId — ví dụ MST trên Master

Header `X-Tax-Code` → query Master → `TenantId`.

**Keyed resolver:**

```csharp
public sealed class TaxCodeTenantIdResolver(
    IHttpContextAccessor http,
    IDbContextFactory<MasterDbContext> masterFactory)
    : ITenantIdResolver
{
    public async Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        var raw = http.HttpContext?.Request.Headers["X-Tax-Code"].ToString();
        if (string.IsNullOrWhiteSpace(raw)) return null;

        await using var db = await masterFactory.CreateDbContextAsync(cancellationToken);
        return await db.Set<Tenant>().AsNoTracking()
            .Where(t => t.TaxCode == raw)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

**Factory kế thừa + chain mặc định:**

```csharp
public sealed class AppTenantIdResolverFactory(IServiceProvider sp)
    : TenantIdResolverFactory(sp)
{
    public override async Task<Guid?> GetTenantIdAsync(CancellationToken ct = default)
    {
        var tax = sp.GetRequiredKeyedService<ITenantIdResolver>(nameof(TaxCodeTenantIdResolver));
        var id = await tax.GetTenantIdAsync(ct).ConfigureAwait(false);
        if (id.HasValue) return id;
        return await base.GetTenantIdAsync(ct).ConfigureAwait(false);
    }
}
```

```csharp
builder.Services.AddKeyedScoped<ITenantIdResolver, TaxCodeTenantIdResolver>(nameof(TaxCodeTenantIdResolver));
builder.Services.AddScoped<ITenantIdResolverFactory, AppTenantIdResolverFactory>();
```

**Thay hoàn toàn:** `AddScoped<ITenantIdResolverFactory, TaxCodeOnlyTenantIdResolverFactory>()` — không gọi header/claim.

**Lưu ý:** Tra Master dùng `IDbContextFactory<MasterDbContext>`, không `IMasterUnitOfWork` trong resolver. Job: `SwitchDbContextAsync(tenantId)`.

---

## ConnectionString — ví dụ MinIO

Factory mặc định gọi keyed resolver với `name = tenantId.ToString()`.

**Cách A — Custom resolver (khuyến nghị):**

```csharp
public sealed class MinioTenantConnectionStringResolver(IMinioClient minio, IConfiguration config)
    : ITenantConnectionStringResolver
{
    public async Task<string?> GetConnectionStringAsync(string name, CancellationToken ct = default)
    {
        if (!Guid.TryParse(name, out _)) return null;
        // đọc object tenants/{name}/db-connection.txt từ bucket
        return cs;
    }
}
```

```csharp
builder.Services.AddCoreDbContext<AppDbContext, MinioTenantConnectionStringResolver>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AppDbContext")!));
```

**Cách B — Kế thừa `DbTenantConnectionStringResolver`**, override `GetConnectionStringAsync`, fallback MinIO.

**Cách C — Thay `ITenantConnectionStringResolverFactory`** (`sealed` trong Jarvis — implement interface trong app).

Vẫn cần `AddCoreDbContext<TDb, TResolver>` nếu dùng interceptor.

---

## Checklist

- [ ] `AddScoped` sau `AddEntityFramework` / `AddCoreDbContext`
- [ ] Resolver tra Master: `IDbContextFactory`, không UoW tenant
- [ ] Job: `SwitchDbContextAsync` khi đã biết `TenantId`
