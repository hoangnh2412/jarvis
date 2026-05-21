# Refactor Authentication — Code Review

**Phạm vi:** `Jarvis.Authentication`, `Jarvis.Authentication.Jwt`, `Jarvis.Authentication.ApiKey`, `Jarvis.Authentication.Cognito`, cấu hình `Sample/appsettings.json` (Authentication + Swagger), tích hợp thực tế trong `Sample/Program.cs`.

**Chuẩn review:** `.opencode/skills/code-review/SKILL.md` + mục tiêu kiến trúc Jarvis (`.opencode/skills/jarvis-dotnet/modules/authentication/SKILL.md`).

**Kết luận nhanh:** Các package hiện tại là **khung mỏng / chưa đủ vai trò base**; JWT và API Key có extension DI nhưng **chưa chạy end-to-end** trong Sample và có **lỗi cấu hình–runtime** với API Key. Cognito gần như **stub**. Chưa đáp ứng yêu cầu mở rộng (password policy, cookie, OpenId, Basic). **Overall: blocked** — cần refactor trước khi coi là production-ready.

---

## Đánh giá theo yêu cầu kiến trúc

| # | Yêu cầu | Hiện trạng | Đáp ứng |
|---|---------|------------|---------|
| 1 | `Jarvis.Authentication` là base chung (Basic, Bearer JWT, Cognito, OpenId, …) | Chỉ có `AuthenticationOption.Type` (string) và `AwsOption`; không có extension `AddAuthentication`, abstraction scheme, options chung, pipeline helper | **Không** |
| 2 | JWT: thêm `Jarvis.Authentication.Jwt`, khai báo đơn giản | Có `AddCoreJwtBearer(configuration)` + bind `Authentication:Jwt:{scheme}`; thiếu validation config, `RequireHttpsMetadata = false`, không gắn `Authentication.Type` | **Một phần** |
| 3 | API Key: thêm `Jarvis.Authentication.ApiKey` | Có `AddCoreApiKey<T>`; provider mặc định **không khớp** appsettings Sample; host phải tự register `T` | **Một phần (có bug)** |
| 4 | Mở rộng customize (password complexity, expire, cookie, …) | Không có options/hook/interface cho policy hoặc cookie | **Không** |

---

## Critical Issues

### 1. API Key: format key bắt buộc `realm:secret` — Sample/config không tương thích

**File:** `Jarvis.Authentication.ApiKey/ApiKeyProvider.cs`

```20:29:Jarvis.Authentication.ApiKey/ApiKeyProvider.cs
    public virtual async Task<IApiKey?> ProvideAsync(string key)
    {
        await Task.Yield();

        var splited = key.Split(":");
        if (splited.Length != 2)
        {
            _logger.LogError($"API KEY do not contains REALM");
            return null;
        }
```

**Scenario:** Client gửi header `X-API-KEY: ZofkXgxwiO6F2s1JJCX5L6Wa7JctPmpO` (đúng với `appsettings.json`). `Split(":")` → length 1 → luôn `null` → **401 mọi request**.

**Impact:** Mọi host dùng config “key thuần” như Sample sẽ **không authenticate được** dù key đúng trong config.

**Suggested fix:** Hỗ trợ hai mode (cấu hình): `SingleKey` (so khớp trực tiếp) và `RealmKey` (`realm:secret`); hoặc đổi provider mặc định khớp skill/doc (`Keys[]` không bắt buộc prefix).

---

### 2. API Key: lệch tên scheme giữa config, named options và `IOptionsFactory.Create(realm)`

**Files:** `AuthenticationBuilderExtension.cs`, `ApiKeyProvider.cs`, `Sample/appsettings.json`

| Thành phần | Giá trị thực tế |
|------------|-----------------|
| `AddCoreApiKey` (overload mặc định) | `authenticationScheme` = `ApiKeyDefaults.AuthenticationScheme` → thường là `"ApiKey"` |
| Section bind | `Authentication:ApiKey:ApiKey` |
| Sample config | `Authentication:ApiKey:Default` |
| Provider lookup | `_options.Create(realm)` với `realm` từ prefix key → `"Default"` |

**Scenario:** Gọi `AddCoreApiKey<ApiKeyProvider>(configuration)` + config Sample.

1. `GetSection("Authentication:ApiKey:ApiKey")` → **rỗng** → `KeyName` null.
2. Header `Default:Zofk...` → `Create("Default")` → **không có** named options `"Default"` (chỉ register `"ApiKey"`).

**Impact:** Cấu hình trong `appsettings` **không được load**; xác thực fail kép với issue #1.

**Suggested fix:** Thống nhất một “scheme name” (ví dụ `Default`) xuyên suốt: section JSON, `Configure<T>(name)`, `ApiKeyOptions.Realm`, và document header format.

---

### 3. Sample: reference auth packages nhưng không đăng ký pipeline

**File:** `Sample/Program.cs`

Không có `AddAuthentication()`, `AddCoreJwtBearer` / `AddCoreApiKey`, `UseAuthentication()`, `UseAuthorization()`.

**Impact:** Cấu hình `Authentication` trong `appsettings.json` **không có tác dụng**; Swagger vẫn khai báo `JWT` / `API_KEY` nhưng runtime **anonymous** — lệch doc/skills và dễ hiểu nhầm khi test bảo mật.

**Suggested fix:** Thêm `builder.Services.AddAuthentication(...)` theo `Authentication:Type` hoặc đăng ký tường minh từng scheme; bật middleware sau `UseCoreCors`.

---

### 4. JWT: `RequireHttpsMetadata = false` mặc định

**File:** `Jarvis.Authentication.Jwt/AuthenticationBuilderExtension.cs` (dòng 32)

**Scenario:** Deploy production quên override `configureOptions` → metadata issuer có thể bị thay đổi qua HTTP (tùy môi trường).

**Impact:** Rủi ro bảo mật **cao** trên môi trường không terminate TLS đúng chuẩn.

**Suggested fix:** Mặc định `true`; chỉ `false` trong Development qua `IHostEnvironment` hoặc config `Authentication:Jwt:*:RequireHttpsMetadata`.

---

### 5. JWT: thiếu config → `IssuerSigningKeys` null, validation không rõ ràng

**File:** `AuthenticationBuilderExtension.cs` — `authOption?.IssuerSigningKeys.Select(...)`.

**Scenario:** Thiếu section `Authentication:Jwt:{scheme}` → `authOption` null → `IssuerSigningKeys` null trên `TokenValidationParameters`.

**Impact:** Mọi token fail hoặc hành vi phụ thuộc version IdentityModel — **khó debug**, dễ coi là “JWT hỏng” thay vì “chưa cấu hình”.

**Suggested fix:** `ValidateOnStart` / throw `OptionsValidationException` khi `ValidateIssuerSigningKey` và không có key; document section bắt buộc.

---

### 6. Cognito: binding config sai tên property

**Files:** `CognitoOption.cs`, `Sample/appsettings.json`

| appsettings | `CognitoOption` |
|-------------|-----------------|
| `UserPools` | `UserPoolIds` |
| `Endpoint` | *(không có)* |

**Impact:** `UserPools` / `Endpoint` **không bind** → client AWS khởi tạo với pool/endpoint sai hoặc rỗng.

---

### 7. Secret trong repo (Sample)

**File:** `Sample/appsettings.json` — API key plaintext.

**Impact:** Lộ credential nếu commit/public repo; vi phạm security checklist.

**Suggested fix:** User secrets / env / secret manager; key mẫu chỉ trong `appsettings.Development.json` hoặc placeholder.

---

## Suggestions

### `Jarvis.Authentication` — Base quá mỏng, `Authentication.Type` không được dùng

**Issue:** `AuthenticationOption` chỉ có `Type`; không có `AddJarvisAuthentication(IConfiguration)` đọc `Type` và gọi module tương ứng.

**Impact:** Host phải biết từng package; không đạt mục tiêu “base chung”.

**Suggested fix:** Trong base (chỉ abstractions + options root, không reference JWT package):

- `AuthenticationRootOptions` với `AuthenticationType` (enum).
- `IAuthenticationModule` hoặc extension method từng satellite đăng ký qua `TryAddEnumerable`.
- Optional: `AddJarvisAuthentication(configuration, Action<AuthenticationBuilder>?)` orchestration ở **Host**, tránh circular package ref.

---

### `Jarvis.Authentication.ApiKey` — Hiệu năng lookup key

**Issue:** `options.Keys.Contains(apikey)` — `string[]`, O(n) mỗi request.

**Impact:** Bottleneck khi danh sách key lớn (multi-tenant, rotate key).

**Suggested fix:** Build `HashSet<string>` (case-sensitive hoặc ordinal ignore case tùy policy) lúc bind options / `IPostConfigureOptions`.

---

### `Jarvis.Authentication.ApiKey` — `await Task.Yield()` không cần thiết

**Issue:** `ProvideAsync` luôn yield rồi xử lý đồng bộ.

**Impact:** Allocation + context switch vô ích trên hot path.

**Suggested fix:** `return Task.FromResult(...)` hoặc `ValueTask`; chỉ `async` khi gọi DB/external.

---

### `Jarvis.Authentication.ApiKey` — Không đăng ký `ApiKeyProvider` mặc định

**Issue:** `AddCoreApiKey<T>` yêu cầu `T : IApiKeyProvider` nhưng không `AddSingleton<IApiKeyProvider, T>()`.

**Impact:** Host quên register → fail lúc resolve (lỗi DI, không phải 401 rõ ràng).

**Suggested fix:** `builder.Services.AddSingleton<IApiKeyProvider, T>()` trong extension; overload không generic dùng `ApiKeyProvider` built-in.

---

### `Jarvis.Authentication.Jwt` — Custom `LifetimeValidator` trùng framework

**Issue:** Copy logic validate lifetime (~40 dòng) trong khi JwtBearer đã có `ValidateLifetime`.

**Impact:** Chi phí bảo trì khi nâng `Microsoft.IdentityModel.*`; khó tái sử dụng cho OpenId (issuer khác).

**Suggested fix:** Dùng `ValidateLifetime` + `TokenValidationParameters.ClockSkew`; chỉ giữ `MaxExpireMinutes` qua `LifetimeValidator` nhỏ hoặc `ISecurityTokenValidator` tùy biến.

---

### `Jarvis.Authentication.Cognito` — Stub, không tích hợp ASP.NET Core Authentication

**Issue:** `IAuthenticationService` rỗng; `CognitoClient` chỉ new `AmazonCognitoIdentityProviderClient`; field public, không `IDisposable` wrapper.

**Impact:** Không dùng được như Bearer/Cognito JWT middleware; không tái sử dụng cho login/sign-up flow.

**Suggested fix:** Tách `Jarvis.Authentication.Cognito` thành: (a) admin API client + options, (b) `AddCognitoJwtBearer` (validate token qua JWKS user pool) — hoặc document rõ package chỉ là **AWS SDK helper**, không phải auth scheme.

---

### `AwsOption` trong base

**Issue:** Credential AWS nằm `Jarvis.Authentication` trong khi chỉ Cognito dùng.

**Impact:** Base package ô nhiễm AWS; host chỉ cần JWT vẫn kéo concept AWS nếu bind nhầm section.

**Suggested fix:** Chuyển `AwsOption` sang `Jarvis.Authentication.Cognito` hoặc `Jarvis.Authentication.Aws` nhỏ.

---

### Swagger vs runtime

**Issue:** `Swagger:SecuritySchemes` = `JWT`, `API_KEY` nhưng không `AddSecurityRequirement` đồng bộ với scheme đã `AddAuthentication`.

**Impact:** Swagger “có nút Authorize” nhưng API không enforce — false sense of security khi demo.

**Suggested fix:** Đăng ký auth trước Swagger; hoặc derive `SecuritySchemes` từ schemes đã add.

---

## Best Practices & Improvements

### Kiến trúc đề xuất (đáp ứng 4 yêu cầu)

```text
Jarvis.Authentication                    ← contracts, root options, password/cookie policy options, validation
    ↑
    ├── Jarvis.Authentication.Jwt        ← AddCoreJwtBearer (optional package)
    ├── Jarvis.Authentication.ApiKey     ← AddCoreApiKey (optional package)
    ├── Jarvis.Authentication.Cognito    ← SDK + JwtBearer từ User Pool (optional)
    └── (tương lai) .Basic, .OpenIdConnect
```

**Base (`Jarvis.Authentication`) nên có:**

| Thành phần | Mục đích |
|------------|----------|
| `AuthenticationRootOptions` | `Type`, default scheme, forward scheme |
| `PasswordPolicyOptions` | Min length, complexity, history — cho Basic/local account |
| `PasswordExpirationOptions` | Max age, warn days — hook `IPasswordExpirationValidator` |
| `JarvisCookieAuthenticationOptions` | Name, HttpOnly, SameSite, sliding expiration — mirror cookie middleware |
| `IAuthenticationCustomizer` / `IPostConfigureOptions<T>` | Extension point không sửa library |
| Options validation (`IValidateOptions<T>`) | Fail fast lúc startup |

**Host wiring mẫu (mục tiêu):**

```csharp
builder.Services
    .AddAuthentication()
    .AddCoreApiKey(builder.Configuration)   // hoặc .AddCoreJwtBearer — chỉ add package cần
    ;
// tùy chọn: .AddJarvisAuthentication(builder.Configuration) đọc Authentication:Type

app.UseAuthentication();
app.UseAuthorization();
```

**Config mẫu thống nhất:**

```json
"Authentication": {
  "Type": "ApiKey",
  "DefaultScheme": "Default",
  "ApiKey": {
    "Default": {
      "KeyName": "X-API-KEY",
      "Mode": "SingleKey",
      "Keys": []
    }
  },
  "Jwt": {
    "Bearer": { "IssuerSigningKeys": [], "ValidateIssuer": true }
  },
  "PasswordPolicy": { "MinLength": 12 },
  "Cookie": { "LoginPath": "/login" }
}
```

### Concurrency / deadlock / memory

| Chủ đề | Đánh giá |
|--------|----------|
| Deadlock | **Không thấy** — không lock/`Wait()`; Cognito ctor sync một lần |
| Thread safety | `AmazonCognitoIdentityProviderClient` thread-safe nếu **singleton** DI; hiện chưa register lifetime |
| Memory | JWT `SaveToken = true` giữ token trên `HttpContext` — chấp nhận được; tránh cache token lớn custom |
| Bottleneck | API Key linear scan + `Task.Yield` (xem Suggestions) |
| NativeAOT | Chưa xét; AWS SDK + IdentityModel thường cần kiểm tra riêng nếu bật AOT |

### Tái sử dụng & mở rộng

- **Đúng hướng:** Tách package theo scheme; `Action<JwtBearerOptions>?` / `Action<ApiKeyOptions>?` cho override.
- **Thiếu:** Policy-based authorization helpers, claims transformation chung, multi-scheme (`Jwt` + `ApiKey`), và **extension points** cho password/cookie như yêu cầu #4.
- **Đặt tên:** Folder `Jarvis.Authentication.*` vs NuGet `Jarvis.Authentications.*` — document một lần trong README base để tránh nhầm package.

### Test đề xuất (khi refactor)

- Integration: header `X-API-KEY` với config Sample → 200 vs 401.
- Options: thiếu `Authentication:Jwt:Bearer:IssuerSigningKeys` → startup fail rõ message.
- Unit: `ApiKeyProvider` với `SingleKey` vs `RealmKey`.

---

## Summary

| Project | Nhận xét ngắn |
|---------|----------------|
| `Jarvis.Authentication` | Chưa là base: thiếu orchestration, shared options, extension DI; `AwsOption` lệch scope |
| `Jarvis.Authentication.Jwt` | Có `AddCoreJwtBearer`, bind config tốt một phần; cần harden HTTPS/metadata, validate options, giảm custom validator |
| `Jarvis.Authentication.ApiKey` | **Blocked** — provider/config/scheme lệch Sample; perf và DI chưa hoàn chỉnh |
| `Jarvis.Authentication.Cognito` | Stub — binding sai, không auth middleware; cần thiết kế lại vai trò |
| `Sample` | Config + Swagger gợi ý auth nhưng **Program.cs không wire** |

**Overall:** **blocked** cho production và cho mục tiêu kiến trúc 4 điểm của bạn. Thứ tự refactor đề xuất:

1. Sửa API Key (format key, scheme name, named options, register provider, HashSet lookup).
2. Mở rộng `Jarvis.Authentication` (root options, password/cookie options, validation, tài liệu).
3. Wire Sample + secrets; bật `UseAuthentication` / `UseAuthorization`.
4. Harden JWT (HTTPS metadata, startup validation).
5. Hoàn thiện hoặc thu hẹp phạm vi Cognito + sửa property bind.
6. (Sau) `Jarvis.Authentication.Basic`, OpenId Connect package tách riêng.

---

## Phụ lục: Ma trận file hiện có

| File | Vai trò |
|------|---------|
| `AuthenticationOption.cs` | Chỉ `Type` string |
| `AwsOption.cs` | AWS credentials (nên thuộc Cognito) |
| `AuthenticationJwtOption.cs` | JWT validation flags |
| `AuthenticationBuilderExtension.cs` (Jwt) | `AddCoreJwtBearer` |
| `AuthenticationApiKeyOption.cs` | `KeyName`, `Keys[]` |
| `ApiKeyProvider.cs` | Validate realm:key |
| `AuthenticationBuilderExtension.cs` (ApiKey) | `AddCoreApiKey<T>` |
| `CognitoOption.cs` | Region, pools, clients |
| `CognitoClient.cs` | AWS client factory |
| `IAuthenticationService.cs` | Empty marker |

*Review date: 2026-05-21*
