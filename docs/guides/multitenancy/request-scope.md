# Request scope — gửi Tenant và Org đúng

Mô hình nghiệp vụ Root → Tenant → Org: [multi-tenant-multi-org.md](./multi-tenant-multi-org.md).

Luật HTTP khi bật working org: [ADR Orgs](../../04-platform/DOC-09-adr/2026-10-17_platform_orgs-module.md) §5.1.1 (D16). Path nào chạy middleware: [ADR lọc path MultiOrg](../../04-platform/DOC-09-adr/2026-11-10_platform_multiorg-path-filter.md).

Host chọn **một** chế độ. Không gắn `MultiOrgMiddleware` rồi gửi request kiểu chỉ tenant.

## Chọn chế độ

| Chế độ | Đăng ký | Pipeline | Request nghiệp vụ |
|--------|---------|----------|-------------------|
| Chỉ tenant | `AddCurrentTenant<TTenant>()` | Không gọi `UseCoreMiddleware<MultiOrgMiddleware>` | `X-Tenant-Id`. Không bắt `X-Org-Id` |
| Tenant và Org | `AddCurrentTenant<TTenant>()` và `AddCurrentOrg<TOrg>()` | `UseAuthentication()` rồi `UseCoreMiddleware<MultiOrgMiddleware>()` | `X-Org-Id` bắt buộc. `X-Tenant-Id` theo bảng bên dưới |

Sample đăng ký cả hai: [`SampleWebExtensions.cs`](../../../Sample/Extensions/SampleWebExtensions.cs) (`AddCurrentTenant`) và [`SampleOrgsExtensions.cs`](../../../Sample/Extensions/SampleOrgsExtensions.cs) (`AddCurrentOrg`). Pipeline: [`Sample/Program.cs`](../../../Sample/Program.cs).

`AddCurrentOrg` đăng ký `OrgFirstRequestScopeResolver` (`IRequestScopeResolver`). Resolver này từ chối request không có org. Vì vậy host chỉ tenant **không** gọi `AddCurrentOrg`.

## Chỉ tenant

Không có middleware tenant trong `Platform.Multitenancy`. Id xuất hiện khi code gọi `ICurrentTenant.GetIdAsync`, theo thứ tự:

1. `ICurrentTenantAccessor` đã có giá trị (`BeginScope` / `Change`) thì dùng giá trị đó.
2. Không có `IRequestScopeResolver`: `ITenantIdResolverFactory.GetTenantIdAsync`.

`GetTenantIdAsync` dừng ở nguồn đầu tiên ra GUID:

| Thứ tự | Nguồn | Mặc định |
|--------|--------|----------|
| 1 | Header | `X-Tenant-Id`. Đổi bằng config `TenantHeaderKey` |
| 2 | Claim user sau `UseAuthentication` | `ClaimTypes.GroupSid`. Đổi bằng `TenantClaimName` |
| 3 | Query | `tenantId`. Đổi bằng `TenantQueryName` |
| 4 | Host của request, khi host là chuỗi GUID | Không có key config |

Không có nguồn nào ra GUID thì `GetIdAsync` trả `null`. Pipeline không trả 403 chỉ vì thiếu header.

Ví dụ:

```http
GET /api/orders HTTP/1.1
X-Tenant-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

## Tenant và Org

`UseCoreMiddleware<MultiOrgMiddleware>()` sau `UseAuthentication()`, trước `UseAuthorization()`. Middleware gọi `IRequestScopeResolver`. Kết quả cấm thì **403** và dừng pipeline. Kết quả Allowed thì `BeginScope` cả org và tenant cho phần request còn lại.

`X-Org-Id` là hằng `HeaderOrgIdResolver.HeaderName`. `X-Tenant-Id` trong chế độ này là hằng `OrgFirstRequestScopeResolver.TenantHeaderName`. Config `TenantHeaderKey` **không** đổi header mà middleware org đọc.

Query `orgId` không thay `X-Org-Id`.

| `X-Tenant-Id` | `X-Org-Id` | Kết quả |
|---------------|------------|---------|
| Không | Không | 403 |
| Không | GUID | Org từ header. Tenant: domain (`ITenantDomainLookup`, nếu host đăng ký) rồi user → query `tenantId` → host. Org không thuộc tenant thì 403 |
| Có | Không | 403 |
| GUID | GUID | Kiểm tra org thuộc tenant. Sai thì 403 |

Header có mặt nhưng rỗng, không phải GUID, hoặc GUID rỗng (`00000000-0000-0000-0000-000000000000`) thì 403.

Có `X-Tenant-Id` thì tenant lấy đúng header đó, không rơi xuống claim hay query.

Ví dụ đủ cặp:

```http
GET /api/orders HTTP/1.1
X-Tenant-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
X-Org-Id: 6ba7b810-9dad-11d1-80b4-00c04fd430c8
```

Chỉ org, tenant lấy từ user đã đăng nhập (claim) hoặc domain:

```http
GET /api/orders HTTP/1.1
Authorization: Bearer …
X-Org-Id: 6ba7b810-9dad-11d1-80b4-00c04fd430c8
```

### Path không cần org

`UseCoreMiddleware<MultiOrgMiddleware>()` đọc `Middlewares:MultiOrg`. `IsEnable` tắt thì không gắn middleware. `Includes` có phần tử thì chỉ path khớp ít nhất một regex mới vào middleware. Path khớp `Excludes` thì bỏ qua. Request đã vào middleware thì luôn resolve scope.

Sample khai báo `Includes` là `^/api/`, `^/hubs/`, `^/sse/`, `^/elsa/`, và `Excludes` cho API có version:

| Path | Working scope |
|------|----------------|
| `/api/v…/authentication…`, `/api/v…/system/install…`, `/api/v…/platform/…` | Bỏ qua |
| `/api/v…/…/account/login`, `/api/v…/…/account/catalog` | Bỏ qua |
| `/api/v…/account/login`, `/api/authentication` (không có segment version) | Bắt buộc |
| `/api/…` còn lại | Bắt buộc |
| `/hubs/…`, `/sse/…`, `/elsa/…` | Bắt buộc |
| SPA, swagger, health, static, path rỗng | Bỏ qua |

Đổi bộ path trong `appsettings`, section `Middlewares:MultiOrg`.

```csharp
app.UseAuthentication();
app.UseCoreMiddleware<MultiOrgMiddleware>();
app.UseAuthorization();
```
