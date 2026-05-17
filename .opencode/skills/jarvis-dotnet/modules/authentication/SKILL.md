---
name: jarvis-dotnet-authentication
description: Cài Jarvis Authentication — JWT, API Key, Cognito. Dùng khi API cần xác thực qua Bearer, header API key, hoặc AWS Cognito.
dependencies:
  - Jarvis.Authentications
  - Jarvis.Authentications.Jwt
  - Jarvis.Authentications.ApiKey
  - Jarvis.Authentications.Cognito
---

# Authentication

**PackageId** dùng `Jarvis.Authentications.*` (có **s**), folder repo `Jarvis.Authentication.*`.

## Packages

| Module | PackageId |
|---|---|
| Base | `Jarvis.Authentications` |
| JWT | `Jarvis.Authentications.Jwt` |
| API Key | `Jarvis.Authentications.ApiKey` |
| Cognito | `Jarvis.Authentications.Cognito` |

## JWT

```csharp
using Jarvis.Authentication.Jwt;

builder.Services.AddAuthentication()
    .AddCoreJwtBearer(builder.Configuration);
```

```json
"Authentication": {
  "Type": "Jwt",
  "Jwt": { }
}
```

## API Key

```csharp
using Jarvis.Authentication.ApiKey;

builder.Services.AddAuthentication()
    .AddCoreApiKey<MyApiKeyProvider>(builder.Configuration);
```

```json
"Authentication": {
  "Type": "ApiKey",
  "ApiKey": {
    "Default": {
      "KeyName": "X-API-KEY",
      "Keys": []
    }
  }
}
```

Implement `IApiKeyProvider` trong host.

## Cognito

ProjectReference `Jarvis.Authentication.Cognito` — dùng `CognitoClient` / `IAuthenticationService` theo section `Authentication:Cognito`.

## Pipeline

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Swagger security: cấu hình `Swagger:SecuritySchemes` (`JWT`, `API_KEY`).
