# Jarvis Framework

## Giới thiệu

Jarvis là framework xây dựng backend ASP.NET Core trên nền tảng .NET 9, áp dụng kiến trúc Clean Architecture phân lớp rõ ràng. Mục tiêu của Jarvis là giúp đội ngũ phát triển có thể khởi tạo một solution backend chuẩn chỉnh từ một thư mục trống, chỉ với vài thao tác, và sẵn sàng chạy Swagger ngay lập tức.

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

Jarvis được thiết kế theo hướng module nguyên tử, mỗi module đảm nhận một mảng chức năng riêng và có thể thêm vào độc lập:

- **Foundation** — module nền tảng: JSON, CORS, WebApi, middleware xử lý response chuẩn.
- **Application** — CQRS với command dispatcher và query dispatcher.
- **Entity Framework** — DbContext, Unit of Work, đa năng cho multitenancy với bốn mô hình CSDL.
- **Caching** — cache hai tầng memory và Redis, pub/sub đồng bộ giữa các node.
- **Authentication** — xác thực qua JWT bearer, API Key, OpenIddict.
- **Blob Storing** — lưu trữ file qua FileSystem và MinIO object storage.
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
| [blobstoring-dotnet](.opencode/skills/blobstoring-dotnet/README.md) | FileSystem / MinIO blob |
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
| Infrastructure | `Jarvis.BlobStoring.FileSystem` | 1.0.x |
| Infrastructure | `Jarvis.BlobStoring.MinIO` | 1.0.x |
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

