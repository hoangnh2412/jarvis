# Jarvis Framework

## Giới thiệu

Jarvis là framework xây dựng backend ASP.NET Core trên nền tảng .NET 9, kết hợp **Clean Architecture** (phân lớp trong solution) và **module Atomic** (gói chức năng độc lập). Mục tiêu của Jarvis là giúp đội ngũ phát triển có thể khởi tạo một solution backend chuẩn chỉnh từ một thư mục trống, chỉ với vài thao tác, và sẵn sàng chạy Swagger ngay lập tức.

Jarvis tách hai trục bổ sung cho nhau — không thay thế lẫn nhau:

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

| Nguyên tắc | Ý nghĩa trong Jarvis |
|------------|----------------------|
| Phụ thuộc một chiều | Host → Infrastructure / Application → Domain → Domain.Shared |
| Domain thuần | Entity, repository interface, event — không reference framework |
| Use case tách biệt | Command / query / handler nằm Application |
| Adapter ở rìa | EF, cache, blob, email, auth scheme đăng ký Infrastructure hoặc Host |
| Composition root mỏng | `AddHostLayer()` / `UseHostLayer()` — logic nằm Layer Extension |

Clean Architecture trả lời: *“File và project này đặt ở đâu?”*

### Module Atomic

Mỗi capability Jarvis là **một module nhỏ, hoàn chỉnh**, có thể bật/tắt mà không kéo theo phần còn lại của framework:

| Nguyên tắc | Ý nghĩa trong Jarvis |
|------------|----------------------|
| Một concern, một package | `Jarvis.Caching`, `Jarvis.HealthChecks`, `Jarvis.BlobStoring` — mỗi gói một việc |
| Core + provider vệ tinh | Abstraction trong core; biến thể trong package con (vd. `Jarvis.Caching.Redis`, `Jarvis.BlobStoring.MinIO`, `Jarvis.Authentications.Jwt`) |
| Chỉ cài cái cần | Solution production không bắt buộc reference toàn bộ repo Jarvis |
| Đăng ký qua extension | `AddCoreBlobStoring()`, `AddJarvisCaching()`, `UseMinIO()` — không sửa source framework |
| Không phụ thuộc vòng giữa module | Module A không reference module B nếu B không thuộc bounded context của A |
| Skill OpenCode song song | Skill `*-dotnet` = orchestrator; `providers/` hoặc `patterns/` = biến thể atomic — agent chỉ load file cần task |

Atomic trả lời: *“Tôi cần thêm Redis cache hay chỉ memory? JWT hay API Key? FileSystem hay MinIO?”*

### Hai trục cùng làm việc

Ví dụ **lưu file đính kèm**:

- **Clean:** interface / use case ở Application; `IBlobStoringService` inject từ Infrastructure hoặc Host.
- **Atomic:** reference `Jarvis.BlobStoring` (FileSystem built-in); thêm `Jarvis.BlobStoring.MinIO` chỉ khi triển khai object storage — không đổi cấu trúc 5 layer.

Ví dụ **cache connection string tenant**:

- **Clean:** resolver interface gần Domain/Application; implementation Infrastructure.
- **Atomic:** `Jarvis.Caching` trước `Jarvis.Multitenancy.EntityFramework` — thứ tự DI document trong skill; ambient tenant ở `Jarvis.Multitenancy`, không nhét CRUD tenant vào Multitenancy core.

Khi scaffold hoặc add module, luôn giữ **layer đúng chỗ** và **package đúng mức** — đó là contract Jarvis với team và với AI agent (skill `jarvis-dotnet` + skill chuyên sâu).

## Cấu trúc repository

Repo Jarvis là monorepo: **framework packages**, **domain modules**, **Sample host**, **autotest**, và **skill AI**.

```text
jarvis/
├── Jarvis.sln
├── frameworks/                    ← Package hạ tầng (NuGet Jarvis.*)
│   ├── Jarvis.Mvc / .Swashbuckle / .HealthChecks / .OpenTelemetry(.DDD)
│   ├── Jarvis.DDD.*             ← Domain / Application contracts + CQRS
│   ├── Jarvis.Multitenancy / .Multitenancy.EntityFramework
│   ├── Jarvis.ORM.EntityFramework / .ORM.Dapper / .Caching(.Redis)
│   ├── Jarvis.Authentication.*  ← Project folder; PackageId = Jarvis.Authentications.*
│   ├── Jarvis.BlobStoring.* / .Notification.*
│   ├── Jarvis.Common
│   └── frontend/                  ← React UI-kit @jarvis/core (PrimeReact)
├── modules/                       ← Module nghiệp vụ / jarvis admin
│   ├── settings/                  ← Jarvis.Modules.Setting (+ EF, Redis, frontend demo)
│   ├── accounts/ · identity/ · tenants/
│   ├── files/ · messaging/ · notifications/ · workflows/
│   └── … (placeholder — xem Roadmap)
├── Sample/                        ← Host demo: API + SPA (clients/web → wwwroot) — SUT cho autotest/sample
├── autotest/                      ← Test-time: @jarvis/autotest + .playwright + sample/
├── UnitTest/
├── ADRs/                          ← Architecture Decision Records — [ADRs/README.md](ADRs/README.md)
├── docs/                          ← Refactor rules, tutorial index
├── .opencode/                     ← Skill AI (*-dotnet)
└── ai-skills/                     ← Skill / ADR bổ sung (submodule nội bộ)
```

| Thư mục | Vai trò |
|---------|---------|
| `frameworks/` | Thư viện hạ tầng Atomic — publish NuGet, reference từ Host/Infrastructure product |
| `modules/` | Module domain (Setting, Account, Tenant…); mỗi folder một bounded context + optional frontend |
| `Sample/` | App chạy thử: gắn hầu hết framework + `Jarvis.Modules.Setting`; SPA Vite/React |
| `autotest/` | Platform test-time TypeScript (`@jarvis/autotest*`) + consumer `sample/` — [autotest/README.md](autotest/README.md) |
| `UnitTest/` | Unit / integration test **C#** cho framework |
| `ADRs/` | Quyết định kiến trúc & tiến độ — [ADRs/README.md](ADRs/README.md) |
| `.opencode/` | Skill scaffold / init / add — hub: [.opencode/README.md](.opencode/README.md) |
| `docs/` | Quy tắc refactor, tutorial skill — bắt đầu từ [docs/tutorial-index.md](docs/tutorial-index.md) |

**Lưu ý đặt tên:** thư mục project `frameworks/Jarvis.Authentication.*` → NuGet **`Jarvis.Authentications.*`** (có chữ **s**). Monorepo dùng `ProjectReference` tới path `frameworks\...`; product bên ngoài dùng NuGet feed.

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

Jarvis dùng **Layer Extension**: mỗi tầng có một tệp mở rộng riêng (HostLayerExtension, ApplicationLayerExtension...). Program.cs chỉ cần gọi ***đúng hai dòng***: một đăng ký toàn bộ dịch vụ, một cấu hình pipeline:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddHostLayer();

var app = builder.Build();
app.UseHostLayer();

app.Run();
```

Mọi phức tạp của dependency injection được ẩn sau các extension method, giúp Program.cs ***luôn mỏng và dễ đọc.***

Chi tiết khung scaffold: [.opencode/skills/jarvis-dotnet/reference/solution-structure.md](.opencode/skills/jarvis-dotnet/reference/solution-structure.md).

## Modules

### Framework (`frameworks/`)

Chiều **Atomic** — mỗi package một concern, bật/tắt độc lập:

| Module | Package chính | Phạm vi |
|--------|---------------|---------|
| Foundation | `Jarvis.Mvc`, `Jarvis.Common` | JSON, CORS, WebApi, SPA (`UseCoreSpa`), middleware response chuẩn |
| Application | `Jarvis.DDD.Application`, `.Contracts` | CQRS command/query dispatcher |
| Domain | `Jarvis.DDD.Domain`, `.Domain.Shared` | Contract entity, repository, enum/shared |
| Multitenancy | `Jarvis.Multitenancy` | Ambient `ICurrentTenant` / `ICurrentTenant<TTenant>`, `AddCurrentTenant` |
| Multitenancy EF | `Jarvis.Multitenancy.EntityFramework` | Opt-in connection resolver + tenant interceptor (`AddMultitenancyEntityFramework`) |
| ORM (EF) | `Jarvis.ORM.EntityFramework` | DbContext, UoW, repository (single / dedicated / hybrid DB); DI `AddEntityFramework()` |
| ORM (Dapper) | `Jarvis.ORM.Dapper` | Connection factory cho read/report; DI `AddOrmDapper()` |
| Caching | `Jarvis.Caching`, `.Redis` | Memory + Redis, pub/sub invalidation |
| Authentication | `Jarvis.Authentications`, `.Jwt`, `.ApiKey`, `.Basic`, `.Cognito` | JWT, API Key, Basic, Cognito |
| Blob Storing | `Jarvis.BlobStoring`, `.MinIO`, `.AwsS3` | FileSystem (core), MinIO, AWS S3 |
| Notification | `Jarvis.Notification`, `.Mailkit` | Email SMTP (Mailkit) |
| Swashbuckle | `Jarvis.Swashbuckle` | Swagger đa phiên bản, security schemes |
| OpenTelemetry | `Jarvis.OpenTelemetry` | Trace, metric, log OTLP |
| OpenTelemetry DDD bridge | `Jarvis.OpenTelemetry.DDD` | Enrich user/tenant từ `ICurrentUser` / `ICurrentTenant` |
| Health Checks | `Jarvis.HealthChecks` | Liveness, readiness, startup |
| Frontend UI-kit | `@jarvis/core` (`frameworks/frontend`) | React + PrimeReact — layout admin, form, CRUD helpers |

### Domain modules (`modules/`)

Module nghiệp vụ / portal quản trị — tách khỏi framework hạ tầng:

| Module | Trạng thái | Phạm vi |
|--------|------------|---------|
| **settings** | ✅ | `Jarvis.Modules.Setting` (+ `.EntityFramework`, `.Redis`); quản lý cấu hình theo Group/Key; frontend demo |
| **accounts** | 📋 | Đăng nhập, profile, quên mật khẩu |
| **identity** | 📋 | User store, Identity |
| **tenants** | 📋 | `Jarvis.Tenants` — catalog/CRUD ([ADR](ADRs/2026-08-07-adr-jarvis-tenants-module.md)); ambient nằm `Jarvis.Multitenancy` |
| **files** | 📋 | Blob browser / quản lý file |
| **messaging** | 📋 | Event bus / realtime |
| **notifications** | 📋 | Đa kênh notification |
| **workflows** | 📋 | Quy trình / background orchestration |

Chi tiết Setting: [modules/settings/README.md](modules/settings/README.md).

## Autotest

Platform **test-time** TypeScript — không publish vào runtime Host, **không** thêm vào `Jarvis.sln`. Hub: [autotest/README.md](autotest/README.md). Ranh giới folder: [ADRs/architecture-software.md](ADRs/architecture-software.md) §0.2. SAD: [ADRs/architecture-autotest.md](ADRs/architecture-autotest.md).

| Artifact | Vai trò |
|----------|---------|
| `@jarvis/autotest` | Core engine-agnostic (`ApiClient`, ports HTTP/browser, `Workflow`, reporting) |
| `@jarvis/autotest.playwright` | Satellite Playwright (`PlaywrightTransport`, `PlaywrightBrowserDriver`) |
| `autotest/sample/` | Consumer demo (layout application / integrations / composition / ui / drivers) — API + UI |

Keyword **`automation`** dành cho Agent/AI sau này — không dùng `@jarvis/automation*`.

`autotest/sample` chạy **chống** Host `Sample/` (API + SPA `clients/web`), không nằm trong `Sample/clients/`. UI smoke: login mock (`admin@gmail.com` / `Admin@123`) rồi mở Setting, Tenant, File.

```bash
cd autotest && npm install && npm run build
dotnet run --project Sample          # SUT
cd autotest/sample && cp .env.example .env && npx playwright install chromium && npm test
```

## OpenCode skills (AI)

Skill trong [.opencode/skills/](.opencode/skills/) — gọi trong Cursor/OpenCode bằng `@.opencode/skills/<tên-skill>/...`. Hub đầy đủ: [.opencode/README.md](.opencode/README.md). Bản đồ prompt & decision tree: [docs/tutorial-index.md](docs/tutorial-index.md).

| Skill | Mô tả |
|-------|--------|
| [jarvis-dotnet](.opencode/skills/jarvis-dotnet/README.md) | Scaffold / init / add solution Jarvis |
| [foundation-dotnet](.opencode/skills/foundation-dotnet/README.md) | Json, CORS, WebApi, ApiResponseWrapper |
| [application-dotnet](.opencode/skills/application-dotnet/README.md) | CQRS Application layer |
| [authentication-dotnet](.opencode/skills/authentication-dotnet/README.md) | JWT, API Key, Basic, Cognito |
| [notification-dotnet](.opencode/skills/notification-dotnet/README.md) | Email SMTP Mailkit |
| [caching-dotnet](.opencode/skills/caching-dotnet/README.md) | Memory + Redis cache |
| [entityframework-dotnet](.opencode/skills/entityframework-dotnet/README.md) | EF multitenancy |
| [swashbuckle-dotnet](.opencode/skills/swashbuckle-dotnet/README.md) | Swagger / OpenAPI |
| [healthcheck-dotnet](.opencode/skills/healthcheck-dotnet/README.md) | Health endpoints |
| [telemetry-dotnet](.opencode/skills/telemetry-dotnet/README.md) | OpenTelemetry OTLP |
| [analyze-metric-dotnet](.opencode/skills/analyze-metric-dotnet/README.md) | Đọc Grafana Dotnet Runtime Metrics |
| [blobstoring-dotnet](.opencode/skills/blobstoring-dotnet/README.md) | FileSystem / MinIO / AWS S3 blob |
| [code-review-dotnet](.opencode/skills/code-review-dotnet/README.md) | Review PR C#/.NET |

## Get started

### 0. Chạy Sample trong monorepo

```bash
dotnet run --project Sample
```

- Swagger: theo `launchSettings` (thường `https://localhost:7006/swagger`)
- SPA Sample: `Sample/clients/web` (Vite) — build vào `Sample/wwwroot`, serve cùng Sample
- UI Setting demo: `modules/settings/frontend` — xem [modules/settings/frontend/README.md](modules/settings/frontend/README.md)
- Autotest sample (API + UI): xem mục [Autotest](#autotest) — Host phải đang chạy

### 1. Chọn cách bắt đầu (product mới)

Jarvis cung cấp bốn cách tiếp cận tuỳ theo tình trạng dự án:

**Cách 1 — Scaffold (khuyến khích)** — Từ folder trống, nhờ AI chạy skill `jarvis-dotnet`:

> "@.opencode/skills/jarvis-dotnet/workflows/scaffold.md — Tạo solution backend .NET 9 tên MyApp từ folder trống."

AI tạo cây thư mục, solution, 5 project, references, Jarvis packages, templates layer extension, Swagger + health checks. Sau scaffold: `dotnet run --project src/MyApp.Host` và mở Swagger.

**Cách 2 — Add** — Đã có solution foundation, muốn thêm module:

> "@.opencode/skills/jarvis-dotnet/workflows/add.md — Thêm xác thực JWT vào project MyApp.Host, dùng Jarvis.Authentications.Jwt."

**Cách 3 — Init** — Đã có solution .NET, muốn cài Jarvis:

> "@.opencode/skills/jarvis-dotnet/workflows/init.md — Cài Jarvis vào solution MyApp. Infrastructure: Jarvis.ORM.EntityFramework + Jarvis.Caching (trước EF)."

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

# Cài Jarvis packages (NuGet) — hoặc ProjectReference tới frameworks\... trong monorepo
dotnet add MyApp.Host package Jarvis.Mvc
dotnet add MyApp.Host package Jarvis.Swashbuckle
dotnet add MyApp.Host package Jarvis.HealthChecks
dotnet add MyApp.Host package Jarvis.OpenTelemetry
dotnet add MyApp.Application package Jarvis.DDD.Application
dotnet add MyApp.Infrastructure package Jarvis.ORM.EntityFramework
dotnet add MyApp.Infrastructure package Jarvis.ORM.Dapper
dotnet add MyApp.Infrastructure package Jarvis.Caching
```

### 2. Danh sách NuGet packages

Mỗi module framework đều có sẵn dưới dạng NuGet (hoặc `ProjectReference` trong monorepo). Chỉ thêm package đúng tầng cần dùng:

| Tầng | Package | Version |
|---|---|---|
| Domain.Shared | `Jarvis.DDD.Domain.Shared` | 1.0.x |
| Domain | `Jarvis.DDD.Domain` | 1.1.x |
| Application | `Jarvis.DDD.Application` | 1.2.x |
| Application | `Jarvis.DDD.Application.Contracts` | 1.2.x |
| Infrastructure | `Jarvis.ORM.EntityFramework` | 1.0.x |
| Infrastructure | `Jarvis.ORM.Dapper` | 1.0.x |
| Infrastructure / Host | `Jarvis.Multitenancy` | 1.0.x |
| Infrastructure | `Jarvis.Multitenancy.EntityFramework` | 1.0.x |
| Infrastructure | `Jarvis.Caching` | 1.1.x |
| Infrastructure | `Jarvis.Caching.Redis` | 1.1.x |
| Infrastructure | `Jarvis.BlobStoring` | 1.0.x |
| Infrastructure | `Jarvis.BlobStoring.MinIO` | 1.0.x |
| Infrastructure | `Jarvis.BlobStoring.AwsS3` | 1.0.x |
| Infrastructure | `Jarvis.Notification` | 1.0.x |
| Infrastructure | `Jarvis.Notification.Mailkit` | 1.0.x |
| Infrastructure / Domain module | `Jarvis.Modules.Setting` | 1.0.x |
| Host | `Jarvis.Mvc` | 1.1.x |
| Host | `Jarvis.Common` | 1.0.x |
| Host | `Jarvis.Swashbuckle` | 1.0.1 |
| Host | `Jarvis.HealthChecks` | 1.0.0 |
| Host | `Jarvis.OpenTelemetry` | 1.0.1 |
| Host | `Jarvis.OpenTelemetry.DDD` | 1.0.0 |
| Host | `Jarvis.Authentications` | 1.0.1 |
| Host | `Jarvis.Authentications.Jwt` | 1.0.1 |
| Host | `Jarvis.Authentications.ApiKey` | 1.0.1 |
| Host | `Jarvis.Authentications.Basic` | 1.0.1 |
| Host | `Jarvis.Authentications.Cognito` | 1.0.1 |
| Frontend (npm) | `@jarvis/core` | `frameworks/frontend` |

> Mẹo: Chỉ thêm package đúng tầng cần dùng để giảm dependency surface. Xem phiên bản mới nhất trên feed NuGet nội bộ. Monorepo: `ProjectReference` tới `frameworks\Jarvis.*\*.csproj` hoặc `modules\settings\Jarvis.Modules.Setting\*.csproj`.

### 3. Kết quả

Sau khi scaffold / chạy Sample:

- Swagger UI mở tại host (vd. `https://localhost:7006/swagger`)
- `GET /api/ping` trả về 200 OK — không cần cơ sở dữ liệu (nếu đã map controller ping)
- `GET /health/live` trả về 200 OK — liveness (khi bật HealthChecks)
- `GET /health/ready` trả về 200 OK (nếu đã cấu hình PostgreSQL / readiness)
- Program.cs mỏng; logic DI nằm Layer Extension / module extension
- Sample: SPA + `UseCoreSpa()` phục vụ `wwwroot`

## Roadmap

Lộ trình phát triển Jarvis framework (.NET 9). Mỗi hạng mục là **cam kết chức năng** sẽ triển khai; tài liệu và skill OpenCode trong [.opencode/skills/](.opencode/skills/) đi kèm khi từng module được phát hành. Tiến độ quyết định kiến trúc: [ADRs/README.md](ADRs/README.md).

**Chú thích trạng thái**

| Ký hiệu | Ý nghĩa |
|---------|---------|
| ✅ | Đã có trên nhánh hiện tại (package + skill / code) — ADR 🟢 khi có |
| 🟡 | Đang làm / accepted một phần — còn việc (xem ADR) |
| 📋 | Kế hoạch / ADR 🔴 Proposed — chưa phát hành đầy đủ |

**Thứ tự triển khai gợi ý**

```text
Nền tảng bảo mật & admin → Dữ liệu & multitenancy → Kiến trúc & messaging
→ Tích hợp (notification, blob, realtime) → Mở rộng (plug-in, ORM/Dapper, chất lượng)
```

**Ưu tiên ADR đang mở** (chi tiết [ADRs/README.md](ADRs/README.md)):

1. 📋 Base CRUD AppService (`ICrudAppService` / `CrudAppService`)
2. 📋 FluentValidation — validate input Application ([ADR](ADRs/2026-08-07-adr-jarvis-fluentvalidation.md))
3. 📋 Split Phase 6 — Skill / README Host (`AddCurrentUser` + `AddCurrentTenant`)
5. 📋 `Jarvis.Tenants` — module catalog/CRUD (khi có nhu cầu sản phẩm)

---

### A. Nền tảng hiện có (baseline)

| Thành phần | Phạm vi | Skill / vị trí |
|------------|---------|----------------|
| ✅ Foundation | JSON, CORS, WebApi, SPA, ApiResponseWrapper | [foundation-dotnet](.opencode/skills/foundation-dotnet/README.md) · `frameworks/Jarvis.Mvc` |
| ✅ Application | CQRS command/query dispatcher | [application-dotnet](.opencode/skills/application-dotnet/README.md) |
| ✅ Multitenancy | `ICurrentTenant` / `<TTenant>`, `AddCurrentTenant` | `frameworks/Jarvis.Multitenancy` · [ADR](ADRs/2026-08-06-adr-jarvis-multitenancy-package.md) |
| ✅ Multitenancy EF | Connection resolver + interceptor (opt-in) | `frameworks/Jarvis.Multitenancy.EntityFramework` · [ADR](ADRs/2026-08-06-adr-jarvis-multitenancy-entityframework.md) |
| ✅ Entity Framework | UoW, repository, hybrid DB | [entityframework-dotnet](.opencode/skills/entityframework-dotnet/README.md) |
| ✅ Caching | Memory + Redis, invalidation pub/sub | [caching-dotnet](.opencode/skills/caching-dotnet/README.md) · [plan](ADRs/refactor-cache-plan.md) 🟢 |
| 🟡 Authentication | JWT, API Key, Basic ✅; Cognito stub; OIDC chưa | [authentication-dotnet](.opencode/skills/authentication-dotnet/README.md) · [refactor](ADRs/refactor-authentication.md) |
| 🟡 Blob storing | FileSystem, MinIO, AWS S3 — đang review | [blobstoring-dotnet](.opencode/skills/blobstoring-dotnet/README.md) · [refactor](ADRs/refactor-blob-storing.md) |
| ✅ Notification | SMTP (Mailkit) | [notification-dotnet](.opencode/skills/notification-dotnet/README.md) |
| ✅ Swashbuckle | Swagger đa version, security schemes | [swashbuckle-dotnet](.opencode/skills/swashbuckle-dotnet/README.md) |
| ✅ OpenTelemetry | Trace, metric, log OTLP | [telemetry-dotnet](.opencode/skills/telemetry-dotnet/README.md) |
| ✅ OpenTelemetry DDD | Enrich user/tenant khỏi Domain | `Jarvis.OpenTelemetry.DDD` · [ADR](ADRs/2026-08-01-adr-techdebt-otel-enrichment-out-of-ddd.md) |
| ✅ Health checks | Liveness, readiness, startup | [healthcheck-dotnet](.opencode/skills/healthcheck-dotnet/README.md) |
| ✅ Setting | Group/Key, cache, encrypt at rest | `modules/settings` |
| ✅ Frontend UI-kit | `@jarvis/core` (PrimeReact) | `frameworks/frontend` |
| ✅ Scaffold solution | Clean Architecture 5 layer, F5 Swagger | [jarvis-dotnet](.opencode/skills/jarvis-dotnet/README.md) |
| ✅ Code review | Checklist C# / Jarvis cho PR | [code-review-dotnet](.opencode/skills/code-review-dotnet/README.md) |

---

### B. Bảo mật, identity & quản trị nền

#### B.1 Authentication

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | JWT Bearer | `Jarvis.Authentications.Jwt`, tích hợp Swagger |
| ✅ | API Key | Header tùy chỉnh, `IApiKeyProvider` |
| ✅ | Basic | `Jarvis.Authentications.Basic` |
| 🟡 | Cognito | Package stub — chưa đủ production ([refactor](ADRs/refactor-authentication.md)) |
| 📋 | OIDC | OpenID Connect / OpenIddict (Azure AD, Keycloak, …); map claim → user/tenant |

#### B.2 User & Identity

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | ASP.NET Core Identity | User store, password policy, lockout, refresh token — `modules/identity` |
| 📋 | Đồng bộ auth | Subject JWT/OIDC ↔ `ApplicationUser` |

#### B.3 Authorization

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | RBAC | Role, permission, policy handler |
| 📋 | ABAC | Phân quyền theo attribute (tenant, resource, action) |
| 📋 | API & CQRS | `[Authorize]`, kiểm tra quyền trong handler |

#### B.4 Jarvis admin (`modules/*`)

Module API + persistence + UI cho portal quản trị (`/api/admin/*`).

| | Module | Phạm vi |
|---|--------|---------|
| ✅ | **Settings** | `Jarvis.Modules.Setting` — Group/Key, form types, cache, encrypt; FE demo |
| 📋 | **Tenants** | `Jarvis.Tenants` — CRUD catalog; **không** gộp Multitenancy ([ADR](ADRs/2026-08-07-adr-jarvis-tenants-module.md)) |
| 📋 | **Permissions** | CRUD permission; seed chuẩn cho policy & menu |
| 📋 | **Roles** | CRUD role; gán permission |
| 📋 | **Users** | CRUD user; đổi mật khẩu; lock/unlock; gán role & tenant — `modules/identity` |
| 📋 | **Account** | Đăng nhập, đăng xuất, user profile, quên mật khẩu — `modules/accounts` |

---

### C. Dữ liệu & multitenancy

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | Ambient tenant | `Jarvis.Multitenancy` — `ICurrentTenant` / `<TTenant>`, `Change`, resolvers ([ADR](ADRs/2026-08-06-adr-jarvis-multitenancy-package.md), [generic](ADRs/2026-08-06-adr-generic-current-tenant.md)) |
| ✅ | Multitenancy EF | `Jarvis.Multitenancy.EntityFramework` — connection + interceptor opt-in ([ADR](ADRs/2026-08-06-adr-jarvis-multitenancy-entityframework.md)) |
| ✅ | ORM family | `Jarvis.ORM.EntityFramework` (giữ `AddEntityFramework`) + `Jarvis.ORM.Dapper` (`AddOrmDapper`) ([ADR](ADRs/2026-08-07-adr-jarvis-orm-packages.md)) |
| ✅ | Split user/tenant packages | `ICurrentUser` / `ICurrentTenant` khỏi Domain; Auth + Multitenancy ([ADR](ADRs/2026-08-01-adr-techdebt-split-current-user-tenant-packages.md) Phase 2–5) |
| 📋 | Skill / Host docs | Phase 6 — document `AddCurrentUser` + `AddCurrentTenant` trên Host |
| 📋 | `Jarvis.Tenants` | Module catalog/CRUD tenant (pattern Setting) |
| 📋 | Partition & migrate | Tool migrate dữ liệu theo tenant; rollback; dry-run |
| 📋 | Health threshold | Readiness `degraded` khi DB/Redis vượt ngưỡng latency |
| 📋 | Dynamic filter | Grammar `field:op:value`; sort/filter an toàn trên `PagedListRequest` |

---

### D. Kiến trúc ứng dụng & messaging

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | CQRS | Command/query dispatcher |
| 📋 | CRUD AppService | `ICrudAppService` / `CrudAppService` trong `Jarvis.DDD.Application` ([ADR](ADRs/2026-08-07-adr-jarvis-crud-app-service.md)) |
| 📋 | FluentValidation | Chuẩn validate input Application; map 400 `BaseResponse` ([ADR](ADRs/2026-08-07-adr-jarvis-fluentvalidation.md) · [docs](https://docs.fluentvalidation.net/en/latest/)) |
| 📋 | DDD | Aggregate, value object, bounded context; convention scaffold |
| 📋 | Domain & integration events | Domain events, outbox/inbox |
| 📋 | Event bus | `IEventBus`; provider **RabbitMQ**, **Kafka** — `modules/messaging` |
| 📋 | Idempotency | API & consumer idempotent; phối hợp outbox |

---

### E. Tích hợp & trải nghiệm runtime

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | Notification | Đa kênh (email/SMS/push); relay — `modules/notifications` |
| 📋 | Blob browser | API duyệt file — `modules/files` |
| 📋 | SignalR | Hub chuẩn; JWT; tenant group; Redis backplane |
| 📋 | Background jobs / workflows | Worker chuẩn; queue — `modules/workflows` |
| 📋 | Resilience | Polly, circuit breaker; gắn OpenTelemetry |

---

### F. Mở rộng framework & chất lượng

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | Plug-and-play | `IJarvisModule`; module NuGet / nội bộ / third-party; discovery & thứ tự DI |
| 📋 | Options & secrets | `IValidateOptions`; Key Vault / biến môi trường |
| 📋 | OpenAPI contract | Versioning; breaking-change policy; contract test |
| 📋 | Observability ops | Grafana runtime metrics — [analyze-metric-dotnet](.opencode/skills/analyze-metric-dotnet/README.md) |
| 📋 | Rate limit & audit | Giới hạn request; audit log thao tác admin |
| 📋 | NuGet & semver | Phát hành package; matrix .NET; changelog breaking |

---

### G. Phụ thuộc giữa các khối

```text
B.1 Authentication ──► B.4 Account
B.2 Identity       ──► B.4 Users
B.3 RBAC           ──► B.4 Roles / Permissions
✅ Multitenancy    ──► C Multitenancy.EF ──► B.4 Tenants (catalog)
✅ C Jarvis.ORM.* (EF + Dapper foundation)
✅ Settings        ──► Sample / admin UI
D CRUD AppService  ──► B.4 / Sample entity CRUD đơn giản
B.4 Jarvis admin ──► E Notification (quên mật khẩu)
D Event bus        ──► E SignalR / Notification relay
F Plug-and-play    ──► toàn bộ module Jarvis.* / modules/*
```

Cập nhật roadmap theo [ADRs/README.md](ADRs/README.md) và khi phát hành phiên bản framework (tag / release note).
