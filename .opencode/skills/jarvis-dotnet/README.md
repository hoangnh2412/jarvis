# jarvis-dotnet

Skill điều phối **scaffold solution .NET 9 phân lớp** và **cài đặt Jarvis framework** trên ASP.NET Core. Agent đọc [SKILL.md](./SKILL.md) và workflow tương ứng khi thực thi.

Hub tất cả skill: [.opencode/README.md](../../README.md).

## Khi nào dùng

| Tình huống | Workflow |
|------------|----------|
| Folder trống → backend mới, F5 Swagger | [workflows/scaffold.md](./workflows/scaffold.md) |
| Solution .NET có sẵn → gắn Jarvis theo layer | [workflows/init.md](./workflows/init.md) |
| Đã có Jarvis foundation → thêm module (JWT, Redis cache, …) | [workflows/add.md](./workflows/add.md) |

**Không dùng skill này cho:**

- Chỉ review code trước PR → dùng [code-review-dotnet](../code-review-dotnet/README.md)
- Chỉ healthcheck readiness provider → dùng [healthcheck-dotnet](../healthcheck-dotnet/README.md)
- Chỉ OTEL plug-in / sampling production → dùng [telemetry-dotnet](../telemetry-dotnet/README.md)

## Cách gọi skill

Trong OpenCode / Cursor agent chat:

### 1. Tham chiếu workflow (khuyến nghị)

```text
@.opencode/skills/jarvis-dotnet/workflows/scaffold.md

Scaffold backend .NET 9:
- Product: Acme
- product: acme
- Jarvis: NuGet feed nội bộ
```

### 2. Tham chiếu orchestrator

```text
@.opencode/skills/jarvis-dotnet/SKILL.md

Dùng skill jarvis-dotnet init — gắn Jarvis vào solution MyApp có sẵn.
```

### 3. Thêm module cụ thể

```text
@.opencode/skills/jarvis-dotnet/modules/authentication/SKILL.md

Thêm JWT vào MyApp.Host.
```

Agent sẽ đọc [SKILL.md](./SKILL.md) và **chỉ** mở file module/workflow cần thiết — không load toàn bộ skill tree.

## Chuẩn bị trước khi chạy

| Việc nên làm | Scaffold | Init / Add |
|--------------|----------|------------|
| Xác định `{Product}` (PascalCase) và `{product}` (kebab) | Bắt buộc | Khuyến nghị |
| Chọn **NuGet feed** hoặc **monorepo** `{JarvisRoot}` | Bắt buộc | Bắt buộc |
| Folder trống hoặc repo mới | Scaffold | — |
| Solution + project Host/Application/Infrastructure đã có | — | Init |
| `dotnet` SDK 9.x cài sẵn | Có | Có |

**Lưu ý develop:** `Jarvis.EntityFramework` yêu cầu `AddJarvisCaching()` **trước** `AddEntityFramework()`. Template scaffold đã wiring đúng thứ tự; khi init/add EF thủ công phải tuân thủ.

**PackageId:** folder repo `Jarvis.Authentication.*` → NuGet **`Jarvis.Authentications.*`** (có chữ **s**).

## Prompt mẫu

### Scaffold từ folder trống (thường dùng nhất)

```text
@.opencode/skills/jarvis-dotnet/workflows/scaffold.md

Scaffold backend .NET 9:
- Product: Acme
- product: acme
- Jarvis: NuGet feed nội bộ
- Mặc định: Swagger + OTEL + health live + ping; chưa bật PostgreSQL readiness

Sau khi xong: dotnet build và báo URL Swagger.
```

### Init — gắn Jarvis vào solution có sẵn

```text
@.opencode/skills/jarvis-dotnet/workflows/init.md

Cài Jarvis vào solution MyApp:
- Host: Jarvis.Mvc, Swashbuckle, HealthChecks, OpenTelemetry
- Application: Jarvis.Application
- Infrastructure: Jarvis.EntityFramework + Jarvis.Caching
```

### Add — thêm module

```text
@.opencode/skills/jarvis-dotnet/workflows/add.md

Thêm xác thực JWT vào MyApp.Host theo modules/authentication/SKILL.md
```

```text
@.opencode/skills/entityframework-dotnet/patterns/single-db/SKILL.md

Cấu hình EF multitenancy single DB cho MyApp.
```

```text
@.opencode/skills/caching-dotnet/workflows/add.md

Bật Redis distributed cache + memory invalidation cho MyApp.Host
```

### Background worker + OTEL

```text
@.opencode/skills/telemetry-dotnet/SKILL.md

Thêm HostedService cron kế thừa BaseWorker; job EF multitenancy theo entityframework-dotnet/README.md
```

## Agent sẽ làm gì

Theo workflow đã chọn:

1. **Scaffold** — tạo repo `{product}-backend`, 5 project + 2 test, layer extensions, copy [templates/layers/](./templates/layers/), cài package Jarvis, `dotnet build`
2. **Init** — thêm package theo layer, tạo `*LayerExtension.cs`, cập nhật `Program.cs` (`AddHostLayer` / `UseHostLayer`)
3. **Add** — đọc `modules/<module>/SKILL.md` hoặc skill độc lập (`caching-dotnet`, `entityframework-dotnet`, `telemetry-dotnet`, …), thêm package + DI + appsettings

**Output bắt buộc (scaffold):**

- Solution 5 project + 2 test projects
- `*LayerExtension.cs` mỗi layer
- `Program.cs` mỏng (chỉ `AddHostLayer()` / `UseHostLayer()`)
- `appsettings` + `launchSettings`
- `dotnet build` thành công

Chi tiết cấu trúc: [reference/solution-structure.md](./reference/solution-structure.md).

## Kết quả sau scaffold

| Kiểm tra | Kỳ vọng |
|----------|---------|
| Startup project | `{Product}.Host` |
| Swagger | `https://localhost:7006/swagger` (port theo `launchSettings.json`) |
| `GET /api/ping` | 200 — không cần DB |
| `GET /health/live` | 200 |
| `GET /health/ready` | 200 nếu PostgreSQL + readiness đã cấu hình; có thể unhealthy nếu chưa có DB |
| `Program.cs` | Vài dòng — logic trong layer extensions |

```bash
dotnet run --project src/{Product}.Host
```

## Modules Jarvis (atomic)

Chỉ mở module cần dùng — [workflows/add.md](./workflows/add.md):

| Module | SKILL | Ghi chú develop |
|--------|-------|-----------------|
| Foundation | [modules/foundation/SKILL.md](./modules/foundation/SKILL.md) | Json, CORS, WebApi, middleware |
| Application | [modules/application/SKILL.md](./modules/application/SKILL.md) | CQRS dispatcher |
| Entity Framework | — | [entityframework-dotnet](../entityframework-dotnet/README.md) · **Caching trước EF** |
| Caching | — | [caching-dotnet](../caching-dotnet/README.md) |
| Authentication | [modules/authentication/SKILL.md](./modules/authentication/SKILL.md) | JWT, API Key, Cognito |
| Blob storing | — | [blobstoring-dotnet](../blobstoring-dotnet/README.md) |
| Notification | [modules/notification/SKILL.md](./modules/notification/SKILL.md) | SMTP Mailkit |
| Swashbuckle | — | [swashbuckle-dotnet](../swashbuckle-dotnet/README.md) |
| OpenTelemetry | — | [telemetry-dotnet](../telemetry-dotnet/README.md) · [SKILL.md](../telemetry-dotnet/SKILL.md) |
| Health checks | — | [healthcheck-dotnet](../healthcheck-dotnet/README.md) · [SKILL.md](../healthcheck-dotnet/SKILL.md) |

## Catalog package (NuGet, develop)

| PackageId | Version | Layer |
|-----------|---------|-------|
| `Jarvis.Domain.Shared` | 1.0.0 | Domain.Shared |
| `Jarvis.Domain` | 1.1.1 | Host |
| `Jarvis.Application` | 1.2.1 | Application |
| `Jarvis.Application.Contracts` | 1.2.1 | Application |
| `Jarvis.EntityFramework` | 1.0.0 | Infrastructure |
| `Jarvis.Caching` | 1.1.0 | Infrastructure |
| `Jarvis.Caching.Redis` | 1.1.0 | Infrastructure (tùy chọn) |
| `Jarvis.Mvc` | 1.1.0 | Host |
| `Jarvis.Swashbuckle` | 1.0.1 | Host |
| `Jarvis.HealthChecks` | 1.0.0 | Host |
| `Jarvis.OpenTelemetry` | 1.0.1 | Host |
| `Jarvis.Authentications.*` | 1.0.1 | Host |

## Sau khi chạy skill

1. **`dotnet build`** — xác nhận không lỗi compile
2. **`dotnet run --project src/{Product}.Host`** — Swagger + ping
3. Thêm entity/handler theo layer Clean Architecture
4. Readiness DB / Redis → [healthcheck-dotnet](../healthcheck-dotnet/README.md)
5. OTEL production (sampling, PII) → [telemetry-dotnet](../telemetry-dotnet/README.md)
6. Trước PR → skill [code-review-dotnet](../code-review-dotnet/README.md)

## Tài liệu liên quan

| File | Nội dung |
|------|----------|
| [SKILL.md](./SKILL.md) | Orchestrator đầy đủ cho agent |
| [reference/solution-structure.md](./reference/solution-structure.md) | Cây thư mục, DI, mapping layer |
| [templates/](./templates/) | Code mẫu scaffold |
| [entityframework-dotnet/README.md](../entityframework-dotnet/README.md) | EF multitenancy |
| [caching-dotnet/README.md](../caching-dotnet/README.md) | Cache |
| [swashbuckle-dotnet/README.md](../swashbuckle-dotnet/README.md) | Swagger |
| [blobstoring-dotnet/README.md](../blobstoring-dotnet/README.md) | Blob FileSystem / MinIO |
| [telemetry-dotnet/README.md](../telemetry-dotnet/README.md) | OpenTelemetry — kiến trúc & workflow |
| [healthcheck-dotnet/README.md](../healthcheck-dotnet/README.md) | Health endpoints & providers |
| [code-review-dotnet/README.md](../code-review-dotnet/README.md) | Review PR |
| [.opencode/README.md](../../README.md) | Hub tất cả skill |
| [README.md](../../../README.md) | Giới thiệu Jarvis framework |

## Lưu ý

- Skill ưu tiên **scaffold chạy được ngay** (Swagger, ping, liveness) — DB readiness có thể bật sau
- **Không** thêm package không dùng — giảm dependency surface
- Monorepo: thay `PackageReference` bằng `ProjectReference` tới `{JarvisRoot}` — xem [templates/layer-csproj/](./templates/layer-csproj/)
- Phiên bản package lấy từ csproj repo Jarvis branch `develop`; cập nhật khi release mới
