# Platform Framework

## Giới thiệu

Platform là framework xây dựng backend ASP.NET Core trên nền tảng .NET 9, kết hợp **Clean Architecture** (phân lớp trong solution) và **module Atomic** (gói chức năng độc lập). Mục tiêu của Platform là giúp đội ngũ phát triển có thể khởi tạo một solution backend chuẩn chỉnh từ một thư mục trống, chỉ với vài thao tác, và sẵn sàng chạy Swagger ngay lập tức.

Platform tách hai trục bổ sung cho nhau — không thay thế lẫn nhau:

```text
  Chiều dọc (Clean Architecture)     Chiều ngang (Atomic modules)
  ─────────────────────────────        ─────────────────────────────
  Host                                 Foundation · Caching · EF · Blob · Auth …
    ↑                                      ↓ mỗi module cắm vào layer phù hợp
  Infrastructure  ←──────────────── adapters, DI extension
    ↑
  Application
    ↑
  Domain
    ↑
  Domain.Shared
```

### Clean Architecture

Mỗi project trong solution có **một trách nhiệm** và **một hướng phụ thuộc**. Domain không biết EF hay Redis; Application chỉ biết interface; Infrastructure cài đặt adapter; Host là composition root gắn mọi thứ lại.

| Nguyên tắc | Ý nghĩa trong Platform |
|------------|----------------------|
| Phụ thuộc một chiều | Host → Infrastructure / Application → Domain → Domain.Shared |
| Domain thuần | Entity, repository interface, event — không reference framework |
| Use case tách biệt | Command / query / handler nằm Application |
| Adapter ở rìa | EF, cache, blob, email, auth scheme đăng ký Infrastructure hoặc Host |
| Composition root mỏng | `AddHostLayer()` / `UseHostLayer()` — logic nằm Layer Extension |

Clean Architecture trả lời: *“File và project này đặt ở đâu?”*

### Module Atomic

Mỗi capability Platform là **một module nhỏ, hoàn chỉnh**, có thể bật/tắt mà không kéo theo phần còn lại của framework:

| Nguyên tắc | Ý nghĩa trong Platform |
|------------|----------------------|
| Một concern, một package | `Platform.Caching`, `Platform.HealthChecks`, `Platform.BlobStoring` — mỗi gói một việc |
| Core + provider vệ tinh | Abstraction trong core; biến thể trong package con (vd. `Platform.Caching.Redis`, `Platform.BlobStoring.MinIO`, `Platform.Authentications.Jwt`) |
| Chỉ cài cái cần | Solution production không bắt buộc reference toàn bộ repo Platform |
| Đăng ký qua extension | `AddCoreBlobStoring()`, `AddPlatformCaching()`, `UseMinIO()` — không sửa source framework |
| Không phụ thuộc vòng giữa module | Module A không reference module B nếu B không thuộc bounded context của A |
| Skill OpenCode song song | Skill `*-dotnet` = orchestrator; `providers/` hoặc `patterns/` = biến thể atomic — agent chỉ load file cần task |

Atomic trả lời: *“Tôi cần thêm Redis cache hay chỉ memory? JWT hay API Key? FileSystem hay MinIO?”*

### Hai trục cùng làm việc

Ví dụ **lưu file đính kèm**:

- **Clean:** interface / use case ở Application; `IBlobStoringService` inject từ Infrastructure hoặc Host.
- **Atomic:** reference `Platform.BlobStoring` (FileSystem built-in); thêm `Platform.BlobStoring.MinIO` chỉ khi triển khai object storage — không đổi cấu trúc 5 layer.

Ví dụ **cache connection string tenant**:

- **Clean:** resolver interface gần Domain/Application; implementation Infrastructure.
- **Atomic:** `Platform.Caching` trước `Platform.Multitenancy.EntityFramework` — thứ tự DI document trong skill; ambient tenant ở `Platform.Multitenancy`, không nhét CRUD tenant vào Multitenancy core.

Khi scaffold hoặc add module, luôn giữ **layer đúng chỗ** và **package đúng mức** — đó là contract Platform với team và với AI agent (skill `platform-dotnet` + skill chuyên sâu).

## Cấu trúc repository

Repo Platform là monorepo: **framework packages**, **domain modules**, **Sample host**, và **UnitTest**.

```text
platform/
├── Platform.sln
├── frameworks/                    ← Package hạ tầng (NuGet Platform.*)
│   ├── Platform.Mvc / .Swashbuckle / .HealthChecks / .OpenTelemetry(.DDD)
│   ├── Platform.DDD.*             ← Domain / Application contracts + CQRS
│   ├── Platform.Multitenancy / .Multitenancy.EntityFramework
│   ├── Platform.ORM.EntityFramework / .ORM.Dapper / .Caching(.Redis)
│   ├── Platform.Authentication.*  ← Project folder; PackageId = Platform.Authentications.* (gồm .Identity: store User/Role)
│   ├── Platform.Authorization / .Authorization.EntityFramework  ← phân quyền RBAC+ABAC (core không EF) / store EF
│   ├── Platform.BlobStoring.* / .Notification.*
│   ├── Platform.Common
│   └── frontend/                  ← React UI-kit @platform/core (PrimeReact)
├── modules/                       ← Module nghiệp vụ / platform admin
│   ├── settings/                  ← Platform.Modules.Setting (+ EF, API, frontend)
│   ├── identity/                  ← Platform.Modules.Account + Platform.Modules.IdentityAdmin
│   ├── tenants/ · files/ · messaging/ · notifications/ · workflows/
│   └── importexport/              ← placeholder (chưa có project trong solution)
├── Sample/                        ← Host demo: API + SPA (clients/web → wwwroot)
├── UnitTest/
├── ADRs/                          ← Architecture Decision Records — [ADRs/README.md](ADRs/README.md)
└── ai-skills/                     ← Pipeline AI SDLC (submodule nội bộ)
```

| Thư mục | Vai trò |
|---------|---------|
| `frameworks/` | Thư viện hạ tầng Atomic — publish NuGet, reference từ Host/Infrastructure product |
| `modules/` | Module domain (Setting, Account, IdentityAdmin…); mỗi folder một bounded context + optional frontend |
| `Sample/` | App chạy thử: framework + Setting; SPA Vite/React. Chưa compose Authorization / Account / IdentityAdmin (hiện chỉ `Lexora.Host` dùng) |
| `UnitTest/` | Unit / integration test cho framework và module |
| `ADRs/` | Quyết định kiến trúc & tiến độ — [ADRs/README.md](ADRs/README.md) |
| `ai-skills/` | Skill / hook AI SDLC (submodule) — [ai-skills/README.md](ai-skills/README.md) |

**Lưu ý đặt tên:** thư mục project `frameworks/Platform.Authentication.*` → NuGet **`Platform.Authentications.*`** (có chữ **s**). Monorepo dùng `ProjectReference` tới path `frameworks\...`; product bên ngoài dùng NuGet feed.

## Architecture

Một solution product tiêu chuẩn gồm năm lớp, xếp chồng theo thứ tự phụ thuộc đi lên:

```text
    Host                  ← Composition root, controllers, middleware
      ↑
    Infrastructure        ← EF Core, repository implementation, adapter
      ↑
    Application           ← Command, query, handler, DTO
      ↑
    Domain                ← Entity, repository interface, domain events
      ↑
    Domain.Shared         ← Enum, hằng số dùng chung
```

- **Domain.Shared** — tầng thấp nhất, chứa enum, hằng số và kiểu dùng chung cho toàn bộ solution.
- **Domain** — tầng thuần túy với entity, repository interface, domain events; không phụ thuộc bất kỳ framework nào.
- **Application** — tầng tổ chức use case: command, query, handler, DTO; phụ thuộc Domain.
- **Infrastructure** — tầng adapter: cài đặt repository, EF Core DbContext, kết nối hạ tầng bên ngoài; phụ thuộc Domain.
- **Host** — tầng trên cùng, là composition root: chứa Program.cs, controllers, middleware pipeline; phụ thuộc tất cả các tầng bên dưới.

Luồng dependency chỉ đi ***một chiều*** từ trên xuống. Host biết Infrastructure và Application, Infrastructure biết Domain, Application biết Domain, Domain biết Domain.Shared. **Không có phụ thuộc vòng hay đi ngược.**

Platform dùng **Layer Extension**: mỗi tầng có một tệp mở rộng riêng (HostLayerExtension, ApplicationLayerExtension...). Program.cs chỉ cần gọi ***đúng hai dòng***: một đăng ký toàn bộ dịch vụ, một cấu hình pipeline:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddHostLayer();

var app = builder.Build();
app.UseHostLayer();

app.Run();
```

Mọi phức tạp của dependency injection được ẩn sau các extension method, giúp Program.cs ***luôn mỏng và dễ đọc.***

Chi tiết khung scaffold: [.opencode/skills/platform-dotnet/reference/solution-structure.md](.opencode/skills/platform-dotnet/reference/solution-structure.md).

## Modules

### Framework (`frameworks/`)

Chiều **Atomic** — mỗi package một concern, bật/tắt độc lập:

| Module | Package chính | Phạm vi |
|--------|---------------|---------|
| Foundation | `Platform.Mvc`, `Platform.Common` | JSON, CORS, WebApi, SPA (`UseCoreSpa`), middleware response chuẩn |
| Application | `Platform.DDD.Application`, `.Contracts` | CQRS command/query dispatcher |
| Domain | `Platform.DDD.Domain`, `.Domain.Shared` | Contract entity, repository, enum/shared |
| Multitenancy | `Platform.Multitenancy` | Ambient `ICurrentTenant` / `ICurrentTenant<TTenant>`, `AddCurrentTenant` |
| Multitenancy EF | `Platform.Multitenancy.EntityFramework` | Opt-in connection resolver + tenant interceptor (`AddMultitenancyEntityFramework`) |
| ORM (EF) | `Platform.ORM.EntityFramework` | DbContext, UoW, repository (single / dedicated / hybrid DB); DI `AddEntityFramework()` |
| ORM (Dapper) | `Platform.ORM.Dapper` | Connection factory cho read/report; DI `AddOrmDapper()` |
| Caching | `Platform.Caching`, `.Redis` | Memory + Redis, pub/sub invalidation |
| Authentication (xác thực) | `Platform.Authentications`, `.Jwt`, `.ApiKey`, `.Basic`, `.Cognito` | JWT, API Key, Basic, Cognito |
| Identity store (danh tính) | `Platform.Authentications.Identity` | Entity `User` (cột `TenantId`/`OrgId`) + `Role` (`RoleLevel`, `IsSystemRole`) cho ASP.NET Identity — dùng chung Account / IdentityAdmin / Authorization |
| Authorization (phân quyền) | `Platform.Authorization`, `.EntityFramework` | Core (không EF): model Permission/Policy/PolicyRule, engine RBAC + ABAC, abstraction Role Governance — DI `AddPlatformAuthorization()`. Store EF: provider, `IdentityDbContextBase`, seeder — DI `AddPlatformAuthorizationEntityFramework()`. Data scope & snapshot cache chưa xong |
| Blob Storing | `Platform.BlobStoring`, `.MinIO`, `.AwsS3` | FileSystem (core), MinIO, AWS S3 |
| Notification | `Platform.Notification`, `.Mailkit` | Email SMTP (Mailkit) |
| Swashbuckle | `Platform.Swashbuckle` | Swagger đa phiên bản, security schemes |
| OpenTelemetry | `Platform.OpenTelemetry` | Trace, metric, log OTLP |
| OpenTelemetry DDD bridge | `Platform.OpenTelemetry.DDD` | Enrich user/tenant từ `ICurrentUser` / `ICurrentTenant` |
| Health Checks | `Platform.HealthChecks` | Liveness, readiness, startup |
| Frontend UI-kit | `@platform/core` (`frameworks/frontend`) | React + PrimeReact — layout admin, form, CRUD helpers |

### Domain modules (`modules/`)

Module nghiệp vụ / portal quản trị — tách khỏi framework hạ tầng:

| Module | Trạng thái | Phạm vi |
|--------|------------|---------|
| **settings** | ✅ | `Platform.Modules.Setting` (+ `.EntityFramework`, `.API`); quản lý cấu hình theo Group/Key; frontend demo |
| **identity** | 🟡 | `Platform.Modules.Account` — login, logout, profile, đổi/quên mật khẩu (AuthN) · `Platform.Modules.IdentityAdmin` — CRUD user/role, gán permission/policy + Role Governance (AuthZ admin). Handler + test xong; chưa có API. User store nằm ở `Platform.Authentication.Identity` |
| **tenants** | 📋 | Catalog/CRUD tenant ([ADR](ADRs/2026-08-07_platform_tenants-module.md)); ambient nằm `Platform.Multitenancy` |
| **files** | 📋 | Blob browser / quản lý file — placeholder |
| **messaging** | 📋 | Event bus / realtime — placeholder |
| **notifications** | 📋 | Đa kênh notification — placeholder |
| **workflows** | 📋 | Quy trình / background orchestration — placeholder |
| **importexport** | 📋 | Import/export dữ liệu — placeholder (folder, chưa trong solution) |

Chi tiết Setting: [modules/settings/README.md](modules/settings/README.md). Authorization: [ADRs/2026-08-14_platform_rbac-abac-authorization.md](ADRs/2026-08-14_platform_rbac-abac-authorization.md).

## Get started

### 0. Chạy Sample trong monorepo

```bash
dotnet run --project Sample
```

- Swagger: theo `launchSettings` (thường `https://localhost:7006/swagger`)
- SPA Sample: `Sample/clients/web` (Vite) — build vào `Sample/wwwroot`, serve cùng Sample
- UI Setting demo: `modules/settings/frontend` — xem [modules/settings/frontend/README.md](modules/settings/frontend/README.md)

### 1. Chọn cách bắt đầu (product mới)

Platform cung cấp bốn cách tiếp cận tuỳ theo tình trạng dự án:

**Cách 1 — Scaffold (khuyến khích)** — Từ folder trống, nhờ AI chạy skill `platform-dotnet`:

> "@.opencode/skills/platform-dotnet/workflows/scaffold.md — Tạo solution backend .NET 9 tên MyApp từ folder trống."

AI tạo cây thư mục, solution, 5 project, references, Platform packages, templates layer extension, Swagger + health checks. Sau scaffold: `dotnet run --project src/MyApp.Host` và mở Swagger.

**Cách 2 — Add** — Đã có solution foundation, muốn thêm module:

> "@.opencode/skills/platform-dotnet/workflows/add.md — Thêm xác thực JWT vào project MyApp.Host, dùng Platform.Authentications.Jwt."

**Cách 3 — Init** — Đã có solution .NET, muốn cài Platform:

> "@.opencode/skills/platform-dotnet/workflows/init.md — Cài Platform vào solution MyApp. Infrastructure: Platform.ORM.EntityFramework + Platform.Caching (trước EF)."

**Cách 4 — Manual** — Tự tạo project và cài package:

```bash
# Tạo solution và các project
dotnet new sln -n MyApp
dotnet new webapi -n MyApp.Host -f net9.0 --use-controllers
dotnet new classlib -n MyApp.Application -f net9.0
dotnet new classlib -n MyApp.Infrastructure -f net9.0
dotnet new classlib -n MyApp.Domain -f net9.0
dotnet new classlib -n MyApp.Domain.Shared -f net9.0

# Thêm project references
dotnet add MyApp.Domain reference MyApp.Domain.Shared
dotnet add MyApp.Application reference MyApp.Domain
dotnet add MyApp.Infrastructure reference MyApp.Domain
dotnet add MyApp.Host reference MyApp.Application MyApp.Infrastructure

# Cài Platform packages (NuGet) — hoặc ProjectReference tới frameworks\... trong monorepo
dotnet add MyApp.Host package Platform.Mvc
dotnet add MyApp.Host package Platform.Swashbuckle
dotnet add MyApp.Host package Platform.HealthChecks
dotnet add MyApp.Host package Platform.OpenTelemetry
dotnet add MyApp.Application package Platform.DDD.Application
dotnet add MyApp.Infrastructure package Platform.ORM.EntityFramework
dotnet add MyApp.Infrastructure package Platform.ORM.Dapper
dotnet add MyApp.Infrastructure package Platform.Caching
```

### 2. Danh sách NuGet packages

Mỗi module framework đều có sẵn dưới dạng NuGet (hoặc `ProjectReference` trong monorepo). Chỉ thêm package đúng tầng cần dùng:

| Tầng | Package | Version |
|---|---|---|
| Domain.Shared | `Platform.DDD.Domain.Shared` | 1.0.x |
| Domain | `Platform.DDD.Domain` | 1.1.x |
| Application | `Platform.DDD.Application` | 1.2.x |
| Application | `Platform.DDD.Application.Contracts` | 1.2.x |
| Infrastructure | `Platform.ORM.EntityFramework` | 1.0.x |
| Infrastructure | `Platform.ORM.Dapper` | 1.0.x |
| Infrastructure / Host | `Platform.Multitenancy` | 1.0.x |
| Infrastructure | `Platform.Multitenancy.EntityFramework` | 1.0.x |
| Infrastructure | `Platform.Caching` | 1.1.x |
| Infrastructure | `Platform.Caching.Redis` | 1.1.x |
| Infrastructure | `Platform.BlobStoring` | 1.0.x |
| Infrastructure | `Platform.BlobStoring.MinIO` | 1.0.x |
| Infrastructure | `Platform.BlobStoring.AwsS3` | 1.0.x |
| Infrastructure | `Platform.Notification` | 1.0.x |
| Infrastructure | `Platform.Notification.Mailkit` | 1.0.x |
| Infrastructure / Domain module | `Platform.Modules.Setting` | 1.0.x |
| Host | `Platform.Mvc` | 1.1.x |
| Host | `Platform.Common` | 1.0.x |
| Host | `Platform.Swashbuckle` | 1.0.1 |
| Host | `Platform.HealthChecks` | 1.0.0 |
| Host | `Platform.OpenTelemetry` | 1.0.1 |
| Host | `Platform.OpenTelemetry.DDD` | 1.0.0 |
| Host | `Platform.Authentications` | 1.0.1 |
| Host | `Platform.Authentications.Jwt` | 1.0.1 |
| Host | `Platform.Authentications.ApiKey` | 1.0.1 |
| Host | `Platform.Authentications.Basic` | 1.0.1 |
| Host | `Platform.Authentications.Cognito` | 1.0.1 |
| Frontend (npm) | `@platform/core` | `frameworks/frontend` |

> Mẹo: Chỉ thêm package đúng tầng cần dùng để giảm dependency surface. Xem phiên bản mới nhất trên feed NuGet nội bộ. Monorepo: `ProjectReference` tới `frameworks\Platform.*\*.csproj` hoặc `modules\settings\Platform.Modules.Setting\*.csproj`.

### 3. Kết quả

Sau khi scaffold / chạy Sample:

- Swagger UI mở tại host (vd. `https://localhost:7006/swagger`)
- `GET /api/ping` trả về 200 OK — không cần cơ sở dữ liệu (nếu đã map controller ping)
- `GET /health/live` trả về 200 OK — liveness (khi bật HealthChecks)
- `GET /health/ready` trả về 200 OK (nếu đã cấu hình PostgreSQL / readiness)
- Program.cs mỏng; logic DI nằm Layer Extension / module extension
- Sample: SPA + `UseCoreSpa()` phục vụ `wwwroot`

## Roadmap

Lộ trình phát triển Platform framework (.NET 9). Mỗi hạng mục là **cam kết chức năng** sẽ triển khai; tiến độ quyết định kiến trúc và trạng thái ADR: [ADRs/README.md](ADRs/README.md).

**Chú thích trạng thái**

| Ký hiệu | Ý nghĩa |
|---------|---------|
| ✅ | Đã có trên nhánh hiện tại (package + code / test) — ADR 🟢 khi có |
| 🟡 | Đang làm / accepted một phần — còn việc (xem ADR) |
| 📋 | Kế hoạch / ADR 🔴 Proposed — chưa phát hành đầy đủ |

**Thứ tự triển khai gợi ý**

```text
Nền tảng bảo mật & admin → Dữ liệu & multitenancy → Kiến trúc & messaging
→ Tích hợp (notification, blob, realtime) → Mở rộng (plug-in, chất lượng)
```

**Ưu tiên ADR đang mở** (chi tiết [ADRs/README.md](ADRs/README.md)):

1. 📋 Base CRUD AppService (`ICrudAppService` / `CrudAppService`)
2. 📋 FluentValidation — validate input Application ([ADR](ADRs/2026-08-07_platform_fluentvalidation.md))
3. 📋 Split Phase 6 — document Host (`AddCurrentUser` + `AddCurrentTenant`)
4. 📋 `Platform.Tenants` — module catalog/CRUD (khi có nhu cầu sản phẩm)
5. 📋 Later — thu hồi token khi đổi quyền ([ADR](ADRs/2026-08-21_platform_authorization-token-revocation.md))

---

### A. Nền tảng hiện có (baseline)

| Thành phần | Phạm vi | Vị trí / ADR |
|------------|---------|--------------|
| ✅ Foundation | JSON, CORS, WebApi, SPA, ApiResponseWrapper | `frameworks/Platform.Mvc` |
| ✅ Application | CQRS command/query dispatcher | `frameworks/Platform.DDD.Application` |
| ✅ Multitenancy | `ICurrentTenant` / `<TTenant>`, `AddCurrentTenant` | `frameworks/Platform.Multitenancy` · [ADR](ADRs/2026-08-06_platform_multitenancy-package.md) |
| ✅ Multitenancy EF | Connection resolver + interceptor (opt-in) | `frameworks/Platform.Multitenancy.EntityFramework` · [ADR](ADRs/2026-08-06_platform_multitenancy-entityframework.md) |
| ✅ Entity Framework | UoW, repository, hybrid DB | `frameworks/Platform.ORM.EntityFramework` · [ADR](ADRs/2026-08-07_platform_orm-packages.md) |
| ✅ ORM (Dapper) | Connection factory cho read/report | `frameworks/Platform.ORM.Dapper` · [ADR](ADRs/2026-08-07_platform_orm-packages.md) |
| ✅ Caching | Memory + Redis, invalidation pub/sub | `frameworks/Platform.Caching` · [plan](ADRs/refactor-cache-plan.md) 🟢 |
| 🟡 Authentication | JWT, API Key, Basic ✅; Cognito stub; OIDC chưa | `frameworks/Platform.Authentication.*` · [refactor](ADRs/refactor-authentication.md) |
| 🟡 Authorization | RBAC+ABAC engine + Role Governance ✅; data scope, snapshot cache, policy admin API, tích hợp endpoint chưa | `frameworks/Platform.Authorization` (+ `.EntityFramework`) · [ADR](ADRs/2026-08-14_platform_rbac-abac-authorization.md) |
| 🟡 Blob storing | FileSystem, MinIO, AWS S3 — đang review | `frameworks/Platform.BlobStoring.*` · [refactor](ADRs/refactor-blob-storing.md) |
| ✅ Notification | SMTP (Mailkit) | `frameworks/Platform.Notification.Mailkit` |
| ✅ Swashbuckle | Swagger đa version, security schemes | `frameworks/Platform.Swashbuckle` |
| ✅ OpenTelemetry | Trace, metric, log OTLP | `frameworks/Platform.OpenTelemetry` |
| ✅ OpenTelemetry DDD | Enrich user/tenant khỏi Domain | `frameworks/Platform.OpenTelemetry.DDD` · [ADR](ADRs/2026-08-01_platform_techdebt-otel-enrichment-out-of-ddd.md) |
| ✅ Health checks | Liveness, readiness, startup | `frameworks/Platform.HealthChecks` |
| ✅ Setting | Group/Key, cache, encrypt at rest | `modules/settings` |
| ✅ Frontend UI-kit | `@platform/core` (PrimeReact) | `frameworks/frontend` |

---

### B. Bảo mật, identity & quản trị nền

#### B.1 Authentication (xác thực)

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | JWT Bearer | `Platform.Authentications.Jwt`, tích hợp Swagger |
| ✅ | API Key | Header tùy chỉnh, `IApiKeyProvider` |
| ✅ | Basic | `Platform.Authentications.Basic` |
| 🟡 | Cognito | Package stub — chưa đủ production ([refactor](ADRs/refactor-authentication.md)) |
| 📋 | OIDC | OpenID Connect / OpenIddict (Azure AD, Keycloak, …); map claim → user/tenant |

#### B.2 User & Identity

| | Hạng mục | Mô tả |
|---|----------|--------|
| 🟡 | ASP.NET Core Identity | `Platform.Authentications.Identity` — `User` (`TenantId`/`OrgId` cột) + `Role`; migration Identity ở product (`Lexora.Infrastructure`); lockout áp khi login ✅; refresh token, wire `UserManager`/`SignInManager` trong package 📋 |
| 📋 | Đồng bộ auth | Subject JWT/OIDC ↔ `User` |

#### B.3 Authorization (phân quyền)

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | RBAC | Permission trên RoleClaim, wildcard `*` / `System.All`, per-role path — `Platform.Authorization` |
| 🟡 | ABAC | Policy/PolicyRule, operand resolver (`User.*` theo cột/claim), Role Governance ✅; data scope query filter 📋 |
| 📋 | API & CQRS | `IAuthorizationHandler` / attribute để endpoint gọi engine; Sample chưa dùng engine (chỉ `[Authorize]` ASP.NET thuần) |
| 📋 | Token revocation | Thu hồi JWT khi đổi permission/policy — [ADR Later](ADRs/2026-08-21_platform_authorization-token-revocation.md) |

#### B.4 Platform admin (`modules/*`)

Module service + (sau này) API persistence/UI cho portal quản trị (`/api/admin/*`).

| | Module | Phạm vi |
|---|--------|---------|
| ✅ | **Settings** | `Platform.Modules.Setting` (+ `.EntityFramework`, `.API`) — Group/Key, form types, cache, encrypt; FE demo |
| 🟡 | **Account** | `modules/identity/Platform.Modules.Account` — login (lockout, tenant claim), logout, profile, đổi/quên mật khẩu (AuthN); handler + unit test; chưa API, email thật |
| 🟡 | **IdentityAdmin** | `modules/identity/Platform.Modules.IdentityAdmin` — CRUD user/role, gán permission/policy, Role Governance C5/C6 (AuthZ admin); handler + unit test; chưa API admin |
| 📋 | **Tenants** | Catalog/CRUD tenant — **không** gộp Multitenancy ([ADR](ADRs/2026-08-07_platform_tenants-module.md)) |

---

### C. Dữ liệu & multitenancy

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | Ambient tenant | `Platform.Multitenancy` — `ICurrentTenant` / `<TTenant>`, `Change`, resolvers ([ADR](ADRs/2026-08-06_platform_multitenancy-package.md), [generic](ADRs/2026-08-06_platform_generic-current-tenant.md)) |
| ✅ | Multitenancy EF | `Platform.Multitenancy.EntityFramework` — connection + interceptor opt-in ([ADR](ADRs/2026-08-06_platform_multitenancy-entityframework.md)) |
| ✅ | ORM family | `Platform.ORM.EntityFramework` (`AddEntityFramework`) + `Platform.ORM.Dapper` (`AddOrmDapper`) — Sample dùng cả hai ([ADR](ADRs/2026-08-07_platform_orm-packages.md)) |
| ✅ | Split user/tenant packages | `ICurrentUser` / `ICurrentTenant` khỏi Domain; Auth + Multitenancy ([ADR](ADRs/2026-08-01_platform_techdebt-split-current-user-tenant-packages.md) Phase 2–5) |
| ✅ | Storage tenant boundary | `IStorageContext.TenantId` private — app dùng `ICurrentTenant` ([ADR](ADRs/2026-08-12_platform_storage-context-tenant-id-private.md)) |
| 📋 | Host docs | Phase 6 — document `AddCurrentUser` + `AddCurrentTenant` trên Host |
| 📋 | Tenants module | Catalog/CRUD tenant (pattern Setting) — `modules/tenants` |
| 📋 | Partition & migrate | Tool migrate dữ liệu theo tenant; rollback; dry-run |
| 📋 | Health threshold | Readiness `degraded` khi DB/Redis vượt ngưỡng latency |
| 📋 | Dynamic filter | Grammar `field:op:value`; sort/filter an toàn trên `PagedListRequest` |

---

### D. Kiến trúc ứng dụng & messaging

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | CQRS | Command/query dispatcher |
| 📋 | CRUD AppService | `ICrudAppService` / `CrudAppService` trong `Platform.DDD.Application` ([ADR](ADRs/2026-08-07_platform_crud-app-service.md)) |
| 📋 | FluentValidation | Chuẩn validate input Application; map 400 `BaseResponse` ([ADR](ADRs/2026-08-07_platform_fluentvalidation.md) · [docs](https://docs.fluentvalidation.net/en/latest/)) |
| 📋 | DDD | Aggregate, value object, bounded context; convention scaffold |
| 📋 | Domain & integration events | Domain events, outbox/inbox |
| 📋 | Event bus | `IEventBus`; provider **RabbitMQ**, **Kafka** — `modules/messaging` |
| 📋 | Idempotency | API & consumer idempotent; phối hợp outbox |

---

### E. Tích hợp & trải nghiệm runtime

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | Notification module | Đa kênh (email/SMS/push); relay — `modules/notifications` (SMTP core đã có ở `Platform.Notification.Mailkit`) |
| 📋 | Blob browser | API duyệt file — `modules/files` |
| 📋 | SignalR | Hub chuẩn; JWT; tenant group; Redis backplane — folder `Platform.Realtime.*` placeholder |
| 📋 | Background jobs / workflows | Worker chuẩn; queue — `modules/workflows` |
| 📋 | Import/export | Module import/export — `modules/importexport` placeholder |
| 📋 | Resilience | Polly, circuit breaker; gắn OpenTelemetry |

---

### F. Mở rộng framework & chất lượng

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | Plug-and-play | `IPlatformModule`; module NuGet / nội bộ / third-party; discovery & thứ tự DI |
| 📋 | Options & secrets | `IValidateOptions`; Key Vault / biến môi trường |
| 📋 | OpenAPI contract | Versioning; breaking-change policy; contract test |
| 📋 | Observability ops | Grafana runtime metrics; phân tích .NET runtime metrics |
| 📋 | Rate limit & audit | Giới hạn request; audit log thao tác admin |
| 📋 | NuGet & semver | Phát hành package; matrix .NET; changelog breaking |

---

### G. Phụ thuộc giữa các khối

```text
B.1 Authentication ──► B.4 Account (🟡)
🟡 B.3 Authorization ──► B.4 IdentityAdmin (🟡)
🟡 B.2 Identity    ──► B.4 IdentityAdmin / Account / B.3 Authorization
✅ Multitenancy    ──► C Multitenancy.EF ──► B.4 Tenants (catalog)
✅ C Platform.ORM.* (EF + Dapper foundation)
✅ Settings        ──► Sample / admin UI
D CRUD AppService  ──► B.4 / Sample entity CRUD đơn giản
B.4 Platform admin ──► E Notification (quên mật khẩu)
D Event bus        ──► E SignalR / Notification relay
F Plug-and-play    ──► toàn bộ module Platform.* / modules/*
```

Cập nhật roadmap theo [ADRs/README.md](ADRs/README.md) và khi phát hành phiên bản framework (tag / release note).
