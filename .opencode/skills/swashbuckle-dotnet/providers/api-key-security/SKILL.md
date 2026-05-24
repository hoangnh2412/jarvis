---
name: swashbuckle-dotnet-api-key-security
description: Cấu hình Swagger SecuritySchemes API Key cho Jarvis.Swashbuckle. Dùng khi API xác thực qua header X-API-KEY.
dependencies:
  - Jarvis.Swashbuckle
  - Jarvis.Authentications.ApiKey
---

# API Key trên Swagger

## appsettings

```json
{
  "Swagger": {
    "SecuritySchemes": ["API_KEY"],
    "ApiKeyHeaderName": "X-API-KEY"
  },
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

## Runtime

```csharp
builder.Services.AddAuthentication()
    .AddCoreApiKey<MyApiKeyProvider>(builder.Configuration);
```

Implement `IApiKeyProvider` trong host.

Chi tiết auth: [authentication-dotnet/providers/api-key/SKILL.md](../../../authentication-dotnet/providers/api-key/SKILL.md).
