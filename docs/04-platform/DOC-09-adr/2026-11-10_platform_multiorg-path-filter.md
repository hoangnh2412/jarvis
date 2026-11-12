# ADR — Lọc path `MultiOrgMiddleware` bằng `Middlewares:MultiOrg`

> **Trạng thái:** 🟢 Accepted + Implemented (2026-11-10)
> **Ngày:** 2026-11-10 · Accept: 2026-11-10 · Implement: 2026-11-10
> **Loại:** HTTP pipeline / cấu hình host
> **Liên quan:** [Module Organization](./2026-10-17_platform_orgs-module.md) (D16 — resolve working org/tenant; bảng header giữ nguyên), [architecture-rules.md](./architecture-rules.md)
> **Phạm vi:** Cách chọn request nào chạy `MultiOrgMiddleware` trong `Platform.MultiOrg`. Host Sample (`Middlewares:MultiOrg`, `UseMultiOrg()`).
> **Ngoài phạm vi:** Luật resolve, 403, `X-Org-Id` / `X-Tenant-Id`, `OrgFirstRequestScopeResolver`. Những phần đó thuộc D16 của [ADR Orgs](./2026-10-17_platform_orgs-module.md). `UseCoreMiddleware` và `MiddlewareOption` giữ nguyên hợp đồng cho mọi middleware.
> **Chú thích icon:** 🟢 xong · 🟡 đang làm · 🔴 chưa làm

---

## 1. Bối cảnh

`UseMultiOrg()` gọi `UseCoreMiddleware<MultiOrgMiddleware>()`. `UseCoreMiddleware` đã đọc `Middlewares:MultiOrg` (`IsEnable`, `Includes`, `Excludes`) và bọc middleware bằng `UseWhen`.

Bên trong, `MultiOrgMiddleware` còn hỏi `IMultiOrgMiddlewareIgnorePredicate`. Implementation mặc định là `MultiOrgMiddlewareIgnoreState`, fallback `MultiOrgScopePaths.IgnoresScope`. Host gán predicate hoặc danh sách tiền tố lúc gọi `UseMultiOrg(ignore, ignoreStartWithPaths)`.

Sample hôm nay:

- `Middlewares:MultiOrg` có `IsEnable: true`, `Includes: []`, `Excludes` chỉ `/health` và `/swagger`.
- `Program.cs` gọi `UseMultiOrg(ignoreStartWithPaths: ["/hubs/", "/sse/", "/elsa/"])`.

`Includes` rỗng nghĩa là mọi path đều lọt cổng cấu hình (trừ regex `Excludes`). Luật path thật nằm ở predicate.

`ignoreStartWithPaths` có nghĩa ngược tên gọi. Path bắt đầu bằng một tiền tố trong danh sách thì **bắt buộc** working org (`ShouldIgnore` trả `false`). Chỉ `/api/…` và các tiền tố đó vào resolver. SPA, static, path rỗng thì bỏ qua.

Trong nhóm `/api/`, `IsScopeExemptVersionedApi` miễn tail sau `/api/v{số}/`:

- bắt đầu bằng `authentication`
- bắt đầu bằng `system/install`
- bắt đầu bằng `platform/`
- chứa `/account/login` hoặc `/account/catalog`

`/api/v1/account/login` vẫn bị bắt scope vì tail là `account/login`, không chứa chuỗi `/account/login`. Path `/api/…` không có segment version cũng bị bắt scope.

## 2. Vấn đề

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| P1 | Hai cổng lọc path: `Middlewares:MultiOrg` và predicate trong middleware | `Excludes` trên JSON không tắt luật `MultiOrgScopePaths`. Host sửa config mà hành vi không đổi |
| P2 | `ignoreStartWithPaths` là danh sách path **cần** scope, gắn bằng tham số C# | Đổi `/hubs/`, `/sse/`, `/elsa/` phải sửa `Program.cs` |
| P3 | Miễn trừ API versioned nằm trong code (`authentication`, `system/install`, `platform/`, `account/login`, `account/catalog`) | Host không khai báo được bộ miễn trừ theo môi trường |

## 3. Ràng buộc

| # | Ràng buộc |
|---|-----------|
| C1 | `Includes` rỗng vẫn nghĩa là mọi path được xét, đúng `UseCoreMiddleware`. ADR này không đổi `MiddlewareOption` hay thứ tự Includes → Excludes → `IgnoreMiddlewareAttribute` |
| C2 | Bảng resolve D16 (header, 403, validate org ∈ tenant) của [ADR Orgs](./2026-10-17_platform_orgs-module.md) giữ nguyên |
| C3 | So khớp regex giữ `RegexOptions.IgnoreCase` và `RegexOptions.CultureInvariant` như `UseCoreMiddleware` đang làm |
| C4 | Một quyết định: request nào được phép vào `MultiOrgMiddleware`. Request đã vào thì middleware luôn resolve scope |

## 4. Phương án đã cân nhắc

| # | Phương án | Tóm tắt | Ưu | Nhược |
|---|-----------|---------|----|-------|
| O1 | Giữ predicate, JSON chỉ thêm `Excludes` | Hai cổng song song | Ít đụng API `UseMultiOrg` | P1 còn. `Includes: []` vẫn không diễn tả luật path |
| O2 | Chỉ `Middlewares:MultiOrg` | Xóa predicate, `MultiOrgScopePaths`, tham số `UseMultiOrg`. Host khai báo `Includes` / `Excludes` | Một cổng, cùng khuôn middleware khác | Breaking API public. `Includes: []` sẽ bắt scope mọi request |
| O3 | `MultiOrgScopePaths` làm mặc định khi `Includes` rỗng | Giữ hành vi cũ nếu host quên JSON | An toàn khi thiếu config | Đụng C1: `Includes` rỗng đang nghĩa là mọi path |

Chọn **O2**.

## 5. Quyết định

Bộ lọc path của working org là section `Middlewares:MultiOrg`. Predicate code bị gỡ.

| # | Quyết định | Chi tiết |
|---|------------|----------|
| D1 | Một cổng | Xóa `IMultiOrgMiddlewareIgnorePredicate`, `MultiOrgMiddlewareIgnoreState`, `MultiOrgScopePaths`. `UseMultiOrg()` không còn `ignore` và `ignoreStartWithPaths`. Thân chỉ gọi `UseCoreMiddleware<MultiOrgMiddleware>()`. Middleware đã chạy thì luôn gọi `IRequestScopeResolver` |
| D2 | Options sẵn có | Dùng `MiddlewareOption` (`IsEnable`, `Includes`, `Excludes`). Không thêm options class. Section `Middlewares:MultiOrg` vì `UseCoreMiddleware` bỏ hậu tố `Middleware` của `MultiOrgMiddleware` |
| D3 | `Includes` = path cần scope | Tiền tố đang truyền qua `ignoreStartWithPaths` chuyển sang `Includes`. Sample: `^/api/`, `^/hubs/`, `^/sse/`, `^/elsa/` |
| D4 | `Excludes` = miễn trừ versioned | Giữ đúng `IsScopeExemptVersionedApi`. `/health` và `/swagger` không cần `Excludes` vì không nằm trong `Includes` |
| D5 | Resolve không đổi | 403, header, `BeginScope` giữ nguyên. ADR này chỉ thay câu mô tả route trong §5.1.1 của [ADR Orgs](./2026-10-17_platform_orgs-module.md) |

### 5.1 Cấu hình Sample (khi implement)

```json
"MultiOrg": {
  "IsEnable": true,
  "Includes": [
    "^/api/",
    "^/hubs/",
    "^/sse/",
    "^/elsa/"
  ],
  "Excludes": [
    "^/api/v\\d+(?:\\.\\d+)?/authentication",
    "^/api/v\\d+(?:\\.\\d+)?/system/install",
    "^/api/v\\d+(?:\\.\\d+)?/platform/",
    "^/api/v\\d+(?:\\.\\d+)?/.*/account/login",
    "^/api/v\\d+(?:\\.\\d+)?/.*/account/catalog"
  ]
}
```

`IsEnable: false` thì `UseCoreMiddleware` không gắn middleware.

`/api/v1/foo/account/login` khớp `Excludes` nên không resolve. `/api/v1/account/login` không khớp vì thiếu `/` trước `account`, nên vẫn resolve. `/api/authentication` không có `v{số}` cũng vẫn resolve.

### 5.2 Inventory khi implement

```text
Xóa
  frameworks/Platform.MultiOrg/Middleware/IMultiOrgMiddlewareIgnorePredicate.cs
  frameworks/Platform.MultiOrg/Middleware/MultiOrgMiddlewareIgnoreState.cs
  frameworks/Platform.MultiOrg/MultiOrgScopePaths.cs

Sửa
  frameworks/Platform.MultiOrg/Middleware/MultiOrgMiddleware.cs
    bỏ IMultiOrgMiddlewareIgnorePredicate và nhánh ShouldIgnore
  frameworks/Platform.MultiOrg/HostApplicationBuilderExtension.cs
    bỏ hai TryAddSingleton của ignore state trong AddOrgFirstScopeResolution
  frameworks/Platform.MultiOrg/HostApplicationBuilderWebExtensions.cs
    UseMultiOrg() không tham số
  Sample/Program.cs
    app.UseMultiOrg()
  Sample/appsettings.json
    Middlewares:MultiOrg theo §5.1
  docs/guides/multitenancy/request-scope.md
    path do host khai báo ở Middlewares:MultiOrg
```

Caller `UseMultiOrg` trong repo chỉ có `Sample/Program.cs`.

## 6. Hệ quả

| Hướng | Hệ quả |
|-------|--------|
| Tốt | Host đổi path cần scope và path miễn trừ trong `appsettings`, cùng khuôn `ApiResponseWrapper` và enrichment |
| Xấu / chi phí | Breaking API `UseMultiOrg(Func<HttpContext, bool>?, IReadOnlyList<string>?)` và các type predicate / `MultiOrgScopePaths`. Gỡ predicate trong khi `Includes` vẫn `[]` sẽ bắt scope cả SPA và login |
| Trung lập | Request lọt cổng vẫn 403 khi thiếu hoặc sai scope, đúng D16 |

Cập nhật JSON Sample đi cùng lúc xóa predicate.

## 7. Confirm

| # | Câu hỏi | Trả lời |
|---|---------|---------|
| Q1 | Accept D1–D5 (phương án O2)? | OK (2026-11-10) |

Sau khi chốt: trạng thái chuyển 🟡 Accepted, ghi ngày Accept. Code chỉ bắt đầu từ phase 1.

## 8. Test cases

Path filter nằm ở `UnitTest/MultiOrg/MultiOrgPathFilterTests`. Resolver D16 nằm ở `OrgFirstScopeResolverTests`.

### 8.1 Smoke

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T1 | Request path không nằm trong `Includes` (`/health`, `/`) khi `IsEnable: true` | Middleware không chạy; không 403 vì thiếu org | `MultiOrgPathFilterTests.Sample_rules_run_resolver_only_for_scoped_paths` | 🟢 |

### 8.2 Regression

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T2 | Bốn rule header của D16 | Giữ kết quả hiện tại | `UnitTest/MultiOrg/OrgFirstScopeResolverTests` (`Rule1_No_Headers_Forbidden_Even_With_Tenant_Domain`, `Rule2_Org_Header_Tenant_From_Domain`, `Rule3_Tenant_Header_Without_Org_Forbidden`, `Rule4_Both_Headers`, `Validation_Failure_Forbidden`) | 🟢 |
| T3 | `/api/v1/orders` không thuộc `Excludes` | Vào resolver (403 với resolver cấm) | `MultiOrgPathFilterTests.Sample_rules_run_resolver_only_for_scoped_paths` | 🟢 |
| T4 | `/hubs/…`, `/sse/…`, `/elsa/…` với `Includes` của Sample | Vào resolver | Cùng test T3 | 🟢 |

### 8.3 Test mới

| # | Case | Expect | Nơi | Trạng thái |
|---|------|--------|-----|------------|
| T5 | `/api/v1/authentication…`, `/api/v1/system/install…`, `/api/v1/platform/…`, `/api/v1/foo/account/login`, `/api/v1/foo/account/catalog` | Không resolve scope | `MultiOrgPathFilterTests.Sample_rules_run_resolver_only_for_scoped_paths` | 🟢 |
| T6 | `/api/v1/account/login` và `/api/authentication` (không version) | Vẫn resolve scope | Cùng test T5 | 🟢 |

### 8.4 Done khi

- T1, T2, T3, T4, T5, T6 đều 🟢.
- Chưa 🟢 Implemented khi T1 và các case path còn 🔴.

## 9. Kế hoạch triển khai

| Phase | Việc | Done khi | Phụ thuộc |
|-------|------|----------|-----------|
| 0 | Confirm §7 | Trạng thái → Accepted | — |
| 1 | Xóa predicate và `MultiOrgScopePaths`; `UseMultiOrg()` không tham số; Sample `Program.cs` + `appsettings.json` theo §5.1 cùng một thay đổi | Build xanh; `Includes` Sample không còn `[]` | 0 |
| 2 | Sửa `docs/guides/multitenancy/request-scope.md`. Thêm test §8. Giữ xanh `OrgFirstScopeResolverTests` | T1–T6 🟢 | 1 |

Phase 0–2 xong 2026-11-10. Cùng ngày bỏ wrapper `UseMultiOrg`: Sample gọi `app.UseCoreMiddleware<MultiOrgMiddleware>()`. `dotnet test UnitTest --filter FullyQualifiedName~UnitTest.MultiOrg`: 25 passed.

## 10. Checklist Done

| # | Việc | Trạng thái |
|---|------|------------|
| 1 | ADR này + dòng trên [README index](./README.md) | 🟢 |
| 2 | Câu trỏ từ §5.1.1 [ADR Orgs](./2026-10-17_platform_orgs-module.md); bảng resolve D16 không sửa | 🟢 |
| 3 | Phase 1 — gỡ predicate, sửa Sample | 🟢 |
| 4 | Phase 2 — guide + §8 | 🟢 |

## 11. Tham chiếu thêm

- `frameworks/Platform.Mvc/ExceptionHandling/ApplicationBuilderExtension.cs` — `UseCoreMiddleware`
- `frameworks/Platform.Mvc/ExceptionHandling/MiddlewareOption.cs`
- `UnitTest/MultiOrg/MultiOrgPathFilterTests.cs` — T1, T3–T6
- `docs/guides/multitenancy/request-scope.md` — path theo `Middlewares:MultiOrg`
