# Authentication (capability)

**Xác thực** request API: JWT Bearer, API Key header, HTTP Basic, và mở rộng (Cognito, cookie, …) trên nền ASP.NET Core Authentication.

## Giá trị

- Cấu hình scheme và profile qua `appsettings` (bật/tắt, đổi profile không rebuild).
- Tách **hành vi scheme** (header name, realm, JWT validate) và **credential** (`Source`: config vs storage).
- Contract provider rõ (`IApiKeyProvider`, `IBasicCredentialProvider`, password policy) cho multitenancy và DB.

## Phạm vi

| Trong phạm vi | Ngoài phạm vi (module khác) |
|---------------|-----------------------------|
| Scheme handlers, `AddPlatformAuthentication`, `AddCore*` | RBAC/ABAC (`Platform.Authorization`) |
| Map `AuthenticationOptions`, password policy validator | Phát hành token login (Identity / `Platform.Modules.Account`) |
| `ICurrentUser` accessor (đọc user sau AuthN) | Quản lý user admin (IdentityAdmin) |

## Phụ thuộc

- ASP.NET Core `Microsoft.AspNetCore.Authentication.*`
- Host đăng ký provider storage và (tuỳ chọn) `ICurrentUserStore`

## Demo

[Sample/Program.cs](../../Sample/Program.cs) + [SampleAuthenticationExtensions](../../Sample/Extensions/SampleAuthenticationExtensions.cs); config [Sample/appsettings.json](../../Sample/appsettings.json) section `Authentication`.

## Tài liệu kỹ thuật

**[guides/authentication/README.md](../guides/authentication/README.md)** — tổng quan đầy đủ (luồng JWT, ApiKey, Basic, `Source`, Sample).
