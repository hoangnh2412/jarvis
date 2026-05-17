---
name: jarvis-dotnet-swashbuckle
description: Cài Jarvis.Swashbuckle — Swagger/OpenAPI multi-version, BaseResponse schema, security schemes. Dùng khi API cần tài liệu Swagger tích hợp Jarvis.
dependencies:
  - Jarvis.Swashbuckle
---

# Swashbuckle

## Package

| PackageId | Version* |
|---|---|
| `Jarvis.Swashbuckle` | 1.0.1 |

\*Xem csproj repo.

Phụ thuộc `Jarvis.Domain.Shared`, Swashbuckle, Asp.Versioning (API versioning do host đăng ký).

## Program.cs

```csharp
using Jarvis.Swashbuckle;

builder.AddCoreSwagger();

// API versioning (host — không thuộc Jarvis.Swashbuckle):
builder.Services.AddApiVersioning(/* … */).AddApiExplorer(/* … */);

var app = builder.Build();
app.UseCoreSwagger();
```

## appsettings.json

```json
{
  "Swagger": {
    "Enable": true,
    "Versions": ["v1", "v2"],
    "ApiKeyHeaderName": "X-API-KEY",
    "SecuritySchemes": ["JWT", "API_KEY"],
    "ApiResponseDocumentationPathPrefixes": ["/api"]
  }
}
```

## XML examples

Đặt `IExamplesProvider<>` trong entry assembly — `AddCoreSwagger` gọi `AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly())`.

Bật `<GenerateDocumentationFile>true</GenerateDocumentationFile>` trên API project.
