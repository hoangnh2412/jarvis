---
name: jarvis-dotnet-foundation
description: Cài foundation Jarvis — Domain.Shared, Domain, Mvc (Json, WebApi, Cors, middleware). Dùng khi bootstrap API ASP.NET Core với response chuẩn và WorkContext.
dependencies:
  - Jarvis.Domain.Shared
  - Jarvis.Domain
  - Jarvis.Mvc
---

# Foundation

## Packages

| Project | PackageId (NuGet) |
|---|---|
| Jarvis.Domain.Shared | `Jarvis.Domain.Shared` |
| Jarvis.Domain | `Jarvis.Domain` |
| Jarvis.Mvc | `Jarvis.Mvc` |

`Jarvis.Mvc` kéo theo `Jarvis.Common`, `Jarvis.Domain.Shared`, `Jarvis.OpenTelemetry` (transitive).

## Program.cs

```csharp
using Jarvis.Domain;
using Jarvis.Mvc;
using Jarvis.Mvc.ApplicationBuilders;
using Jarvis.Mvc.ExceptionHandling;

builder.AddCoreJson();
builder.AddCoreCors();
builder.AddCoreDomain();
builder.AddCoreWebApi();

var app = builder.Build();
app.UseCoreCors();
app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
app.MapControllers();
```

## appsettings.json

```json
{
  "Json": {
    "IgnoreNull": true,
    "NamingPolicy": "CamelCase"
  },
  "Cors": { },
  "Middlewares": {
    "ApiResponseWrapper": {
      "IsEnable": true,
      "Includes": ["^/api"]
    }
  }
}
```

## Extension chính

| Extension | Mục đích |
|---|---|
| `AddCoreJson` | Controllers + JSON/Newtonsoft + BadRequest `BaseResponse` |
| `AddCoreWebApi` | `AddEndpointsApiExplorer`, `HttpContextAccessor` |
| `AddCoreDomain` | `IWorkContext` |
| `AddCoreCors` | CORS từ section `Cors` |
| `UseCoreMiddleware<T>` | Middleware pipeline (wrapper, …) |
