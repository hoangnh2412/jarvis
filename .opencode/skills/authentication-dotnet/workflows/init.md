# Workflow: Khởi tạo authentication Jarvis

Áp dụng khi Host **chưa** có `AddAuthentication()` Jarvis.

## Checklist

```text
- [ ] 1. Chọn provider (jwt | api-key | cognito)
- [ ] 2. Đọc providers/<name>/SKILL.md
- [ ] 3. Package Jarvis.Authentications.*
- [ ] 4. AddAuthentication chain + appsettings Authentication
- [ ] 5. UseAuthentication + UseAuthorization
- [ ] 6. Validate 401/200
```

## Bước 1 — Chọn scheme

| Nhu cầu | Provider |
|---------|----------|
| Bearer JWT | [providers/jwt/SKILL.md](../providers/jwt/SKILL.md) |
| Header API key | [providers/api-key/SKILL.md](../providers/api-key/SKILL.md) |
| AWS Cognito | [providers/cognito/SKILL.md](../providers/cognito/SKILL.md) |

## Bước 2 — Program

[templates/program-auth.cs](../templates/program-auth.cs) — chỉnh theo provider.

## Bước 3 — appsettings

[templates/appsettings-authentication.json](../templates/appsettings-authentication.json)

## Bước 4 — Swagger (tùy chọn)

[swashbuckle-dotnet/providers/jwt-security](../../swashbuckle-dotnet/providers/jwt-security/SKILL.md) hoặc api-key-security.

## Anti-patterns

- Quên `UseAuthorization` sau `UseAuthentication`
- Hard-code JWT secret / API keys trong appsettings committed
