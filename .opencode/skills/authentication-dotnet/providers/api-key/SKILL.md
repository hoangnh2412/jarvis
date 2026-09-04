---
name: authentication-dotnet-api-key
description: Đăng ký Jarvis API Key AddCoreApiKey với IApiKeyProvider tùy chỉnh. Dùng khi client gửi key qua header X-API-KEY hoặc tên tùy config.
dependencies:
  - Jarvis.Authentications.ApiKey
---

# API Key

## Package

```xml
<PackageReference Include="Jarvis.Authentications.ApiKey" Version="1.0.1" />
```

## Program.cs

```csharp
using Jarvis.Authentication.ApiKey;

builder.Services.AddAuthentication()
    .AddCoreApiKey<MyApiKeyProvider>(builder.Configuration);

builder.Services.AddScoped<MyApiKeyProvider>();
```

Implement `IApiKeyProvider` — validate key, map client id.

## appsettings.json

```json
{
  "Authentication": {
    "Type": "ApiKey",
    "ApiKey": {
      "Default": {
        "KeyName": "X-API-KEY",
        "Keys": []
      }
    }
  }
}
```

**Keys** và secret — chỉ env/secret store, không commit.

## Swagger

[swashbuckle-dotnet/providers/api-key-security](../../../swashbuckle-dotnet/providers/api-key-security/SKILL.md).

## Validate

- Request thiếu/sai header → 401
- Key hợp lệ → 200 trên endpoint protected
