---
name: jarvis-dotnet-authentication
description: Cài Jarvis Authentication — JWT Bearer, API Key, HTTP Basic, Composite policy scheme. Dùng khi API cần xác định danh tính request (không bao gồm authorization policies).
dependencies:
  - Jarvis.Authentications
  - Jarvis.Authentications.Jwt
  - Jarvis.Authentications.ApiKey
  - Jarvis.Authentications.Basic
  - Jarvis.Authentications.Cognito
---

# Authentication

**PackageId** dùng `Jarvis.Authentications.*` (có **s**), folder repo `Jarvis.Authentication.*`.

Phạm vi module này: **xác thực** (identity) — JWT, ApiKey, Basic, Composite forward. **Authorization** (policies, roles, `[Authorize]`) là story riêng; chỉ gọi `UseAuthorization()` khi host đã có policies.

Chi tiết thiết kế / test matrix: `docs/refactor-authentication.md`.

## Packages

| Module | Project | PackageId |
|---|---|---|
| Base | `Jarvis.Authentication` | `Jarvis.Authentications` |
| JWT | `Jarvis.Authentication.Jwt` | `Jarvis.Authentications.Jwt` |
| API Key | `Jarvis.Authentication.ApiKey` | `Jarvis.Authentications.ApiKey` |
| Basic | `Jarvis.Authentication.Basic` | `Jarvis.Authentications.Basic` |
| Cognito | `Jarvis.Authentication.Cognito` | `Jarvis.Authentications.Cognito` |

Chỉ reference package cần dùng. Ví dụ API chỉ ApiKey: Base + ApiKey.

## Scheme names (chuẩn Jarvis)

| Constant | Giá trị | Ý nghĩa |
|---|---|---|
| `JarvisAuthenticationSchemes.Composite` | `Composite` | Policy scheme — forward sang scheme con |
| `JarvisAuthenticationSchemes.ApiKey` | `Default` | ApiKey scheme (trùng tên section config `Authentication:ApiKey:Default`) |
| `JarvisAuthenticationSchemes.Basic` | `Basic` | HTTP Basic scheme |
| JWT | `Bearer` | `JwtBearerDefaults.AuthenticationScheme` |

## Entry point — `AddJarvisAuthentication`

Luôn bắt đầu từ base; đăng ký scheme trong callback:

```csharp
using Jarvis.Authentication;
using Jarvis.Authentication.ApiKey;
using Jarvis.Authentication.Basic;
using Jarvis.Authentication.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

builder.Services.AddJarvisAuthentication(builder.Configuration, auth =>
{
    auth.AddJarvisCompositeScheme(includeBasic: true); // khi bật ≥ 2 scheme
    auth.AddCoreJwtBearer(builder.Configuration, JwtBearerDefaults.AuthenticationScheme);
    auth.AddCoreApiKey(builder.Configuration, JarvisAuthenticationSchemes.ApiKey);
    auth.AddCoreBasic(builder.Configuration, JarvisAuthenticationSchemes.Basic);
});
```

`AddJarvisAuthentication` bind `Authentication` → `AuthenticationRootOptions`, set `DefaultAuthenticateScheme` / `DefaultChallengeScheme` từ config.

### Composite (nhiều scheme)

Khi host bật **hơn một** loại auth, dùng Composite làm default scheme:

```csharp
auth.AddJarvisCompositeScheme(includeBasic: basicEnabled);
```

Forward rule (`AddJarvisCompositeScheme`):

1. Header `X-API-KEY` (hoặc tên custom) → scheme **Default** (ApiKey)
2. `Authorization: Basic …` → **Basic** (nếu `includeBasic: true`)
3. `Authorization: Bearer …` → **Bearer**
4. Không khớp → **Bearer** hoặc **Basic** (tùy `includeBasic`)

Config mẫu:

```json
"Authentication": {
  "DefaultAuthenticateScheme": "Composite",
  "DefaultChallengeScheme": "Composite",
  "Schemes": {
    "Jwt": { "Enabled": true },
    "ApiKey": { "Enabled": true },
    "Basic": { "Enabled": true }
  }
}
```

Pattern đầy đủ trong Sample: `Sample/Extensions/SampleAuthenticationExtensions.cs`.

## appsettings.json — khung chung

```json
{
  "Authentication": {
    "Type": "ApiKey",
    "DefaultAuthenticateScheme": "Default",
    "DefaultChallengeScheme": "Default",
    "Schemes": {
      "Jwt": { "Enabled": false },
      "ApiKey": { "Enabled": true },
      "Basic": { "Enabled": false }
    },
    "ApiKey": {
      "Default": {
        "KeyName": "X-API-KEY",
        "Mode": "SingleKey",
        "Keys": []
      }
    },
    "Jwt": {
      "Bearer": {
        "Authority": "https://your-idp",
        "Audience": "your-api",
        "RequireHttpsMetadata": true
      }
    },
    "Basic": {
      "Default": {
        "Realm": "Jarvis API",
        "Users": {
          "service": {
            "Password": "change-me",
            "Roles": [ "integration" ]
          }
        }
      }
    }
  }
}
```

- `Authentication:Type` — legacy fallback khi `Schemes:*:Enabled` chưa set (`Jwt` | `ApiKey`).
- **Không** commit secret/plaintext key vào `appsettings.json` gốc — đặt trong `appsettings.Development.json`, User Secrets, hoặc vault.
- `Authentication:ApiKey:Default:Mode`: `SingleKey` | `RealmKey`.

## JWT

```csharp
using Jarvis.Authentication.Jwt;

auth.AddCoreJwtBearer(configuration, JwtBearerDefaults.AuthenticationScheme);
```

Config section: `Authentication:Jwt:{scheme}` — thường `Bearer`.

Hai chế độ:

| Chế độ | Config | Ghi chú |
|---|---|---|
| OIDC / Authority | `Authority`, `Audience` | Metadata discovery; không cần `IssuerSigningKeys` |
| Symmetric (dev/test) | `IssuerSigningKeys[]`, `ValidateAudience`, `ValidateIssuer` | `RequireHttpsMetadata: false` chỉ dev |

```json
"Jwt": {
  "Bearer": {
    "Authority": "https://login.example.com",
    "Audience": "api://my-api",
    "RequireHttpsMetadata": true
  }
}
```

## API Key

```csharp
using Jarvis.Authentication.ApiKey;

// Mặc định: ApiKeyProvider đọc keys từ config
auth.AddCoreApiKey(configuration, JarvisAuthenticationSchemes.ApiKey);

// Custom provider
auth.AddCoreApiKey<MyApiKeyProvider>(configuration, JarvisAuthenticationSchemes.ApiKey);
```

Config: `Authentication:ApiKey:{realm}` — `KeyName`, `Mode`, `Keys[]`. `AddCoreApiKey` bind **mọi** section con dưới `Authentication:ApiKey` (realm phụ chỉ đăng ký khi có ít nhất một key).

### `SingleKey` (mặc định)

Header = secret thuần:

```http
X-API-KEY: my-secret-key
```

```json
"ApiKey": {
  "Default": {
    "KeyName": "X-API-KEY",
    "Mode": "SingleKey",
    "Keys": ["my-secret-key"]
  }
}
```

### `RealmKey`

Header = `{realm}:{secret}` — `realm` trùng tên section config:

```http
X-API-KEY: Default:my-secret-key
X-API-KEY: Integration:partner-key
```

```json
"ApiKey": {
  "Default": {
    "KeyName": "X-API-KEY",
    "Mode": "RealmKey",
    "Keys": ["my-secret-key"]
  },
  "Integration": {
    "KeyName": "X-API-KEY",
    "Mode": "RealmKey",
    "Keys": ["partner-key"]
  }
}
```

So khớp phần **sau** dấu `:` với `Keys[]` của realm tương ứng. Realm không có key trong config → từ chối.

Built-in `ApiKeyProvider` đọc keys từ config (`HashSet`, so khớng O(1)). Implement `IApiKeyProvider` khi keys lấy từ DB/vault.

Client gửi header theo `KeyName` (mặc định `X-API-KEY`).

## Basic

```csharp
using Jarvis.Authentication.Basic;

auth.AddCoreBasic(configuration, JarvisAuthenticationSchemes.Basic);
// configurationKey mặc định "Default" → section Authentication:Basic:Default

auth.AddCoreBasic<MyBasicValidator>(configuration, JarvisAuthenticationSchemes.Basic);
```

Config: `Authentication:Basic:Default` — `Realm`, `Users:{username}:Password`, `Users:{username}:Roles[]`.

- Validate on start: ít nhất một user, mỗi user phải có `Password`.
- Mặc định `ConfigBasicCredentialValidator` so khớp password từ config (dev/integration). Production: `IBasicCredentialValidator` custom (hash, DB, LDAP).

```csharp
public interface IBasicCredentialValidator
{
    Task<BasicValidationResult?> ValidateAsync(
        string schemeName, string username, string password, CancellationToken cancellationToken = default);
}
```

Client: `Authorization: Basic {base64(user:pass)}`.

## Cognito

`Jarvis.Authentication.Cognito` — legacy AWS Cognito client (`CognitoClient`, `IAuthenticationService`), section `Authentication:Cognito`.

Chưa tích hợp vào Composite/Jarvis pipeline mới. Khi cần JWT từ Cognito user pool, ưu tiên **Jwt + Authority** trỏ JWKS Cognito thay vì package Cognito cũ (trừ khi host vẫn phụ thuộc API cũ).

## Pipeline

```csharp
app.UseAuthentication();
// app.UseAuthorization(); — chỉ khi đã AddAuthorization + policies
```

Thứ tự: routing → authentication → authorization → endpoints.

Probe endpoint (Sample): `GET /api/_auth-test/whoami` — `[AllowAnonymous]`, trả `authenticated`, `scheme`, `name`.

## Swagger

Cấu hình `Swagger:SecuritySchemes` trong host (Jarvis.Swashbuckle): `JWT` (Bearer), `API_KEY` (header), thêm `Basic` nếu cần.

## Mẫu host tối thiểu (chỉ ApiKey)

```xml
<ProjectReference Include="..\Jarvis.Authentication\Jarvis.Authentication.csproj" />
<ProjectReference Include="..\Jarvis.Authentication.ApiKey\Jarvis.Authentication.ApiKey.csproj" />
```

```csharp
builder.Services.AddJarvisAuthentication(builder.Configuration, auth =>
    auth.AddCoreApiKey(builder.Configuration, JarvisAuthenticationSchemes.ApiKey));
```

```json
"Authentication": {
  "Type": "ApiKey",
  "DefaultAuthenticateScheme": "Default",
  "DefaultChallengeScheme": "Default",
  "Schemes": { "ApiKey": { "Enabled": true } },
  "ApiKey": {
    "Default": {
      "KeyName": "X-API-KEY",
      "Mode": "SingleKey",
      "Keys": []
    }
  }
}
```

Secrets: `appsettings.Development.json` hoặc User Secrets — không để key thật trong `appsettings.json` production.

## Tests

Project: `tests/Jarvis.Authentication.Tests` — chạy:

```bash
dotnet test tests/Jarvis.Authentication.Tests/Jarvis.Authentication.Tests.csproj
```

Coverage: ApiKey, Jwt, Basic, Composite forward, config validation. Tham chiếu `AuthenticationTestServer` + `AuthenticationConfigurationBuilder` khi thêm test mới.

## Checklist thêm module vào project

```text
- [ ] ProjectReference / PackageReference đúng package (Jwt, ApiKey, Basic, …)
- [ ] AddJarvisAuthentication + scheme extensions trong Program hoặc *AuthenticationExtensions
- [ ] Section Authentication trong appsettings (+ Development cho secrets)
- [ ] DefaultAuthenticateScheme = scheme đơn hoặc Composite nếu ≥ 2 scheme
- [ ] app.UseAuthentication()
- [ ] dotnet test Jarvis.Authentication.Tests (nếu sửa framework auth)
```
