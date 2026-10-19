# Authentication — cấu hình qua `appsettings`

Tổng quan kiến trúc và luồng runtime: [README.md](./README.md).

Platform **không thay** mô hình scheme/handler của [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/). Mục tiêu: **orchestration từ config** — bật/tắt handler và chọn profile chi tiết **không cần rebuild**.

## Cấu trúc `Authentication`

| Nhánh | Trả lời câu hỏi |
|-------|------------------|
| `DefaultScheme`, `DefaultAuthenticateScheme`, `DefaultChallengeScheme`, `DefaultSignInScheme`, `DefaultSignOutScheme`, `DefaultForbidScheme`, `RequireAuthenticatedSignIn` | Map 1:1 `AuthenticationOptions` của ASP.NET Core |
| `Schemes:{Kind}` | `Enabled`, `Profile`, và `Profiles:{name}` |
| `PasswordPolicy` | `Source` (`Configuration` \| `Storage`), `Defaults` |

ASP.NET Core vẫn quyết định **cách** authenticate/challenge theo tên scheme đã đăng ký; Platform chỉ đọc JSON và gọi `AddCore*`.

## Map sang `AuthenticationOptions` (ASP.NET Core)

| `AuthenticationOptions` | `Authentication:{property}` trong appsettings | Ghi chú |
|-------------------------|-----------------------------------------------|---------|
| Handler đăng ký | `Schemes.*.Enabled` + host `AddCore*` | Đăng ký handler |
| Chi tiết handler | `Schemes:{Kind}:Profiles:{Profile}` | KeyName, JWT authority, Realm, … |
| `DefaultScheme` | `DefaultScheme` | Fallback khi `DefaultAuthenticateScheme` / `DefaultChallengeScheme` / … không set — **không** gom nhiều scheme trên một request |
| `DefaultAuthenticateScheme` | `DefaultAuthenticateScheme` | Một scheme (`Bearer`, `ApiKey`, `Basic`); trống → `DefaultScheme` → `Type` |
| `DefaultChallengeScheme` | `DefaultChallengeScheme` | Thường giống authenticate (API) |
| `DefaultSignInScheme` | `DefaultSignInScheme` | Vd. `Cookies` khi có `AddCookie` |
| `DefaultSignOutScheme` | `DefaultSignOutScheme` | Cùng cookie scheme |
| `DefaultForbidScheme` | `DefaultForbidScheme` | Tùy chọn |
| `RequireAuthenticatedSignIn` | `RequireAuthenticatedSignIn` | `true` / `false` (mặc định `false`) |

Gán qua `AuthenticationSchemeResolution.ApplyTo` trong `AddPlatformAuthentication`.

**Authenticate vs Challenge** (như tài liệu ASP.NET Core): authenticate = đọc credential đã gửi; challenge = yêu cầu client đăng nhập (401 / redirect). Platform map hai property default; hành vi cụ thể vẫn do handler.

**AuthN vs AuthZ:** `AuthenticationOptions` chỉ tầng *ai*; quyền RBAC/ABAC → `Platform.Authorization` + `[Authorize(Policy = …)]`.

Ví dụ API + web (khi đã đăng ký Cookie + JWT):

```json
"DefaultAuthenticateScheme": "Bearer",
"DefaultChallengeScheme": "Bearer",
"DefaultSignInScheme": "Cookies",
"DefaultSignOutScheme": "Cookies",
"RequireAuthenticatedSignIn": true
```

Endpoint có thể ghi đè: `[Authorize(AuthenticationSchemes = "Cookies")]`.

## `Schemes` — Enabled, Profile, và `Profiles`

```json
"Schemes": {
  "ApiKey": {
    "Enabled": true,
    "Profile": "Default",
    "Profiles": {
      "Default": { "Source": "Storage", "KeyName": "X-API-KEY" },
      "Integration": { "Source": "Configuration", "KeyName": "X-API-KEY", "Key": "..." }
    }
  },
  "Basic": {
    "Enabled": true,
    "Profile": "Default",
    "Profiles": {
      "Default": { "Realm": "Basic Authentication" }
    }
  },
  "Bearer": {
    "Enabled": false,
    "Profile": "Default",
    "Profiles": {
      "Default": { "Authority": "", "ValidateAudience": true }
    }
  }
}
```

| Field | Ý nghĩa |
|-------|---------|
| `Enabled` | `true` → host đăng ký handler (`AddCoreApiKey`, …). `false` → không đăng ký; request kiểu đó → **401**. |
| `Profile` | Tên profile active (Basic/Bearer handler). ApiKey có thể dùng nhiều profile qua prefix `profile:secret`. |
| `Profiles.{name}` | `Source` + bind option (`KeyName`, `Realm`, JWT, …). Mỗi profile có thể `Configuration` hoặc `Storage` riêng. |

Đường dẫn bind: `Authentication:Schemes:ApiKey:Profiles:{Profile}`, tương tự `Basic`, `Bearer`.

- Secret tra storage (`IApiKeyProvider`). Xem [storage-backed-providers](./storage-backed-providers.md).
- Header secret thuần → profile từ `Schemes.ApiKey.Profile` / `ApiKeyProviderOptions.DefaultRealm`.

Profile sai / section trống → **fallback** `Default`, không “tắt” ASP.NET Core.

## Default authenticate / challenge

| Giá trị | Hành vi |
|---------|---------|
| `Bearer`, `Basic`, `Default` (scheme ApiKey) | Scheme mặc định cho `[Authorize]` — một handler ASP.NET Core. Nhiều scheme trên endpoint: `[Authorize(AuthenticationSchemes = "Bearer,Basic")]` hoặc policy scheme tùy host. |

Xem `AuthenticationRootOptions` (XML) và [current-user](./current-user.md).

## Sample

`AddSampleAuthentication` đọc `Schemes` + `Profile` và gọi `AddCore*`. Đổi `Enabled` / `Profile` / `DefaultAuthenticateScheme` trong `appsettings` (+ User Secrets cho secret) là đủ cho dev.

## Mở rộng (Cookie, OpenIdConnect, OAuth)

Cùng pattern: `Schemes:OpenIdConnect` với `Enabled`, `Profile`, `Profiles:{name}` khi có package satellite và đăng ký trong host.
