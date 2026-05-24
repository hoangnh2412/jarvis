---
name: swashbuckle-dotnet-jwt-security
description: Cấu hình Swagger SecuritySchemes JWT cho Jarvis.Swashbuckle. Dùng khi API dùng Bearer token và cần nút Authorize trên Swagger UI.
dependencies:
  - Jarvis.Swashbuckle
  - Jarvis.Authentications.Jwt
---

# JWT trên Swagger

## appsettings

```json
{
  "Swagger": {
    "Enable": true,
    "Versions": ["v1"],
    "SecuritySchemes": ["JWT"],
    "ApiResponseDocumentationPathPrefixes": ["/api"]
  },
  "Authentication": {
    "Type": "Jwt",
    "Jwt": { }
  }
}
```

## Runtime (Host)

```csharp
builder.Services.AddAuthentication()
    .AddCoreJwtBearer(builder.Configuration);
```

Chi tiết auth: [jarvis-dotnet/modules/authentication/SKILL.md](../../jarvis-dotnet/modules/authentication/SKILL.md).

## Pipeline

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Trước `MapControllers`.
