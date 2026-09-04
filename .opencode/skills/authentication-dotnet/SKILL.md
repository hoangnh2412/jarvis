---
name: authentication-dotnet
description: Thiết lập Jarvis Authentication — JWT Bearer, API Key, AWS Cognito. Dùng khi API ASP.NET Core cần xác thực Bearer, header API key, hoặc Cognito qua Jarvis.Authentications.*.
metadata:
  audience: hoangnh
  workflow: github
---

# Jarvis.Authentication — Orchestrator

Skill điều phối `Jarvis.Authentications.*` trên ASP.NET Core Host.

**Lưu ý PackageId:** folder repo `Jarvis.Authentication.*` → NuGet **`Jarvis.Authentications.*`** (có **s**).

Hướng dẫn: [README.md](README.md).

## Khi nào dùng workflow nào

| Tình huống | Workflow |
|---|---|
| Host chưa có authentication Jarvis | [workflows/init.md](workflows/init.md) |
| Thêm scheme (JWT + API Key) hoặc Cognito | [workflows/add.md](workflows/add.md) |

## Quy tắc cốt lõi

- `builder.Services.AddAuthentication()` rồi chain `.AddCoreJwtBearer` / `.AddCoreApiKey<>` / Cognito.
- Pipeline: `UseAuthentication()` → `UseAuthorization()` **trước** `MapControllers`.
- Section `Authentication` trong appsettings — `Type`, nhánh `Jwt` / `ApiKey` / `Cognito`.
- API Key: implement `IApiKeyProvider` trong host.
- Swagger security: [swashbuckle-dotnet](../swashbuckle-dotnet/README.md) — `SecuritySchemes` JWT / API_KEY.
- Không commit secret/key vào repo — env / secret store.

## Packages

| PackageId | Khi nào |
|---|---|
| `Jarvis.Authentications` | Base |
| `Jarvis.Authentications.Jwt` | Bearer JWT |
| `Jarvis.Authentications.ApiKey` | Header API key |
| `Jarvis.Authentications.Cognito` | AWS Cognito |

## Providers (atomic)

| Provider | Path |
|---|---|
| JWT Bearer | [providers/jwt/SKILL.md](providers/jwt/SKILL.md) |
| API Key | [providers/api-key/SKILL.md](providers/api-key/SKILL.md) |
| Cognito | [providers/cognito/SKILL.md](providers/cognito/SKILL.md) |

## Templates

- [templates/program-auth.cs](templates/program-auth.cs)
- [templates/appsettings-authentication.json](templates/appsettings-authentication.json)

## Output bắt buộc

- `AddAuthentication` chain + `UseAuthentication` / `UseAuthorization`
- `appsettings` section `Authentication`
- Protected endpoint trả 401 khi thiếu credential (validate)
