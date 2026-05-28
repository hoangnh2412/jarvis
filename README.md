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
- **Atomic:** `Jarvis.Caching` trước `Jarvis.EntityFramework` — thứ tự DI document trong skill, không nhét logic cache vào EF core.

Khi scaffold hoặc add module, luôn giữ **layer đúng chỗ** và **package đúng mức** — đó là contract Jarvis với team và với AI agent (skill `jarvis-dotnet` + skill chuyên sâu).

## Architecture

Một solution Jarvis tiêu chuẩn gồm năm lớp chính, xếp chồng theo thứ tự phụ thuộc đi lên:

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

## Modules

Danh sách module Jarvis (chiều **Atomic** — xem [Triết lý thiết kế](#triết-lý-thiết-kế)). Mỗi module đảm nhận một mảng chức năng và có thể thêm vào độc lập qua NuGet + skill `*-dotnet`:

- **Foundation** — module nền tảng: JSON, CORS, WebApi, middleware xử lý response chuẩn.
- **Application** — CQRS với command dispatcher và query dispatcher.
- **Entity Framework** — DbContext, Unit of Work, đa năng cho multitenancy với bốn mô hình CSDL.
- **Caching** — cache hai tầng memory và Redis, pub/sub đồng bộ giữa các node.
- **Authentication** — JWT bearer, API Key, Cognito (OIDC, Identity, platform admin — [Roadmap](#roadmap)).
- **Blob Storing** — lưu trữ file qua FileSystem (core), MinIO và AWS S3 (package vệ tinh).
- **Notification** — gửi email qua SMTP sử dụng Mailkit.
- **Swashbuckle** — Swagger đa phiên bản, bảo mật, BaseResponse schema.
- **OpenTelemetry** — thu thập trace, metric và log theo chuẩn OTLP.
- **Health Checks** — endpoint liveness, readiness và startup.

## OpenCode skills (AI)

Skill trong [.opencode/skills/](.opencode/skills/) — gọi trong Cursor/OpenCode bằng `@.opencode/skills/<tên-skill>/...`. Hub đầy đủ: [.opencode/README.md](.opencode/README.md).

| Skill | Mô tả |
|-------|--------|
| [jarvis-dotnet](.opencode/skills/jarvis-dotnet/README.md) | Scaffold / init / add solution Jarvis |
| [foundation-dotnet](.opencode/skills/foundation-dotnet/README.md) | Json, CORS, WebApi, ApiResponseWrapper |
| [application-dotnet](.opencode/skills/application-dotnet/README.md) | CQRS Application layer |
| [authentication-dotnet](.opencode/skills/authentication-dotnet/README.md) | JWT, API Key, Cognito |
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

### 1. Chọn cách bắt đầu

Jarvis cung cấp ba cách tiếp cận tuỳ theo tình trạng dự án của bạn:

**Cách 1 — Scaffold (khuyến khích)** — Từ folder trống, nhờ AI chạy skill `jarvis-dotnet` với thông tin sản phẩm:

> "@.opencode/skills/jarvis-dotnet/workflows/scaffold.md — Tạo solution backend .NET 9 tên MyApp từ folder trống."

AI sẽ tự động tạo cây thư mục, solution, 5 project, thêm project references, cài Jarvis packages, copy templates layer extension, và cấu hình Swagger + health checks. Sau scaffold, chỉ cần `dotnet run --project src/MyApp.Host` và mở Swagger.

**Cách 2 — Add** — Đã có solution foundation, muốn thêm module:

> "Thêm xác thực JWT vào project MyApp.Host, dùng Jarvis.Authentications.Jwt."

AI sẽ thêm NuGet package, đăng ký `AddCoreJwtBearer` trong Program.cs, và cập nhật appsettings.

**Cách 3 — Init** — Đã có solution .NET, muốn cài Jarvis vào:

> "@.opencode/skills/jarvis-dotnet/workflows/init.md — Cài Jarvis vào solution MyApp. Infrastructure: Jarvis.EntityFramework + Jarvis.Caching (trước EF)."

AI sẽ thêm package theo từng tầng, tạo `HostLayerExtension.cs`, cập nhật Program.cs với `AddHostLayer()` / `UseHostLayer()`, và thêm cấu hình appsettings.

**Cách 4 — Manual** — Tự tay tạo project và cài package bằng dotnet CLI:

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

# Cài Jarvis packages
dotnet add MyApp.Host package Jarvis.Mvc
dotnet add MyApp.Host package Jarvis.Swashbuckle
dotnet add MyApp.Host package Jarvis.HealthChecks
dotnet add MyApp.Host package Jarvis.OpenTelemetry
dotnet add MyApp.Application package Jarvis.Application
dotnet add MyApp.Infrastructure package Jarvis.EntityFramework
```

### 2. Danh sách NuGet packages

Mỗi module Jarvis đều có sẵn dưới dạng NuGet package. Tuỳ vào tầng cần cài, thêm các gói tương ứng:

| Tầng | Package | Version |
|---|---|---|
| Domain.Shared | `Jarvis.Domain.Shared` | 1.0.x |
| Application | `Jarvis.Application` | 1.0.x |
| Application | `Jarvis.Application.Contracts` | 1.0.x |
| Infrastructure | `Jarvis.EntityFramework` | 1.0.x |
| Infrastructure | `Jarvis.Caching` | 1.0.x |
| Infrastructure | `Jarvis.Caching.Redis` | 1.0.x |
| Infrastructure | `Jarvis.BlobStoring` | 1.0.x |
| Infrastructure | `Jarvis.BlobStoring.MinIO` | 1.0.x |
| Infrastructure | `Jarvis.BlobStoring.AwsS3` | 1.0.x |
| Infrastructure | `Jarvis.Notification` | 1.0.x |
| Infrastructure | `Jarvis.Notification.Mailkit` | 1.0.x |
| Host | `Jarvis.Mvc` | 1.0.x |
| Host | `Jarvis.Swashbuckle` | 1.0.1 |
| Host | `Jarvis.HealthChecks` | 1.0.0 |
| Host | `Jarvis.OpenTelemetry` | 1.0.1 |
| Host | `Jarvis.Authentications` | 1.0.x |
| Host | `Jarvis.Authentications.Jwt` | 1.0.x |
| Host | `Jarvis.Authentications.ApiKey` | 1.0.x |

> Mẹo: Chỉ thêm package đúng tầng cần dùng để giảm dependency surface. Xem phiên bản mới nhất trên feed NuGet nội bộ.

### 3. Kết quả

Sau khi scaffold và build, chạy F5 bạn sẽ thấy:

- Swagger UI mở tại `https://localhost:7006/swagger`
- `GET /api/ping` trả về 200 OK — không cần cơ sở dữ liệu
- `GET /health/live` trả về 200 OK — liveness sẵn sàng
- `GET /health/ready` trả về 200 OK (nếu đã cấu hình PostgreSQL)
- Program.cs chỉ vỏn vẹn vài dòng, mọi logic nằm trong Layer Extension

## Roadmap

Lộ trình phát triển Jarvis framework (.NET 9). Mỗi hạng mục là **cam kết chức năng** sẽ triển khai; tài liệu và skill OpenCode trong [.opencode/skills/](.opencode/skills/) đi kèm khi từng module được phát hành.

**Chú thích trạng thái**

| Ký hiệu | Ý nghĩa |
|---------|---------|
| ✅ | Đã có trên nhánh hiện tại (package + skill) |
| 📋 | Kế hoạch — chưa phát hành đầy đủ |

**Thứ tự triển khai gợi ý**

```text
Nền tảng bảo mật & admin → Dữ liệu & multitenancy → Kiến trúc & messaging
→ Tích hợp (notification, blob, realtime) → Mở rộng (plug-in, Dapper, chất lượng)
```

---

### A. Nền tảng hiện có (baseline)

| Thành phần | Phạm vi | Skill |
|------------|---------|-------|
| ✅ Foundation | JSON, CORS, WebApi, ApiResponseWrapper | [foundation-dotnet](.opencode/skills/foundation-dotnet/README.md) |
| ✅ Application | CQRS command/query dispatcher | [application-dotnet](.opencode/skills/application-dotnet/README.md) |
| ✅ Entity Framework | UoW, repository, multitenancy (single / dedicated / hybrid) | [entityframework-dotnet](.opencode/skills/entityframework-dotnet/README.md) |
| ✅ Caching | Memory + Redis, invalidation pub/sub | [caching-dotnet](.opencode/skills/caching-dotnet/README.md) |
| ✅ Authentication | JWT Bearer, API Key, Cognito | [authentication-dotnet](.opencode/skills/authentication-dotnet/README.md) |
| ✅ Blob storing | FileSystem, MinIO, AWS S3 | [blobstoring-dotnet](.opencode/skills/blobstoring-dotnet/README.md) |
| ✅ Notification | SMTP (Mailkit) | [notification-dotnet](.opencode/skills/notification-dotnet/README.md) |
| ✅ Swashbuckle | Swagger đa version, security schemes | [swashbuckle-dotnet](.opencode/skills/swashbuckle-dotnet/README.md) |
| ✅ OpenTelemetry | Trace, metric, log OTLP | [telemetry-dotnet](.opencode/skills/telemetry-dotnet/README.md) |
| ✅ Health checks | Liveness, readiness, startup | [healthcheck-dotnet](.opencode/skills/healthcheck-dotnet/README.md) |
| ✅ Scaffold solution | Clean Architecture 5 layer, F5 Swagger | [jarvis-dotnet](.opencode/skills/jarvis-dotnet/README.md) |
| ✅ Code review | Checklist C# / Jarvis cho PR | [code-review-dotnet](.opencode/skills/code-review-dotnet/README.md) |

---

### B. Bảo mật, identity & quản trị nền

#### B.1 Authentication

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | JWT Bearer | `Jarvis.Authentications.Jwt`, tích hợp Swagger |
| ✅ | API Key | Header tùy chỉnh, `IApiKeyProvider` |
| 📋 | OIDC | OpenID Connect (Azure AD, Keycloak, …); map claim → user/tenant |

#### B.2 User & Identity

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | ASP.NET Core Identity | User store, password policy, lockout, refresh token |
| 📋 | Đồng bộ auth | Subject JWT/OIDC ↔ `ApplicationUser` |

#### B.3 Authorization

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | RBAC | Role, permission, policy handler |
| 📋 | ABAC | Phân quyền theo attribute (tenant, resource, action) |
| 📋 | API & CQRS | `[Authorize]`, kiểm tra quyền trong handler |

#### B.4 Platform admin (`Jarvis.Platform.*`)

Module API + persistence cho portal quản trị (`/api/admin/*`).

| | Module | Phạm vi |
|---|--------|---------|
| 📋 | **Settings** | CRUD tham số hệ thống; kiểu: Text, Number, DateTime, DateRange, Password (mask, audit, cache invalidate) |
| 📋 | **Tenants** | CRUD tenant; cây phân cấp parent/child; metadata & connection |
| 📋 | **Permissions** | CRUD permission; seed chuẩn cho policy & menu |
| 📋 | **Roles** | CRUD role; gán permission |
| 📋 | **Users** | CRUD user; đổi mật khẩu; lock/unlock; gán role & tenant |
| 📋 | **Account** | Đăng nhập, đăng xuất, user profile, quên mật khẩu (email/OTP) |

---

### C. Dữ liệu & multitenancy

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | EF multitenancy | Shared DB, dedicated DB, hybrid; UoW, `SwitchDbContextAsync` |
| 📋 | Partition & migrate | Tool migrate dữ liệu theo tenant; rollback; dry-run |
| 📋 | Health threshold | Readiness `degraded` khi DB/Redis vượt ngưỡng latency |
| 📋 | Dynamic filter | Grammar `field:op:value`; sort/filter an toàn trên `PagedListRequest` |
| 📋 | Dapper | ORM song song EF; read/report; multitenancy & dynamic filter |

---

### D. Kiến trúc ứng dụng & messaging

| | Hạng mục | Mô tả |
|---|----------|--------|
| ✅ | CQRS | Command/query dispatcher |
| 📋 | DDD | Aggregate, value object, bounded context; convention scaffold |
| 📋 | Domain & integration events | Domain events, outbox/inbox |
| 📋 | Event bus | `IEventBus`; provider **RabbitMQ**, **Kafka**; correlation & tenant header |
| 📋 | Idempotency | API & consumer idempotent; phối hợp outbox |

---

### E. Tích hợp & trải nghiệm runtime

| | Hạng mục | Mô tả |
|---|----------|--------|
| 📋 | Notification | Đa kênh (email/SMS/push); relay pattern; email template; AWS SES/SNS |
| 📋 | Blob browser | API duyệt file (list/upload/download); metadata & preview; phân quyền tenant |
| 📋 | SignalR | Hub chuẩn; JWT; tenant group; Redis backplane; tích hợp event bus |
| 📋 | Background jobs | Worker chuẩn; queue; scope multitenancy |
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
C EF multitenancy  ──► B.4 Tenants
B.4 Platform admin ──► E Notification (quên mật khẩu)
D Event bus        ──► E SignalR / Notification relay
F Plug-and-play    ──► toàn bộ module Jarvis.*
```

Cập nhật roadmap khi phát hành phiên bản framework (tag / release note).

