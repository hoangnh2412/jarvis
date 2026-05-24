# authentication-dotnet

Skill tích hợp **Jarvis.Authentication** — JWT, API Key, Cognito.

Agent đọc [SKILL.md](./SKILL.md).

## PackageId

Folder repo: `Jarvis.Authentication.*` → NuGet: **`Jarvis.Authentications.*`**.

## Khi nào dùng

| Tình huống | Workflow / Provider |
|------------|---------------------|
| Bật JWT lần đầu | [workflows/init.md](./workflows/init.md) + [providers/jwt](./providers/jwt/SKILL.md) |
| Thêm API Key / Cognito | [workflows/add.md](./workflows/add.md) |
| Swagger Authorize | [swashbuckle-dotnet](../swashbuckle-dotnet/providers/jwt-security/SKILL.md) |

## Cách gọi

```text
@.opencode/skills/authentication-dotnet/providers/jwt/SKILL.md

Thêm JWT Bearer cho MyApp.Host — Authentication:Type Jwt.
```

## Providers

| Scheme | SKILL |
|--------|-------|
| JWT | [providers/jwt/SKILL.md](./providers/jwt/SKILL.md) |
| API Key | [providers/api-key/SKILL.md](./providers/api-key/SKILL.md) |
| Cognito | [providers/cognito/SKILL.md](./providers/cognito/SKILL.md) |

## Pipeline

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Trước `MapControllers`.

## Liên quan

- [swashbuckle-dotnet](../swashbuckle-dotnet/README.md)
- [jarvis-dotnet](../jarvis-dotnet/README.md)
