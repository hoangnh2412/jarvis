---
name: foundation-dotnet
description: Thiết lập Jarvis foundation — Domain.Shared, Domain, Mvc (Json, WebApi, Cors, ApiResponseWrapper, IWorkContext). Dùng khi bootstrap API ASP.NET Core với response chuẩn BaseResponse.
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis Foundation — Orchestrator

Skill điều phối lớp nền: `Jarvis.Domain.Shared`, `Jarvis.Domain`, `Jarvis.Mvc` trên ASP.NET Core.

Hướng dẫn người dùng: [README.md](README.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Host chưa có Json/CORS/WebApi/middleware Jarvis | [workflows/init.md](workflows/init.md) |
| Đã có foundation, chỉnh CORS / wrapper / JSON | [workflows/add.md](workflows/add.md) |

## Quy tắc cốt lõi

- `Jarvis.Mvc` kéo transitive `Jarvis.Common`, `Jarvis.Domain.Shared`, `Jarvis.OpenTelemetry` — cân nhắc khi chỉ cần domain thuần.
- Thứ tự đăng ký: `AddCoreJson` → `AddCoreCors` → `AddCoreDomain` → `AddCoreWebApi`.
- Pipeline: `UseCoreCors` → `UseCoreMiddleware<ApiResponseWrapperMiddleware>` → `MapControllers`.
- Section `Middlewares:ApiResponseWrapper` — `Includes` regex path (vd. `^/api`).
- `AddCoreDomain` đăng ký `IWorkContext` — dùng cho enricher / tenant context app.

## Packages

| PackageId | Layer |
|---|---|
| `Jarvis.Domain.Shared` | Domain.Shared |
| `Jarvis.Domain` | Host (enricher, WorkContext) |
| `Jarvis.Mvc` | Host |

## Templates

- [templates/program-setup.cs](templates/program-setup.cs)
- [templates/appsettings-foundation.json](templates/appsettings-foundation.json)

## Liên quan

- Scaffold toàn solution: [jarvis-dotnet](../jarvis-dotnet/README.md)
- Swagger sau foundation: [swashbuckle-dotnet](../swashbuckle-dotnet/README.md)
- OTEL (transitive từ Mvc): [telemetry-dotnet](../telemetry-dotnet/README.md)

## Output bắt buộc

- Extensions `AddCore*` / `UseCore*` trên Host
- `appsettings`: `Json`, `Cors`, `Middlewares`
- API trả `BaseResponse` cho path trong `Includes` (nếu bật wrapper)
