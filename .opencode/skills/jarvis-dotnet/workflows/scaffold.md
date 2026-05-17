# Workflow: Scaffold solution từ folder trống

**Mục tiêu:** Tạo repo `{product}-backend` phân lớp + Jarvis, `dotnet build` thành công, **F5** mở Swagger.

**Đầu vào bắt buộc từ user:**

- `{Product}` — PascalCase (vd. `Acme`)
- `{product}` — kebab/lowercase (vd. `acme`)
- `{JarvisRoot}` — đường dẫn tới repo Jarvis framework (monorepo) **hoặc** dùng NuGet feed

## Checklist agent

```text
- [ ] 0. Hỏi / xác nhận 5.9 (Q1–Q7) — dùng defaults nếu user không chỉ định
- [ ] 1. Tạo cây thư mục repo (5.1)
- [ ] 2. dotnet new sln + 5 projects + tests (5.2, 5.10 bước 1–2)
- [ ] 3. Project references đúng thứ tự (5.11)
- [ ] 4. Thêm Jarvis PackageReference / ProjectReference theo layer
- [ ] 5. Tạo folder structure (5.3–5.8) + *LayerExtension.cs
- [ ] 6. Copy templates Host / Infrastructure / Application / Domain
- [ ] 7. appsettings + launchSettings (Swagger F5)
- [ ] 8. dotnet build && dotnet run --project Host
```

## Bước 0 — Placeholder

Thay toàn bộ trong templates:

| Placeholder | Ví dụ |
|---|---|
| `{Product}` | `Acme` |
| `{product}` | `acme` |
| `{JarvisRoot}` | `../../../Jarvis` (tùy vị trí repo) |

## Bước 1 — Tạo repo

```bash
PRODUCT=Acme
product=acme
mkdir -p "${product}-backend"/{docs,src,tests,build}
cd "${product}-backend"
```

Tạo `README.md`, `docs/Architecture.md` (mô tả ngắn 4 layer + Jarvis).

## Bước 2 — Solution & projects

```bash
cd src
dotnet new sln -n "${PRODUCT}"

dotnet new classlib -n "${PRODUCT}.Domain.Shared" -f net9.0 -o "${PRODUCT}.Domain.Shared"
dotnet new classlib -n "${PRODUCT}.Domain" -f net9.0 -o "${PRODUCT}.Domain"
dotnet new classlib -n "${PRODUCT}.Application" -f net9.0 -o "${PRODUCT}.Application"
dotnet new classlib -n "${PRODUCT}.Infrastructure" -f net9.0 -o "${PRODUCT}.Infrastructure"
dotnet new webapi -n "${PRODUCT}.Host" -f net9.0 -o "${PRODUCT}.Host" --use-controllers

dotnet new xunit -n "${PRODUCT}.Domain.Tests" -f net9.0 -o "../tests/${PRODUCT}.Domain.Tests"
dotnet new xunit -n "${PRODUCT}.Application.Tests" -f net9.0 -o "../tests/${PRODUCT}.Application.Tests"

dotnet sln add "${PRODUCT}.Domain.Shared" "${PRODUCT}.Domain" "${PRODUCT}.Application" \
  "${PRODUCT}.Infrastructure" "${PRODUCT}.Host" \
  "../tests/${PRODUCT}.Domain.Tests" "../tests/${PRODUCT}.Application.Tests"

dotnet add "${PRODUCT}.Domain" reference "${PRODUCT}.Domain.Shared"
dotnet add "${PRODUCT}.Application" reference "${PRODUCT}.Domain"
dotnet add "${PRODUCT}.Infrastructure" reference "${PRODUCT}.Domain"
dotnet add "${PRODUCT}.Host" reference "${PRODUCT}.Application" "${PRODUCT}.Infrastructure"
dotnet add "../tests/${PRODUCT}.Domain.Tests" reference "${PRODUCT}.Domain"
dotnet add "../tests/${PRODUCT}.Application.Tests" reference "${PRODUCT}.Application"
```

## Bước 3 — Jarvis references

**Host** — xem [templates/layer-csproj/Host.csproj.xml](../templates/layer-csproj/Host.csproj.xml).

**Infrastructure** — [templates/layer-csproj/Infrastructure.csproj.xml](../templates/layer-csproj/Infrastructure.csproj.xml).

**Application** — [templates/layer-csproj/Application.csproj.xml](../templates/layer-csproj/Application.csproj.xml).

**Domain.Shared** (tùy chọn) — `Jarvis.Domain.Shared`.

Thêm package host bên thứ ba:

```bash
dotnet add "${PRODUCT}.Host" package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add "${PRODUCT}.Infrastructure" package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add "${PRODUCT}.Host" package Microsoft.EntityFrameworkCore.Design
```

## Bước 4 — Folder structure

Tạo cây theo [reference/solution-structure.md](../reference/solution-structure.md) và [templates/solution-tree.txt](../templates/solution-tree.txt).

## Bước 5 — Layer extensions & code

Copy từ `templates/layers/` → đúng project (đổi namespace `{Product}`):

| Template | Đích |
|---|---|
| `layers/DomainLayerExtension.cs` | `{Product}.Domain` |
| `layers/ApplicationLayerExtension.cs` | `{Product}.Application` |
| `layers/InfrastructureLayerExtension.cs` | `{Product}.Infrastructure` |
| `layers/IAppUnitOfWork.cs` | `{Product}.Domain/Repositories/` |
| `layers/AppDbContext.cs` | `{Product}.Infrastructure/Persistence/` |
| `layers/AppUnitOfWork.cs` | `{Product}.Infrastructure/Persistence/` |
| `layers/EnrichTraceService.cs` | `{Product}.Host/Services/` |
| `layers/EnrichLogService.cs` | `{Product}.Host/Services/` |
| `templates/docs-README.md` | repo `README.md` |
| `templates/docs-Architecture.md` | `docs/Architecture.md` |
| `layers/HostLayerExtension.cs` | `{Product}.Host` |
| `layers/Program.cs` | `{Product}.Host` (thay file mặc định) |
| `layers/PingController.cs` | `{Product}.Host/Controllers/` |
| `layers/appsettings.json` | `{Product}.Host` |
| `layers/appsettings.Development.json` | `{Product}.Host` |
| `layers/launchSettings.json` | `{Product}.Host/Properties/` |

## Bước 6 — Program.cs (composition root)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddHostLayer();
var app = builder.Build();
app.UseHostLayer();
app.Run();
```

## Bước 7 — F5 validate

```bash
cd src
dotnet build
dotnet run --project "${PRODUCT}.Host"
```

- Browser: `https://localhost:7006/swagger` (hoặc port trong launchSettings).
- `GET /api/ping` → 200.
- `/health/live` → 200 (không cần DB cho liveness).

**PostgreSQL:** connection string trong `appsettings.Development.json`. Nếu chưa có DB, readiness có thể unhealthy — ping và swagger vẫn chạy. Bật DB + readiness sau theo [healthcheck-dotnet](../../healthcheck-dotnet/SKILL.md).

## Bước 8 — Sau scaffold

- Thêm entity vào `Domain/Entities/`
- Thêm handler vào `Application/Features/`
- Đăng ký handler trong `ApplicationLayerExtension`
- Readiness: skill [healthcheck-dotnet](../../healthcheck-dotnet/SKILL.md)
