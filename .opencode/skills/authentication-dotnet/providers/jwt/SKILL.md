---
name: authentication-dotnet-jwt
description: Đăng ký Jarvis JWT Bearer AddCoreJwtBearer và section Authentication Jwt. Dùng khi API cần xác thực Bearer token.
dependencies:
  - Jarvis.Authentications.Jwt
---

# JWT Bearer

## Package

```xml
<PackageReference Include="Jarvis.Authentications.Jwt" Version="1.0.1" />
```

## Program.cs

```csharp
using Jarvis.Authentication.Jwt;

builder.Services.AddAuthentication()
    .AddCoreJwtBearer(builder.Configuration);
```

## appsettings.json

```json
{
  "Authentication": {
    "Type": "Jwt",
    "Jwt": { }
  }
}
```

Chi tiết key JWT theo implementation package — bind từ `IConfiguration`, secret qua env.

## Pipeline

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

## Swagger

[swashbuckle-dotnet/providers/jwt-security](../../../swashbuckle-dotnet/providers/jwt-security/SKILL.md) — `SecuritySchemes: ["JWT"]`.

## Validate

- Endpoint `[Authorize]` → 401 không token
- Swagger Authorize + gọi API thành công
